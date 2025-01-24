package example.demo;

import com.azure.core.credential.TokenRequestContext;
import com.azure.identity.DefaultAzureCredential;
import com.azure.identity.DefaultAzureCredentialBuilder;
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
public class App {
    public static void main(String[] args) {

        Config redissonconfig = getConfig();

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

    private static Config getConfig(){
        String redisAccessKey = System.getenv("REDIS_ACCESS_KEY");
        String redisUserName = System.getenv("REDIS_USER_NAME");

        if (redisAccessKey != null && !redisAccessKey.isEmpty()) {
            System.out.println("Auth with Redis key");
            return getConfigAuthWithKey();
        } else if (redisUserName != null && !redisUserName.isEmpty()) {
            System.out.println("Auth with Microsoft Entra ID");
            return getConfigAuthWithAAD();
        } else {
            throw new RuntimeException("Neither REDIS_ACCESS_KEY nor REDIS_USER_NAME is defined");
        }
    }

    private static Config getConfigAuthWithKey() {
        // Connect to the Azure Cache for Redis over the TLS/SSL port using the key
        Config redissonconfig = new Config();
        redissonconfig.useSingleServer().setPassword(System.getenv("REDIS_ACCESS_KEY"))
            .setAddress(String.format("rediss://%s:%s", System.getenv("REDIS_CACHE_HOSTNAME"), System.getenv("REDIS_CACHE_PORT")));
        return redissonconfig;
    }

    private static Config getConfigAuthWithAAD() {
        //Construct a Token Credential from Identity library, e.g. DefaultAzureCredential / ClientSecretCredential / Client CertificateCredential / ManagedIdentityCredential etc.
        DefaultAzureCredential defaultAzureCredential = new DefaultAzureCredentialBuilder().build();

        // Fetch a Microsoft Entra token to be used for authentication.
        String token = defaultAzureCredential
            .getToken(new TokenRequestContext()
                .addScopes("acca5fbb-b7e4-4009-81f1-37e38fd66d78/.default")).block().getToken();

        // Connect to the Azure Cache for Redis over the TLS/SSL port using the key
        Config redissonconfig = new Config();
        redissonconfig.useSingleServer()
            .setAddress(String.format("rediss://%s:%s", System.getenv("REDIS_CACHE_HOSTNAME"), System.getenv("REDIS_CACHE_PORT")))
            .setUsername(System.getenv("REDIS_USER_NAME")) // (Required) Username is Object ID of your managed identity or service principal
            .setPassword(token); // Microsoft Entra access token as password is required.
        return redissonconfig;
    }

}
