import redis
import logging
import psutil
import time

logging.basicConfig(level=logging.DEBUG)
logger = logging.getLogger(__name__)

# change {cacheName} into your own cache name
# host = '{cacheName}.redis.cache.windows.net'
# change into your password
# password = 'password'
pool = redis.ConnectionPool(host=host, password=password, port=6380, connection_class=redis.SSLConnection)


def main():
    simple_set_get()


def simple_set_get():
    try:
        r = redis.StrictRedis(connection_pool=pool)
        r.set('foo', 'bar')
        print(r.get('foo'))
    except redis.exceptions.ConnectionError as error:
        logger.warning('Failed to connect to redis server - {0}.'.format(str(error)), exc_info=False)
        log_system_metrics()
        log_pool_info()


def log_system_metrics():
    logger.info('System metrics: CPU - {0}%, Memory - {1}'.format(psutil.cpu_percent(), metrics2str(psutil.virtual_memory())))


def log_pool_info():
    logger.info('Pool usage: Max - {0}, Used - {1}, Available - {2}'.format(pool.max_connections, len(pool._in_use_connections), len(pool._available_connections)))


def bytes2human(n):
    # >>> bytes2human(10000)
    # '9.8K'
    # >>> bytes2human(100001221)
    # '95.4M'
    symbols = ('K', 'M', 'G', 'T', 'P', 'E', 'Z', 'Y')
    prefix = {}
    for i, s in enumerate(symbols):
        prefix[s] = 1 << (i + 1) * 10
    for s in reversed(symbols):
        if n >= prefix[s]:
            value = float(n) / prefix[s]
            return '%.1f%s' % (value, s)
    return "%sB" % n


def metrics2str(nt):
    metrics = []
    for name in nt._fields:
        value = getattr(nt, name)
        if name != 'percent':
            value = bytes2human(value)
        metrics.append('%s : %s' % (name.capitalize(), value))

    return ", ".join(metrics)


def test_recover_from_disconnect():
    while True:
        simple_set_get()
        time.sleep(10)


if __name__ == '__main__':
    main()
