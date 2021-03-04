using System;
using System.Configuration;
using StackExchange.Redis;

namespace DotNet.ClientSamples.StackExchange.Redis
{
    /// <summary>
    /// ConnectionHelper supports reconnections on RedisConnectionException.
    /// Current retry policy is fixed time interval retry, which mean two reconnect won't happen in reconnectMinFrequency
    /// </summary> 
    public static class ConnectionHelper
    {
        private static DateTimeOffset lastReconnectTime = DateTimeOffset.MinValue;
        private static DateTimeOffset firstErrorTime = DateTimeOffset.MinValue;

        private static DateTimeOffset previousErrorTime = DateTimeOffset.MinValue;

        // In general, let StackExchange.Redis handle most reconnects, 
        // so limit the frequency of how often this will actually reconnect.
        public static TimeSpan reconnectMinFrequency = TimeSpan.FromSeconds(60);

        // if errors continue for longer than the below threshold, then the 
        // multiplexer seems to not be reconnecting, so re-create the multiplexer
        public static TimeSpan reconnectErrorThreshold = TimeSpan.FromSeconds(30);

        private static readonly object reconnectLock = new object();
        private static ConfigurationOptions configuration;

        private static Lazy<ConnectionMultiplexer> multiplexer;
        private static bool initialized = false;


        /// <exception cref="ObjectDisposedException">when force reconnecting.</exception>
        public static ConnectionMultiplexer Connection
        {
            get
            {
                EnsureInitialized();
                return multiplexer.Value;
            }
        }

        public static void Initialize()
        {
            var hostName = ConfigurationManager.AppSettings["RedisCacheHostName"];
            var password = ConfigurationManager.AppSettings["RedisCachePassword"];
            if (string.IsNullOrEmpty(hostName) || string.IsNullOrEmpty(password))
            {
                throw new ArgumentException("Please provide cacheName and password");
            }
            var enableSsl = bool.Parse(ConfigurationManager.AppSettings["useSsl"]);
            var connectRetry = int.Parse(ConfigurationManager.AppSettings["RedisConnectRetry"]);
            var connectTimeoutInMilliseconds = int.Parse(ConfigurationManager.AppSettings["RedisConnectTimeoutInMilliseconds"]);

            Initialize(hostName, password, connectRetry, connectTimeoutInMilliseconds, enableSsl);
        }

        public static void Initialize(string hostName, string password, int connectRetry,
            int connectTimeoutInMilliseconds, bool useSsl)
        {
            ConfigurationOptions config = new ConfigurationOptions();
            config.EndPoints.Add(hostName);
            config.Password = password;
            config.Ssl = useSsl;
            config.AbortOnConnectFail = false;
            config.ConnectRetry = connectRetry;
            config.ConnectTimeout = connectTimeoutInMilliseconds;
            configuration = config;
            initialized = true;
            multiplexer = CreateMultiplexer();
        }

        /// <summary>
        /// Force a new ConnectionMultiplexer to be created.  
        /// NOTES: 
        ///     1. Users of the ConnectionMultiplexer MUST handle ObjectDisposedExceptions, which can now happen when get Connection property
        ///     2. Don't call ForceReconnect for Timeouts, just for RedisConnectionExceptions
        ///     3. Call this method every time you see a connection exception, the code will wait to reconnect:
        ///         a. for at least the "ReconnectErrorThreshold" time of repeated errors before actually reconnecting
        ///         b. not reconnect more frequently than configured in "ReconnectMinFrequency"
        /// </summary>
        public static void ForceReconnect()
        {
            EnsureInitialized();
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
                            "ForceReconnect at {0:hh\\:mm\\:ss}, firstError at {1:hh\\:mm\\:ss}, previousError at {2:hh\\:mm\\:ss}, lastConnect at {3:hh\\:mm\\:ss}",
                            now, firstErrorTime, previousErrorTime, lastReconnectTime);
                        firstErrorTime = DateTimeOffset.MinValue;
                        previousErrorTime = DateTimeOffset.MinValue;
                        lastReconnectTime = now;
                        CloseMultiplexer(multiplexer);
                        multiplexer = CreateMultiplexer();
                    }
                    else
                    {
                        LogUtility.LogInfo(
                            "ForceReconnect delay due to error threshold {0}s, firstError at {1:hh\\:mm\\:ss}, previousError at {2:hh\\:mm\\:ss}, lastConnect at {3:hh\\:mm\\:ss}",
                            reconnectErrorThreshold.TotalSeconds, firstErrorTime, previousErrorTime, lastReconnectTime);
                    }
                }
            }
            else
            {
                LogUtility.LogInfo(
                    "ForceReconnect delay due to current min frequency: {0}s, lastConnect at {1:hh\\:mm\\:ss}", reconnectMinFrequency.TotalSeconds, lastReconnectTime);
            }
        }

        private static void EnsureInitialized()
        {
            if (!initialized)
            {
                throw new Exception("Please Call Initialize() before get Connection.");
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
                LogUtility.LogInfo("ForceReconnect to close old multiplexer...");
                multiplexer.Value.Close();
                LogUtility.LogInfo("ForceReconnect closed old multiplexer");
            }
            catch (Exception e)
            {
                LogUtility.LogError("Exception when closing old multiplexer {0}.", e);
            }
        }
    }

}

