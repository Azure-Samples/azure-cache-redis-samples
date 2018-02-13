package benchmark;

import cluster.JedisClusterHelper;
import org.apache.commons.lang.RandomStringUtils;
import org.apache.log4j.Logger;
import pool.JedisHelper;
import redis.clients.jedis.JedisCluster;
import redis.clients.jedis.exceptions.JedisConnectionException;

import java.util.Random;

public class JedisClusterBenchmarkTest implements BenchmarkTest {
    private final static Logger logger = Logger.getLogger(JedisClusterBenchmarkTest.class);
    private final JedisCluster jedisCluster;
    private final BenchmarkArgs args;
    private final Random random = new Random();
    private static final String DEFAULT_STRING = "foo";

    public JedisClusterBenchmarkTest(BenchmarkArgs args) {
        this.args = args;
        this.jedisCluster = JedisClusterHelper.getCluster(args.configFilePath);
        logger.info(JedisClusterHelper.getClusterConfig(jedisCluster));
    }

    @Override
    public void runOnce() throws ConnectionException {
        try {
            if (random.nextInt(10) <= 3) {
                jedisCluster.set(getString(), getString());
            } else {
                jedisCluster.get(getString());
            }
        } catch (JedisConnectionException e){
            throw new ConnectionException(e);
        }
    }

    @Override
    public void logUsage() {
        JedisClusterHelper.getClusterUsage(jedisCluster);
    }

    private String getString(){
        if(!args.random){
            return DEFAULT_STRING;
        } else {
            return RandomStringUtils.random(args.dataSizeInBytes);
        }
    }
}
