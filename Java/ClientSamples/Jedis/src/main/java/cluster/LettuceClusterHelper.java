package cluster;

import com.lambdaworks.redis.cluster.api.StatefulRedisClusterConnection;
import config.RedisClientConfiguration;
import org.apache.commons.pool2.impl.GenericObjectPool;

public class LettuceClusterHelper {
    public static GenericObjectPool<StatefulRedisClusterConnection<String, String>> getCluster() {
        RedisClientConfiguration clientConfiguration = RedisClientConfiguration.builder().build();
        LettuceClusterFactory clusterFactory = new LettuceClusterFactory(clientConfiguration);
        return clusterFactory.createInstance();
    }

    public static GenericObjectPool<StatefulRedisClusterConnection<String, String>> getCluster(String configFilePath){
        RedisClientConfiguration clientConfiguration = RedisClientConfiguration.builder().propertyFile(configFilePath).build();
        LettuceClusterFactory clusterFactory = new LettuceClusterFactory(clientConfiguration);
        return clusterFactory.createInstance();
    }

    public static String getPoolConfig(GenericObjectPool<StatefulRedisClusterConnection<String, String>> pool){
        return String.format("BlockWhenExhausted = %s, TestOnBorrow = %s, TestOnCreate = %s, MaxWaitMills = %s", pool.getBlockWhenExhausted(), pool.getTestOnBorrow(), pool.getTestOnCreate(), pool.getMaxWaitMillis());
    }

    public static String getPoolUsage(GenericObjectPool<StatefulRedisClusterConnection<String, String>> pool){
        return String.format("Active = %s, Idle = %s, Created = %s, Destroyed = %s, MaxBorrowWait = %s, DestroyedValidation = %s", pool.getNumActive(), pool.getNumIdle(), pool.getCreatedCount(), pool.getDestroyedCount(), pool.getMaxBorrowWaitTimeMillis(), pool.getDestroyedByBorrowValidationCount());
    }
}
