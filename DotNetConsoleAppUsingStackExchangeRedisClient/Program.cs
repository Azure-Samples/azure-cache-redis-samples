using System;
using System.Configuration;
using System.Threading;
using Commons.Pool;
using StackExchange.Redis;

namespace DotNetConsoleAppUsingStackExchangeRedisClient
{

    public static class Program
    {
        public static void Main(string[] args)
        {
            //ForceConnectSample();
            ConnectionPoolSample();
        }

        private static void ForceConnectSample()
        {
            InitConnectionHelper();

            ExecuteRedisOperation(() => Connection);
        }

        private static void ConnectionPoolSample()
        {

            var poolManager = new PoolManager();

            // Create a new pool.
            var connectionFactory = new ConnectionFactory<ConnectionMultiplexer>(buildConfigurationOptions());
            var connectionPool = poolManager.NewPool<ConnectionMultiplexer>()
                .InitialSize(0)
                .MaxSize(10)
                .WithFactory(connectionFactory)
                .Instance();

            var connection = connectionPool.Acquire();

            ExecuteRedisOperation(() => connection);

            connectionPool.Return(connection);

            // When pool manager is disposed, the pool is disposed too.
            poolManager.Dispose();
        }

        private static void ExecuteRedisOperation(Func<ConnectionMultiplexer> connection)
        {
            var key = "key";
            var value = "value";
            OperationExecutor(() => connection.Invoke().GetDatabase().KeyDelete(key));
            OperationExecutor(() => connection.Invoke().GetDatabase().StringSet(key, value));
            var newValue = OperationExecutor(() => connection.Invoke().GetDatabase().StringGet(key));

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
                    LogUtility.LogInfo("Try to ForceReconnect at {0:dd\\.hh\\:mm\\:ss}",
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
            ConnectionHelper.InitializeConnection(buildConfigurationOptions());
        }

        private static ConfigurationOptions buildConfigurationOptions()
        {
            ConfigurationOptions config = new ConfigurationOptions();
            config.EndPoints.Add(ConfigurationManager.AppSettings["RedisCacheName"]);
            config.Password = ConfigurationManager.AppSettings["RedisCachePassword"];
            config.Ssl = bool.Parse(ConfigurationManager.AppSettings["enableSsl"]);
            config.AbortOnConnectFail = false;
            config.ConnectRetry = int.Parse(ConfigurationManager.AppSettings["RedisConnectRetry"]);
            config.ConnectTimeout = int.Parse(ConfigurationManager.AppSettings["RedisConnectTimeout"]);
            return config;
        }

        private static ConnectionMultiplexer Connection => ConnectionHelper.Connection;

    }
}

