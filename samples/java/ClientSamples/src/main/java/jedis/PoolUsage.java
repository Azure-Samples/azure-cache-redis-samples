package jedis;

import common.RedisClientConfig;
import common.Utils;
import redis.clients.jedis.Jedis;
import redis.clients.jedis.JedisPool;
import redis.clients.jedis.Protocol;
import redis.clients.jedis.exceptions.JedisConnectionException;

import javax.net.ssl.HostnameVerifier;
import javax.net.ssl.HttpsURLConnection;
import javax.net.ssl.SSLParameters;
import javax.net.ssl.SSLSocketFactory;

public class PoolUsage {
    public static void main(String args[]){

        // Each thread should get its own Jedis instance from JedisPool.
        // JedisPool is thread-safe while Jedis is not thread-safe

        JedisPool jedisPool = createPool(RedisClientConfig.getInstance());

        try(Jedis jedis = jedisPool.getResource()){
            jedis.set("key", "value");
            String value = jedis.get("key");
            System.out.println(value);
        } catch (JedisConnectionException e){
            System.out.println(String.format("Failed to connect to Redis server: %s", e));
            System.out.println(Utils.getSystemMetrics());
            System.out.println(Utils.getPoolUsage(jedisPool));
        } // implicitly call Jedis.close() to return instance in try clause
    }

    public static JedisPool createPool(RedisClientConfig clientConfig){
        SSLSocketFactory sslSocketFactory = null;
        SSLParameters sslParameters = null;
        HostnameVerifier hostnameVerifier = null;

        if (clientConfig.USE_SSL) {
            // Use the default socket factory (validates the certificate chain against the JVM trust store).
            sslSocketFactory = (SSLSocketFactory) SSLSocketFactory.getDefault();

            // Enable endpoint identification so the handshake verifies that the server certificate
            // actually matches HOST_NAME. Without this, an attacker presenting any trusted
            // certificate could impersonate the Redis server.
            sslParameters = new SSLParameters();
            sslParameters.setEndpointIdentificationAlgorithm("HTTPS");

            // Additional post-handshake hostname check as defense in depth.
            hostnameVerifier = HttpsURLConnection.getDefaultHostnameVerifier();
        }

        return new JedisPool(clientConfig.POOL_CONFIG, clientConfig.HOST_NAME, clientConfig.getPort(), clientConfig.CONNECT_TIMEOUT_MILLS, clientConfig.OPERATION_TIMEOUT_MILLS, clientConfig.PASSWORD, Protocol.DEFAULT_DATABASE, clientConfig.CLIENT_NAME, clientConfig.USE_SSL, sslSocketFactory, sslParameters, hostnameVerifier);
    }
}
