package com.microsoft.azure.redis.jedis.example;

import com.microsoft.azure.redis.jedis.pool.JedisPoolHelper;
import com.microsoft.azure.redis.jedis.util.MetricsUtil;
import org.apache.log4j.Logger;
import redis.clients.jedis.Jedis;
import redis.clients.jedis.exceptions.JedisConnectionException;

import java.util.Set;
import java.util.stream.IntStream;

public class JedisPoolSample {
    private final static Logger logger = Logger.getLogger(JedisPoolSample.class);

    public static void main(String args[]){
        IntStream.range(0, 10).parallel().forEach((i) -> sampleForJedisPool());
    }

    public static void sampleForJedisPool(){
        try(Jedis jedis = JedisPoolHelper.getPool().getResource()){
            String key = "foo";
            String value = "bar";

            jedis.set(key, value);
            String newValue = jedis.get(key);
            logger.info(String.format("Current value for key %s is %s", key, newValue));
            jedis.zadd("sose", 0, "car"); jedis.zadd("sose", 0, "bike");
            Set<String> sose = jedis.zrange("sose", 0, -1);
        } catch (JedisConnectionException e){
            logger.warn("Failed to connect to Jedis server: ", e);
            logger.info(MetricsUtil.getSystemMetrics());
            logger.info(JedisPoolHelper.getPoolStatistics());
        }
    }
}
