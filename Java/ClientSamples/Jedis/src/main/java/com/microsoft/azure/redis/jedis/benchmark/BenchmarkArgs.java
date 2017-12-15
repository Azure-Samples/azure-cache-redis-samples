package com.microsoft.azure.redis.jedis.benchmark;

import com.beust.jcommander.Parameter;

// Follow the same format as redis official benchmark. Please refer https://redis.io/topics/benchmarks
public class BenchmarkArgs
{

    @Parameter(names = "-n", description = "Total number of tests (default 10)")
    public Integer numberOfTests = 10;

    @Parameter(names = "-r", description = "Use random key and value (default false)")
    public boolean random = false;

    @Parameter(names = "-o", description = "Max operations per second (default -1 no limit)")
    public int maxOperationPerSecond = -1;

    @Parameter(names = "-v", description = "Print result after every test (default false)")
    public boolean verbose = false;

    @Parameter(names = "-d", description = "Data size of SET/GET value in bytes (default 2)")
    public Integer dataSizeInBytes = 2;

    /* Unsupported
    @Parameter(names = "-t", description = "Number of threads (default 1)")
    public Integer numberOfThreads = 1;

    @Parameter(names = "-c", description = "Number of parallel connections (default 50)")
    public Integer numberOfConnections = 50;

    @Parameter(names = "-h", description = "Server hostname (default 127.0.0.1)")
    public String host = "localhost";

    @Parameter(names = "-p", description = "Server port (default 6379)")
    public Integer port = 6379;
    */
}
