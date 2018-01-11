package com.microsoft.azure.redis.jedis.cluster;

import com.microsoft.azure.redis.jedis.config.JedisConfiguration;
import redis.clients.jedis.JedisCluster;

public class JedisClusterHelper {
    private static JedisCluster jedisCluster;
    private static JedisClusterFactory jedisClusterFactory;
    private static JedisConfiguration jedisClientConfiguration;

    static {
        jedisClientConfiguration = JedisConfiguration.builder().build();
        jedisClusterFactory = new JedisClusterFactory(jedisClientConfiguration);
        jedisCluster = jedisClusterFactory.createJedisCluster();
    }

    public static JedisCluster getCluster() {
        return jedisCluster;
    }

    public static JedisClusterFactory getJedisClusterFactory() {
        return jedisClusterFactory;
    }

    public static JedisConfiguration getJedisClientConfiguration() {
        return jedisClientConfiguration;
    }
}
