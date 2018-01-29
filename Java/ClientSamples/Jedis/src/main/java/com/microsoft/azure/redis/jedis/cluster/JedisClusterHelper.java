package com.microsoft.azure.redis.jedis.cluster;

import com.microsoft.azure.redis.jedis.config.JedisConfiguration;
import com.microsoft.azure.redis.jedis.pool.JedisPoolHelper;
import redis.clients.jedis.JedisCluster;
import redis.clients.jedis.JedisPool;

import java.util.stream.Collectors;

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

    public static String getClusterUsage(JedisCluster jedisCluster){
        return jedisCluster.getClusterNodes().entrySet().stream().map(e -> e.getKey() + ":" + JedisPoolHelper.getPoolUsage(e.getValue())).collect(Collectors.joining(", "));
    }

    public static String getClusterConfig(JedisCluster jedisCluster){
        return jedisCluster.getClusterNodes().entrySet().stream().map(e -> e.getKey() + ":" + JedisPoolHelper.getPoolConfig(e.getValue())).collect(Collectors.joining(", "));
    }
    
}
