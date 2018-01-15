package com.azure.jedis.pool.test;

import com.azure.jedis.example.JedisPoolSample;
import com.azure.jedis.pool.JedisPoolHelper;
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
                try (Jedis jedis = JedisPoolHelper.getJedis()){
                    JedisPoolSample.simpleSetGet(jedis);
                    Thread.sleep(10 * 1000);
                } catch (JedisConnectionException e) {
                    e.printStackTrace();
                } catch (InterruptedException e) {
                    e.printStackTrace();
                }
            }
        });
    }



}
