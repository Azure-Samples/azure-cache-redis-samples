using System;

namespace DotNet.ClientSamples.StackExchange.Redis.Benchmark
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

        public TimeSpan GetTimeSpan()
        {
            return EndTime - StartTime;
        }

        public override string ToString()
        {
            return $"[{StartTime}, {EndTime}, {GetTimeSpan()}]";
        }
    }
}
