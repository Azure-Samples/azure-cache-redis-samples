package pool;

import redis.clients.jedis.Jedis;
import redis.clients.jedis.exceptions.JedisConnectionException;

public class JedisPoolSample {
    public static void main(String args[]){

        // Each thread get its own Jedis instance from pool.
        // When operation is finished, calling Jedis.close() will return the instance
        // JedisPool is thread-safe while Jedis is not thread-safe

        try(Jedis jedis = JedisHelper.getJedis()){
            simpleSetGet(jedis);
        } catch (JedisConnectionException e){
            System.out.println(String.format("Failed to connect to Redis server: %s", e));
            System.out.println(JedisHelper.getSystemMetrics());
            System.out.println(JedisHelper.getPoolUsage());
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
