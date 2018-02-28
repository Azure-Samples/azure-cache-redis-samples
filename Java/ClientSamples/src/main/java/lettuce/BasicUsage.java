package lettuce;

import com.lambdaworks.redis.RedisClient;
import com.lambdaworks.redis.RedisURI;
import com.lambdaworks.redis.api.StatefulRedisConnection;
import com.lambdaworks.redis.cluster.RedisClusterClient;
import com.lambdaworks.redis.cluster.api.StatefulRedisClusterConnection;
import common.RedisClientConfig;

public class BasicUsage {

    public static void main(String args[]){

        RedisClientConfig config = RedisClientConfig.getInstance();
        RedisURI redisUri = RedisURI.Builder.redis(config.HOST_NAME).withPassword(config.PASSWORD).withSsl(config.USE_SSL).withPort(config.getPort()).build();

        RedisClient client = RedisClient.create(redisUri);
        StatefulRedisConnection<String, String> connection = client.connect();

        connection.sync().set("key", "value");
        String value = connection.sync().get("key");
        System.out.println(value);

        // Close client
        connection.close();
        client.shutdown();
    }
}
