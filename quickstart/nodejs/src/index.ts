import { DefaultAzureCredential } from '@azure/identity';
import { EntraIdCredentialsProviderFactory, REDIS_SCOPE_DEFAULT } from '@redis/entraid';
import { createCluster } from '@redis/client';
import * as net from 'node:net';

const resourceEndpoint = process.env.AZURE_MANAGED_REDIS_HOST_NAME!;
if (!resourceEndpoint) {
    console.error('AZURE_MANAGED_REDIS_HOST_NAME is not set. It should look like: `cache-name.region-name.redis.azure.net`. Find the endpoint in the Azure portal. Do not include the port.');
    process.exit(1);
}
// Azure Managed Redis default port
const resourcePort = 10000

let client;
let endpointUrl = `rediss://${resourceEndpoint}:${resourcePort}`;
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
                    tls: true
                }

            }, 
            nodeAddressMap(address) {
                const [hostName, port] = address.split(":");

                // On Azure Managed Redis the nodes have the same host, only the port is dynamic
                // so if the address is an IP and we are using TLS we will use the redisHost instead to allow certificate
                // validation, otherwise we use the address provided by the discovered cluster topology
                const host =
                    net.isIP(hostName) !== 0
                    ? resourceEndpoint
                    : hostName;

                return {
                    host,
                    port: Number(port),
                };
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