package com.microsoft.azure.redis.jedis.test;

import cluster.JedisClusterHelper;
import org.junit.Ignore;
import org.junit.Test;
import redis.clients.jedis.commands.JedisClusterCommands;
import redis.clients.jedis.exceptions.JedisConnectionException;

import java.util.stream.IntStream;

@Ignore
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

    public static void simpleSetGet(JedisClusterCommands jedis){
            String key = "foo";
            String value = "bar";

            jedis.set(key, value);
            System.out.println(jedis.get(key));
    }

}
