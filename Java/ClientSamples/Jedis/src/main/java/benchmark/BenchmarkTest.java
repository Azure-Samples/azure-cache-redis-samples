package benchmark;

public interface BenchmarkTest {
    void runOnce() throws ConnectionException;
    void logUsage();

}
