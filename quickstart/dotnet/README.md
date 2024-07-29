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
- [Azure CLI](https://learn.microsoft.com/cli/azure/install-azure-cli)

## Set up the working environment

### 1. Set up local credential for using Entra ID
This sample uses Microsoft Entra ID to authenticate connections to an Azure Cache for Redis resource.
The following line of code in ```Redistest/RedisConnection.cs``` obtains the [default credential](https://learn.microsoft.com/dotnet/api/azure.identity.defaultazurecredential?view=azure-dotnet) from your local machine or an Azure resource as the identity for authentication and authorization.

```csharp
var configurationOptions = await ConfigurationOptions.Parse($"{_redisHostName}:6380").ConfigureForAzureWithTokenCredentialAsync(new DefaultAzureCredential());
```

One of the common way for signing into to your Azure account is to use the Azure CLI. Bring up the Command Prompt. Run

```cli
az login
```

For other methods of sign into Azure with Azure CLI, such as using a Service Principal, see [Sign into Azure with Azure CLI](https://learn.microsoft.com/cli/azure/authenticate-azure-cli)

### 2. Point to an Azure Cache for Redis instance from local configuration

Edit the *App.config* file and add the following contents:

```xml
<appSettings>
    <add key="RedisHostName" value="<cache-name>.redis.cache.windows.net"/>
</appSettings>
```

### 3. Add the permissions to allow the Entra ID to connect to the Azure Cache for Redis instance
Follow instruction at [Use Microsoft Entra ID for cache authentication](https://learn.microsoft.com/azure/azure-cache-for-redis/cache-azure-active-directory-for-authentication)


## Run the sample

[Download the sample code to your development PC.](/README.md#get-the-samples)

In Visual Studio, click **File** > **Open** > **Project/Solution**.

Navigate to the folder containing this sample, and select the solution file contained within it.

If the *CacheSecrets.config* file is not located at *C:\AppSecrets\CacheSecrets.config*, open your *App.config* file and update the `appSettings` `file` attribute to the correct path.

Press **Ctrl+F5** to build and run the console app.

## References

* [Quickstart article on the documentation site](https://docs.microsoft.com/azure/azure-cache-for-redis/cache-dotnet-how-to-use-azure-redis-cache)