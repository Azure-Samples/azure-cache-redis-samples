package lettuce;

import com.lambdaworks.redis.ClientOptions;
import com.lambdaworks.redis.RedisClient;
import com.lambdaworks.redis.RedisConnectionException;
import com.lambdaworks.redis.RedisURI;
import com.lambdaworks.redis.api.StatefulRedisConnection;
import com.lambdaworks.redis.cluster.ClusterClientOptions;
import com.lambdaworks.redis.cluster.ClusterTopologyRefreshOptions;
import com.lambdaworks.redis.cluster.RedisClusterClient;
import com.lambdaworks.redis.cluster.api.StatefulRedisClusterConnection;
import com.lambdaworks.redis.support.ConnectionPoolSupport;
import common.RedisClientConfig;
import common.Utils;
import org.apache.commons.pool2.impl.GenericObjectPool;

public class PoolUsage {

    public static void main(String args[]){

        // StatefulRedis*Connection is thread-safe and can be shared between threads
        // It operates in a pipelining way, please refer https://github.com/lettuce-io/lettuce-core/wiki/Pipelining-and-command-flushing

        // Connection pool is only needed when you need many dedicated connections such as transactions
        // or blocking operation with each worker thread get its dedicated connection
        // Please refer https://github.com/lettuce-io/lettuce-core/wiki/Connection-Pooling

        // Each thread get its own connection from pool.
        // When operation is finished, calling connection.close() will return the instance.
        // Try clause already handles this

        clusterPoolUsage();
        nonClusterPoolUsage();
    }

    private static void nonClusterPoolUsage() {
        GenericObjectPool<StatefulRedisConnection<String, String>> pool = createPool(RedisClientConfig.getInstance());

        try(StatefulRedisConnection<String, String> connection = pool.borrowObject()){
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

    private static void clusterPoolUsage(){
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

    private static GenericObjectPool<StatefulRedisConnection<String, String>> createPool(RedisClientConfig config){
        RedisURI redisUri = RedisURI.Builder.redis(config.HOST_NAME)
                .withSsl(config.USE_SSL)
                .withPassword(config.PASSWORD)
                .withPort(config.getPort())
                .build();

        RedisClient client = RedisClient.create(redisUri);

        client.setOptions(ClientOptions.builder().autoReconnect(true).build());

        return ConnectionPoolSupport
                .createGenericObjectPool(() -> client.connect(), config.POOL_CONFIG);
    }
}
