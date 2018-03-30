package lettuce;

import com.lambdaworks.redis.RedisClient;
import com.lambdaworks.redis.RedisFuture;
import com.lambdaworks.redis.RedisURI;
import com.lambdaworks.redis.cluster.RedisClusterClient;
import com.lambdaworks.redis.cluster.pubsub.StatefulRedisClusterPubSubConnection;
import com.lambdaworks.redis.pubsub.RedisPubSubAdapter;
import com.lambdaworks.redis.pubsub.StatefulRedisPubSubConnection;
import com.lambdaworks.redis.pubsub.api.async.RedisPubSubAsyncCommands;
import com.lambdaworks.redis.pubsub.api.rx.RedisPubSubReactiveCommands;
import com.lambdaworks.redis.pubsub.api.sync.RedisPubSubCommands;
import common.RedisClientConfig;

public class PubSubUsage {

    public static void main(String[] args){
        syncSubscription();
        asyncSubscription();
        reactSubscription();
        publish();
        clusterSubcription();
        clusterPublish();
    }

    private static void syncSubscription(){
        RedisClient client = createClient();
        StatefulRedisPubSubConnection<String, String> connection = client.connectPubSub();
        connection.addListener(new PrintPubSubListener<>());

        RedisPubSubCommands<String, String> sync = connection.sync();
        sync.subscribe("channel.sync");
    }

    private static void asyncSubscription(){
        RedisClient client = createClient();
        StatefulRedisPubSubConnection<String, String> connection = client.connectPubSub();
        connection.addListener(new PrintPubSubListener<>());

        RedisPubSubAsyncCommands<String, String> async = connection.async();
        RedisFuture<Void> future = async.subscribe("channel.async");
    }

    private static void reactSubscription(){
        RedisClient client = createClient();
        StatefulRedisPubSubConnection<String, String> connection = client.connectPubSub();

        RedisPubSubReactiveCommands<String, String> reactive = connection.reactive();

        reactive.subscribe("channel.react").subscribe();
        reactive.observeChannels().doOnNext(patternMessage -> System.out.println(String.format("Message %s from channel %s", patternMessage.getMessage(), patternMessage.getChannel()))).subscribe();
    }

    private static void publish(){
        RedisClient client = createClient();
        StatefulRedisPubSubConnection<String, String> connection = client.connectPubSub();

        RedisPubSubCommands<String, String> sync = connection.sync();
        sync.publish("channel.sync", "sync");
        sync.publish("channel.react", "reactive");
        sync.publish("channel.async", "async");
    }

    private static void clusterSubcription(){
        RedisClusterClient clusterClient = createClusterClient();
        StatefulRedisClusterPubSubConnection<String, String> connection = clusterClient.connectPubSub();
        connection.addListener(new PrintPubSubListener<>());

        RedisPubSubCommands<String, String> sync = connection.sync();
        sync.subscribe("channel.cluster");
    }

    private static void clusterPublish(){
        RedisClusterClient clusterClient = createClusterClient();
        StatefulRedisClusterPubSubConnection<String, String> connection = clusterClient.connectPubSub();

        RedisPubSubCommands<String, String> sync = connection.sync();
        sync.publish("channel.cluster", "cluster");
    }

    private static class PrintPubSubListener<K, V> extends RedisPubSubAdapter<K, V>{
        @Override
        public void message(K channel, V message) {
            System.out.println(String.format("Message %s from channel %s", message, channel));
        }

        @Override
        public void message(K pattern, K channel, V message) {
            System.out.println(String.format("Message %s with pattern %s from channel %s", message, pattern, channel));
        }

        @Override
        public void subscribed(K channel, long count) {
            System.out.println(String.format("Subscribed to channel %s. Count: %s", channel, count));
        }

        @Override
        public void psubscribed(K pattern, long count) {
            System.out.println(String.format("Subscribed to pattern %s. Count: %s", pattern, count));
        }

        @Override
        public void unsubscribed(K channel, long count) {
            System.out.println(String.format("Unsubscribed to channel %s. Count: %s", channel, count));
        }

        @Override
        public void punsubscribed(K pattern, long count) {
            System.out.println(String.format("Unsubscribed to pattern %s. Count: %s", pattern, count));
        }
    }

    private static RedisClient createClient(){
        RedisClientConfig config = RedisClientConfig.getInstance();
        RedisURI redisUri = RedisURI.Builder.redis(config.HOST_NAME)
                .withSsl(config.USE_SSL)
                .withPassword(config.PASSWORD)
                .withPort(config.getPort())
                .build();

        return RedisClient.create(redisUri);
    }

    private static RedisClusterClient createClusterClient(){
        RedisClientConfig config = RedisClientConfig.getInstance();
        RedisURI redisUri = RedisURI.Builder.redis(config.HOST_NAME).
                withPassword(config.PASSWORD).
                withSsl(config.USE_SSL).
                withPort(config.getPort()).
                build();

        return RedisClusterClient.create(redisUri);
    }
}
