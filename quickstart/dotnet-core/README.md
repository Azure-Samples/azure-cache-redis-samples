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
- [Azure CLI](https://learn.microsoft.com/en-us/cli/azure/install-azure-cli)

## Run the sample

[Download the sample code to your development PC.](/README.md#get-the-samples)

### 1. Set up local credential for using Entra ID
This sample uses Microsoft Entra ID for connecting to an Azure Cache for Redis instance.
The following line of code in *ContosoTeamStats/RedisConnection.cs* obtains the default credential from your local machine or an Azure resource as the identity for authentication and authorization.

```csharp
var configurationOptions = await ConfigurationOptions.Parse($"{_redisHostName}:6380").ConfigureForAzureWithTokenCredentialAsync(new DefaultAzureCredential());
```

One of the common way for signing into to your Azure account is to use the Azure CLI. Bring up the Command Prompt. Run

```cli
az login
```

For other methods of sign into Azure with Azure CLI, such as using a Service Principal, see [Sign into Azure with Azure CLI](https://learn.microsoft.com/cli/azure/authenticate-azure-cli)

### 2. Add the permissions to allow the Entra ID to connect to the Azure Cache for Redis instance
Follow instruction at [Use Microsoft Entra ID for cache authentication](https://learn.microsoft.com/azure/azure-cache-for-redis/cache-azure-active-directory-for-authentication)

### 3. In your command window, change directories to the folder containing this sample.

### 4. Execute the following command to restore the packages:

```
dotnet restore
```

### 5. Execute the following command to store a new secret named *CacheConnection*, after replacing the placeholders (including angle brackets) for your cache name and primary access key:

```
dotnet user-secrets set RedisHostName <Your_Redis_Host_Name. i.e. myrediscache.redis.cache.windows.net>
```

### 6. Execute the following command in your command window to build the app:

```
dotnet build
```

Then run the app with the following command:

```
dotnet run
```

## References

* [Quickstart article on the documentation site](https://docs.microsoft.com/azure/azure-cache-for-redis/cache-dotnet-core-quickstart)
