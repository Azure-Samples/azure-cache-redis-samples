package com.microsoft.azure.redis.jedis.pool;

import com.microsoft.azure.redis.jedis.config.JedisConfiguration;
import org.apache.commons.pool2.impl.GenericObjectPoolConfig;
import redis.clients.jedis.JedisPool;

public class JedisPoolHelper {

    public static JedisPool getPool(){
        return getPool(null);
    }

    public static JedisPool getPool(String configFilePath){
        JedisConfiguration jedisClientConfiguration = JedisConfiguration.builder().propertyFile(configFilePath).build();
        JedisPoolFactory jedisPoolFactory = new JedisPoolFactory(jedisClientConfiguration);
        return jedisPoolFactory.createJedisPool();
    }
}
