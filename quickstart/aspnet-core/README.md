---
page_type: sample
languages:
- csharp
name: 'Quickstart: Use Azure Cache for Redis with an ASP.NET Core web app'
description: Learn how to use an ASP.NET Core web application to connect to Azure Cache for Redis to store and retrieve data from the cache.
products:
- azure
- aspnet-core
- azure-cache-redis
---
# Quickstart: Use Azure Cache for Redis with an ASP.NET Core web app

This sample shows you how to use an ASP.NET Core web application to connect to Azure Cache for Redis to store and retrieve data from the cache. See the [accompanying article](https://docs.microsoft.com/azure/azure-cache-for-redis/cache-web-app-aspnet-core-howto) on the documentation site for details, including best practices and how to create the sample code from scratch.

## Prerequisites

- Azure subscription - [create one for free](https://azure.microsoft.com/free/)
- Azure Cache for Redis cache - [create one](https://docs.microsoft.com/azure/azure-cache-for-redis/quickstart-create-redis)
- [.NET Core SDK](https://dotnet.microsoft.com/download)

## Run the sample

[Download the sample code to your development PC.](/README.md#get-the-samples)

In your command window, change directories to the folder containing this sample.

Execute the following command to restore the packages:

```
dotnet restore
```

Execute the following command to store a new secret named *CacheConnection*, after replacing the placeholders (including angle brackets) for your cache name and primary access key:

```
dotnet user-secrets set CacheConnection "<cache name>.redis.cache.windows.net,abortConnect=false,ssl=true,allowAdmin=true,password=<primary-access-key>"
```

Execute the following command in your command window to build the app:

```
dotnet build
```

Run the app with the following command:

```
dotnet run
```

Browse to `https://localhost:5001` in your web browser.

Select **Azure Cache for Redis Test** in the navigation bar of the web page to test cache access.

## References

* [Quickstart article on the documentation site](https://docs.microsoft.com/azure/azure-cache-for-redis/cache-web-app-aspnet-core-howto)
