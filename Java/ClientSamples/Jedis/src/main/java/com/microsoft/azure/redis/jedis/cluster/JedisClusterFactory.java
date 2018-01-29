package com.microsoft.azure.redis.jedis.cluster;

import com.microsoft.azure.redis.jedis.config.JedisConfiguration;
import redis.clients.jedis.HostAndPort;
import redis.clients.jedis.JedisCluster;

public class JedisClusterFactory {
    private JedisConfiguration configuration;

    public JedisClusterFactory(JedisConfiguration configuration) {
        this.configuration = configuration;
    }

    public JedisCluster createJedisCluster(){
        HostAndPort clusterNode = new HostAndPort(configuration.getHostName(), configuration.getPort());

        return new JedisCluster(clusterNode, (int)configuration.getConnectTimeout().toMillis(), (int)configuration.getOperationTimeout().toMillis(), configuration.getRetryCount(), configuration.getPassword(), configuration.getClientName().toString(), configuration.getPoolConfig(), configuration.isUseSsl());
    }
}
