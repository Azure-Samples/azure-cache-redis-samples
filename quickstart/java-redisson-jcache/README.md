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

In this quickstart, you incorporate Azure Cache for Redis into a Java app using the [Redisson](https://redisson.org/) Redis client and JCP standard JCache API to have access to a secure, dedicated cache that is accessible from any application within Azure.

## Prerequisites

- Azure subscription - [create one for free](https://azure.microsoft.com/free/)
- [Apache Maven](https://maven.apache.org/download.cgi)
- [Use Microsoft Entra ID for cache authentication](https://learn.microsoft.com/azure/azure-cache-for-redis/cache-azure-active-directory-for-authentication)


## Set up the working environment

### Authentication with Redis Key

Depending on your operating system, add environment variables for your cache's **Host name** and **Primary access key**. Open a command prompt, or a terminal window, and set up the following values:

```CMD
set REDIS_CACHE_HOSTNAME=<YOUR_HOST_NAME>.redis.cache.windows.net
set REDIS_CACHE_KEY=<YOUR_PRIMARY_ACCESS_KEY>
set AUTH_TYPE=RedisKey
```

```bash
export REDIS_CACHE_HOSTNAME=<YOUR_HOST_NAME>.redis.cache.windows.net
export REDIS_CACHE_KEY=<YOUR_PRIMARY_ACCESS_KEY>
export AUTH_TYPE=RedisKey
```

Replace the placeholders with the following values:

- `<YOUR_HOST_NAME>`: The DNS host name, obtained from the *Properties* section of your Azure Cache for Redis resource in the Azure portal.
- `<YOUR_PRIMARY_ACCESS_KEY>`: The primary access key, obtained from the *Access keys* section of your Azure Cache for Redis resource in the Azure portal.

### Authentication with Microsoft Entra ID

Depending on your operating system, add environment variables for your cache's **Host name** and **USER_NAME**. Open a command prompt, or a terminal window, and set up the following values:

```CMD
set REDIS_CACHE_HOSTNAME=<YOUR_HOST_NAME>.redis.cache.windows.net
set USER_NAME=<USER_NAME>
set AUTH_TYPE=MicrosoftEntraID
```

```bash
export REDIS_CACHE_HOSTNAME=<YOUR_HOST_NAME>.redis.cache.windows.net
export USER_NAME=<USER_NAME>
export AUTH_TYPE=MicrosoftEntraID
```

Replace the placeholders with the following values:

- `<YOUR_HOST_NAME>`: The DNS host name, obtained from the *Properties* section of your Azure Cache for Redis resource in the Azure portal.
- `<USER_NAME>`: Object ID of your managed identity or service principal.


## Run the sample

[Download the sample code to your development PC.](/README.md#get-the-samples)

Change directories to the folder containing this sample.

Execute the following Maven command to build and run the app:

```CMD
mvn compile exec:java -D exec.mainClass=example.demo.App
```
