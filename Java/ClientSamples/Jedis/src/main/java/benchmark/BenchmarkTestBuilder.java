package benchmark;

public class BenchmarkTestBuilder {
    private final BenchmarkArgs args;

    private BenchmarkTestBuilder(BenchmarkArgs args) {
        this.args = args;
    }

    public static BenchmarkTestBuilder builder(BenchmarkArgs args){
        return new BenchmarkTestBuilder(args);
    }

    public BenchmarkTest build(){
        if(args.library.equalsIgnoreCase(Library.JEDIS.toString()) && args.isCluster){
            return new JedisClusterBenchmarkTest(args);
        } else if(args.library.equalsIgnoreCase(Library.LETTUCE.toString()) && args.isCluster){
            return new LettuceClusterBenchmarkTest(args);
        }

        throw new UnsupportedOperationException("Benchmark test type not supported");
    }
}
