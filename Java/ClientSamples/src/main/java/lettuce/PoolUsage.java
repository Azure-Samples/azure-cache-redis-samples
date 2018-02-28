package lettuce;

import com.lambdaworks.redis.ClientOptions;
import com.lambdaworks.redis.RedisClient;
import com.lambdaworks.redis.RedisConnectionException;
import com.lambdaworks.redis.RedisURI;
import com.lambdaworks.redis.api.StatefulRedisConnection;
import com.lambdaworks.redis.support.ConnectionPoolSupport;
import common.RedisClientConfig;
import common.Utils;
import org.apache.commons.pool2.impl.GenericObjectPool;

public class PoolUsage {
    public static void main(String args[]){

        // Each thread get its own connection from pool.
        // When operation is finished, calling connection.close() will return the instance. Try clause already handles this

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

    public static GenericObjectPool<StatefulRedisConnection<String, String>> createPool(RedisClientConfig config){
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
