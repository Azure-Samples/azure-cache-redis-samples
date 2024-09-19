using Azure.Core;
using Azure.Identity;
using StackExchange.Redis;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace ConnectFromAKSSample
{
    class Program
    {
        static async Task Main(string[] args)
        {
            try
            {
                string connectionOption = Environment.GetEnvironmentVariable("CONNECTION_OPTION");
                string cacheHostName = Environment.GetEnvironmentVariable("CACHE_HOSTNAME");
                ConfigurationOptions? configurationOptions = null;

                switch (connectionOption)
                {
                    case "MANAGED_IDENTITY":
                        Console.WriteLine($"Connecting with managed identity..");
                        configurationOptions = await ConfigurationOptions.Parse($"{cacheHostName}:6380").ConfigureForAzureWithTokenCredentialAsync(new DefaultAzureCredential());
                        configurationOptions.AbortOnConnectFail = false; // Fail fast for the purposes of this sample. In production code, this should remain false to retry connections on startup
                        break;

                    case "ACCESS_KEY":
                        Console.WriteLine("Connecting with an access key.");
                        string connectionString = Environment.GetEnvironmentVariable("CONNECTION_STRING");
                        configurationOptions = ConfigurationOptions.Parse(connectionString);
                        configurationOptions.AbortOnConnectFail = true; // Fail fast for the purposes of this sample. In production code, this should remain false to retry connections on startup
                        break;

                    default:
                        return;

                }

                // Create a connection to Redis
                ConnectionMultiplexer redis = await ConnectionMultiplexer.ConnectAsync(configurationOptions);

                // Get the database instance
                IDatabase db = redis.GetDatabase();

                // Set a key-value pair in Redis
                string key = "myKey";
                string value = "Hello, Redis!";
                await db.StringSetAsync(key, value);

                // Retrieve the value from Redis
                string retrievedValue = await db.StringGetAsync(key);
                Console.WriteLine($"Retrieved value from Redis: {retrievedValue}");

                while (true)
                {
                    // Read and write a key every 2 minutes and output a '+' to show that the connection is working
                    try
                    {
                        // NOTE: Always use the *Async() versions of StackExchange.Redis methods if possible (e.g. StringSetAsync(), StringGetAsync())
                        await db.StringSetAsync("key", DateTime.UtcNow.ToString());
                        Console.WriteLine($"Success! Previous value: {value}");
                    }
                    catch (Exception ex)
                    {
                        // NOTE: Production applications should implement a retry strategy to handle any commands that fail
                        Console.WriteLine($"Failed to execute a Redis command: {ex}");
                    }

                    await Task.Delay(TimeSpan.FromSeconds(30));
                }

                // Close the Redis connection
                redis.Close();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message + ex.StackTrace);
            }
        }
    }
}
