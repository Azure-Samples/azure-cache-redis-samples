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

            public TimeSpan GeTimeSpan()
            {
                return EndTime - StartTime;
            }

            public override string ToString()
            {
                return $"[{StartTime}, {EndTime}, {GeTimeSpan()}]";
            }
        }

        private static Random random = new Random();
        private static List<Interval> reconnectIntervals = new List<Interval>();
        private static bool isConnected = true;
        private static Interval interval = new Interval(); 

        public static void DoTest(int times)
        {
            Initialize();
            while (reconnectIntervals.Count < times)
            {
                SimulateWorkLoad();
            }

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
                    PrintResult();
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
            List<TimeSpan> timeSpans = reconnectIntervals.Select(i => i.GeTimeSpan()).ToList();
            timeSpans.Sort();
            int sizePlusOne = timeSpans.Count + 1;

            LogUtility.LogInfo("Connect intervals are " + string.Join(", ", reconnectIntervals));
            LogUtility.LogInfo("50 % <= reconnect time in seconds: " + timeSpans[sizePlusOne / 2 - 1]);
            LogUtility.LogInfo("90 % <= reconnect time in seconds: " + timeSpans[sizePlusOne * 90 / 100 - 1]);
            LogUtility.LogInfo("95 % <= reconnect time in seconds: " + timeSpans[sizePlusOne * 95 / 100 - 1]);
            LogUtility.LogInfo("99 % <= reconnect time in seconds: " + timeSpans[sizePlusOne * 99 / 100 - 1]);
            LogUtility.LogInfo("99.9 % <= reconnect time in seconds: " + timeSpans[sizePlusOne * 999 / 1000 - 1]);
            LogUtility.LogInfo("Min reconnect time in seconds: " + timeSpans.First().TotalSeconds);
            LogUtility.LogInfo("Max reconnect time in seconds: " + timeSpans.Last().TotalSeconds);
            LogUtility.LogInfo("Avg reconnect time in seconds: " + timeSpans.Average(t => t.TotalSeconds));
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
