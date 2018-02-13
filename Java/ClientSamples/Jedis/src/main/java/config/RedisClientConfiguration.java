package config;

import com.beust.jcommander.Strings;
import pool.JedisPoolFactory;
import org.apache.log4j.Logger;
import redis.clients.jedis.JedisPoolConfig;

import javax.net.ssl.HostnameVerifier;
import javax.net.ssl.SSLParameters;
import javax.net.ssl.SSLSocketFactory;
import java.io.*;
import java.time.Duration;
import java.util.HashMap;
import java.util.Map;
import java.util.Optional;
import java.util.Properties;

/**
 * Jedis pool Configuration class used for setting up {@link redis.clients.jedis.JedisPool} via {@link JedisPoolFactory} using connecting
 * to a single node <a href="http://redis.io/">Redis</a> installation.
 *
 */
public class RedisClientConfiguration {
	private static final Logger logger = Logger.getLogger(RedisClientConfiguration.class);
	private final Map<String, String> configs = new HashMap<>();
	private static final int SSL_PORT = 6380;
	private static final int NON_SSL_PORT = 6379;
	private static final String DEFAULT_PROPERTY_FILE_NAME = "redis.properties";
	private final Optional<SSLSocketFactory> sslSocketFactory;
	private final Optional<SSLParameters> sslParameters;
	private final Optional<HostnameVerifier> hostnameVerifier;
	private Optional<String> propertyFilePath;
	private final JedisPoolConfig poolConfig;

	public RedisClientConfiguration(Optional<String> propertyFilePath, SSLSocketFactory sslSocketFactory,
									SSLParameters sslParameters, HostnameVerifier hostnameVerifier) {
		this.sslSocketFactory = Optional.ofNullable(sslSocketFactory);
		this.sslParameters = Optional.ofNullable(sslParameters);
		this.hostnameVerifier = Optional.ofNullable(hostnameVerifier);
		this.propertyFilePath = propertyFilePath;
		loadConfig();
		validate();
		this.poolConfig = buildPoolConfig();
	}

	private void loadConfig(){
		Properties properties = new Properties();

		try (InputStream input = getPropertyStream()){
			properties.load(input);
			for (RedisConfigKey key : RedisConfigKey.values()) {
				configs.put(key.getKey(), properties.getProperty(key.getKey(), key.getDefaultValue()));
			}

			logger.info("Configs are " + configs);

		} catch (IOException e) {
			logger.error("Failed to load redis config from file.", e);
		}
	}

	private InputStream getPropertyStream() throws FileNotFoundException {
		if(!propertyFilePath.isPresent() || Strings.isStringEmpty(propertyFilePath.get())){
			logger.info("Load property from file " + DEFAULT_PROPERTY_FILE_NAME);
			return getClass().getClassLoader().getResourceAsStream(DEFAULT_PROPERTY_FILE_NAME);
		} else {
			logger.info("Load property from file " + propertyFilePath.get());
			return new FileInputStream(String.format("./%s",propertyFilePath.get()));

		}
	}

	public String getHostName() {
		return configs.get(RedisConfigKey.HOST_NAME.getKey());
	}

	public int getPort() {
		return Boolean.parseBoolean(configs.get(RedisConfigKey.USE_SSL.getKey())) ? SSL_PORT : NON_SSL_PORT;
	}

	public boolean isUseSsl() {
		return Boolean.parseBoolean(configs.get(RedisConfigKey.USE_SSL.getKey()));
	}

	public Optional<SSLSocketFactory> getSslSocketFactory() {
		return sslSocketFactory;
	}

	public Optional<SSLParameters> getSslParameters() {
		return sslParameters;
	}

	public Optional<HostnameVerifier> getHostnameVerifier() {
		return hostnameVerifier;
	}

	public JedisPoolConfig getPoolConfig() {
		return poolConfig;
	}

	public Optional<String> getClientName() {
		return Optional.ofNullable(configs.get(RedisConfigKey.CLIENT_NAME.getKey()));
	}

	public Duration getConnectTimeout() {
		return Duration.ofMillis(Integer.parseInt(configs.get(RedisConfigKey.CONNECT_TIMEOUT.getKey())));
	}

	public Duration getOperationTimeout() {
		return Duration.ofMillis(Integer.parseInt(configs.get(RedisConfigKey.OPERATION_TIMEOUT.getKey())));
	}

	public String getPassword() {
		return configs.get(RedisConfigKey.PASSWORD.getKey());
	}

	public int getRetryCount() {
		return Integer.parseInt(configs.get(RedisConfigKey.CONNECT_RETRY.getKey()));
	}

	public static JedisPoolConfigurationBuilder builder() {
		return new JedisPoolConfigurationBuilder();
	}

	private void validate(){
		if(!configs.containsKey(RedisConfigKey.HOST_NAME.getKey()) || !configs.containsKey(RedisConfigKey.PASSWORD.getKey())){
			logger.error("hostName and password must be provided");
			throw new IllegalArgumentException("hostName and password must be provided");
		}
	}

	private JedisPoolConfig buildPoolConfig(){
		JedisPoolConfig poolConfig = new JedisPoolConfig();

		// Each thread trying to access Redis needs its own Jedis instance from the pool.
		// Using too small a value here can lead to performance problems, too big and you have wasted resources.
		poolConfig.setMaxTotal(Integer.parseInt(configs.get(RedisConfigKey.POOL_MAX_TOTAL.getKey())));
		poolConfig.setMaxIdle(Integer.parseInt(configs.get(RedisConfigKey.POOL_MAX_IDLE.getKey())));

		// Using "false" here will make it easier to debug when your maxTotal/minIdle/etc settings need adjusting.
		// Setting it to "true" will result better behavior when unexpected load hits in production
		poolConfig.setBlockWhenExhausted(true);

		// How long to wait before throwing when pool is exhausted
		poolConfig.setMaxWaitMillis(Integer.parseInt(configs.get(RedisConfigKey.OPERATION_TIMEOUT.getKey())));

		// This controls the number of connections that should be maintained for bursts of load.
		// Increase this value when you see pool.getResource() taking a long time to complete under burst scenarios
		poolConfig.setMinIdle(Integer.parseInt(configs.get(RedisConfigKey.POOL_MIN_IDLE.getKey())));

		return poolConfig;
	}

	public static class JedisPoolConfigurationBuilder {
		private SSLSocketFactory sslSocketFactory;
		private SSLParameters sslParameters;
		private HostnameVerifier hostnameVerifier;
		private Optional<String> propertyFilePath = Optional.empty();

		private JedisPoolConfigurationBuilder() {}

		public JedisPoolConfigurationBuilder sslSocketFactory(SSLSocketFactory sslSocketFactory) {
			this.sslSocketFactory = sslSocketFactory;
			return this;
		}

		public JedisPoolConfigurationBuilder sslParameters(SSLParameters sslParameters) {
			this.sslParameters = sslParameters;
			return this;
		}

		public JedisPoolConfigurationBuilder hostnameVerifier(HostnameVerifier hostnameVerifier) {
			this.hostnameVerifier = hostnameVerifier;
			return this;
		}

		public JedisPoolConfigurationBuilder propertyFile(String path) {
			this.propertyFilePath = Optional.of(path);
			return this;
		}

		public RedisClientConfiguration build() {

			return new RedisClientConfiguration(propertyFilePath, sslSocketFactory, sslParameters, hostnameVerifier);
		}
	}



}
