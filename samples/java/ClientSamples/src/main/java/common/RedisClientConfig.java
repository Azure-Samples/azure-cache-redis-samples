package common;

import org.apache.commons.pool2.impl.GenericObjectPoolConfig;
import redis.clients.jedis.JedisPoolConfig;

public class RedisClientConfig {

    private RedisClientConfig(){}

    // Change into your Azure redis cache name
    public final static String HOST_NAME = "{cacheName}.redis.cache.windows.net";

    // Change into your Azure redis access key
    public final static String PASSWORD = "password";

    // In Azure, 6379(disabled by default) is non-ssl and 6380 is SSL/TLS
    public final static boolean USE_SSL = true;
    public static final int SSL_PORT = 6380;
    public static final int NON_SSL_PORT = 6379;

    // How long to allow for new connections to be established
    // In general, this should be at least 5000.
    // If client has high spikes CPU usage, set to 15000 or 20000
    public final static int CONNECT_TIMEOUT_MILLS = 5000;

    // How long you are willing to wait for a response from Redis
    // Recommend 1000
    public final static int OPERATION_TIMEOUT_MILLS = 1000;
    public final static String CLIENT_NAME = "clientName";

    public final static GenericObjectPoolConfig POOL_CONFIG = createPoolConfig();

    // Max number of connections that can be created at a given time
    // Too small can lead to performance problems, too big can lead to wasted resources.
    public final static int POOL_MAX_TOTAL = 200;

    // Max number of connections that can be idle in the pool without being immediately evicted
    public final static int POOL_MAX_IDLE = 100;

    // This controls the number of connections that should be maintained for bursts of load.
    // Increase this value when you see getResource() or borrowObject() taking a long time to complete under burst scenarios
    public final static int POOL_MIN_IDLE = 50;

    // "true" will result better behavior when unexpected load hits in production
    // "false" makes it easier to debug when your maxTotal/minIdle/etc settings need adjusting.
    public final static boolean POOL_BLOCK_WHEN_EXHAUSTED = true;

    // How long to wait before throwing when pool is exhausted
    // Recommend this to be same as OPERATION_TIMEOUT_MILLS
    public final static int POOL_MAX_WAIT_MILLIS = OPERATION_TIMEOUT_MILLS;

    // For cluster use, control max attempt to try reconnect before refresh slots by calling CLUSTER NODES/SLOTS
    public final static int RECONNECT_MAX_ATTEMPTS = 3;

    private final static RedisClientConfig INSTANCE = new RedisClientConfig();

    private static JedisPoolConfig createPoolConfig(){
        JedisPoolConfig poolConfig = new JedisPoolConfig();

        poolConfig.setMaxTotal(POOL_MAX_TOTAL);
        poolConfig.setMaxIdle(POOL_MAX_IDLE);
        poolConfig.setBlockWhenExhausted(POOL_BLOCK_WHEN_EXHAUSTED);
        poolConfig.setMaxWaitMillis(POOL_MAX_WAIT_MILLIS);
        poolConfig.setMinIdle(POOL_MIN_IDLE);

        return poolConfig;
    }

    public static int getPort(){
        return USE_SSL ? SSL_PORT: NON_SSL_PORT;
    }

    public static RedisClientConfig getInstance(){
        return INSTANCE;
    }

}
