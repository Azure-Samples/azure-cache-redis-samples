using System;
using StackExchange.Redis;

namespace DotNet.ClientSamples.StackExchange.Redis
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
            ForceReconnect.InitConnectionHelper();
            var key = "key";
            var value = "value";
            try
            {
                ForceReconnect.OperationExecutor(() => ForceReconnect.Connection.GetDatabase().KeyDelete(key));
                ForceReconnect.OperationExecutor(() => ForceReconnect.Connection.GetDatabase().StringSet(key, value));
                var newValue = ForceReconnect.OperationExecutor(() => ForceReconnect.Connection.GetDatabase().StringGet(key));

                Console.WriteLine("new value is {0}, expected value is {1}", newValue, value);
            }
            catch (RedisConnectionException)
            {
                ForceReconnect.DoForceReconnect();
            }
            catch (RedisTimeoutException)
            {
                // Sometimes failed to connect will throw timeout exception
                ForceReconnect.DoForceReconnect();
            }
        }

        private static void InitLogger()
        {
            LogUtility.Logger = Console.Out;
        }
    }
}
