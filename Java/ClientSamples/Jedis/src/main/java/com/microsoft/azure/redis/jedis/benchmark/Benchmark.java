package com.microsoft.azure.redis.jedis.benchmark;

import com.beust.jcommander.JCommander;
import org.apache.commons.lang.RandomStringUtils;
import org.apache.commons.pool2.impl.GenericObjectPoolConfig;
import redis.clients.jedis.Jedis;
import redis.clients.jedis.JedisPool;

import java.util.ArrayList;
import java.util.Collections;
import java.util.List;
import java.util.concurrent.CountDownLatch;
import java.util.concurrent.LinkedBlockingQueue;
import java.util.concurrent.TimeUnit;

public class Benchmark {
    private final LinkedBlockingQueue<Long> setRunTimes = new LinkedBlockingQueue<>();
    private PausableThreadPoolExecutor executor;
    private final JedisPool pool;
    private final String data;
    private final CountDownLatch shutDownLatch;
    private long totalNanoRunTime;
    private int numberOfOperations;

    public Benchmark(CommandLineArgs commandLineArgs)
    {
        this.numberOfOperations = commandLineArgs.numberOfOperations;
        this.executor = new PausableThreadPoolExecutor(commandLineArgs.numberOfThreads, commandLineArgs.numberOfThreads, 5, TimeUnit.SECONDS, new LinkedBlockingQueue<>());
        GenericObjectPoolConfig poolConfig = new GenericObjectPoolConfig();
        poolConfig.setTestOnBorrow(true);
        poolConfig.setTestOnReturn(true);
        poolConfig.setMaxTotal(commandLineArgs.numberOfConnections);
        this.pool = new JedisPool(poolConfig, commandLineArgs.host, commandLineArgs.port);
        this.data = RandomStringUtils.random(commandLineArgs.dataSizeInBytes);
        shutDownLatch = new CountDownLatch(this.numberOfOperations);
    }

    class SetTask implements Runnable
    {
        private CountDownLatch latch;

        SetTask(CountDownLatch latch)
        {
            this.latch = latch;
        }

        public void run()
        {
            long startTime = System.nanoTime();
            Jedis jedis = pool.getResource();
            jedis.set(RandomStringUtils.random(15), data);
            setRunTimes.offer(System.nanoTime() - startTime);
            jedis.close();
            latch.countDown();
        }
    }

    public void performBenchmark() throws InterruptedException
    {
        executor.pause();
        for (int i = 0; i < numberOfOperations ; i++)
        {
            executor.submit(new SetTask(shutDownLatch));
        }
        long startTime = System.nanoTime();
        executor.resume();
        executor.shutdown();
        shutDownLatch.await();
        totalNanoRunTime = System.nanoTime() - startTime;
    }

    public void printStats()
    {
        List<Long> points = new ArrayList<>();
        setRunTimes.drainTo(points);
        Collections.sort(points);
        long sum = 0;
        for (Long l : points)
        {
            sum += l;
        }
        System.out.println("Data size :" + data.length());
        System.out.println("Threads : " + executor.getMaximumPoolSize());
        System.out.println("Time Test Ran for (ms) : " + TimeUnit.NANOSECONDS.toMillis(totalNanoRunTime));
        System.out.println("Average : " + TimeUnit.NANOSECONDS.toMillis(sum / points.size()));
        System.out.println("50 % <=" + TimeUnit.NANOSECONDS.toMillis(points.get((points.size() / 2) - 1)));
        System.out.println("90 % <=" + TimeUnit.NANOSECONDS.toMillis(points.get((points.size() * 90 / 100) - 1)));
        System.out.println("95 % <=" + TimeUnit.NANOSECONDS.toMillis(points.get((points.size() * 95 / 100) - 1)));
        System.out.println("99 % <=" + TimeUnit.NANOSECONDS.toMillis(points.get((points.size() * 99 / 100) - 1)));
        System.out.println("99.9 % <=" + TimeUnit.NANOSECONDS.toMillis(points.get((points.size() * 999 / 1000) - 1)));
        System.out.println("100 % <=" + TimeUnit.NANOSECONDS.toMillis(points.get(points.size() - 1)));
        System.out.println((numberOfOperations * 1000 / TimeUnit.NANOSECONDS.toMillis(totalNanoRunTime)) + " Operations per second");
    }

    public static void main(String[] args) throws InterruptedException
    {
        CommandLineArgs commandLineArgs = new CommandLineArgs();
        JCommander.newBuilder()
                .addObject(commandLineArgs)
                .build()
                .parse(args);
        Benchmark benchmark = new Benchmark(commandLineArgs);
        benchmark.performBenchmark();
        benchmark.printStats();
    }
}
