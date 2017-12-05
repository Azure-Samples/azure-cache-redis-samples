package com.microsoft.azure.redis.jedis.pool.test;

import com.microsoft.azure.redis.jedis.pool.JedisPoolHelper;
import org.junit.Test;
import redis.clients.jedis.Jedis;
import redis.clients.jedis.exceptions.JedisConnectionException;

import java.util.stream.IntStream;

public class JedisPoolTest {

    @Test
    public void testRecoverFromRedisConnectionException() {
        IntStream.range(0, 10).parallel().forEach((i) ->
        {
            while(true) {
                try {
                    simpleSetGet();
                    Thread.sleep(10 * 1000);
                } catch (JedisConnectionException e) {
                    e.printStackTrace();
                } catch (InterruptedException e) {
                    e.printStackTrace();
                }
            }
        });
    }

    public static void simpleSetGet(){
        try(Jedis jedis = JedisPoolHelper.getPool().getResource()){
            String key = "foo";
            String value = "bar";

            jedis.set(key, value);
            System.out.print(jedis.get(key));
        }
    }

}
