using System;
using System.Net.Sockets;
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
            ConnectionHelper.Initialize();
            var key = "key";
            var value = "value";
            try
            {
                ConnectionHelper.Connection.GetDatabase().KeyDelete(key);
                ConnectionHelper.Connection.GetDatabase().StringSet(key, value);
                var newValue = ConnectionHelper.Connection.GetDatabase().StringGet(key);

                Console.WriteLine("new value is {0}, expected value is {1}", newValue, value);
            }
            catch (Exception ex) when (ex is RedisConnectionException || ex is SocketException)
            {
                ConnectionHelper.ForceReconnect();
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
