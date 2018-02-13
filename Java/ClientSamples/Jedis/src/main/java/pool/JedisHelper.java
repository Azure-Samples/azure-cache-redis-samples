package pool;

import config.RedisClientConfiguration;
import redis.clients.jedis.JedisPool;

public class JedisHelper {

    public static JedisPool getPool(){
        return getPool(null);
    }

    public static JedisPool getPool(String configFilePath){
        RedisClientConfiguration jedisClientConfiguration = RedisClientConfiguration.builder().propertyFile(configFilePath).build();
        JedisPoolFactory jedisPoolFactory = new JedisPoolFactory(jedisClientConfiguration);
        return jedisPoolFactory.createJedisPool();
    }

    public static String getPoolConfig(JedisPool jedisPool){
        return String.format("BlockWhenExhausted = %s, TestOnBorrow = %s, TestOnCreate = %s, MaxWaitMills = %s", jedisPool.getBlockWhenExhausted(), jedisPool.getTestOnBorrow(), jedisPool.getTestOnCreate(), jedisPool.getMaxWaitMillis());
    }

    public static String getPoolUsage(JedisPool jedisPool){
        return String.format("Active = %s, Idle = %s, Created = %s, Destroyed = %s, MaxBorrowWait = %s, DestroyedValidation = %s", jedisPool.getNumActive(), jedisPool.getNumIdle(), jedisPool.getCreatedCount(), jedisPool.getDestroyedCount(), jedisPool.getMaxBorrowWaitTimeMillis(), jedisPool.getDestroyedByBorrowValidationCount());
    }
}
