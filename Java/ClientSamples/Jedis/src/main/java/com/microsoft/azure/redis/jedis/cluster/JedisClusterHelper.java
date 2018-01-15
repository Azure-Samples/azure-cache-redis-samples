package com.microsoft.azure.redis.jedis.cluster;

import com.microsoft.azure.redis.jedis.config.JedisConfiguration;
import redis.clients.jedis.JedisCluster;

public class JedisClusterHelper {
    public static JedisCluster getCluster() {
        JedisConfiguration jedisClientConfiguration = JedisConfiguration.builder().build();
        JedisClusterFactory jedisClusterFactory = new JedisClusterFactory(jedisClientConfiguration);
        return jedisClusterFactory.createJedisCluster();
    }

    public static JedisCluster getCluster(String configFilePath){
        JedisConfiguration jedisClientConfiguration = JedisConfiguration.builder().propertyFile(configFilePath).build();
        JedisClusterFactory jedisClusterFactory = new JedisClusterFactory(jedisClientConfiguration);
        return jedisClusterFactory.createJedisCluster();
    }
    
}
