package cluster;

import com.lambdaworks.redis.RedisURI;
import com.lambdaworks.redis.cluster.ClusterClientOptions;
import com.lambdaworks.redis.cluster.ClusterTopologyRefreshOptions;
import com.lambdaworks.redis.cluster.RedisClusterClient;
import com.lambdaworks.redis.cluster.api.StatefulRedisClusterConnection;
import com.lambdaworks.redis.support.ConnectionPoolSupport;
import config.RedisClientConfiguration;
import org.apache.commons.pool2.impl.GenericObjectPool;
import org.apache.commons.pool2.impl.GenericObjectPoolConfig;
import redis.clients.jedis.HostAndPort;
import redis.clients.jedis.JedisCluster;

import java.util.concurrent.TimeUnit;

public class LettuceClusterFactory {
    private RedisClientConfiguration configuration;

    public LettuceClusterFactory(RedisClientConfiguration configuration) {
        this.configuration = configuration;
    }

    public GenericObjectPool<StatefulRedisClusterConnection<String, String>> createInstance(){
        RedisURI redisUri = RedisURI.Builder.redis(configuration.getHostName()).withPassword(configuration.getPassword()).withSsl(configuration.isUseSsl()).withPort(configuration.getPort()).build();
        RedisClusterClient clusterClient = RedisClusterClient.create(redisUri);
        ClusterTopologyRefreshOptions topologyRefreshOptions = ClusterTopologyRefreshOptions.builder()
                .enableAllAdaptiveRefreshTriggers()
                .build();
        clusterClient.setOptions(ClusterClientOptions.builder()
                .topologyRefreshOptions(topologyRefreshOptions)
                .build());
        return ConnectionPoolSupport
                .createGenericObjectPool(() -> clusterClient.connect(), configuration.getPoolConfig());

    }
}
