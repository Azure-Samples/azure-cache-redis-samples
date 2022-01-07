using Microsoft.Extensions.Configuration;
using StackExchange.Redis;
using System;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace ContosoTeamStats
{
    public class RedisConnection : IDisposable
    {
        private long _lastReconnectTicks = DateTimeOffset.MinValue.UtcTicks;
        private DateTimeOffset _firstErrorTime = DateTimeOffset.MinValue;
        private DateTimeOffset _previousErrorTime = DateTimeOffset.MinValue;

        // In general, let StackExchange.Redis handle most reconnects,
        // so limit the frequency of how often ForceReconnect() will
        // actually reconnect.
        private TimeSpan _reconnectMinInterval = TimeSpan.FromSeconds(60);

        // If errors continue for longer than the below threshold, then the
        // multiplexer seems to not be reconnecting, so ForceReconnect() will
        // re-create the multiplexer.
        private TimeSpan _reconnectErrorThreshold = TimeSpan.FromSeconds(30);
        private TimeSpan _restartConnectionTimeout = TimeSpan.FromSeconds(15);
        private const int _retryMaxAttempts = 5;

        private SemaphoreSlim _reconnectSemaphore = new SemaphoreSlim(initialCount: 1, maxCount: 1);
        private SemaphoreSlim _initSemaphore = new SemaphoreSlim(initialCount: 1, maxCount: 1);
        private IConfiguration _configuration;
        private ConnectionMultiplexer _connection;
        private IDatabase _database;

        public RedisConnection(IConfiguration configuration)
        {
            _configuration = configuration;
            _connection = CreateConnectionAsync().GetAwaiter().GetResult();
            _database = _connection.GetDatabase();
        }

        // In real applications, consider using a framework such as
        // Polly to make it easier to customize the retry approach.
        // For more info, please see: https://github.com/App-vNext/Polly
        public async Task<T> BasicRetryAsync<T>(Func<IDatabase, Task<T>> func)
        {
            int reconnectRetry = 0;

            while (true)
            {
                try
                {
                    return await func(_database);
                }
                catch (Exception ex) when (ex is RedisConnectionException || ex is SocketException)
                {
                    reconnectRetry++;
                    if (reconnectRetry > _retryMaxAttempts)
                        throw;
                    try
                    {
                        await ForceReconnectAsync();
                    }
                    catch (ObjectDisposedException) { }
                }
            }
        }

        // This method may return null if it fails to acquire the semaphore in time.
        // Use the return value to update the "connection" field
        private async Task<ConnectionMultiplexer> CreateConnectionAsync()
        {
            if (_connection != null)
            {
                // If we already have a good connection, let's re-use it
                return _connection;
            }

            try
            {
                await _initSemaphore.WaitAsync(_restartConnectionTimeout);
            }
            catch
            {
                // We failed to enter the semaphore in the given amount of time. Connection will either be null, or have a value that was created by another thread.
                return _connection;
            }

            // We entered the semaphore successfully.
            try
            {
                if (_connection != null)
                {
                    // Another thread must have finished creating a new connection while we were waiting to enter the semaphore. Let's use it
                    return _connection;
                }

                // Otherwise, we really need to create a new connection.
                string cacheConnection = _configuration["CacheConnection"].ToString();
                return await ConnectionMultiplexer.ConnectAsync(cacheConnection);
            }
            finally
            {
                _initSemaphore.Release();
            }
        }

        private static async Task CloseConnectionAsync(ConnectionMultiplexer oldConnection)
        {
            if (oldConnection == null)
            {
                return;
            }
            try
            {
                await oldConnection.CloseAsync();
            }
            catch (Exception)
            {
                // Ignore any errors from the oldConnection
            }
        }

        /// <summary>
        /// Force a new ConnectionMultiplexer to be created.
        /// NOTES:
        ///     1. Users of the ConnectionMultiplexer MUST handle ObjectDisposedExceptions, which can now happen as a result of calling ForceReconnectAsync().
        ///     2. Call ForceReconnectAsync() for RedisConnectionExceptions and RedisSocketExceptions. You can also call it for RedisTimeoutExceptions,
        ///         but only if you're using generous ReconnectMinInterval and ReconnectErrorThreshold. Otherwise, establishing new connections can cause
        ///         a cascade failure on a server that's timing out because it's already overloaded.
        ///     3. The code will:
        ///         a. wait to reconnect for at least the "ReconnectErrorThreshold" time of repeated errors before actually reconnecting
        ///         b. not reconnect more frequently than configured in "ReconnectMinInterval"
        /// </summary>
        private async Task ForceReconnectAsync()
        {
            var utcNow = DateTimeOffset.UtcNow;
            long previousTicks = Interlocked.Read(ref _lastReconnectTicks);
            var previousReconnectTime = new DateTimeOffset(previousTicks, TimeSpan.Zero);
            TimeSpan elapsedSinceLastReconnect = utcNow - previousReconnectTime;

            // If multiple threads call ForceReconnectAsync at the same time, we only want to honor one of them.
            if (elapsedSinceLastReconnect < _reconnectMinInterval)
            {
                return;
            }

            try
            {
                await _reconnectSemaphore.WaitAsync(_restartConnectionTimeout);
            }
            catch
            {
                // If we fail to enter the semaphore, then it is possible that another thread has already done so.
                // ForceReconnectAsync() can be retried while connectivity problems persist.
                return;
            }

            try
            {
                utcNow = DateTimeOffset.UtcNow;
                elapsedSinceLastReconnect = utcNow - previousReconnectTime;

                if (_firstErrorTime == DateTimeOffset.MinValue)
                {
                    // We haven't seen an error since last reconnect, so set initial values.
                    _firstErrorTime = utcNow;
                    _previousErrorTime = utcNow;
                    return;
                }

                if (elapsedSinceLastReconnect < _reconnectMinInterval)
                {
                    return; // Some other thread made it through the check and the lock, so nothing to do.
                }

                TimeSpan elapsedSinceFirstError = utcNow - _firstErrorTime;
                TimeSpan elapsedSinceMostRecentError = utcNow - _previousErrorTime;

                bool shouldReconnect =
                    elapsedSinceFirstError >= _reconnectErrorThreshold // Make sure we gave the multiplexer enough time to reconnect on its own if it could.
                    && elapsedSinceMostRecentError <= _reconnectErrorThreshold; // Make sure we aren't working on stale data (e.g. if there was a gap in errors, don't reconnect yet).

                // Update the previousErrorTime timestamp to be now (e.g. this reconnect request).
                _previousErrorTime = utcNow;

                if (!shouldReconnect)
                {
                    return;
                }

                _firstErrorTime = DateTimeOffset.MinValue;
                _previousErrorTime = DateTimeOffset.MinValue;

                ConnectionMultiplexer oldConnection = _connection;
                await CloseConnectionAsync(oldConnection);
                Interlocked.Exchange(ref _connection, null);
                ConnectionMultiplexer newConnection = await CreateConnectionAsync();
                Interlocked.Exchange(ref _connection, newConnection);
                Interlocked.Exchange(ref _lastReconnectTicks, utcNow.UtcTicks);
                IDatabase newDatabase = _connection.GetDatabase();
                Interlocked.Exchange(ref _database, newDatabase);
            }
            finally
            {
                _reconnectSemaphore.Release();
            }
        }

        public void Dispose()
        {
            try { _connection.Close(); } catch { }
        }
    }
}