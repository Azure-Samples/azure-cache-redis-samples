using System;

namespace DotNet.ClientSamples.StackExchange.Redis.Backoff
{
    // Back-off policy when retrying an operation.
    interface BackOff
    {

        /// Gets the time span to wait before retrying the operation or throw exception to
        /// indicate that no retries should be made.
        TimeSpan NextBackOff();

        /// Reset to initial state
        void Reset();

    }

}
