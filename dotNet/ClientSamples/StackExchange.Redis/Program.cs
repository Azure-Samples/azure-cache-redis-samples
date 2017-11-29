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
                ForceReconnect.Connection.GetDatabase().KeyDelete(key);
                ForceReconnect.Connection.GetDatabase().StringSet(key, value);
                var newValue = ForceReconnect.Connection.GetDatabase().StringGet(key);

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
            catch (ObjectDisposedException)
            {
                LogUtility.LogInfo("Retry later since reconnection is in progress");
            }
        }

        private static void InitLogger()
        {
            LogUtility.Logger = Console.Out;
        }
    }
}
