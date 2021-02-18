---
page_type: sample
languages:
- javascript
- nodejs
name: 'Quickstart: Use Azure Cache for Redis in Node.js'
description: Learn how to incorporate Azure Cache for Redis into a Node.js app.
products:
- azure
- azure-cache-redis
---
# Quickstart: Use Azure Cache for Redis in Node.js

This sample show you how to incorporate Azure Cache for Redis into a Node.js app. See the [accompanying article](https://docs.microsoft.com/azure/azure-cache-for-redis/cache-nodejs-get-started) on the documentation site for details, including best practices and how to create the sample code from scratch.

## Prerequisites

- Azure subscription - [create one for free](https://azure.microsoft.com/free/)
- Azure Cache for Redis cache - [create one](https://docs.microsoft.com/azure/azure-cache-for-redis/quickstart-create-redis)
- [node_redis](https://github.com/mranney/node_redis), which you can install with the command `npm install redis`.

For examples of using other Node.js clients, see the individual documentation for the Node.js clients listed at [Node.js Redis clients](https://redis.io/clients#nodejs).

## Set up the working environment

Add environment variables for your cache's **HOST NAME** and **Primary** access key. You will use these variables from your code instead of including the sensitive information directly in your code.

```
set REDISCACHEHOSTNAME=contosoCache.redis.cache.windows.net
set REDISCACHEKEY=XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX
```

## Run the sample

[Download the sample code to your development PC.](/README.md#get-the-samples)

Change directories to the folder containing this sample.

Run the script with Node.js.

```
node redistest.js
```

## References

* [Quickstart article on the documentation site](https://docs.microsoft.com/azure/azure-cache-for-redis/cache-nodejs-get-started)
