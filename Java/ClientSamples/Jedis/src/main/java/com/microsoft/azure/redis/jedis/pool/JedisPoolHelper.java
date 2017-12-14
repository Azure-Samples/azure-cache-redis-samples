package com.microsoft.azure.redis.jedis.pool;

import org.apache.commons.pool2.impl.GenericObjectPoolConfig;
import redis.clients.jedis.JedisPool;

public class JedisPoolHelper {
    private static JedisPool jedisPool;
    private static JedisPoolFactory jedisPoolFactory;
    private static JedisPoolConfiguration jedisClientConfiguration;

    static {
        jedisClientConfiguration = JedisPoolConfiguration.builder().build();
        jedisPoolFactory = new JedisPoolFactory(jedisClientConfiguration);
        jedisPool = jedisPoolFactory.createJedisPool();
    }

    public static JedisPool getPool() {
        return jedisPool;
    }

    public static String getPoolStatistics() {
        int active = jedisPool.getNumActive();
        int idle = jedisPool.getNumIdle();
        int total = active + idle;
        GenericObjectPoolConfig poolConfig = jedisClientConfiguration.getPoolConfig();
        return String.format(
                "JedisPool: Active=%d, Idle=%d, Waiters=%d, total=%d, maxTotal=%d, minIdle=%d, maxIdle=%d",
                active,
                idle,
                jedisPool.getNumWaiters(),
                total,
                poolConfig.getMaxTotal(),
                poolConfig.getMinIdle(),
                poolConfig.getMaxIdle()
        );

    }

    public static JedisPoolFactory getJedisPoolFactory() {
        return jedisPoolFactory;
    }

    public static JedisPoolConfiguration getJedisClientConfiguration() {
        return jedisClientConfiguration;
    }
}
