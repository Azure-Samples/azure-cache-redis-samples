var redis = require("redis");

async function testCache() {

    // Connect to the Azure Cache for Redis over the TLS port using the key.
    var cacheHostName = process.env.REDISCACHEHOSTNAME;
    var cachePassword = process.env.REDISCACHEKEY;
    var cacheConnection = redis.createClient({
        // rediss for TLS
        url: "rediss://" + cacheHostName + ":6380",
        password: cachePassword,
    });
    await cacheConnection.connect();

    // Perform cache operations using the cache connection object...

    // Simple PING command
    console.log("\nCache command: PING");
    console.log("Cache response : " + await cacheConnection.ping());

    // Simple get and put of integral data types into the cache
    console.log("\nCache command: GET Message");
    console.log("Cache response : " + await cacheConnection.get("Message"));

    console.log("\nCache command: SET Message");
    console.log("Cache response : " + await cacheConnection.set("Message",
        "Hello! The cache is working from Node.js!"));

    // Demonstrate "SET Message" executed as expected...
    console.log("\nCache command: GET Message");
    console.log("Cache response : " + await cacheConnection.get("Message"));

    // Get the client list, useful to see if connection list is growing...
    console.log("\nCache command: CLIENT LIST");
    console.log("Cache response : " + await cacheConnection.sendCommand(["CLIENT", "LIST"]));

    console.log("\nDone");
    process.exit();
}

testCache();
