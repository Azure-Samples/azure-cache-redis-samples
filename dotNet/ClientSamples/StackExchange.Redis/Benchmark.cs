using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
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
        private static List<Interval> reconnectIntervals = new List<Interval>();
        private static bool isConnected = false;
        private static Interval interval = new Interval(); 

        public static void DoTest(int times)
        {
            Initialize();
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
                    String value = ConnectionHelper.Connection.GetDatabase().StringGet(GetRandomString());
                    LogUtility.LogDebug("Current value is " + value);
                }

                if (!isConnected)
                {
                    interval.End();
                    reconnectIntervals.Add(interval);
                    isConnected = true;
                    LogUtility.LogInfo("Connected.");
                }

                Thread.Sleep(TimeSpan.FromMilliseconds(2));
            }
            catch (Exception ex) when (ex is RedisConnectionException || ex is SocketException || ex is RedisTimeoutException)
            {
                if (isConnected)
                {
                    interval = new Interval();
                    interval.Start();
                    isConnected = false;
                    LogUtility.LogInfo("Disconnected.");
                }

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

            Console.WriteLine("Min reconnect time in seconds: " + timeSpans.Min.Seconds);
            Console.WriteLine("Max reconnect time in seconds: " + timeSpans.Max.Seconds);
            Console.WriteLine("Avg reconnect time in seconds: " + timeSpans.Average(t => t.Seconds));
        }

        // TODO: return random generated string
        private static string GetRandomString()
        {
            return "foo";
        }

        private static void Initialize()
        {
            LogUtility.Logger = Console.Out;
            LogUtility.Level = LogUtility.LogLevel.Info;
            ConnectionHelper.Initialize();
        }
    }
}
