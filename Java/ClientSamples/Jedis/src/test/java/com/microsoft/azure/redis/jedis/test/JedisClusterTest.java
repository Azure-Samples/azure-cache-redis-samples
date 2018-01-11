package com.microsoft.azure.redis.jedis.test;

import com.microsoft.azure.redis.jedis.cluster.JedisClusterHelper;
import org.junit.Test;
import redis.clients.jedis.JedisCommands;
import redis.clients.jedis.exceptions.JedisConnectionException;

import java.util.stream.IntStream;

public class JedisClusterTest {

    @Test
    public void testRecoverFromRedisConnectionException() {
        IntStream.range(0, 10).parallel().forEach((i) ->
        {
            while(true) {
                try{
                    simpleSetGet(JedisClusterHelper.getCluster());
                } catch (JedisConnectionException e) {
                    e.printStackTrace();
                }
            }
        });
    }

    public static void simpleSetGet(JedisCommands jedis){
            String key = "foo";
            String value = "bar";

            jedis.set(key, value);
            System.out.print(jedis.get(key));
    }

}
