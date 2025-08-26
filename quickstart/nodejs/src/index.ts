import { DefaultAzureCredential } from '@azure/identity';
import { EntraIdCredentialsProviderFactory, REDIS_SCOPE_DEFAULT } from '@redis/entraid';
import { createClient } from '@redis/client';

const resourceEndpoint = process.env.AZURE_MANAGED_REDIS_HOST_NAME!;
if (!resourceEndpoint) {
    console.error('AZURE_MANAGED_REDIS_HOST_NAME is not set. It should look like: `cache-name.region-name.redis.azure.net:10000`. Find the endpoint in the Azure portal.');
    process.exit(1);
}

let client;
let endpointUrl = `rediss://${resourceEndpoint}`;
console.log('Using Redis endpoint:', endpointUrl);

try {

    function getClient() {

        if (!resourceEndpoint) throw new Error('AZURE_MANAGED_REDIS_HOST_NAME must be set');

        const credential = new DefaultAzureCredential();

        const provider = EntraIdCredentialsProviderFactory.createForDefaultAzureCredential({
            credential,
            scopes: REDIS_SCOPE_DEFAULT,
            options: {},
            tokenManagerConfig: {
                expirationRefreshRatio: 0.8
            }
        });

        const client = createClient({
            url: endpointUrl,
            credentialsProvider: provider,
            socket: {
                reconnectStrategy:() => new Error('Failure to connect'),
                timeout: 15000
            }
        });

        client.on('error', (err) => console.error('Redis client error:', err));

        return client;
    }
    client = getClient();

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
