package com.microsoft.azure.redis.jedis.benchmark;

import java.util.concurrent.*;
import java.util.concurrent.locks.Condition;
import java.util.concurrent.locks.ReentrantLock;

public class PausableThreadPoolExecutor extends ThreadPoolExecutor
{

    private boolean isPaused;
    private ReentrantLock pauseLock = new ReentrantLock();
    private Condition unpaused = pauseLock.newCondition();

    public PausableThreadPoolExecutor(int i, int i1, long l, TimeUnit timeUnit, BlockingQueue<Runnable> runnables)
    {
        super(i, i1, l, timeUnit, runnables);
    }

    public PausableThreadPoolExecutor(int i, int i1, long l, TimeUnit timeUnit, BlockingQueue<Runnable> runnables, ThreadFactory threadFactory)
    {
        super(i, i1, l, timeUnit, runnables, threadFactory);
    }

    public PausableThreadPoolExecutor(int i, int i1, long l, TimeUnit timeUnit, BlockingQueue<Runnable> runnables, RejectedExecutionHandler rejectedExecutionHandler)
    {
        super(i, i1, l, timeUnit, runnables, rejectedExecutionHandler);
    }

    public PausableThreadPoolExecutor(int i, int i1, long l, TimeUnit timeUnit, BlockingQueue<Runnable> runnables, ThreadFactory threadFactory, RejectedExecutionHandler rejectedExecutionHandler)
    {
        super(i, i1, l, timeUnit, runnables, threadFactory, rejectedExecutionHandler);
    }


    protected void beforeExecute(Thread t, Runnable r)
    {
        super.beforeExecute(t, r);
        pauseLock.lock();
        try
        {
            while (isPaused) unpaused.await();
        }
        catch (InterruptedException ie)
        {
            t.interrupt();
        }
        finally
        {
            pauseLock.unlock();
        }
    }

    public void pause()
    {
        pauseLock.lock();
        try
        {
            isPaused = true;
        }
        finally
        {
            pauseLock.unlock();
        }
    }

    public void resume()
    {
        pauseLock.lock();
        try
        {
            isPaused = false;
            unpaused.signalAll();
        }
        finally
        {
            pauseLock.unlock();
        }
    }
}
