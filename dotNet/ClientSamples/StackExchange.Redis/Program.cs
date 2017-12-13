
namespace DotNet.ClientSamples.StackExchange.Redis
{
    class Program
    {
        public static void Main(string[] args)
        {
            int times = int.Parse(args[0]);
            Benchmark.DoTest(times);
        }
    }
}
