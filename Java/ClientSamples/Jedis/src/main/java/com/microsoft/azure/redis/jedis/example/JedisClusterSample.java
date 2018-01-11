package com.microsoft.azure.redis.jedis.example;

import com.microsoft.azure.redis.jedis.cluster.JedisClusterHelper;
import com.microsoft.azure.redis.jedis.pool.JedisPoolHelper;
import com.microsoft.azure.redis.jedis.util.MetricsUtil;
import org.apache.log4j.Logger;
import redis.clients.jedis.Jedis;
import redis.clients.jedis.JedisCluster;
import redis.clients.jedis.exceptions.JedisConnectionException;

import java.io.IOException;
import java.util.Set;
import java.util.stream.IntStream;

public class JedisClusterSample {
    private final static Logger logger = Logger.getLogger(JedisClusterSample.class);

    public static void main(String args[]){
        IntStream.range(0, 9).parallel().forEach((i) -> sampleForJedisCluster());
    }

    public static void sampleForJedisCluster() {
        try {
            JedisCluster jedisCluster = JedisClusterHelper.getCluster();
            String key = "foo";
            String value = "bar";

            jedisCluster.set(key, value);
            String newValue = jedisCluster.get(key);
            logger.info(String.format("Current value for key %s is %s", key, newValue));
        } catch (JedisConnectionException e) {
            logger.warn("Failed to connect to Jedis server: ", e);
        }
    }
}
