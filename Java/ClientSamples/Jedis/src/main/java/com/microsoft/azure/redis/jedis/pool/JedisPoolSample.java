package com.microsoft.azure.redis.jedis.pool;

import javax.net.ssl.HostnameVerifier;
import javax.net.ssl.SSLPeerUnverifiedException;
import javax.net.ssl.SSLSession;

import org.apache.commons.pool2.impl.GenericObjectPoolConfig;
import org.apache.log4j.Logger;
import redis.clients.jedis.*;
import java.util.Set;

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
        sampleForJedisPool();
        logPoolCurrentUsage(getJedisPool(), jedisClientConfiguration.getPoolConfig());

        // when closing your application
        getJedisPool().destroy();
    }

    private static void sampleForJedisPool(){
        try(Jedis jedis = getJedisPool().getResource()){
            String key = "foo";
            String value = "bar";

            jedis.set(key, value);
            String newValue = jedis.get(key);
            logger.info(String.format("Current value for key %s is %s", key, newValue));
            jedis.zadd("sose", 0, "car"); jedis.zadd("sose", 0, "bike");
            Set<String> sose = jedis.zrange("sose", 0, -1);
        }
    }

    private static void logPoolCurrentUsage(JedisPool jedisPool, GenericObjectPoolConfig poolConfig)
    {
        int active = jedisPool.getNumActive();
        int idle = jedisPool.getNumIdle();
        int total = active + idle;
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


}
