import redis

# change {cacheName} into your own cache name
# host = '{cacheName}.redis.cache.windows.net'
# change into your password
# password = 'password'
# Azure redis default port is 6380 for SSL, 6379 for Non-ssl
port = 6380

pool = redis.ConnectionPool(host=host, password=password, port=port, connection_class=redis.SSLConnection)

try:
    r = redis.StrictRedis(connection_pool=pool)
    r.set('foo', 'bar')
    print(r.get('foo'))
except redis.exceptions.ConnectionError as error:
    print('Failed to connect to redis server - {0}.'.format(str(error)))
    print('Pool usage: Max - {0}, Used - {1}, Available - {2}'.format(pool.max_connections, pool.in_use_connections, pool.available_connections))

