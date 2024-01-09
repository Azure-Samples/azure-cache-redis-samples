---
page_type: sample
languages:
- php
name: 'Quickstart: Use Azure Cache for Redis in PHP'
description: Learn how to interact with Azure Cache for Redis in your PHP app.
products:
- azure
- azure-cache-redis
---

# Quickstart: Use Azure Cache for Redis in PHP

This sample helps to understand how to interact with Azure Cache for Redis in your PHP application. See the
[accompanying article](https://docs.microsoft.com/azure/azure-cache-for-redis/cache-python-get-started) on the documentation site for details, including best practices and how to create the sample code from scratch.

## Prerequisites

- Azure subscription - [create one for free](https://azure.microsoft.com/free/).
- PHP >= 7.2 - download [here](https://www.php.net/downloads.php).

## Install Predis

Predis is an open-source client supported by [Redis](https://redis.io/) organization.

### Composer

Predis client available on [packagist.org](https://packagist.org/packages/predis/predis) and distributed with Composer dependency manager. You can download Composer [here](https://getcomposer.org/download/).

To get Predis with Composer, simply run:

```shell
composer require predis/predis
```

### GitHub

Predis source-code also available on GitHub [repository](https://github.com/predis/predis). Feel free to check README for more information about Predis.

## Run the sample

Before you run a sample script, you need to set up a Redis host and password. This values represented by following environment variables:

```shell
export REDIS_HOST=your_redis_host
export REDIS_PASSWORD=your_redis_password
```

After the previous steps you can run sample script from the repository root directory like this:
```shell
php quickstart/php/test.php
```

## References

* [Quickstart article on the documentation site](https://docs.microsoft.com/azure/azure-cache-for-redis/cache-python-get-started)
* [Predis documentation](https://github.com/predis/predis?tab=readme-ov-file#predis)