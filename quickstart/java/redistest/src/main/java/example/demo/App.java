package example.demo;

import redis.clients.jedis.Jedis;
import redis.clients.jedis.JedisPool;
import redis.clients.jedis.JedisPoolConfig;
import redis.clients.jedis.Protocol;

/**
 * Redis test
 *
 */
public class App {
    private static Object staticLock = new Object();
    private static JedisPool pool;
    private static String host;
    private static int port;
    private static String password;
    private static JedisPoolConfig config;

    // Should be called exactly once during app startup logic.
    public static void initializeSettings(String host, int port, String password) {
        App.host = host;
        App.port = port;
        App.password = password;
    }

    public static JedisPool getPoolInstance() {
        if (pool == null) { // avoid synchronization lock if initialization has already happened
            synchronized(staticLock) {
                if (pool == null) { // don't re-initialize if another thread beat us to it.
                    JedisPoolConfig poolConfig = getPoolConfig();
                    boolean useSsl = port == 6380 ? true : false;
                    int db = 0;
                    pool = new JedisPool(poolConfig, host, port, Protocol.DEFAULT_TIMEOUT, password, db, useSsl);
                }
            }
        }
        return pool;
    }

    public static JedisPoolConfig getPoolConfig() {
        if (config == null) {
            JedisPoolConfig poolConfig = new JedisPoolConfig();

            // Each thread trying to access Redis needs its own Jedis instance from the pool.
            // Using too small a value here can lead to performance problems, too big and you have wasted resources.
            int maxConnections = 200;
            poolConfig.setMaxTotal(maxConnections);
            poolConfig.setMaxIdle(maxConnections);

            // Using "false" here will make it easier to debug when your maxTotal/minIdle/etc settings need adjusting.
            // Setting it to "true" will result better behavior when unexpected load hits in production
            poolConfig.setBlockWhenExhausted(true);

            // How long to wait before throwing when pool is exhausted
            poolConfig.setMaxWaitMillis(JedisPoolConfig.DEFAULT_MAX_WAIT_MILLIS);

            // This controls the number of connections that should be maintained for bursts of load.
            // Increase this value when you see pool.getResource() taking a long time to complete under burst scenarios
            poolConfig.setMinIdle(50);

            config = poolConfig;
        }

        return config;
    }

    public static void main( String[] args ) {
        String cacheHostname = System.getenv("REDISCACHEHOSTNAME");
        String cachekey = System.getenv("REDISCACHEKEY");

        // Connect to the Azure Cache for Redis over the TLS/SSL port using the key.
        initializeSettings(cacheHostname, 6380, cachekey);
        JedisPool jedisPool = getPoolInstance();
        Jedis jedis = jedisPool.getResource();

        // Perform cache operations using the cache connection object...

        // Simple PING command        
        System.out.println( "\nCache Command  : Ping" );
        System.out.println( "Cache Response : " + jedis.ping());

        // Simple get and put of integral data types into the cache
        System.out.println( "\nCache Command  : GET Message" );
        System.out.println( "Cache Response : " + jedis.get("Message"));

        System.out.println( "\nCache Command  : SET Message" );
        System.out.println( "Cache Response : " + jedis.set("Message", "Hello! The cache is working from Java!"));

        // Demonstrate "SET Message" executed as expected...
        System.out.println( "\nCache Command  : GET Message" );
        System.out.println( "Cache Response : " + jedis.get("Message"));

        // Get the client list, useful to see if connection list is growing...
        System.out.println( "\nCache Command  : CLIENT LIST" );
        System.out.println( "Cache Response : " + jedis.clientList());

        jedis.close();
        jedisPool.close();
    }
}
