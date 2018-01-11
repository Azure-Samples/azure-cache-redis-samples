package com.microsoft.azure.redis.jedis.cluster;

import com.microsoft.azure.redis.jedis.config.JedisConfiguration;
import redis.clients.jedis.HostAndPort;
import redis.clients.jedis.JedisCluster;

import java.util.Arrays;
import java.util.HashSet;
import java.util.Set;

public class JedisClusterFactory {
    private JedisConfiguration configuration;

    public JedisClusterFactory(JedisConfiguration configuration) {
        this.configuration = configuration;
    }

    public JedisCluster createJedisCluster(){
        Set<HostAndPort> clusterNodes = new HashSet<>(Arrays.asList(new HostAndPort(configuration.getHostName(), configuration.getPort())));

        return new JedisCluster(clusterNodes, (int)configuration.getConnectTimeout().toMillis(), (int)configuration.getOperationTimeout().toMillis(), 5, configuration.getPassword(), configuration.getPoolConfig());
    }
}
