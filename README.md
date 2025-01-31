---
page_type: sample
languages:
- aspx-csharp
- csharp
- go
- java
- javascript
- nodejs
- python
- rust
- php
name: Azure Cache for Redis samples
description: Learn how to use Azure Cache for Redis to have access to a secure, dedicated cache that is accessible from any application within Azure.
products:
- azure
- aspnet
- aspnet-core
- dotnet
- azure-cache-redis
---

# Samples repository for Azure Cache for Redis

This repository contains samples that demonstrate best practices and general recommendations to communicate with Azure Redis Cache Service caches from various Redis client frameworks. To find out more about the Azure Cache for Redis service, please visit the [documentation site](https://docs.microsoft.com/azure/azure-cache-for-redis).

## Get the samples

- The easiest way to use these samples without using Git is to download the current version as a [ZIP file](https://github.com/Azure-Samples/azure-cache-redis/archive/main.zip).

  - On Windows, before you unzip the archive, right-click it, select **Properties**, and then select **Unblock**.
  - Be sure to unzip the entire archive and not just individual samples.

- Alternatively, clone this sample repository using a Git client.

## Build and run the samples

Please see the description of each individual sample for instructions on how to build and run it.

### Azure Cache for Redis quickstarts

The following quickstarts demonstrate how to incorporate Azure Cache for Redis into an app.
If you want to create one of the quickstart apps from scratch, please follow the corresponding article on the [documentation site](https://docs.microsoft.com/azure/azure-cache-for-redis).

| Quickstart                                                                            | Platform | Description                                                                                                                                                                    |
|---------------------------------------------------------------------------------------| -------- |--------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|
| [Quickstart ASP.NET for Windows](/quickstart/aspnet)                                  | Windows | Learn how to use an ASP.NET web application to connect to Azure Cache for Redis to store and retrieve data from the cache.                                                     |
| [Quickstart ASP.NET Core](/quickstart/aspnet-core)                                    | Windows, Linux, macOS | Learn how to use an ASP.NET Core web application to connect to Azure Cache for Redis to store and retrieve data from the cache.                                                |
| [Quickstart C# .NET for Windows](/quickstart/dotnet)                                  | Windows | Learn how to incorporate Azure Cache for Redis into a C# .NET Framework console app using the StackExchange.Redis Redis client.                                                |
| [Quickstart C# .NET Core](/quickstart/dotnet-core)                                    | Windows, Linux, macOS | Learn how to incorporate Azure Cache for Redis into a C# .NET Core console app using the StackExchange.Redis Redis client.                                                     |
| [Quickstart Java](/quickstart/java)                                                   | Windows, Linux, macOS | Learn how to incorporate Azure Cache for Redis into a Java app using the Jedis Redis client.                                                                                   |
| [Quickstart Java Redisson](/quickstart/java-redisson)                                 | Windows, Linux, macOS | Learn how to incorporate Azure Cache for Redis into a Java app using the Redisson Redis client.                                                                                |
| [Quickstart Java Redisson JCache](/quickstart/java-redisson-jcache)                   | Windows, Linux, macOS | Learn how to incorporate Azure Cache for Redis into a Java app using the [JSR-107 JCache standard](https://jcp.org/en/jsr/detail?id=107) support in the Redisson Redis client. |
| [Quickstart Node.js](/quickstart/nodejs)                                              | Node.js | Learn how to incorporate Azure Cache for Redis into a Node.js app.                                                                                                             |
| [Quickstart Python](/quickstart/python)                                               | Windows, Linux, macOS | Learn how to incorporate Azure Cache for Redis into a Python app.                                                                                                              |
| [Quickstart Rust](https://github.com/Azure-Samples/azure-redis-cache-rust-quickstart) | Windows, Linux, macOS | Learn how to incorporate Azure Cache for Redis into a Rust app.                                                                                                                |
| [Quickstart Go](https://github.com/Azure-Samples/azure-redis-cache-go-quickstart)     | Windows, Linux, macOS | Learn how to incorporate Azure Cache for Redis into a Go app.                                                                                                                  |
| [Quickstart PHP](/quickstart/php)                                                     | Windows, Linux, macOS | Learn how to incorporate Azure Cache for Redis into a PHP app.                                                                                                                 |

### Samples

| Sample | Platform | Description |
| ------ | -------- | ----------- |
| [Basic, cluster, and pool connections in Java console app with Jedis and Lettuce](/samples/java/ClientSamples/) | Windows, Linux, macOS | Learn the general approaches for using Jedis's `JedisCluster` and `JedisPool` and Lettuce's `RedisClient` and `ConnectionPoolSupport`. |

## Resources

- [Azure Cache for Redis documentation](https://docs.microsoft.com/azure/azure-cache-for-redis)
