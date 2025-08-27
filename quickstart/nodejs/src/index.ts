import { DefaultAzureCredential } from '@azure/identity';
import { EntraIdCredentialsProviderFactory, REDIS_SCOPE_DEFAULT } from '@redis/entraid';
import { createCluster, RedisClusterType, RedisModules, RedisFunctions, RedisScripts } from '@redis/client';
import * as net from 'node:net';

const redisEndpoint = process.env.REDIS_ENDPOINT!;
if (!redisEndpoint) {
    console.error('REDIS_ENDPOINT is not set. It should look like: `cache-name.region-name.redis.azure.net:<PORT>`. Find the endpoint in the Azure portal.');
    process.exit(1);
}

const [redisHostName, _] = redisEndpoint.split(":");

let client;

function createRedisClient(): RedisClusterType<RedisModules, RedisFunctions, RedisScripts>  {

    const credential = new DefaultAzureCredential();

    const provider = EntraIdCredentialsProviderFactory.createForDefaultAzureCredential({
        credential,
        scopes: REDIS_SCOPE_DEFAULT,
        options: {},
        tokenManagerConfig: {
            expirationRefreshRatio: 0.8
        }
    });

    const client = createCluster<RedisModules, RedisFunctions, RedisScripts>({
        rootNodes: [{ url: `rediss://${redisEndpoint}` }],
        defaults: {
            credentialsProvider: provider,
            socket: {
                connectTimeout: 15000,
                tls: true,

                // This quickstart code uses a fail fast `reconnectStrategy` which
                // is suitable only in sample code. The purpose is to quickly
                // demonstrate the functionality without getting stuck in
                // reconnection loops if your endpoint or authentication is not
                // correctly configured. In production code, a more robust
                // `reconnectStrategy` should be implemented.
                reconnectStrategy: () => new Error('Failure to connect')
            }

        },
        nodeAddressMap(incomingAddress) {
            const [hostNameOrIP, port] = incomingAddress.split(":");

            const address =
                net.isIP(hostNameOrIP) !== 0
                    ? redisHostName
                    : hostNameOrIP;

            return {
                host: address,
                port: Number(port),
            };
        }

    });

    client.on('error', (err) => console.error('Redis cluster error:', err));

    return client;
}

try {

    client = createRedisClient();

    await client.connect();

    const pingResult = await client.ping();
    console.log('Ping result:', pingResult);

    const setResult = await client.set("Message", "Hello! The cache is working from Node.js!");
    console.log('Set result:', setResult);

    const getResult = await client.get("Message");
    console.log('Get result:', getResult);

} catch (err) {
    console.error('Error:', err);
} finally {
    if (client) {
        try {
            await client.quit();
        } catch (quitErr) {
            console.error('Error occurred while quitting Redis client:', quitErr);

        }
    }
}
