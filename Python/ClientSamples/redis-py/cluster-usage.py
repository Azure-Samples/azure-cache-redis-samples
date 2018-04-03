
# pip install redis-py-cluster

from rediscluster import StrictRedisCluster
from rediscluster.connection import ClusterConnectionPool, SSLClusterConnection

# change {cacheName} into your own cache name
# host = '{cacheName}.redis.cache.windows.net'
# change into your password
# password = 'password'
# Azure redis default port is 6380 for ssl, 6379 for Non-ssl
port = 6380

startup_nodes = [{"host": host, "port": port}]

# Skip full coverage check is required since Azure redis block all config command
pool = ClusterConnectionPool(startup_nodes=startup_nodes, password=password, connection_class=SSLClusterConnection, skip_full_coverage_check=True)

# Note: decode_responses must be set to True when used with python3
rc = StrictRedisCluster(connection_pool=pool, decode_responses=True)

rc.set("foo", "bar")

print(rc.get("foo"))
