using System;
using Commons.Pool;
using StackExchange.Redis;

namespace DotNetConsoleAppUsingStackExchangeRedisClient
{

    /// <summary>
    /// A Reids connection factory. 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class ConnectionFactory<T> : IPooledObjectFactory<T> where T: IConnectionMultiplexer
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

        public T Create()
        {
            return (T)Convert.ChangeType(ConnectionMultiplexer.Connect(ConfigurationOptions), typeof(T));
        }
    }
}

