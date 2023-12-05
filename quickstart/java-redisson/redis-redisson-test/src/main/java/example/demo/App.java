package example.demo;

import org.redisson.Redisson;
import org.redisson.api.RedissonClient;
import org.redisson.config.Config;
import org.redisson.jcache.configuration.RedissonConfiguration;

import javax.cache.Cache;
import javax.cache.CacheManager;
import javax.cache.Caching;
import javax.cache.configuration.Configuration;
import javax.cache.configuration.MutableConfiguration;
import java.time.LocalDateTime;


/**
 * Redis test
 *
 */
public class App
{
    public static void main(String[] args)
    {
        // Connect to the Azure Cache for Redis over the TLS/SSL port using the key
        Config redissonconfig = new Config();
        redissonconfig.useSingleServer().setPassword(System.getenv("REDIS_CACHE_KEY"))
            .setAddress(String.format("rediss://%s:6380", System.getenv("REDIS_CACHE_HOSTNAME")));

        RedissonClient redissonClient = Redisson.create(redissonconfig);

        MutableConfiguration<String, String> jcacheConfig = new MutableConfiguration<>();
        Configuration<String, String> config = RedissonConfiguration.fromInstance(redissonClient, jcacheConfig);

        // Perform cache operations using JCache
        CacheManager manager = Caching.getCachingProvider().getCacheManager();
        Cache<String, String> map = manager.createCache("test", config);

        // Simple get and put of string data into the cache
        System.out.println("\nCache Command  : GET Message");
        System.out.println("Cache Response : " + map.get("Message"));

        System.out.println("\nCache Command  : SET Message");
        map.put("Message",
            String.format("Hello! The cache is working from Java! %s", LocalDateTime.now()));

        // Demonstrate "SET Message" executed as expected
        System.out.println("\nCache Command  : GET Message");
        System.out.println("Cache Response : " + map.get("Message"));

        redissonClient.shutdown();
    }
}
