package cluster;

import config.RedisClientConfiguration;
import redis.clients.jedis.HostAndPort;
import redis.clients.jedis.JedisCluster;

public class JedisClusterFactory {
    private RedisClientConfiguration configuration;

    public JedisClusterFactory(RedisClientConfiguration configuration) {
        this.configuration = configuration;
    }

    public JedisCluster createInstance(){
        HostAndPort clusterNode = new HostAndPort(configuration.getHostName(), configuration.getPort());

        return new JedisCluster(clusterNode, (int)configuration.getConnectTimeout().toMillis(), (int)configuration.getOperationTimeout().toMillis(), configuration.getRetryCount(), configuration.getPassword(), configuration.getClientName().toString(), configuration.getPoolConfig(), configuration.isUseSsl());
    }
}
