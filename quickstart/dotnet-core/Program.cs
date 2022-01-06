using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using StackExchange.Redis;
using System;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace Redistest
{
    class Employee
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public int Age { get; set; }

        public Employee(string employeeId, string name, int age)
        {
            Id = employeeId;
            Name = name;
            Age = age;
        }
    }

    class Program
    {
        private static IConfigurationRoot Configuration { get; set; }
        private static long _lastReconnectTicks = DateTimeOffset.MinValue.UtcTicks;
        private static DateTimeOffset _firstErrorTime = DateTimeOffset.MinValue;
        private static DateTimeOffset _previousErrorTime = DateTimeOffset.MinValue;

        private static SemaphoreSlim _reconnectSemaphore = new SemaphoreSlim(initialCount: 1, maxCount: 1);
        private static SemaphoreSlim _initSemaphore = new SemaphoreSlim(initialCount: 1, maxCount: 1);

        private static ConnectionMultiplexer _connection;
        private static bool _didInitialize = false;

        // In general, let StackExchange.Redis handle most reconnects,
        // so limit the frequency of how often ForceReconnect() will
        // actually reconnect.
        public static TimeSpan ReconnectMinInterval => TimeSpan.FromSeconds(60);

        // If errors continue for longer than the below threshold, then the
        // multiplexer seems to not be reconnecting, so ForceReconnect() will
        // re-create the multiplexer.
        public static TimeSpan ReconnectErrorThreshold => TimeSpan.FromSeconds(30);

        public static TimeSpan RestartConnectionTimeout => TimeSpan.FromSeconds(15);

        public static int RetryMaxAttempts => 5;
        
        public static ConnectionMultiplexer Connection { get { return _connection ?? CreateConnectionAsync().GetAwaiter().GetResult(); } }

        private static async Task InitializeAsync()
        {
            if (_didInitialize)
            {
                throw new InvalidOperationException("Cannot initialize more than once.");
            }

            var builder = new ConfigurationBuilder()
                .AddUserSecrets<Program>();

            Configuration = builder.Build();
            _connection = await CreateConnectionAsync();
            _didInitialize = true;

        }

        // This method may return null if it fails to acquire the semaphore in time.
        // Use the return value to update the "connection" field
        private static async Task<ConnectionMultiplexer> CreateConnectionAsync()
        {
            if (_connection != null)
            {
                // If we already have a good connection, let's re-use it
                return _connection;
            }

            try
            {
                await _initSemaphore.WaitAsync(RestartConnectionTimeout);
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
                string cacheConnection = Configuration["CacheConnection"].ToString();
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
        public static async Task ForceReconnectAsync()
        {
            var utcNow = DateTimeOffset.UtcNow;
            long previousTicks = Interlocked.Read(ref _lastReconnectTicks);
            var previousReconnectTime = new DateTimeOffset(previousTicks, TimeSpan.Zero);
            TimeSpan elapsedSinceLastReconnect = utcNow - previousReconnectTime;

            // If multiple threads call ForceReconnectAsync at the same time, we only want to honor one of them.
            if (elapsedSinceLastReconnect < ReconnectMinInterval)
            {
                return;
            }

            try
            {
                await _reconnectSemaphore.WaitAsync(RestartConnectionTimeout);
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

                if (elapsedSinceLastReconnect < ReconnectMinInterval)
                {
                    return; // Some other thread made it through the check and the lock, so nothing to do.
                }

                TimeSpan elapsedSinceFirstError = utcNow - _firstErrorTime;
                TimeSpan elapsedSinceMostRecentError = utcNow - _previousErrorTime;

                bool shouldReconnect =
                    elapsedSinceFirstError >= ReconnectErrorThreshold // Make sure we gave the multiplexer enough time to reconnect on its own if it could.
                    && elapsedSinceMostRecentError <= ReconnectErrorThreshold; // Make sure we aren't working on stale data (e.g. if there was a gap in errors, don't reconnect yet).

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
            }
            finally
            {
                _reconnectSemaphore.Release();
            }
        }

        // In real applications, consider using a framework such as
        // Polly to make it easier to customize the retry approach.
        private static async Task<T> BasicRetryAsync<T>(Func<IDatabase, Task<T>> func)
        {
            int reconnectRetry = 0;

            IDatabase cache = Connection.GetDatabase();
            while (true)
            {
                try
                {
                    return await func(cache);
                }
                catch (Exception ex) when (ex is RedisConnectionException || ex is SocketException)
                {
                    reconnectRetry++;
                    if (reconnectRetry > RetryMaxAttempts)
                        throw;
                    try
                    {
                        await ForceReconnectAsync();
                        cache = Connection.GetDatabase();
                    }
                    catch (ObjectDisposedException) { }
                }
            }
        }

        static async Task Main(string[] args)
        {
            await InitializeAsync();

            // Perform cache operations using the cache object...
            Console.WriteLine("Running... Press 'q' to quit.");
            ConsoleKey inputKey = ConsoleKey.A; // Init value, it may be anything different to Q

            while (inputKey != ConsoleKey.Q)
            {
                for (int i = 0; i < 3; i++)
                {
                    Task.Run(() => RunRedisCommands());
                }

                Thread.Sleep(2000);

                if (Console.KeyAvailable)
                {
                    inputKey = Console.ReadKey().Key;
                }
            }

            await CloseConnectionAsync(_connection);
        }

        private static async Task RunRedisCommands()
        {

            // Simple PING command
            string cacheCommand = "PING";
            Console.WriteLine($"\nCache command: " + cacheCommand);
            RedisResult pingResult = await BasicRetryAsync(async (db) => await db.ExecuteAsync(cacheCommand));
            Console.WriteLine($"Cache response: {pingResult}");

            // Simple get and put of integral data types into the cache
            cacheCommand = "GET Message";
            Console.WriteLine($"\nCache command: {cacheCommand} or StringGet()");
            RedisValue getMessageResult = await BasicRetryAsync(async (db) => await db.StringGetAsync("Message"));
            Console.WriteLine($"Cache response: {getMessageResult}");

            cacheCommand = "SET Message \"Hello! The cache is working from a .NET Core console app!\"";
            Console.WriteLine($"\nCache command: {cacheCommand} or StringSet()");
            bool stringSetResult = await BasicRetryAsync(async (db) => await db.StringSetAsync("Message", "Hello! The cache is working from a .NET Core console app!"));
            Console.WriteLine($"Cache response: {stringSetResult}");

            // Demonstrate "SET Message" executed as expected...
            cacheCommand = "GET Message";
            Console.WriteLine($"\nCache command: {cacheCommand} or StringGet()");
            getMessageResult = await BasicRetryAsync(async (db) => await db.StringGetAsync("Message"));
            Console.WriteLine($"Cache response: {getMessageResult}");

            // Store .NET object to cache
            Employee e007 = new Employee("007", "Davide Columbo", 100);
            stringSetResult = await BasicRetryAsync(async (db) => await db.StringSetAsync("e007", JsonConvert.SerializeObject(e007)));
            Console.WriteLine($"\nCache response from storing Employee .NET object: {stringSetResult}");


            // Retrieve .NET object from cache
            getMessageResult = await BasicRetryAsync(async (db) => await db.StringGetAsync("e007"));
            Employee e007FromCache = JsonConvert.DeserializeObject<Employee>(getMessageResult);
            Console.WriteLine($"Deserialized Employee .NET object :\n");
            Console.WriteLine("\tEmployee.Name : " + e007FromCache.Name);
            Console.WriteLine("\tEmployee.Id   : " + e007FromCache.Id);
            Console.WriteLine("\tEmployee.Age  : " + e007FromCache.Age + "\n");
        }
    }
}