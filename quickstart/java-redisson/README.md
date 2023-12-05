---
page_type: sample
languages:
- java
name: 'Quickstart: Use Azure Cache for Redis in Java with Redisson Redis client'
description: Learn how to incorporate Azure Cache for Redis into a Java app using the Redisson Redis client.
products:
- azure
- azure-cache-redis
---
# Quickstart: Use Azure Cache for Redis in Java with Redisson Redis client

This sample show you how to incorporate Azure Cache for Redis into a Java app using the [Redisson](https://redisson.org/) Redis client.

## Prerequisites

- Azure subscription - [create one for free](https://azure.microsoft.com/free/)
- Azure Cache for Redis cache - [create one](https://docs.microsoft.com/azure/azure-cache-for-redis/quickstart-create-redis)
- [Apache Maven](https://maven.apache.org/download.cgi)

## Set up the working environment

Depending on your operating system, add environment variables for your cache's **Host name** and **Primary access key**. Open a command prompt, or a terminal window, and set up the following values:

```CMD
set REDIS_CACHE_HOSTNAME=<YOUR_HOST_NAME>.redis.cache.windows.net
set REDIS_CACHE_KEY=<YOUR_PRIMARY_ACCESS_KEY>
```

```bash
export REDIS_CACHE_HOSTNAME=<YOUR_HOST_NAME>.redis.cache.windows.net
export REDIS_CACHE_KEY=<YOUR_PRIMARY_ACCESS_KEY>
```

Replace the placeholders with the following values:

- `<YOUR_HOST_NAME>`: The DNS host name, obtained from the *Properties* section of your Azure Cache for Redis resource in the Azure portal.
- `<YOUR_PRIMARY_ACCESS_KEY>`: The primary access key, obtained from the *Access keys* section of your Azure Cache for Redis resource in the Azure portal.

## Run the sample

[Download the sample code to your development PC.](/README.md#get-the-samples)

Change directories to the folder containing this sample.

Execute the following Maven command to build and run the app:

```CMD
mvn compile exec:java -D exec.mainClass=example.demo.App
```
