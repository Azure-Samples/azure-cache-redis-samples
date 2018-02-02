package pool;

import com.sun.management.OperatingSystemMXBean;
import org.apache.commons.pool2.impl.GenericObjectPoolConfig;
import redis.clients.jedis.Jedis;
import redis.clients.jedis.JedisPool;
import redis.clients.jedis.JedisPoolConfig;
import redis.clients.jedis.Protocol;

import java.lang.management.ManagementFactory;
import java.text.DecimalFormat;

public class JedisPoolHelper {
    private static final int SSL_PORT = 6380;
    private static final int NON_SSL_PORT = 6379;

    private final static JedisPool jedisPool;

    // Change into your Azure redis cache name
    private final static String HOST_NAME = "{cacheName}.redis.cache.windows.net";

    // Change into your Azure redis access key
    private final static String PASSWORD = "password";

    // In Azure, 6379(disabled by default) is non-ssl and 6380 is SSL/TLS
    private final static boolean USE_SSL = true;

    // How long to allow for new connections to be established
    // In general, this should be at least 5000.
    // If client has high spikes CPU usage, set to 15000 or 20000
    private final static int CONNECT_TIMEOUT_MILLS = 5000;

    // How long you are willing to wait for a response from Redis
    // Recommend 1000
    private final static int OPERATION_TIMEOUT_MILLS = 1000;
    private final static String CLIENT_NAME = "client1";

    private final static GenericObjectPoolConfig poolConfig;

    // Max number of connections that can be created at a given time
    // Too small can lead to performance problems, too big can lead to wasted resources.
    private final static int POOL_MAX_TOTAL = 200;

    // Max number of connections that can be idle in the pool without being immediately evicted
    private final static int POOL_MAX_IDLE = 100;

    // This controls the number of connections that should be maintained for bursts of load.
    // Increase this value when you see pool.getResource() taking a long time to complete under burst scenarios
    private final static int POOL_MIN_IDLE = 50;

    // "true" will result better behavior when unexpected load hits in production
    // "false" makes it easier to debug when your maxTotal/minIdle/etc settings need adjusting.
    private final static boolean POOL_BLOCK_WHEN_EXHAUSTED = true;

    // How long to wait before throwing when pool is exhausted
    // Recommend this to be same as OPERATION_TIMEOUT_MILLS
    private final static int POOL_MAX_WAIT_MILLIS = OPERATION_TIMEOUT_MILLS;

    static {
        poolConfig = createPoolConfig();
        jedisPool = createJedisPool();
    }

    public static Jedis getJedis() {
        return jedisPool.getResource();
    }

    public static String getPoolUsage() {
        int active = jedisPool.getNumActive();
        int idle = jedisPool.getNumIdle();
        int total = active + idle;
        return String.format(
                "JedisPool: Active=%d, Idle=%d, Waiters=%d, total=%d, maxTotal=%d, minIdle=%d, maxIdle=%d",
                active,
                idle,
                jedisPool.getNumWaiters(),
                total,
                poolConfig.getMaxTotal(),
                poolConfig.getMinIdle(),
                poolConfig.getMaxIdle()
        );

    }

    private static JedisPoolConfig createPoolConfig(){
        JedisPoolConfig poolConfig = new JedisPoolConfig();

        poolConfig.setMaxTotal(POOL_MAX_TOTAL);
        poolConfig.setMaxIdle(POOL_MAX_IDLE);
        poolConfig.setBlockWhenExhausted(POOL_BLOCK_WHEN_EXHAUSTED);
        poolConfig.setMaxWaitMillis(POOL_MAX_WAIT_MILLIS);
        poolConfig.setMinIdle(POOL_MIN_IDLE);

        return poolConfig;
    }

    private static JedisPool createJedisPool(){
        return new JedisPool(poolConfig, HOST_NAME, getPort(USE_SSL), CONNECT_TIMEOUT_MILLS, OPERATION_TIMEOUT_MILLS, PASSWORD, Protocol.DEFAULT_DATABASE, CLIENT_NAME, USE_SSL, null, null, null);
    }

    private static int getPort(boolean useSSL){
        return useSSL? SSL_PORT: NON_SSL_PORT;
    }

    public static String getSystemMetrics() {
        OperatingSystemMXBean operatingSystemMXBean = (OperatingSystemMXBean) ManagementFactory.getOperatingSystemMXBean();
        String freeMemorySize = toReadableSize(Runtime.getRuntime().freeMemory());
        String maxMemorySize = toReadableSize(Runtime.getRuntime().maxMemory());
        String totalMemorySize = toReadableSize(Runtime.getRuntime().totalMemory());
        String systemCpuLoad = toReadablePercent(operatingSystemMXBean.getSystemCpuLoad());
        String processCpuLoad = toReadablePercent(operatingSystemMXBean.getProcessCpuLoad());

        return String.format("Free memory: %s, Max memory: %s, Total memory: %s, System CPU load: %s, Process CPU load: %s", freeMemorySize, maxMemorySize, totalMemorySize, systemCpuLoad, processCpuLoad);
    }

    private static String toReadableSize(long size) {
        if(size <= 0) return "0";
        final String[] units = new String[] { "B", "kB", "MB", "GB", "TB" };
        int digitGroups = (int) (Math.log10(size)/Math.log10(1024));
        return new DecimalFormat("#,##0.#").format(size/Math.pow(1024, digitGroups)) + " " + units[digitGroups];
    }

    private static String toReadablePercent(double percent){
        return String.format("%.02f %%", percent * 100);
    }
}
