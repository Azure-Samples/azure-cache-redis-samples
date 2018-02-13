package benchmark;

import cluster.LettuceClusterHelper;
import com.lambdaworks.redis.RedisCommandTimeoutException;
import com.lambdaworks.redis.RedisConnectionException;
import com.lambdaworks.redis.RedisException;
import com.lambdaworks.redis.cluster.api.StatefulRedisClusterConnection;
import org.apache.commons.lang.RandomStringUtils;
import org.apache.commons.pool2.impl.GenericObjectPool;
import org.apache.log4j.Logger;

import java.io.IOException;
import java.util.Random;

public class LettuceClusterBenchmarkTest implements BenchmarkTest {
    private final static Logger logger = Logger.getLogger(LettuceClusterBenchmarkTest.class);
    private final GenericObjectPool<StatefulRedisClusterConnection<String, String>> clusterPool;
    private final BenchmarkArgs args;
    private final Random random = new Random();
    private static final String DEFAULT_STRING = "foo";

    public LettuceClusterBenchmarkTest(BenchmarkArgs args) {
        this.args = args;
        this.clusterPool = LettuceClusterHelper.getCluster(args.configFilePath);
    }

    @Override
    public void runOnce() throws ConnectionException {
        try (StatefulRedisClusterConnection<String, String> connection = clusterPool.borrowObject()){
            if (random.nextInt(10) <= 3) {
                connection.sync().set(getString(), getString());
            } else {
                connection.sync().get(getString());
            }
        }catch (RedisCommandTimeoutException e) {
            logger.warn("command timeout");
        }
        catch (IOException | RedisException e){
            logger.error(e.getMessage());
            throw new ConnectionException(e);
        } catch (Exception e){
            logger.error("Exception :" + e.getMessage());
        }
    }

    @Override
    public void logUsage() {
        LettuceClusterHelper.getPoolUsage(clusterPool);
    }

    private String getString(){
        if(!args.random){
            return DEFAULT_STRING;
        } else {
            return RandomStringUtils.random(args.dataSizeInBytes);
        }
    }
}
