---
page_type: sample
languages:
- csharp
name: 'Quickstart: Use Azure Cache for Redis in .NET Framework'
description: Learn how to incorporate Azure Cache for Redis into a C# .NET Framework console app using the StackExchange.Redis Redis client.
products:
- azure
- dotnet
- azure-cache-redis
---
# Quickstart: Use Azure Cache for Redis in .NET Framework

This sample shows you how to incorporate Azure Cache for Redis into a C# .NET Framework console app using the [StackExchange.Redis](https://github.com/StackExchange/StackExchange.Redis) Redis client. See the [accompanying article](https://docs.microsoft.com/azure/azure-cache-for-redis/cache-dotnet-how-to-use-azure-redis-cache) on the documentation site for details, including best practices and how to create the sample code from scratch.

## Prerequisites

- Azure subscription - [create one for free](https://azure.microsoft.com/free/)
- Azure Cache for Redis cache - [create one](https://docs.microsoft.com/azure/azure-cache-for-redis/quickstart-create-redis)
- [Visual Studio 2019](https://www.visualstudio.com/downloads/)
- [.NET Framework 4 or higher](https://www.microsoft.com/net/download/dotnet-framework-runtime), which is required by the StackExchange.Redis client.

## Set up the working environment

Create a file on your computer named *CacheSecrets.config* and place it in a location where it won't be checked in with the source code of your sample application. For this quickstart, the *CacheSecrets.config* file is located here, *C:\AppSecrets\CacheSecrets.config*.

Edit the *CacheSecrets.config* file and add the following contents:

```xml
<appSettings>
    <add key="CacheConnection" value="<cache-name>.redis.cache.windows.net,abortConnect=false,ssl=true,allowAdmin=true,password=<access-key>"/>
</appSettings>
```

Replace `<cache-name>` with your cache host name.

Replace `<access-key>` with the primary key for your cache.

## Run the sample

[Download the sample code to your development PC.](/README.md#get-the-samples)

In Visual Studio, click **File** > **Open** > **Project/Solution**.

Navigate to the folder containing this sample, and select the solution file contained within it.

If the *CacheSecrets.config* file is not located at *C:\AppSecrets\CacheSecrets.config*, open your *App.config* file and update the `appSettings` `file` attribute to the correct path.

Press **Ctrl+F5** to build and run the console app.

## References

* [Quickstart article on the documentation site](https://docs.microsoft.com/azure/azure-cache-for-redis/cache-dotnet-how-to-use-azure-redis-cache)