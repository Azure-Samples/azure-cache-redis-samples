using Azure.Identity;
using StackExchange.Redis;
using static System.Console;

WriteLine("This sample shows how to connect to an Azure Redis cache using managed identity or access keys.");
try
{
    var connectionOption = Environment.GetEnvironmentVariable("CONNECTION_OPTION");
    var cacheHostName = Environment.GetEnvironmentVariable("CACHE_HOSTNAME");
    ConfigurationOptions? configurationOptions = null;

    switch (connectionOption)
    {
        case "MANAGED_IDENTITY":
            WriteLine($"Connecting to {cacheHostName} with managed identity..");
            configurationOptions = await ConfigurationOptions.Parse($"{cacheHostName}:6380").ConfigureForAzureWithTokenCredentialAsync(new DefaultAzureCredential());
            configurationOptions.AbortOnConnectFail = false; // Fail fast for the purposes of this sample. In production code, this should remain false to retry connections on startup
            break;

        case "ACCESS_KEY":
            WriteLine("Connecting to {cacheHostName} with an access key..");
            var connectionString = Environment.GetEnvironmentVariable("CONNECTION_STRING");
            configurationOptions = ConfigurationOptions.Parse(connectionString);
            configurationOptions.AbortOnConnectFail = true; // Fail fast for the purposes of this sample. In production code, this should remain false to retry connections on startup
            break;

        default:
            Error.WriteLine("Invalid connection option!");
            return;

    }

    using (ConnectionMultiplexer redis = await ConnectionMultiplexer.ConnectAsync(configurationOptions))
    {
        // Get the database instance
        IDatabase db = redis.GetDatabase();

        // Set a key-value pair in Redis
        string key = "myKey";
        string value = "Hello, Redis!";
        await db.StringSetAsync(key, value);

        // Retrieve the value from Redis
        string retrievedValue = await db.StringGetAsync(key);
        WriteLine($"Retrieved value from Redis: {retrievedValue}");

        // Close the Redis connection
        redis.Close();
    }
}
catch (Exception ex)
{
    WriteLine(ex.Message + ex.StackTrace);
    WriteLine(ex);
}

