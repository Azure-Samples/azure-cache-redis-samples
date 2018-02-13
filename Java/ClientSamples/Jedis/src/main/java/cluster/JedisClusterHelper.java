package cluster;

import config.RedisClientConfiguration;
import pool.JedisHelper;
import redis.clients.jedis.JedisCluster;

import java.util.stream.Collectors;

public class JedisClusterHelper {
    public static JedisCluster getCluster() {
        RedisClientConfiguration jedisClientConfiguration = RedisClientConfiguration.builder().build();
        JedisClusterFactory jedisClusterFactory = new JedisClusterFactory(jedisClientConfiguration);
        return jedisClusterFactory.createInstance();
    }

    public static JedisCluster getCluster(String configFilePath){
        RedisClientConfiguration jedisClientConfiguration = RedisClientConfiguration.builder().propertyFile(configFilePath).build();
        JedisClusterFactory jedisClusterFactory = new JedisClusterFactory(jedisClientConfiguration);
        return jedisClusterFactory.createInstance();
    }

    public static String getClusterUsage(JedisCluster jedisCluster){
        return jedisCluster.getClusterNodes().entrySet().stream().map(e -> e.getKey() + ":" + JedisHelper.getPoolUsage(e.getValue())).collect(Collectors.joining(", "));
    }

    public static String getClusterConfig(JedisCluster jedisCluster){
        return jedisCluster.getClusterNodes().entrySet().stream().map(e -> e.getKey() + ":" + JedisHelper.getPoolConfig(e.getValue())).collect(Collectors.joining(", "));
    }
    
}
