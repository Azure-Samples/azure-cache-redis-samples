var redis = require("redis");

// Environment variables for cache
const cacheHostName = process.env.AZURE_CACHE_FOR_REDIS_HOST_NAME;
const cachePassword = process.env.AZURE_CACHE_FOR_REDIS_ACCESS_KEY;

if(!cacheHostName) throw Error("AZURE_CACHE_FOR_REDIS_HOST_NAME is empty")
if(!cachePassword) throw Error("AZURE_CACHE_FOR_REDIS_ACCESS_KEY is empty")

async function testCache() {

    // Connection configuration
    const cacheConnection = redis.createClient({
        // rediss for TLS
        url: `rediss://${cacheHostName}:6380`,
        password: cachePassword
    });

    // Connect to Redis
    await cacheConnection.connect();

    // PING command
    console.log("\nCache command: PING");
    console.log("Cache response : " + await cacheConnection.ping());

    // GET
    console.log("\nCache command: GET Message");
    console.log("Cache response : " + await cacheConnection.get("Message"));

    // SET
    console.log("\nCache command: SET Message");
    console.log("Cache response : " + await cacheConnection.set("Message",
        "Hello! The cache is working from Node.js!"));

    // GET again
    console.log("\nCache command: GET Message");
    console.log("Cache response : " + await cacheConnection.get("Message"));

    // Client list, useful to see if connection list is growing...
    console.log("\nCache command: CLIENT LIST");
    console.log("Cache response : " + await cacheConnection.sendCommand(["CLIENT", "LIST"]));

    // Disconnect
    cacheConnection.disconnect()

    return "Done"
}

testCache().then((result) => console.log(result)).catch(ex => console.log(ex));