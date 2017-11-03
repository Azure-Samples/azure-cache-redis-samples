using System;

namespace DotNet.ClientSamples.StackExchange.Redis.Backoff
{
    public class ExponentialBackOff : BackOff
    {

        // The default initial interval value (0.5 seconds).
        public static readonly TimeSpan DEFAULT_INITIAL_INTERVAL = TimeSpan.FromSeconds(0.5);

        // The default multiplier value (1.5 which is 50% increase per back off).
        public static readonly double DEFAULT_MULTIPLIER = 1.5;

        // The default maximum elapsed time (15 minutes).
        public static readonly TimeSpan DEFAULT_MAX_INTERVAL = TimeSpan.FromMinutes(15);

        // he default maximum elapsed time (15 minutes).
        public static readonly TimeSpan DEFAULT_MAX_ELAPSED_TIME = TimeSpan.FromMinutes(15);

        public TimeSpan CurrentInterval { get; private set; } = DEFAULT_INITIAL_INTERVAL;
        public TimeSpan InitialInterval { get; } = DEFAULT_INITIAL_INTERVAL;
        private double Multiplier { get; } = DEFAULT_MULTIPLIER;
        public TimeSpan MaxInterval { get; } = DEFAULT_MAX_INTERVAL;
        public TimeSpan MaxElapsedTime { get; } = DEFAULT_MAX_ELAPSED_TIME;

        public TimeSpan ElapsedTime { get; set; } = TimeSpan.Zero;

        public ExponentialBackOff()
        {
        }

        public ExponentialBackOff(TimeSpan initialInterval, double multiplier)
        {
            CheckMultiplier(multiplier);
            this.InitialInterval = initialInterval;
            this.Multiplier = multiplier;
        }

        private void CheckMultiplier(double multiplier)
        {
            if (multiplier < 1)
            {
                throw new ArgumentException("Multiplier must be greater than 1.");
            }
        }

        public TimeSpan NextBackOff()
        {
            if (ElapsedTime > MaxElapsedTime)
            {
                throw new Exception("ElapsedTime has exceeded MaxElapsedTime");
            }

            IncrementCurrentInterval();
            return CurrentInterval;
        }

        public void Reset()
        {
            CurrentInterval = InitialInterval;
            ElapsedTime = TimeSpan.Zero;
        }

        private void IncrementCurrentInterval()
        {
            if (CurrentInterval.TotalSeconds >= MaxInterval.TotalSeconds / Multiplier)
            {
                CurrentInterval = MaxInterval;
            }
            else
            {
                CurrentInterval = TimeSpan.FromTicks((long)(CurrentInterval.Ticks * Multiplier));
            }
        }
    }
}
