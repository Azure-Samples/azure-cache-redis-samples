package com.microsoft.azure.redis.jedis.pool;

import com.sun.management.OperatingSystemMXBean;
import org.apache.commons.pool2.impl.GenericObjectPoolConfig;
import org.apache.log4j.Logger;
import redis.clients.jedis.Jedis;
import redis.clients.jedis.JedisPool;
import redis.clients.jedis.exceptions.JedisConnectionException;

import javax.net.ssl.HostnameVerifier;
import javax.net.ssl.SSLPeerUnverifiedException;
import javax.net.ssl.SSLSession;
import java.lang.management.ManagementFactory;
import java.text.DecimalFormat;
import java.util.Set;
import java.util.stream.IntStream;

public class JedisPoolSample {
    private static JedisPool jedisPool;
    private static JedisPoolFactory jedisPoolFactory;
    private static JedisPoolConfiguration jedisClientConfiguration;
    private final static Logger logger = Logger.getLogger(JedisPoolSample.class);

    static {
        jedisClientConfiguration = JedisPoolConfiguration.builder().build();
        jedisPoolFactory = new JedisPoolFactory(jedisClientConfiguration);
        jedisPool = jedisPoolFactory.createJedisPool();
    }

    public static JedisPool getJedisPool() {
        return jedisPool;
    }

    public static void main(String args[]){
        IntStream.range(0, 10).parallel().forEach((i) -> sampleForJedisPool());

        // when closing your application
        getJedisPool().destroy();
    }

    public static void sampleForJedisPool(){
        try(Jedis jedis = getJedisPool().getResource()){
            String key = "foo";
            String value = "bar";

            jedis.set(key, value);
            String newValue = jedis.get(key);
            logger.info(String.format("Current value for key %s is %s", key, newValue));
            jedis.zadd("sose", 0, "car"); jedis.zadd("sose", 0, "bike");
            Set<String> sose = jedis.zrange("sose", 0, -1);
        } catch (JedisConnectionException e){
            logger.warn("Failed to connect to Jedis server: ", e);
            logPoolCurrentUsage();
        }
    }

    public static void logPoolCurrentUsage()
    {
        int active = jedisPool.getNumActive();
        int idle = jedisPool.getNumIdle();
        int total = active + idle;
        GenericObjectPoolConfig poolConfig = jedisClientConfiguration.getPoolConfig();
        logger.info(getSystemMetrics());
        logger.info(String.format(
                "JedisPool: Active=%d, Idle=%d, Waiters=%d, total=%d, maxTotal=%d, minIdle=%d, maxIdle=%d",
                active,
                idle,
                jedisPool.getNumWaiters(),
                total,
                poolConfig.getMaxTotal(),
                poolConfig.getMinIdle(),
                poolConfig.getMaxIdle()
        ));

    }

    private static String getSystemMetrics() {
        OperatingSystemMXBean operatingSystemMXBean = (OperatingSystemMXBean)ManagementFactory.getOperatingSystemMXBean();
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

    private static class SimpleHostNameVerifier implements HostnameVerifier {

        private String exactCN;
        private String wildCardCN;
        public SimpleHostNameVerifier(String cacheHostname)
        {
            exactCN = "CN=" + cacheHostname;
            wildCardCN = "CN=*" + cacheHostname.substring(cacheHostname.indexOf('.'));
        }

        public boolean verify(String s, SSLSession sslSession) {
            try {
                String cn = sslSession.getPeerPrincipal().getName();
                return cn.equalsIgnoreCase(wildCardCN) || cn.equalsIgnoreCase(exactCN);
            } catch (SSLPeerUnverifiedException ex) {
                return false;
            }
        }
    }

    public static JedisPoolFactory getJedisPoolFactory() {
        return jedisPoolFactory;
    }

    public static JedisPoolConfiguration getJedisClientConfiguration() {
        return jedisClientConfiguration;
    }
}
