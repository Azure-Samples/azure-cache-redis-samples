using System.Text;
using CommandLine;

namespace DotNet.ClientSamples.StackExchange.Redis.Benchmark
{
    class BenchmarkOptions
    {
        [Option('n', "number", DefaultValue = 10, HelpText = "Total number of tests. (default 10)")]
        public int NumberTests { get; set; }

        [Option('o', "ops", DefaultValue = -1, HelpText = "Max operations per second. (default -1 means no limit)")]
        public int MaxOperationsPerSecond { get; set; }

        [Option('v', "verbose", DefaultValue = false, HelpText = "Print result after each test. (default false)")]
        public bool Verbose { get; set; }

        // Unsuppoted
        [Option('c', "connections", DefaultValue = 1, HelpText = "Number of parallel connections (default 1)")]
        public int Connections { get; set; }

        // Unsuppoted
        [Option('t', "threads", DefaultValue = 1, HelpText = "Number of threads (default 1)")]
        public int Threads { get; set; }

        [HelpOption]
        public string GetUsage()
        {
            var usage = new StringBuilder();
            usage.AppendLine("Reconnect time benchmark");
            usage.AppendLine("Usage: -n --number number of tests");
            usage.AppendLine("       -o --ops max operations per second");
            usage.AppendLine("       -v --verbose print result after each test.s");
            return usage.ToString();
        }
    }
}
