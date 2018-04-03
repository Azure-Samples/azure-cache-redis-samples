
# pip install redis

import redis

# change {cacheName} into your own cache name
# host = '{cacheName}.redis.cache.windows.net'
# change into your password
# password = 'password'
# Azure redis default port is 6380 for ssl, 6379 for Non-ssl
port = 6380

try:
    r = redis.StrictRedis(host=host, password=password, port=port, ssl=True)
    r.set('foo', 'bar')
    print(r.get('foo'))
except redis.exceptions.ConnectionError as error:
    print('Failed to connect to redis server - {0}.'.format(str(error)))

