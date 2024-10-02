using Azure.Identity;
using StackExchange.Redis;
using static System.Console;

WriteLine("This sample shows how to connect to an Azure Redis cache using managed identity (AKS workload identity) or an access key.");
try
{
    var authenticationType = Environment.GetEnvironmentVariable("AUTHENTICATION_TYPE");
    var redisHostName = Environment.GetEnvironmentVariable("REDIS_HOSTNAME");
    ConfigurationOptions? configurationOptions = null;

    switch (authenticationType)
    {
        case "WORKLOAD_IDENTITY":
            WriteLine($"Connecting to {redisHostName} with workload identity..");
            configurationOptions = await ConfigurationOptions.Parse($"{redisHostName}:6380").ConfigureForAzureWithTokenCredentialAsync(new DefaultAzureCredential());
            configurationOptions.AbortOnConnectFail = false; // Fail fast for the purposes of this sample. In production code, this should remain false to retry connections on startup
            break;

        case "ACCESS_KEY":
            WriteLine("Connecting to {cacheHostName} with an access key..");
            var redisAccessKey = Environment.GetEnvironmentVariable("REDIS_ACCESSKEY");
            var redisPort = Environment.GetEnvironmentVariable("REDIS_PORT");
            configurationOptions = ConfigurationOptions.Parse($"{redisHostName}:{redisPort},password={redisAccessKey},ssl=True,abortConnect=False");
            configurationOptions.AbortOnConnectFail = true; // Fail fast for the purposes of this sample. In production code, this should remain false to retry connections on startup
            break;

        default:
            Error.WriteLine("Invalid authentication type!");
            return;

    }

    using ConnectionMultiplexer redis = await ConnectionMultiplexer.ConnectAsync(configurationOptions);
    {
        // Get the database instance
        IDatabase db = redis.GetDatabase();

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
}
catch (Exception ex)
{
    WriteLine(ex);
}

