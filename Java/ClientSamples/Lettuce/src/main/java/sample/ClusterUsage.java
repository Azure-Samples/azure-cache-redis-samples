package sample;

import com.lambdaworks.redis.RedisURI;
import com.lambdaworks.redis.cluster.RedisClusterClient;
import com.lambdaworks.redis.cluster.api.StatefulRedisClusterConnection;
import com.lambdaworks.redis.cluster.api.sync.RedisAdvancedClusterCommands;

public class ClusterUsage {
    private static final String HOST = "warren-eus-cluster.redis.cache.windows.net";
    private static final String PASSWORD = "Z7rhnViMSnLAqceIV8uxfkjNP5XoIZp5g9VmsF44zW0=";


    public static void main(String args[]){

        RedisURI redisUri = RedisURI.Builder.redis(HOST).withPassword(PASSWORD).withSsl(true).withPort(6380).build();

        RedisClusterClient clusterClient = RedisClusterClient.create(redisUri);
        StatefulRedisClusterConnection<String, String> connection = clusterClient.connect();
        RedisAdvancedClusterCommands<String, String> syncCommands = connection.sync();

        syncCommands.set("foo", "bar");
        String value = syncCommands.get("foo");

        System.out.println(value);
        connection.close();
        clusterClient.shutdown();
    }
}
