package jedis;

import common.RedisClientConfig;
import common.Utils;
import redis.clients.jedis.HostAndPort;
import redis.clients.jedis.JedisCluster;
import redis.clients.jedis.JedisPool;
import redis.clients.jedis.Protocol;
import redis.clients.jedis.exceptions.JedisConnectionException;

public class ClusterUsage {
    public static void main(String args[]){

        // JedisCluster is thread-safe.
        // Under the hood, it has a connection pool for each connected node.
        // Every time you execute command, it will borrow and return jedis instance from pool implicitly

        JedisCluster jedisCluster = createCluster(RedisClientConfig.getInstance());

        try {
            jedisCluster.set("key", "value");
            String value = jedisCluster.get("key");
            System.out.println(value);
        } catch (JedisConnectionException e){
            System.out.println(String.format("Failed to connect to Redis server: %s", e));
            System.out.println(Utils.getSystemMetrics());
            //TODO: log cluster pool usage when JedisCluter expose this
        }
    }

    public static JedisCluster createCluster(RedisClientConfig clientConfig){
        //TODO: JedisCluster doesn't support SSL. You can build own version based on https://github.com/xetorthio/jedis/pull/1550
        HostAndPort hostAndPort = new HostAndPort(clientConfig.CLIENT_NAME, clientConfig.getPort());
        return new JedisCluster(hostAndPort, clientConfig.CONNECT_TIMEOUT_MILLS, clientConfig.OPERATION_TIMEOUT_MILLS, clientConfig.RECONNECT_MAX_ATTEMPTS, clientConfig. PASSWORD, clientConfig.POOL_CONFIG);
    }
}
