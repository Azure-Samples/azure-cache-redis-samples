using System;
using System.Threading;
using StackExchange.Redis;

namespace DotNetConsoleAppUsingStackExchangeRedisClient
{
    /// <summary>
    /// ConnectinHelper to support force reconnect when RedisConnectionException happend. 
    /// Current retry policy is fixed time interval retry, which mean two reconnect won't happen in reconnectMinFrequency
    /// </summary> 
    public static class ConnectionHelper
    {
        private static DateTimeOffset lastReconnectTime = DateTimeOffset.MinValue;
        private static DateTimeOffset firstErrorTime = DateTimeOffset.MinValue;

        private static DateTimeOffset previousErrorTime = DateTimeOffset.MinValue;

        // In general, let StackExchange.Redis handle most reconnects, 
        // so limit the frequency of how often this will actually reconnect.
        public static TimeSpan reconnectMinFrequency;

        // if errors continue for longer than the below threshold, then the 
        // multiplexer seems to not be reconnecting, so re-create the multiplexer
        public static TimeSpan reconnectErrorThreshold;

        private static readonly object reconnectLock = new object();
        private static ConfigurationOptions configuration;

        private static Lazy<ConnectionMultiplexer> multiplexer;

        public static ConnectionMultiplexer Connection { get { return multiplexer.Value; } }


        // Call this method before get Connection
        public static void InitializeConnection(ConfigurationOptions configuration, int reconnectInterval = 10,
            int reconnectErrorThreshold = 5)
        {
            ConnectionHelper.configuration = configuration;
            ConnectionHelper.configuration.AbortOnConnectFail = false;
            ConnectionHelper.reconnectMinFrequency = TimeSpan.FromSeconds(reconnectInterval);
            ConnectionHelper.reconnectErrorThreshold = TimeSpan.FromSeconds(reconnectErrorThreshold);
            multiplexer = CreateMultiplexer();
        }

        /// <summary>
        /// Force a new ConnectionMultiplexer to be created.  
        /// NOTES: 
        ///     1. Users of the ConnectionMultiplexer MUST handle ObjectDisposedExceptions, which can now happen as a result of calling ForceReconnect()
        ///     2. Don't call ForceReconnect for Timeouts, just for RedisConnectionExceptions
        ///     3. Call this method every time you see a connection exception, the code will wait to reconnect:
        ///         a. for at least the "ReconnectErrorThreshold" time of repeated errors before actually reconnecting
        ///         b. not reconnect more frequently than configured in "ReconnectMinFrequency"
        /// </summary>   
        public static void ForceReconnect()
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
                        LogUtility.LogInfo(
                            "ForceReconnect at {0:dd\\.hh\\:mm\\:ss}, elapsedSinceFirstError: {1}, elapsedSinceMostRecentError at {2}", now, elapsedSinceFirstError.Seconds, elapsedSinceMostRecentError.Seconds);
                        firstErrorTime = DateTimeOffset.MinValue;
                        previousErrorTime = DateTimeOffset.MinValue;
                        lastReconnectTime = now;
                        CloseMultiplexer(multiplexer);
                        multiplexer = CreateMultiplexer();
                    } else
                    {
                        LogUtility.LogInfo(
                            "ForceReconnect delay due to error threshold, firstError at {0:dd\\.hh\\:mm\\:ss}, previousError at {1:dd\\.hh\\:mm\\:ss}, lastConnect at {2:dd\\.hh\\:mm\\:ss}",
                            firstErrorTime, previousErrorTime, lastReconnectTime);

                        // Put thread to sleep to avoid busy wait
                        if (elapsedSinceFirstError < reconnectErrorThreshold)
                        {
                            LogUtility.LogInfo("ForceReconnect delay due to error threshold, sleep {0} seconds",
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
                    "ForceReconnect delay due to min frequency, sleep {0} seconds, lastConnect at {1:dd\\.hh\\:mm\\:ss}",
                    (reconnectMinFrequency - elapsedSinceLastReconnect).Seconds, lastReconnectTime);
                Thread.Sleep(reconnectMinFrequency - elapsedSinceLastReconnect);
            }
        }

        private static Lazy<ConnectionMultiplexer> CreateMultiplexer()
        {
            lastReconnectTime = DateTimeOffset.UtcNow;
            return new Lazy<ConnectionMultiplexer>(() => ConnectionMultiplexer.Connect(configuration));
        }

        private static void CloseMultiplexer(Lazy<ConnectionMultiplexer> multiplexer)
        {
            if (multiplexer == null)
            {
                return;
            }

            try
            {
                LogUtility.LogInfo("ForceConnect to close old multiplexer...");
                multiplexer.Value.Close();
                LogUtility.LogInfo("ForceConnect closed old multiplexer");
            }
            catch (Exception e)
            {
                LogUtility.LogError("Exception when closing old multiplexer {0}.", e);
            }
        }
    }

}

