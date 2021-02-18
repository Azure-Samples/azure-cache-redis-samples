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
- [Visual Studio 2019](https://www.visualstudio.com/downloads/) with the **ASP.NET and web development** and **Azure development** workloads.

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