package com.azure.jedis.example;

import com.azure.jedis.pool.JedisPoolHelper;
import redis.clients.jedis.Jedis;
import redis.clients.jedis.exceptions.JedisConnectionException;

import java.util.stream.IntStream;

public class JedisPoolSample {
    public static void main(String args[]){
        IntStream.range(0, 10).parallel().forEach((i) -> sampleForJedisPool());
    }

    public static void sampleForJedisPool(){
        // Each thread get its own Jedis instance from pool.
        // When operation is finished, calling Jedis.close() will return the instance
        try(Jedis jedis = JedisPoolHelper.getJedis()){
            simpleSetGet(jedis);
        } catch (JedisConnectionException e){
            System.out.println(String.format("Failed to connect to Jedis server: %s", e));
            System.out.println(JedisPoolHelper.getSystemMetrics());
            System.out.println(JedisPoolHelper.getPoolUsage());
        }
    }

    public static void simpleSetGet(Jedis jedis){
        String key = "foo";
        String value = "bar";

        jedis.set(key, value);
        String newValue = jedis.get(key);
        System.out.println(String.format("Current value for key %s is %s", key, newValue));
    }
}
