using Azure.Identity;
using StackExchange.Redis;
using System;
using System.Configuration;
using System.Text.Json;
using System.Threading.Tasks;

namespace RedisTest
{
    internal class Employee
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public int Age { get; set; }
    }

    internal class Program
    {
        static async Task Main(string[] args)
        {
            // Connection string should be the cache host name and port e.g. "cachename.redis.azure.net:10000"
            var connectionString = ConfigurationManager.AppSettings["RedisConnectionString"].ToString();

            // Connect to the cache using Azure credentials loaded from the current environment. For details see https://learn.microsoft.com/dotnet/azure/sdk/authentication
            // One approach is to assign yourself to an access policy on the cache (see https://aka.ms/redis/entra-auth), and ensure
            // that you're logged in with the same account in Visual Studio (see File -> Account Settings...)
            // For other authentication methods see https://github.com/Azure/Microsoft.Azure.StackExchangeRedis
            var configurationOptions = await ConfigurationOptions.Parse(connectionString).ConfigureForAzureWithTokenCredentialAsync(new DefaultAzureCredential());
            configurationOptions.AbortOnConnectFail = true; // Remove this option for code in production. It makes connections less resilient, but easier to diagnose connection problems while debugging.

            Console.WriteLine($"Connecting to '{connectionString}'...");
            using (var redisConnection = await ConnectionMultiplexer.ConnectAsync(configurationOptions))
            {
                Console.WriteLine($"Connected successfully!");
                Console.WriteLine();

                var redis = redisConnection.GetDatabase();

                Console.WriteLine("Executing Redis commands on two threads to demonstrate how a single Redis connection can safely multiplex commands from many concurrent threads...");
                Console.WriteLine();

                var thread1 = RunRedisCommandsAsync("Thread1", redis);
                var thread2 = RunRedisCommandsAsync("Thread2", redis);

                await Task.WhenAll(thread1, thread2);
            }
        }

        private static async Task RunRedisCommandsAsync(string threadName, IDatabase redis)
        {
            // Simple PING command
            Console.WriteLine($"{threadName}: Command: PING \"{threadName}\"");
            var pingResult = await redis.ExecuteAsync($"PING", new[] { threadName });
            Console.WriteLine($"{threadName}: PING response: {pingResult}");

            // Simple get and put of integral data types into the cache
            string key = threadName;
            string value = $"Hello from a .NET Framework console app on thread: '{threadName}'!";

            // In production applications, consider using a framework such as Polly (https://github.com/App-vNext/Polly)
            // to automatically retry commands that may occasionally fail.
            Console.WriteLine($"{threadName}: Command: GET {key}");
            Console.WriteLine($"{threadName}: GET response: {await redis.StringGetAsync(key)}");

            Console.WriteLine($"{threadName}: Command: SET {key} \"{value}\"");
            Console.WriteLine($"{threadName}: SET response: {await redis.StringSetAsync(key, value)}");

            Console.WriteLine($"{threadName}: Command: GET {key}");
            Console.WriteLine($"{threadName}: GET response: {await redis.StringGetAsync(key)}");

            var employee = new Employee
            {
                Id = "007",
                Name = "Davide Columbo",
                Age = 100
            };

            // Store serialized object to cache
            await redis.StringSetAsync(employee.Id, JsonSerializer.Serialize(employee));

            // Retrieve serialized object from cache
            var deserializedEmployee = JsonSerializer.Deserialize<Employee>(await redis.StringGetAsync(employee.Id));
            Console.WriteLine($"{threadName}: Deserialized Employee object properties:");
            Console.WriteLine($"{threadName}: Employee.Name : {deserializedEmployee.Name}");
            Console.WriteLine($"{threadName}: Employee.Id   : {deserializedEmployee.Id}");
            Console.WriteLine($"{threadName}: Employee.Age  : {deserializedEmployee.Age}");
        }
    }
}
