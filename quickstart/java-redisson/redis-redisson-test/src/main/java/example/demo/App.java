package example.demo;

import org.redisson.Redisson;
import org.redisson.api.RMap;
import org.redisson.api.RedissonClient;
import org.redisson.config.Config;

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
        Config config = new Config();
        config.useSingleServer().setPassword(System.getenv("REDISCACHEKEY"))
            .setAddress(String.format("rediss://%s:%s", System.getenv("REDIS_CACHE_HOSTNAME"), System.getenv("REDIS_CACHE_PORT")));
        RedissonClient client = Redisson.create(config);

        // Perform cache operations using the cache client
        RMap<String, String> map = client.getMap("test");

        // Simple get and put of string data into the cache
        System.out.println("\nCache Command  : GET Message");
        System.out.println("Cache Response : " + map.get("Message"));

        System.out.println("\nCache Command  : SET Message");
        System.out.println("Cache Response : " + map.put("Message",
            String.format("Hello! The cache is working from Java! %s", LocalDateTime.now())));

        // Demonstrate "SET Message" executed as expected
        System.out.println("\nCache Command  : GET Message");
        System.out.println("Cache Response : " + map.get("Message"));

        client.shutdown();
    }
}
