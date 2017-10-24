using System;
using System.Threading;
using StackExchange.Redis;

namespace DotNetConsoleAppUsingStackExchangeRedisClient
{
    /// <summary>
    /// ConnectionHelper supports connections and reconnections on RedisConnectionException exceptions.  
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

        // Call InitializeConnection before get Connection
        public static void InitializeConnection(ConfigurationOptions configuration, int reconnectMinFrequencyInSeconds = 10,
            int reconnectErrorThresholdInSeconds = 5)
        {
            ConnectionHelper.configuration = configuration;
            ConnectionHelper.configuration.AbortOnConnectFail = false;
            ConnectionHelper.reconnectMinFrequency = TimeSpan.FromSeconds(reconnectMinFrequencyInSeconds);
            ConnectionHelper.reconnectErrorThreshold = TimeSpan.FromSeconds(reconnectErrorThresholdInSeconds);
            multiplexer = CreateMultiplexer();
        }

        public static void InitializeConnection(String connectionString, int reconnectMinFrequencyInSeconds = 10,
            int reconnectErrorThresholdInSeconds = 5)
        {
            InitializeConnection(ConfigurationOptions.Parse(connectionString), reconnectMinFrequencyInSeconds, reconnectErrorThresholdInSeconds);
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

                    if (firstErrorTime == DateTimeOffset.MinValue)
                    {
                        // We haven't seen an error since last reconnect, so set initial values.
                        firstErrorTime = now;
                        previousErrorTime = now;
                        return;
                    }

                    // Some other thread made it through the check and the lock, so wait to next connect time.
                    if (elapsedSinceLastReconnect < reconnectMinFrequency)
                    {
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
                    }
                }
            }
            else
            {
                LogUtility.LogInfo(
                    "ForceReconnect delay due to min frequency, lastConnect at {1:dd\\.hh\\:mm\\:ss}",
                    (reconnectMinFrequency - elapsedSinceLastReconnect).Seconds, lastReconnectTime);
            }
        }

        private static Lazy<ConnectionMultiplexer> CreateMultiplexer()
        {
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

