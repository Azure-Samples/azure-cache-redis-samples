---
page_type: sample
languages:
- csharp
name: 'Quickstart: Use Azure Cache for Redis in .NET Core'
description: Learn how to incorporate Azure Cache for Redis into a C# .NET Core console app using the StackExchange.Redis Redis client.
products:
- azure
- dotnet
- azure-cache-redis
---
# Quickstart: Use Azure Cache for Redis in .NET Core

This sample shows you how to incorporate Azure Cache for Redis into a C# .NET Core console app using the [StackExchange.Redis](https://github.com/StackExchange/StackExchange.Redis) Redis client. See the [accompanying article](https://docs.microsoft.com/azure/azure-cache-for-redis/cache-dotnet-core-quickstart) on the documentation site for details, including best practices and how to create the sample code from scratch.

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

Then run the app with the following command:

```
dotnet run
```

## References

* [Quickstart article on the documentation site](https://docs.microsoft.com/azure/azure-cache-for-redis/cache-dotnet-core-quickstart)
