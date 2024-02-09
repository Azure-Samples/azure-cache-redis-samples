---
page_type: sample
languages:
- java
name: 'Quickstart: Use Azure Cache for Redis in Java with Redisson Redis client and JCache'
description: Learn how to incorporate Azure Cache for Redis into a Java app using the Redisson Redis client and JCache.
products:
- azure
- azure-cache-redis
---
# Quickstart: Use Azure Cache for Redis in Java with Redisson Redis client and JCache
In this quickstart, you will integrate Azure Cache for Redis into a Java app using the [Redisson](https://redisson.org/) client library for Redis and JCP standard JCache API to have access to a secure, dedicated cache that is accessible from any application within Azure.

## Prerequisites
- Azure subscription - [create one for free](https://azure.microsoft.com/free/)
- [Apache Maven](https://maven.apache.org/download.cgi)
- [Create an open-source Redis cache](https://learn.microsoft.com/azure/azure-cache-for-redis/quickstart-create-redis) or [Create a Redis Enterprise cache](https://learn.microsoft.com/azure/azure-cache-for-redis/quickstart-create-redis-enterprise)

## Set up the working environment
Code in this sample will load settings from environment variables to determine which Redis cache to connect to, and how to authenticate the connection. 

### Provide the Redis cache host name
1. Find your cache's host name in its 'Overview' in the Azure portal. 
    - For Enterprise tier caches use the 'Endpoint' value. It should end with `.cache.azure.net:10000`, including the port number.
    - For other tier caches use the 'Host name' value ending with `.cache.windows.net`, and append the port number `:6380`.
1. Depending on your OS, open a command prompt or terminal window and use the appropriate command to set a `REDIS_HOST_NAME` environment variable:
    ```CMD
    set REDIS_HOST_NAME=<YOUR_HOST_NAME>
    ```
    ```bash
    export REDIS_HOST_NAME=<YOUR_HOST_NAME>
    ```

### Authentication Option 1: Access Key
1. Find your cache's primary or secondary access key in the Azure portal under 'Settings' > 'Access Keys' or 'Settings' > 'Authentication' > 'Access keys'. 
1. Use a command like these to set the `REDIS_ACCESS_KEY` environment variable:
    ```CMD
    set REDIS_ACCESS_KEY=<YOUR_ACCESS_KEY>
    ```
    ```bash
    export REDIS_ACCESS_KEY=<YOUR_ACCESS_KEY>
    ```

### Authentication Option 2: Microsoft Entra ID
NOTE: Entra ID authentication is not currently supported on Enterprise tier caches. 

1. Configure your cache and grant access to an Entra identity according to the instructions in [Use Microsoft Entra ID for cache authentication](https://learn.microsoft.com/azure/azure-cache-for-redis/cache-azure-active-directory-for-authentication). 
1. This sample uses `DefaultAzureCredential` to determine which Entra identity to use for authentication. You'll need to log in with the correct identity, or configure environment variables as described in [Azure authentication with Java and Azure Identity](https://learn.microsoft.com/en-us/azure/developer/java/sdk/identity).
1. Find the identity's user name in the Azure portal on your cache's 'Settings' > 'Authentication' > 'Microsoft Entra Authentication' view. 
1. Use a command like these to set the `REDIS_USER_NAME` environment variable:
    ```CMD
    set REDIS_USER_NAME=<REDIS_USER_NAME>
    ```
    ```bash
    export REDIS_USER_NAME=<REDIS_USER_NAME>
    ```

## Run the sample
1. [Download the sample code to your development PC](/README.md#get-the-samples)
1. Change directories to the folder containing this sample
1. Execute the following Maven command to build and run the app:
    ```CMD
    mvn compile exec:java -D exec.mainClass=example.demo.App
    ```
