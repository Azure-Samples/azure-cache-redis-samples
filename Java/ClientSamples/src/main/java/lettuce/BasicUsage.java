package lettuce;

import com.lambdaworks.redis.ReadFrom;
import com.lambdaworks.redis.RedisClient;
import com.lambdaworks.redis.RedisURI;
import com.lambdaworks.redis.api.StatefulRedisConnection;
import com.lambdaworks.redis.cluster.RedisClusterClient;
import com.lambdaworks.redis.cluster.api.StatefulRedisClusterConnection;
import com.lambdaworks.redis.codec.Utf8StringCodec;
import com.lambdaworks.redis.masterslave.MasterSlave;
import com.lambdaworks.redis.masterslave.StatefulRedisMasterSlaveConnection;
import common.RedisClientConfig;

public class BasicUsage {

    public static void main(String args[]){

        // StatefulRedis*Connection is thread-safe and can be shared between threads
        // It operates in a pipelining way, please refer https://github.com/lettuce-io/lettuce-core/wiki/Pipelining-and-command-flushing

        basicUsage(); // for Azure Redis Basic tier
        masterSlaveUsage(); // for Azure Redis Standard and Premium with cluster disabled
        clusterUsage(); // for Azure Redis Premium with cluster enabled
    }

    private static void clusterUsage() {
        RedisClientConfig config = RedisClientConfig.getInstance();
        RedisURI redisUri = RedisURI.Builder.redis(config.HOST_NAME).
                withPassword(config.PASSWORD).
                withSsl(config.USE_SSL).
                withPort(config.getPort()).
                build();

        RedisClusterClient clusterClient = RedisClusterClient.create(redisUri);
        StatefulRedisClusterConnection<String, String> connection = clusterClient.connect();

        connection.sync().set("key", "value");
        String value = connection.sync().get("key");
        System.out.println(value);

        // Close client
        connection.close();
        clusterClient.shutdown();
    }

    private static void masterSlaveUsage() {
        RedisClientConfig config = RedisClientConfig.getInstance();
        RedisURI redisUri = RedisURI.Builder.redis(config.HOST_NAME)
                .withSsl(config.USE_SSL)
                .withPassword(config.PASSWORD)
                .withPort(config.getPort())
                .build();

        RedisClient client = RedisClient.create(redisUri);
        StatefulRedisMasterSlaveConnection<String, String> connection = MasterSlave.connect(client, new Utf8StringCodec(), redisUri);
        connection.setReadFrom(ReadFrom.MASTER_PREFERRED);

        connection.sync().set("key", "value");
        String value = connection.sync().get("key");
        System.out.println(value);

        // Close client
        connection.close();
        client.shutdown();
    }

    private static void basicUsage(){
        RedisClientConfig config = RedisClientConfig.getInstance();
        RedisURI redisUri = RedisURI.Builder.redis(config.HOST_NAME)
                .withSsl(config.USE_SSL)
                .withPassword(config.PASSWORD)
                .withPort(config.getPort())
                .build();

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
