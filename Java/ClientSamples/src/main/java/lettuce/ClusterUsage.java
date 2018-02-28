package lettuce;

import com.lambdaworks.redis.RedisURI;
import com.lambdaworks.redis.cluster.RedisClusterClient;
import com.lambdaworks.redis.cluster.api.StatefulRedisClusterConnection;
import com.lambdaworks.redis.cluster.api.sync.RedisAdvancedClusterCommands;
import common.RedisClientConfig;

public class ClusterUsage {

    public static void main(String args[]){

        RedisClientConfig config = RedisClientConfig.getInstance();
        RedisURI redisUri = RedisURI.Builder.redis(config.HOST_NAME).withPassword(config.PASSWORD).withSsl(config.USE_SSL).withPort(config.getPort()).build();

        RedisClusterClient clusterClient = RedisClusterClient.create(redisUri);
        StatefulRedisClusterConnection<String, String> connection = clusterClient.connect();

        connection.sync().set("key", "value");
        String value = connection.sync().get("key");
        System.out.println(value);

        // Close client
        connection.close();
        clusterClient.shutdown();
    }
}
