package com.microsoft.azure.redis.jedis.benchmark;

import com.beust.jcommander.Parameter;
import com.beust.jcommander.internal.Lists;

import java.util.List;

// Follow the same format as redis offical benchmark. Please refer https://redis.io/topics/benchmarks
public class CommandLineArgs
{
    @Parameter
    public List<String> parameters = Lists.newArrayList();

    @Parameter(names = "-n", description = "Total number of requests (default 100000)")
    public Integer numberOfOperations = 100000;

    @Parameter(names = "-t", description = "Number of threads (default 1)")
    public Integer numberOfThreads = 1;

    @Parameter(names = "-c", description = "Number of parallel connections (default 50)")
    public Integer numberOfConnections = 50;

    @Parameter(names = "-h", description = "Server hostname (default 127.0.0.1)")
    public String host = "localhost";

    @Parameter(names = "-p", description = "Server port (default 6379)")
    public Integer port = 6379;

    @Parameter(names = "-d", description = "Data size of SET/GET value in bytes (default 2)")
    public Integer dataSizeInBytes = 2;
}
