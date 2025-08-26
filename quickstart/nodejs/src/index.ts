import { DefaultAzureCredential } from '@azure/identity';
import { EntraIdCredentialsProviderFactory, REDIS_SCOPE_DEFAULT } from '@redis/entraid';
import { createCluster } from '@redis/client';

const resourceEndpoint = process.env.AZURE_MANAGED_REDIS_HOST_NAME!;
if (!resourceEndpoint) {
    console.error('AZURE_MANAGED_REDIS_HOST_NAME is not set. It should look like: `cache-name.region-name.redis.azure.net:10000`. Find the endpoint in the Azure portal.');
    process.exit(1);
}

let client;
let endpointUrl = `rediss://${resourceEndpoint}`;
console.log('Using Redis endpoint:', endpointUrl);

try {

    function getCluster() {

        if (!endpointUrl) throw new Error('AZURE_MANAGED_REDIS_HOST_NAME must be set');

        const credential = new DefaultAzureCredential();

        const provider = EntraIdCredentialsProviderFactory.createForDefaultAzureCredential({
            credential,
            scopes: REDIS_SCOPE_DEFAULT,
            options: {},
            tokenManagerConfig: {
                expirationRefreshRatio: 0.8
            }
        });

        const client = createCluster({
            rootNodes: [{ url: endpointUrl }],
            defaults: {
                credentialsProvider: provider,
                socket: { 
                    connectTimeout: 15000, 
                    reconnectStrategy:() => new Error('Failure to connect'), 
                    // The following 2 properties are required for Azure Managed Redis
                    tls: true,
                    rejectUnauthorized: false  
                    
                }
            }
        });

        client.on('error', (err) => console.error('Redis cluster error:', err));

        return client;
    }

    client = getCluster();

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
            console.error('Failed to quit client:', quitErr);
        }
    }
}
/*
Client error: 
Using Redis endpoint: rediss://diberry-managed-redis.eastus2.redis.azure.net:10000
Ping result: PONG
Error: SimpleError: MOVED 999 4.150.65.49:8501
    at #decodeSimpleError (/workspaces/azure-cache-redis-samples/quickstart/nodejs/node_modules/@redis/client/dist/lib/RESP/decoder.js:459:13)
    at #decodeTypeValue (/workspaces/azure-cache-redis-samples/quickstart/nodejs/node_modules/@redis/client/dist/lib/RESP/decoder.js:104:91)
    at Decoder.write (/workspaces/azure-cache-redis-samples/quickstart/nodejs/node_modules/@redis/client/dist/lib/RESP/decoder.js:74:38)
    at RedisSocket.<anonymous> (/workspaces/azure-cache-redis-samples/quickstart/nodejs/node_modules/@redis/client/dist/lib/client/index.js:399:37)
    at RedisSocket.emit (node:events:518:28)
    at TLSSocket.<anonymous> (/workspaces/azure-cache-redis-samples/quickstart/nodejs/node_modules/@redis/client/dist/lib/client/socket.js:205:38)
    at TLSSocket.emit (node:events:518:28)
    at addChunk (node:internal/streams/readable:561:12)
    at readableAddChunkPushByteMode (node:internal/streams/readable:512:3)
    at Readable.push (node:internal/streams/readable:392:5)
*/