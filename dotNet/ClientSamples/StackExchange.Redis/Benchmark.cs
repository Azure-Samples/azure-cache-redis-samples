using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Threading;
using StackExchange.Redis;

namespace DotNet.ClientSamples.StackExchange.Redis
{
    class Benchmark
    {
        class Interval
        {
            public DateTime StartTime { get; private set; }
            public DateTime EndTime { get; private set; }

            public void Start()
            {
                StartTime = DateTime.Now; 
            }

            public void End()
            {
                EndTime = DateTime.Now;
            }
        }

        private static Random random = new Random();
        private static int times = 100;
        private static List<Interval> reconnectIntervals = new List<Interval>();
        private static volatile bool isConnected = true; 

        public static void DoTest()
        {
            while (reconnectIntervals.Count < times)
            {
                SimulateWorkLoad();
            }

            PrintResult();
        }

        // 70% read, 30% write
        private static void SimulateWorkLoad()
        {
            try
            {
                if (random.Next() % 10 <= 3)
                {
                    ConnectionHelper.Connection.GetDatabase().StringSet(GetRandomString(), GetRandomString());
                }
                else
                {
                    ConnectionHelper.Connection.GetDatabase().StringGet(GetRandomString());
                }
                Thread.Sleep(TimeSpan.FromMilliseconds(2));
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

        private static void PrintResult()
        {
            SortedSet<TimeSpan> timeSpans = new SortedSet<TimeSpan>(reconnectIntervals.Select(interval => interval.EndTime - interval.StartTime).ToList());

            Console.WriteLine("Min reconnect time: " + timeSpans.Min);
            Console.WriteLine("Max reconnect time: " + timeSpans.Max);
            Console.WriteLine("Avg reconnect time: " + timeSpans.Average());
        }

        private static string GetRandomString()
        {
            return "";
        }

        private static void Initialize()
        {
            LogUtility.Logger = Console.Out;
            ConnectionHelper.Initialize();
        }
    }
}
