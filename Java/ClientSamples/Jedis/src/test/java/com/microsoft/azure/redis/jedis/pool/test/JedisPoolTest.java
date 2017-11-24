package com.microsoft.azure.redis.jedis.pool.test;

import com.microsoft.azure.redis.jedis.pool.JedisPoolSample;
import org.junit.Test;
import redis.clients.jedis.exceptions.JedisConnectionException;

import java.util.stream.IntStream;

public class JedisPoolTest {

    @Test
    public void testRecoverFromRedisConnectionException() {
        IntStream.range(0, 10).parallel().forEach((i) ->
        {
            while(true) {
                try {
                    JedisPoolSample.sampleForJedisPool();
                    Thread.sleep(10 * 1000);
                } catch (JedisConnectionException e) {
                    JedisPoolSample.logPoolCurrentUsage();
                    e.printStackTrace();
                } catch (InterruptedException e) {
                    e.printStackTrace();
                }
            }
        });
    }
}
