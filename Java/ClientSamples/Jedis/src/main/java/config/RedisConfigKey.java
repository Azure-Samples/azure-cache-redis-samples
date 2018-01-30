package config;

public enum RedisConfigKey {

    HOST_NAME("hostname", null),
    PASSWORD("password", null),
    CONNECT_RETRY("connect.retry", "5"),
    CONNECT_TIMEOUT("connect.timeout.ms", "15000"),
    OPERATION_TIMEOUT("operation.timeout.ms", "15000"),
    POOL_MAX_TOTAL("pool.max.total", "200"),
    POOL_MAX_IDLE("pool.max.idle", "200"),
    POOL_MIN_IDLE("pool.min.idle", "50"),
    CLIENT_NAME("client.name", null),
    USE_SSL("useSsl", "true");

    private final String key;
    private final String defaultValue;

    RedisConfigKey(String key, String defaultValue){
        this.key = key;
        this.defaultValue = defaultValue;
    }

    public String getKey() {
        return key;
    }

    public String getDefaultValue() {
        return defaultValue;
    }
}
