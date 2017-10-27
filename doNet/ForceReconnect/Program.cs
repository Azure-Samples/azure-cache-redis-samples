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
            OperationExecutor(() => ConnectionHelper.Connection.GetDatabase().KeyDelete(key));
            OperationExecutor(() => ConnectionHelper.Connection.GetDatabase().StringSet(key, value));
            var newValue = OperationExecutor(() => ConnectionHelper.Connection.GetDatabase().StringGet(key));

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
                    retryTimes--;
                }
                catch (Exception e)
                {
                    LogUtility.LogError("Exception {0} thrown when executing {1}", e, redisOperation);
                    throw;
                }
            }

            return redisOperation.Invoke();
        }

        private static void InitConnectionHelper()
        {
            var hostName = ConfigurationManager.AppSettings["RedisCacheHostName"];
            var password = ConfigurationManager.AppSettings["RedisCachePassword"];
            var enableSsl = bool.Parse(ConfigurationManager.AppSettings["enableSsl"]);
            var connectRetry = int.Parse(ConfigurationManager.AppSettings["RedisConnectRetry"]);
            var connectTimeoutInMilliseconds = int.Parse(ConfigurationManager.AppSettings["RedisConnectTimeoutInMilliseconds"]);

            ConnectionHelper.InitializeConnection(hostName, password, connectRetry, connectTimeoutInMilliseconds, enableSsl);
        }

        private static void InitLogger()
        {
            LogUtility.Logger = Console.Out;
        }

    }
}

