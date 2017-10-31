using System;
using System.Configuration;
using StackExchange.Redis;

namespace Samples
{
    class Program
    {

        public static void Main(string[] args)
        {
            SampleForForceReconnect();
        }

        private static void SampleForForceReconnect()
        {
            InitLogger();
            InitConnectionHelper();
            var key = "key";
            var value = "value";
            try
            {
                OperationExecutor(() => ForceReconnect.Connection.GetDatabase().KeyDelete(key));
                OperationExecutor(() => ForceReconnect.Connection.GetDatabase().StringSet(key, value));
                var newValue = OperationExecutor(() => ForceReconnect.Connection.GetDatabase().StringGet(key));

                Console.WriteLine("new value is {0}, expected value is {1}", newValue, value);
            }
            catch (RedisConnectionException)
            {
                ForceReconnect.DoForceReconnect();
            }
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
            if (string.IsNullOrEmpty(hostName) || string.IsNullOrEmpty(password))
            {
                throw new ArgumentException("Please provide cacheName and password");
            }
            var enableSsl = bool.Parse(ConfigurationManager.AppSettings["useSsl"]);
            var connectRetry = int.Parse(ConfigurationManager.AppSettings["RedisConnectRetry"]);
            var connectTimeoutInMilliseconds = int.Parse(ConfigurationManager.AppSettings["RedisConnectTimeoutInMilliseconds"]);

            ForceReconnect.InitializeConnection(hostName, password, connectRetry, connectTimeoutInMilliseconds, enableSsl);
        }

        private static void InitLogger()
        {
            LogUtility.Logger = Console.Out;
        }
    }
}
