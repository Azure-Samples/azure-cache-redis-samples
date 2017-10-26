using StackExchange.Redis;

namespace DotNetConsoleAppUsingStackExchangeRedisClient.Commons.Pool
{
    class ConnectionValidator: IPooledObjectValidator<ReconnectionMultiplexer>
    {
        public bool ValidateOnAcquire => true;
        public bool Validate(ReconnectionMultiplexer obj) => obj.Connection.IsConnected;
    }
}
