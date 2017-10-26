using System;
using StackExchange.Redis;

namespace DotNetConsoleAppUsingStackExchangeRedisClient.Commons.Pool
{

    /// <summary>
    /// A Reids connection factory. 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class ConnectionFactory<T> : IPooledObjectFactory<T> where T: ReconnectionMultiplexer
    {
        public ConfigurationOptions ConfigurationOptions { get; }

        public ConnectionFactory(ConfigurationOptions config)
        {
            ConfigurationOptions = config;
        }

        public void Destroy(T obj)
        {
            obj.Close();
        }

        public void Heal(T obj)
        {
            obj.ForceReconnect();
        }

        public T Create()
        {
            return (T)Convert.ChangeType(new ReconnectionMultiplexer(ConfigurationOptions), typeof(T));
        }
    }
}

