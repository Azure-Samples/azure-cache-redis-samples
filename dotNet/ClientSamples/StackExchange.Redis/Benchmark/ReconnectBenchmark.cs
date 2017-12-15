using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Threading;
using CommandLine;
using StackExchange.Redis;

namespace DotNet.ClientSamples.StackExchange.Redis.Benchmark
{
    class ReconnectBenchmark
    {
        private static readonly Random random = new Random();
        private static readonly List<Interval> reconnectIntervals = new List<Interval>();
        private static volatile bool isConnected = true;
        private static volatile Interval interval = new Interval();
        private static BenchmarkOptions options;

        public static void Test(string[] args)
        {
            Initialize(args);
            while (reconnectIntervals.Count < options.NumberTests)
            {
                SimulateWorkload();
            }

            LogUtility.LogInfo("Test finished.");
            PrintResult();
        }

        private static void ParseOptions(string[] args)
        {
            var options = new BenchmarkOptions();
            if (Parser.Default.ParseArgumentsStrict(args, options, () =>
            {
                Console.WriteLine(options.GetUsage());
                Environment.Exit(-2);
            }
            ))
            {
                ReconnectBenchmark.options = options;
                LogUtility.LogInfo("Start testing ...");
            }
            
        }

        // 70% read, 30% write
        private static void SimulateWorkload()
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

                CheckUnconnected();
                SleepIfNecessary();
            }
            catch (Exception ex) when (ex is RedisConnectionException || ex is SocketException)
            {
                CheckConnected();
                ConnectionHelper.ForceReconnect();
            }
            catch (ObjectDisposedException)
            {
                LogUtility.LogInfo("Retry later since reconnection is in progress");
            }
            catch (RedisTimeoutException)
            {
                LogUtility.LogDebug("Timeout when performing an operation");
            }
            catch (Exception e)
            {
                LogUtility.LogError("Exception thrown", e);
            }

        }

        // Sleep to make sure not exceed max operations per second
        private static void SleepIfNecessary()
        {
            if (options.MaxOperationsPerSecond >= 0)
            {
                Thread.Sleep(TimeSpan.FromMilliseconds(1000.0 / options.MaxOperationsPerSecond));
            }
        }

        private static void CheckUnconnected()
        {
            if (!isConnected)
            {
                interval.End();
                reconnectIntervals.Add(interval);
                isConnected = true;

                if (options.Verbose)
                {
                    LogUtility.LogInfo("Connected.");
                    PrintResult();
                }
            }
        }

        private static void CheckConnected()
        {
            if (isConnected)
            {
                interval = new Interval();
                interval.Start();
                isConnected = false;

                if (options.Verbose)
                {
                    LogUtility.LogInfo("Disconnected.");
                }
            }
        }

        private static void PrintResult()
        {
            List<TimeSpan> timeSpans = reconnectIntervals.Select(i => i.GetTimeSpan()).ToList();
            timeSpans.Sort();

            LogUtility.LogInfo($"{reconnectIntervals.Count} tests ran");
            LogUtility.LogInfo("Connect intervals are " + string.Join(", ", reconnectIntervals));
            LogUtility.LogInfo("Min reconnect time in seconds: " + timeSpans.First().TotalSeconds);
            LogUtility.LogInfo("Max reconnect time in seconds: " + timeSpans.Last().TotalSeconds);
            LogUtility.LogInfo("Avg reconnect time in seconds: " + timeSpans.Average(t => t.TotalSeconds));

            int[] percentiles = {50, 90, 95, 99};

            foreach (var percentile in percentiles)
            {
                LogUtility.LogInfo(
                    $"{percentile} % <= reconnect time in seconds: {timeSpans[(timeSpans.Count + 1) * percentile / 100 - 1]} ");
            }
        }

        // TODO: return random generated string
        private static string GetRandomString()
        {
            return "foo";
        }

        private static void Initialize(string[] args)
        {
            LogUtility.Logger = Console.Out;
            LogUtility.Level = LogUtility.LogLevel.Info;
            ParseOptions(args);
            ConnectionHelper.Initialize();
        }
    }
}
