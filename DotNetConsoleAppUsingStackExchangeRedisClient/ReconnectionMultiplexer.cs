using System;
using System.Threading;
using DotNetConsoleAppUsingStackExchangeRedisClient;
using StackExchange.Redis;

namespace HelloWorld
{
    /// <summary>
    /// Wrapper of ConnectionMultiplexer support force reconnect when RedisConnectionException happend. 
    /// Current retry policy is fixed time interval retry, which mean two reconnect won't happen in reconnectMinFrequency
    /// </summary> 
    class ReconnectionMultiplexer
    {
        private ConnectionMultiplexer connectionMultiplexer;
        private DateTimeOffset lastReconnectTime = DateTimeOffset.MinValue;
        private DateTimeOffset firstErrorTime = DateTimeOffset.MinValue;

        private DateTimeOffset previousErrorTime = DateTimeOffset.MinValue;

        // In general, let StackExchange.Redis handle most reconnects, 
        // so limit the frequency of how often this will actually reconnect.
        public readonly TimeSpan reconnectMinFrequency;

        // if errors continue for longer than the below threshold, then the 
        // multiplexer seems to not be reconnecting, so re-create the multiplexer
        public readonly TimeSpan reconnectErrorThreshold;

        private readonly object reconnectLock = new object();
        private readonly ConfigurationOptions configuration;

        public ReconnectionMultiplexer(ConfigurationOptions configuration, int reconnectInterval = 10,
            int reconnectErrorThreshold = 5)
        {
            this.configuration = configuration;
            this.configuration.AbortOnConnectFail = false;
            this.reconnectMinFrequency = TimeSpan.FromSeconds(reconnectInterval);
            this.reconnectErrorThreshold = TimeSpan.FromSeconds(reconnectErrorThreshold);
            CreateMultiplexer();
        }

        public ConnectionMultiplexer GetMultiplexer()
        { 
            return connectionMultiplexer;
        }

        public void ForceReconnect()
        {
            var previousReconnect = lastReconnectTime;
            var elapsedSinceLastReconnect = DateTimeOffset.UtcNow - previousReconnect;

            // If mulitple threads call ForceReconnect at the same time, we only want to honor one of them.
            if (elapsedSinceLastReconnect > reconnectMinFrequency)
            {
                lock (reconnectLock)
                {
                    var now = DateTimeOffset.UtcNow;
                    elapsedSinceLastReconnect = now - lastReconnectTime;

                    // Some other thread made it through the check and the lock, so wait to next connect time.
                    if (elapsedSinceLastReconnect < reconnectMinFrequency)
                    {
                        return;
                    }

                    if (firstErrorTime == DateTimeOffset.MinValue)
                    {
                        // We haven't seen an error since last reconnect, so set initial values.
                        firstErrorTime = now;
                        previousErrorTime = now;
                        return;
                    }

                    var elapsedSinceFirstError = now - firstErrorTime;
                    var elapsedSinceMostRecentError = now - previousErrorTime;
                    previousErrorTime = now;

                    var shouldReconnect =
                        elapsedSinceFirstError >=
                        reconnectErrorThreshold // make sure we gave the multiplexer enough time to reconnect on its own if it can
                        && elapsedSinceMostRecentError <=
                        reconnectErrorThreshold; //make sure we aren't working on stale data (e.g. if there was a gap in errors, don't reconnect yet).                    

                    if (shouldReconnect)
                    {
                        LogUtility.LogInfo($"ForceReconnect: now: {now.ToString()}");
                        LogUtility.LogInfo($"ForceReconnect: elapsedSinceLastReconnect: {elapsedSinceLastReconnect.ToString()}, ReconnectFrequency: {reconnectMinFrequency.ToString()}");
                        LogUtility.LogInfo($"ForceReconnect: elapsedSinceFirstError: {elapsedSinceFirstError.ToString()}, elapsedSinceMostRecentError: {elapsedSinceMostRecentError.ToString()}, ReconnectErrorThreshold: {reconnectErrorThreshold.ToString()}");
                        firstErrorTime = DateTimeOffset.MinValue;
                        previousErrorTime = DateTimeOffset.MinValue;
                        lastReconnectTime = now;
                        CloseMultiplexer(connectionMultiplexer);
                        CreateMultiplexer();
                    } else
                    {
                        
                        LogUtility.LogInfo(
                            "Reconnect delay due to error threshold, firstError at {0:dd\\.hh\\:mm\\:ss}, previousError at {1:dd\\.hh\\:mm\\:ss}, lastConnect at {2:dd\\.hh\\:mm\\:ss}",
                            firstErrorTime, previousErrorTime, lastReconnectTime);

                        // Put thread to sleep to avoid busy wait
                        if (elapsedSinceFirstError < reconnectErrorThreshold)
                        {
                            LogUtility.LogInfo("Reconnect delay due to error threshold, sleep {0} seconds",
                                (reconnectErrorThreshold - elapsedSinceFirstError).Seconds);
                            Thread.Sleep(reconnectErrorThreshold - elapsedSinceFirstError);
                        }
                        else
                        {
                            Thread.Sleep(TimeSpan.FromSeconds(1));
                        }

                    }
                }
            }
            else
            {

                // Put thread to sleep to avoid busy wait
                LogUtility.LogInfo(
                    "Reconnect delay due to min frequency, sleep {0} seconds, lastConnect at {1:dd\\.hh\\:mm\\:ss}",
                    (reconnectMinFrequency - elapsedSinceLastReconnect).Seconds, lastReconnectTime);
                Thread.Sleep(reconnectMinFrequency - elapsedSinceLastReconnect);
            }
        }

        private void CreateMultiplexer()
        {
            connectionMultiplexer = ConnectionMultiplexer.Connect(configuration);
            lastReconnectTime = DateTimeOffset.UtcNow;
        }

        private void CloseMultiplexer(ConnectionMultiplexer multiplexer)
        {

            try
            {
                LogUtility.LogInfo("closing old multiplexer.");
                multiplexer.Close();
            }
            catch (Exception)
            {
                // Example error condition: if accessing old.Value causes a connection attempt and that fails.
                // TODO: log exception
            }
        }
    }

}

