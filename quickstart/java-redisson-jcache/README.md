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

### Option 1: Authentication with an Access Key

Depending on your operating system, add environment variables for your cache's **Host name** and **Primary access key**. Open a command prompt, or a terminal window, and set up the following values. Ensure the value for `REDIS_CACHE_HOSTNAME` is the fully qualified domain name. This value typically ends with `.redis.cache.windows.net`.

```CMD
set REDIS_CACHE_HOSTNAME=<YOUR_HOST_NAME>
set REDIS_ACCESS_KEY=<YOUR_PRIMARY_ACCESS_KEY>
set REDIS_CACHE_PORT=<YOUR_REDIS_PORT>
```

```bash
export REDIS_CACHE_HOSTNAME=<YOUR_HOST_NAME>
export REDIS_ACCESS_KEY=<YOUR_PRIMARY_ACCESS_KEY>
export REDIS_CACHE_PORT=<YOUR_REDIS_PORT>
```

Replace the placeholders with the following values:

- `<YOUR_HOST_NAME>`: The DNS host name. In the *Settings* section of the Azure Cache for Redis resource in the Azure portal, select *Properties*. Select the copy icon to the right of the *Host name* field.
- `<YOUR_PRIMARY_ACCESS_KEY>`: The primary access key. In the *Settings* section of the Azure Cache for Redis resource in the Azure portal, select *Authentication*. Select the copy icon to the right of the *Primary* field.
- `<YOUR_REDIS_PORT>`: The port number, obtained from the *Properties* section of your Azure Cache for Redis resource in the Azure portal.
  - The default port number is `6380` for **Azure Cache for Redis**.
  - The default port number is `10000` for **Azure Managed Redis**.

### Option 2: Authentication with Microsoft Entra ID

- [Use Microsoft Entra ID for cache authentication](https://learn.microsoft.com/azure/azure-cache-for-redis/cache-azure-active-directory-for-authentication)

Depending on your operating system, add environment variables for your cache's **Host name** and **Username**. Open a command prompt, or a terminal window, and set up the following values:

```CMD
set REDIS_CACHE_HOSTNAME=<YOUR_HOST_NAME>
set REDIS_USER_NAME=<REDIS_USER_NAME>
set REDIS_CACHE_PORT=<YOUR_REDIS_PORT>
```

```bash
export REDIS_CACHE_HOSTNAME=<YOUR_HOST_NAME>
export REDIS_USER_NAME=<REDIS_USER_NAME>
export REDIS_CACHE_PORT=<YOUR_REDIS_PORT>
```

Replace the placeholders with the following values:

- `<YOUR_HOST_NAME>`: The DNS host name, obtained from the *Properties* section of your Azure Cache for Redis resource in the Azure portal.
- `<REDIS_USER_NAME>`: Object ID of your managed identity or service principal.
  - You can find this value in the "(PREVIEW) Data Access Configuration" view on your cache resource in the Azure Portal. It appears on the "Redis Users" tab, in the "Username" column.
- `<YOUR_REDIS_PORT>`: The port number, obtained from the *Properties* section of your Azure Cache for Redis resource in the Azure portal.
  - The default port number is `6380` for **Azure Cache for Redis**.
  - The default port number is `10000` for **Azure Managed Redis**.


It is also possible to use `DefaultAzureCredential` to provide the identity to be used with the Redis connection. For more details, see [Azure authentication with Java and Azure Identity](https://learn.microsoft.com/en-us/azure/developer/java/sdk/identity).


## Run the sample

[Download the sample code to your development PC.](/README.md#get-the-samples)

Change directories to the folder containing this sample.

Execute the following Maven command to build and run the app:

```CMD
mvn compile exec:java -D exec.mainClass=example.demo.App
```
