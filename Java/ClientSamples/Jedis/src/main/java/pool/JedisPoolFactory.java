package pool;

import config.RedisClientConfiguration;
import redis.clients.jedis.JedisPool;
import redis.clients.jedis.Protocol;

public class JedisPoolFactory {
    private RedisClientConfiguration configuration;

    public JedisPoolFactory(RedisClientConfiguration configuration) {
        this.configuration = configuration;
    }

    public JedisPool createJedisPool(){
        return new JedisPool(configuration.getPoolConfig(), configuration.getHostName(), configuration.getPort(), (int)configuration.getConnectTimeout().toMillis(), (int)configuration.getOperationTimeout().toMillis(),
                configuration.getPassword(), Protocol.DEFAULT_DATABASE, configuration.getClientName().orElse(""), configuration.isUseSsl(),
                configuration.getSslSocketFactory().orElse(null), //
                configuration.getSslParameters().orElse(null), //
                configuration.getHostnameVerifier().orElse(null));
    }
}
