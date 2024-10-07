using Azure.Identity;
using StackExchange.Redis;
using static System.Console;

WriteLine("This sample shows how to connect to an Azure Redis cache using an Microsoft Entra identity or an access key. " +
"You can also run this sample locally with your user principal configured as a Redis User for your redis instance.");
try
{
    var authenticationType = Environment.GetEnvironmentVariable("AUTHENTICATION_TYPE");
    var redisHostName = Environment.GetEnvironmentVariable("REDIS_HOSTNAME");
    var redisPort = Environment.GetEnvironmentVariable("REDIS_PORT");
    ConfigurationOptions? configurationOptions = null;

    switch (authenticationType)
    {
        case "WORKLOAD_IDENTITY":
            WriteLine($"Connecting to {redisHostName} with workload identity..");
            configurationOptions = await ConfigurationOptions.Parse($"{redisHostName}:{redisPort}").ConfigureForAzureWithTokenCredentialAsync(new DefaultAzureCredential());
            configurationOptions.AbortOnConnectFail = true; // Fail fast for the purposes of this sample. In production code, this should remain false to retry connections on startup
            break;

        case "ACCESS_KEY":
            WriteLine("Connecting to {cacheHostName} with an access key..");
            var redisAccessKey = Environment.GetEnvironmentVariable("REDIS_ACCESSKEY");
            configurationOptions = ConfigurationOptions.Parse($"{redisHostName}:{redisPort},password={redisAccessKey}");
            configurationOptions.AbortOnConnectFail = true; // Fail fast for the purposes of this sample. In production code, this should remain false to retry connections on startup
            break;

        default:
            Error.WriteLine("Invalid authentication type!");
            return;
    }

    using ConnectionMultiplexer redis = await ConnectionMultiplexer.ConnectAsync(configurationOptions);

    // Get the default database within the Redis cache
    var db = redis.GetDatabase();

    // Set a key-value pair in Redis
    var key = "myKey";
    var value = "Hello, Redis!";
    await db.StringSetAsync(key, value);

    // Retrieve the value from Redis
    var retrievedValue = await db.StringGetAsync(key);
    WriteLine($"Retrieved value from Redis: {retrievedValue}");

    // Close the Redis connection
    redis.Close();
}
catch (Exception ex)
{
    WriteLine(ex);
}

