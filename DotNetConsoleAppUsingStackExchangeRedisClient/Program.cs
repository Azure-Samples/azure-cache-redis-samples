using System;
using System.Configuration;
using System.Threading;
using StackExchange.Redis;

namespace DotNetConsoleAppUsingStackExchangeRedisClient
{

    public static class Program
    {
        public static void Main(string[] args)
        {
            InitLogger();
            InitConnectionHelper();
            var key = "key";
            var value = "value";
            OperationExecutor(() => Connection.GetDatabase().KeyDelete(key));
            OperationExecutor(() => Connection.GetDatabase().StringSet(key, value));
            var newValue = OperationExecutor(() => Connection.GetDatabase().StringGet(key));

            Console.WriteLine("new value is {0}, expected value is {1}", newValue, value);
        }

        // OperationExecutor will retry if RedisConnectionException happens 
        // After retryTimes, exception will be thrown out
        private static object OperationExecutor(Func<object> redisOperation, int retryTimes = 10)
        {
            while (retryTimes > 0)
            {
                try
                {
                    return redisOperation.Invoke();
                }
                catch (ObjectDisposedException)
                {
                    // Retry later as this can be caused by force reconnect by closing multiplexer
                    LogUtility.LogInfo("object disposing exception at {0:dd\\.hh\\:mm\\:ss}",
                        DateTimeOffset.UtcNow);
                    Thread.Sleep(TimeSpan.FromSeconds(1));
                }
                catch (RedisConnectionException)
                {
                    retryTimes--;
                    // Try once after reconnect
                    LogUtility.LogInfo("Force reconnect at {0:dd\\.hh\\:mm\\:ss}",
                        DateTimeOffset.UtcNow);
                    ConnectionHelper.ForceReconnect();
                    if (retryTimes == 0)
                    {
                        throw;
                    }
                }
                catch (Exception e)
                {
                    LogUtility.LogError("Exception {0} thrown when exccuting {1}", e, redisOperation);
                    throw;
                }
            }

            return redisOperation.Invoke();
        }

        private static void InitConnectionHelper()
        {
            ConfigurationOptions config = new ConfigurationOptions();
            config.EndPoints.Add(ConfigurationManager.AppSettings["RedisCacheHostName"]);
            config.Password = ConfigurationManager.AppSettings["RedisCachePassword"];
            config.Ssl = bool.Parse(ConfigurationManager.AppSettings["enableSsl"]);
            config.AbortOnConnectFail = false;
            config.ConnectRetry = int.Parse(ConfigurationManager.AppSettings["RedisConnectRetry"]);
            config.ConnectTimeout = int.Parse(ConfigurationManager.AppSettings["RedisConnectTimeoutInMilliseconds"]);

            ConnectionHelper.InitializeConnection(config);
        }

        private static void InitLogger()
        {
            LogUtility.Logger = Console.Out;
        }

        private static ConnectionMultiplexer Connection => ConnectionHelper.Connection;

    }
}

