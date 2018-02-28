package lettuce;

import com.lambdaworks.redis.RedisConnectionException;
import com.lambdaworks.redis.RedisURI;
import com.lambdaworks.redis.cluster.ClusterClientOptions;
import com.lambdaworks.redis.cluster.ClusterTopologyRefreshOptions;
import com.lambdaworks.redis.cluster.RedisClusterClient;
import com.lambdaworks.redis.cluster.api.StatefulRedisClusterConnection;
import com.lambdaworks.redis.support.ConnectionPoolSupport;
import common.RedisClientConfig;
import common.Utils;
import org.apache.commons.pool2.impl.GenericObjectPool;

public class ClusterPoolUsage {

    public static void main(String args[]){

        // Each thread get its own Jedis instance from jedis.pool.
        // When operation is finished, calling Jedis.close() will return the instance
        // JedisPool is thread-safe while Jedis is not thread-safe

        GenericObjectPool<StatefulRedisClusterConnection<String, String>> pool = createClusterPool(RedisClientConfig.getInstance());

        try(StatefulRedisClusterConnection<String, String> connection = pool.borrowObject()){
            connection.sync().set("key", "value");
            String value = connection.sync().get("key");
            System.out.println(value);
        } catch (RedisConnectionException e){
            System.out.println(String.format("Failed to connect to Redis server: %s", e));
            System.out.println(Utils.getSystemMetrics());
            System.out.println(Utils.getPoolUsage(pool));
        } catch (Exception e) {
            e.printStackTrace();
        }

        pool.close();
    }

    public static GenericObjectPool<StatefulRedisClusterConnection<String, String>> createClusterPool(RedisClientConfig config){
        RedisURI redisUri = RedisURI.Builder.redis(config.HOST_NAME)
                .withSsl(config.USE_SSL)
                .withPassword(config.PASSWORD)
                .withPort(config.getPort())
                .build();

        RedisClusterClient client = RedisClusterClient.create(redisUri);

        ClusterTopologyRefreshOptions topologyRefreshOptions = ClusterTopologyRefreshOptions.builder()
                .enablePeriodicRefresh()
                .enableAllAdaptiveRefreshTriggers() // Refresh slots when got MOVED, ASKED error or reconnect max times reached
                .refreshTriggersReconnectAttempts(config.RECONNECT_MAX_ATTEMPTS)
                .build();

        client.setOptions(ClusterClientOptions.builder().autoReconnect(true).topologyRefreshOptions(topologyRefreshOptions).build());

        return ConnectionPoolSupport
                .createGenericObjectPool(() -> client.connect(), config.POOL_CONFIG);
    }
}
