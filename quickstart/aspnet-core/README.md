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

This sample shows you how to use an ASP.NET Core web application to connect to Azure Cache for Redis to store and retrieve data from the cache. Microsoft Entra ID is used to authenticate the connection to the Redis cache in Azure. See the [accompanying article](https://docs.microsoft.com/azure/azure-cache-for-redis/cache-web-app-aspnet-core-howto) on the documentation site for details, including best practices and how to create the sample code from scratch.

## Prerequisites

- Azure subscription. [Start free](https://azure.microsoft.com/free)
- .NET 8 or above. [Download](https://dotnet.microsoft.com/download/dotnet/8.0)
- Azure Developer CLI. [Install](https://learn.microsoft.com/azure/developer/azure-developer-cli/install-azd?tabs=winget-windows%2Cbrew-mac%2Cscript-linux&pivots=os-windows)
- (optional) Visual Studio. [Download](https://visualstudio.microsoft.com/)

## Run the sample in Azure

[Download the sample code to your development PC.](/README.md#get-the-samples)

In your command window, change directories to the folder containing this sample.

Execute the following command to restore the packages:

1. Login to Azure from your azd command

    ```
    azd auth login
    ```

2. Provision the Azure resources and deploy your web application

    ```
    azd up
    ```


Select **Azure Cache for Redis Test** in the navigation bar of the web page to test cache access.

3. Tear down the resources to stay cost-efficient

    ```
    azd down
    ```

## References

* [Quickstart article on the documentation site](https://docs.microsoft.com/azure/azure-cache-for-redis/cache-web-app-aspnet-core-howto)
