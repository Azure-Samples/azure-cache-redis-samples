---
page_type: sample
languages:
- aspx-csharp
- csharp
name: 'Quickstart: Use Azure Cache for Redis with an ASP.NET web app'
description: Learn how to use an ASP.NET web application to connect to Azure Cache for Redis to store and retrieve data from the cache.
products:
- azure
- aspnet
- azure-cache-redis
---
# Quickstart: Use Azure Cache for Redis with an ASP.NET web app

This sample shows you how to use an ASP.NET web application to connect to Azure Cache for Redis to store and retrieve data from the cache. See the [accompanying article](https://docs.microsoft.com/azure/azure-cache-for-redis/cache-web-app-howto) on the documentation site for details, including best practices and how to create the sample code from scratch.

## Prerequisites

- Azure subscription - [create one for free](https://azure.microsoft.com/free/dotnet)
- Azure Cache for Redis cache - [create one](https://docs.microsoft.com/azure/azure-cache-for-redis/quickstart-create-redis)
- [Visual Studio](https://www.visualstudio.com/downloads/) with the **ASP.NET and web development** and **Azure development** workloads.
- [Azure CLI](https://learn.microsoft.com/en-us/cli/azure/install-azure-cli)

## Set up the working environment

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

## Run the sample locally

[Download the sample code to your development PC.](/README.md#get-the-samples)

In Visual Studio, click **File** > **Open** > **Project/Solution**.

Navigate to the folder containing this sample, and select the solution file contained within it.

Open **Tools** > **NuGet Package Manager** > **Package Manager Console** and install the bootstrap NuGet package:

```pwsh
Install-Package bootstrap -Version 3.4.1
```

If the *CacheSecrets.config* file is not located at *C:\AppSecrets\CacheSecrets.config*, open your *web.config* file and update the `appSettings` `file` attribute to the correct path.

In Visual Studio, select **Debug** > **Start Debugging** to build and start the app locally for testing and debugging.

In the browser, select **Azure Cache for Redis Test** on the navigation bar.

## Publish and run in Azure

See the [accompanying article](https://docs.microsoft.com/azure/azure-cache-for-redis/cache-web-app-howto) on the documentation site for details about how to publish and run the app in Azure.

## References

* [Quickstart article on the documentation site](https://docs.microsoft.com/azure/azure-cache-for-redis/cache-web-app-howto)