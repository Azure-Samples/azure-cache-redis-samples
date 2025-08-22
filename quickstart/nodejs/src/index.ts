
import { useIdentityPlugin, DefaultAzureCredential } from '@azure/identity';
import { nativeBrokerPlugin } from "@azure/identity-broker";
import { EntraIdCredentialsProviderFactory, REDIS_SCOPE_DEFAULT } from '@redis/entraid';
import { createClient } from '@redis/client';

const resourceEndpoint = process.env.AZURE_MANAGED_REDIS_HOST_NAME!;
if (!resourceEndpoint) {
    console.error('AZURE_MANAGED_REDIS_HOST_NAME is not set. It should look like: rediss://YOUR-RESOURCE_NAME.redis.cache.windows.net:<YOUR-RESOURCE-PORT>. Find the endpoint in the Azure portal.');
}

useIdentityPlugin(nativeBrokerPlugin);

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
        url: resourceEndpoint,
        credentialsProvider: provider,
        socket: {
            reconnectStrategy: (retries) => Math.min(100 + retries * 50, 2000)
        }

    });

    client.on('error', (err) => console.error('Redis client error:', err));

    return client;
}

const client = getClient();

try {

    await client.connect();

    const pingResult = await client.ping();
    console.log('Ping result:', pingResult);

    const setResult = await client.set("Message", "Hello! The cache is working from Node.js!");
    console.log('Set result:', setResult);

    const getResult = await client.get("Message");
    console.log('Get result:', getResult);

} catch (err) {
    console.error('Error closing Redis client:', err);
} finally {
    await client.close();
}

