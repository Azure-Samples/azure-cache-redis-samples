using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using StackExchange.Redis;
using System;
using System.Threading.Tasks;

namespace Redistest
{
    class Employee
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public int Age { get; set; }

        public Employee(string employeeId, string name, int age)
        {
            Id = employeeId;
            Name = name;
            Age = age;
        }
    }

    class Program
    {
        private static IConfigurationRoot _configuration;
        private static RedisConnection _redisConnection;

        static async Task Main(string[] args)
        {
            // Initialize
            var builder = new ConfigurationBuilder()
                .AddUserSecrets<Program>();
            _configuration = builder.Build();
            _redisConnection = new RedisConnection(_configuration);

            try
            {
                // Perform cache operations using the cache object...
                Console.WriteLine("Running... Press any key to quit.");

                while (!Console.KeyAvailable)
                {
                    Task thread1 = Task.Run(() => RunRedisCommandsAsync("Thread 1"));
                    Task thread2 = Task.Run(() => RunRedisCommandsAsync("Thread 2"));

                    Task.WaitAll(thread1, thread2);
                }
            }
            finally
            {
                _redisConnection.Dispose();
            }
        }

        private static async Task RunRedisCommandsAsync(string prefix)
        {

            // Simple PING command
            string cacheCommand = "PING";
            Console.WriteLine($"{Environment.NewLine}{prefix}: Cache command: {cacheCommand}");
            RedisResult pingResult = await _redisConnection.BasicRetryAsync(async (db) => await db.ExecuteAsync(cacheCommand));
            Console.WriteLine($"{prefix}: Cache response: {pingResult}");

            // Simple get and put of integral data types into the cache
            cacheCommand = "GET Message";
            Console.WriteLine($"{Environment.NewLine}{prefix}: Cache command: {cacheCommand} or StringGet()");
            RedisValue getMessageResult = await _redisConnection.BasicRetryAsync(async (db) => await db.StringGetAsync("Message"));
            Console.WriteLine($"{prefix}: Cache response: {getMessageResult}");

            cacheCommand = "SET Message \"Hello! The cache is working from a .NET Core console app!\"";
            Console.WriteLine($"{Environment.NewLine}{prefix}: Cache command: {cacheCommand} or StringSet()");
            bool stringSetResult = await _redisConnection.BasicRetryAsync(async (db) => await db.StringSetAsync("Message", "Hello! The cache is working from a .NET Core console app!"));
            Console.WriteLine($"{prefix}: Cache response: {stringSetResult}");

            cacheCommand = "GET Message";
            Console.WriteLine($"{Environment.NewLine}{prefix}: Cache command: {cacheCommand} or StringGet()");
            getMessageResult = await _redisConnection.BasicRetryAsync(async (db) => await db.StringGetAsync("Message"));
            Console.WriteLine($"{prefix}: Cache response: {getMessageResult}");

            // Store serialized object to cache
            Employee e007 = new Employee("007", "Davide Columbo", 100);
            stringSetResult = await _redisConnection.BasicRetryAsync(async (db) => await db.StringSetAsync("e007", JsonConvert.SerializeObject(e007)));
            Console.WriteLine($"{Environment.NewLine}{prefix}: Cache response from storing serialized Employee object: {stringSetResult}");


            // Retrieve serialized object from cache
            getMessageResult = await _redisConnection.BasicRetryAsync(async (db) => await db.StringGetAsync("e007"));
            Employee e007FromCache = JsonConvert.DeserializeObject<Employee>(getMessageResult);
            Console.WriteLine($"{prefix}: Deserialized Employee .NET object:{Environment.NewLine}");
            Console.WriteLine($"\t{prefix}: Employee.Name : {e007FromCache.Name}");
            Console.WriteLine($"\t{prefix}: Employee.Id   : {e007FromCache.Id}");
            Console.WriteLine($"\t{prefix}: Employee.Age  : {e007FromCache.Age}{Environment.NewLine}");
        }
    }
}