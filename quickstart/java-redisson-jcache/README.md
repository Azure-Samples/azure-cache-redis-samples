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

Depending on your operating system, add environment variables for your cache's **Host name** and **USERNAME**. Open a command prompt, or a terminal window, and set up the following values:

```CMD
set REDIS_CACHE_HOSTNAME=<YOUR_HOST_NAME>.redis.cache.windows.net
set USERNAME=<USERNAME>
```

```bash
export REDIS_CACHE_HOSTNAME=<YOUR_HOST_NAME>.redis.cache.windows.net
export USERNAME=<USERNAME>
```

Replace the placeholders with the following values:

- `<YOUR_HOST_NAME>`: The DNS host name, obtained from the *Properties* section of your Azure Cache for Redis resource in the Azure portal.
- `<USERNAME>`: Object ID of your managed identity or service principal.

## Run the sample

[Download the sample code to your development PC.](/README.md#get-the-samples)

Change directories to the folder containing this sample.

Execute the following Maven command to build and run the app:

```CMD
mvn compile exec:java -D exec.mainClass=example.demo.App
```
