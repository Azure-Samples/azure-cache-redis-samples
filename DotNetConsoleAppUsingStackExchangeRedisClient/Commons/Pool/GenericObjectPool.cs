using Commons.Collections.Map;
using Commons.Utils;
using System;
using System.Collections.Concurrent;
using System.Threading;

namespace DotNetConsoleAppUsingStackExchangeRedisClient.Commons.Pool
{
    /// <summary>
    /// The generic object pool. The pool is used when the object behavior is identical, for example, the connection pool.
    /// </summary>
    /// <typeparam name="T">The type of the pooled object.</typeparam>
    internal class GenericObjectPool<T> : IObjectPool<T> where T : class
    {
        private class NeverValidateValidator : IPooledObjectValidator<T>
        {
            public bool ValidateOnAcquire => false;

            public bool Validate(T obj) => false;
        }

        private readonly IPooledObjectFactory<T> factory;
        private readonly ConcurrentQueue<T> objQueue;
        private readonly ReaderWriterLockSlim locker;
        private readonly int initialSize;
        private readonly int maxSize;
        private int createdCount;
        private readonly AutoResetEvent objectReturned;
        private readonly ReferenceMap<T, bool> idleObjects;
        private readonly IPooledObjectValidator<T> validator;
        private readonly int acquiredInvalidLimit;

        /// <summary>
        /// Constructor with the intialSize and maxSize. <see cref="ArgumentException"/> is thrown when <paramref name="initialSize"/> is larger than <paramref name="maxSize"/>
        /// <see cref="IPooledObjectFactory{T}.Create"/> is called to create objects with the number of <paramref name="initialSize"/>. If it returns the object with null, it is not 
        /// added to the pool.
        /// </summary>
        /// <param name="initialSize">The initial object number of the pool.</param>
        /// <param name="maxSize">The max object number of the pool.</param>
        /// <param name="factory">The factory create and destroy the pooled object.</param>
        /// <param name="validator">Validator instance. Can be <see langword="null"/>.</param>
        public GenericObjectPool(int initialSize, int maxSize, IPooledObjectFactory<T> factory, IPooledObjectValidator<T> validator)
        {
            if (initialSize < 0)
            {
                throw new ArgumentException();
            }
            if (maxSize != -1 && maxSize < initialSize)
            {
                throw new ArgumentException();
            }
            if (factory == null)
            {
                throw new ArgumentNullException(nameof(factory));
            }
            this.initialSize = initialSize;
            this.maxSize = maxSize;
            this.factory = factory;
            this.validator = validator ?? new NeverValidateValidator();

            createdCount = 0;
            objectReturned = new AutoResetEvent(false);
            locker = new ReaderWriterLockSlim();
            objQueue = new ConcurrentQueue<T>();
            idleObjects = new ReferenceMap<T, bool>();
            for (var i = 0; i < initialSize; i++)
            {
                objQueue.Enqueue(factory.Create());
                createdCount++;
            }
        }

        /// <summary>
        /// Acquires an object from the pool. The method is blocked when there is no object available 
        /// in the pool. When there is no object, it waits until any object is returned to the pool.
        /// The method is recommended to use when the objects are sufficient.
        /// </summary>
        /// <returns>The pooled object</returns>
        public T Acquire()
        {
            AtomicInt32 acquiredInvalidCounter = AtomicInt32.From(0);

            T obj;
            var spin = new SpinWait();
            bool localValidateOnAcquire = validator.ValidateOnAcquire;

            bool acquired = false;
            TryAcquire(-1, out obj);
            do
            {
                spin.SpinOnce();
                if (null == obj)
                {
                    TryAcquire(-1, out obj);
                }
                else
                {
                    if (localValidateOnAcquire && !validator.Validate(obj))
                    {
                        acquiredInvalidCounter.Increment();
                        locker.EnterWriteLock();
                        try
                        {
                            DoInvalidateObject(obj);
                        }
                        finally
                        {
                            obj = null;
                            locker.ExitWriteLock();
                        }

                    }
                    else
                    {
                        acquired = true;
                    }
                }
            } while (!acquired);

            return obj;
        }

        /// <summary>
        /// Tries to acquire an object in the pool. The method waits until <paramref name="timeout"/> expires.
        /// When no object is available when waiting times out, it returns false.
        /// When <paramref name="timeout"/> is set to 0, it only tests whether there is any
        /// object in the pool and return immediately. If <paramref name="timeout"/> is set to 
        /// a negative number, it waits infinitely. But it does not guarantee that an object is 
        /// acquired. The return result still can be false. If you want to wait infinitely and expect 
        /// a object is returned anyway, use <see cref="Acquire"/> method.
        /// </summary>
        /// <param name="timeout">The time to wait for an object available in the pool.</param>
        /// <param name="obj">The object acquired.</param>
        /// <returns>True if an object is acquired, otherwise false.</returns>
        public bool TryAcquire(int timeout, out T obj)
        {
            var acquired = false;
            var localTimeout = timeout < 0 ? -1 : timeout;

            if (objQueue.TryDequeue(out obj))
            {
                acquired = true;
                locker.EnterWriteLock();
                try
                {
                    idleObjects[obj] = false;
                }
                finally
                {
                    locker.ExitWriteLock();
                }
            }
            else
            {
                locker.EnterWriteLock();
                try
                {
                    if (maxSize == -1 || createdCount < maxSize)
                    {
                        obj = factory.Create();
                        createdCount++;
                        acquired = true;
                        idleObjects[obj] = false;
                    }
                }
                finally
                {
                    locker.ExitWriteLock();
                }
                if (!acquired)
                {
                    if (objectReturned.WaitOne(localTimeout))
                    {
                        locker.EnterWriteLock();
                        try
                        {
                            acquired = objQueue.TryDequeue(out obj);
                            if (acquired)
                            {
                                idleObjects[obj] = false;
                            }
                        }
                        finally
                        {
                            locker.ExitWriteLock();
                        }
                    }
                }
            }

            return acquired;
        }

        /// <summary>
        /// Returns the object to the pool. If the object is already returned to the pool, <see cref="InvalidOperationException"/> is thrown.
        /// </summary>
        /// <param name="obj">The object to return.</param>
        public void Return(T obj)
        {
            locker.EnterUpgradeableReadLock();
            try
            {
                if (!idleObjects.ContainsKey(obj))
                {
                    throw new InvalidOperationException("The object is never created by the pool.");
                }
                if (idleObjects[obj])
                {
                    throw new InvalidOperationException("The object is already returned to the pool.");
                }
                locker.EnterWriteLock();
                try
                {
                    objQueue.Enqueue(obj);
                    idleObjects[obj] = true;
                }
                finally
                {
                    locker.ExitWriteLock();
                }
            }
            finally
            {
                locker.ExitUpgradeableReadLock();
            }
            objectReturned.Set();
        }

        /// <summary>
        /// The number of objects available in the pool.
        /// </summary>
        public int IdleCount
        {
            get
            {
                var count = 0;
                locker.EnterReadLock();
                try
                {
                    count = objQueue.Count;
                }
                finally
                {
                    locker.ExitReadLock();
                }
                return count;
            }
        }

        /// <summary>
        /// The number of objects which are actively used by pool consumers. 
        /// Those objects are already acquired.
        /// </summary>
        public int ActiveCount
        {
            get
            {
                var created = 0;
                var idleCount = 0;
                locker.EnterReadLock();
                try
                {
                    created = createdCount;
                    idleCount = objQueue.Count;
                }
                finally
                {
                    locker.ExitReadLock();
                }
                return created - idleCount;
            }
        }

        /// <summary>
        /// The size of the pool.
        /// </summary>
        public int Capacity
        {
            get
            {
                return maxSize;
            }
        }

        /// <summary>
        /// The initial size of the pool.
        /// </summary>
        public int InitialSize
        {
            get { return initialSize; }
        }

        /// <summary>
        /// Dispose the pool.
        /// </summary>
        public void Dispose()
        {
            locker.EnterWriteLock();
            try
            {
                foreach(var element in idleObjects)
                {
                    factory.Destroy(element.Key);
                }
                objectReturned.Dispose();
                idleObjects.Clear();
            }
            finally
            {
                locker.ExitWriteLock();
            }
        }

        public void Invalidate(T obj)
        {
            locker.EnterUpgradeableReadLock();
            try
            {
                if (!idleObjects.ContainsKey(obj))
                {
                    throw new InvalidOperationException("The object was never created by the pool or was previously invalidated.");
                }

                locker.EnterWriteLock();
                try
                {
                    DoInvalidateObject(obj);
                }
                finally
                {
                    locker.ExitWriteLock();
                }
            }
            finally
            {
                locker.ExitUpgradeableReadLock();
            }
            objectReturned.Set();
        }

        private void DoInvalidateObject(T obj)
        {
            if (idleObjects.ContainsKey(obj))
            {
                idleObjects.Remove(obj);
                createdCount -= 1;
            }

            int actualCount = objQueue.Count;
            T temp;
            for (int i = 0; i < actualCount; i++)
            {
                if (objQueue.TryDequeue(out temp) && !ReferenceEquals(temp, obj))
                {
                    objQueue.Enqueue(temp);
                }
            }

            factory.Destroy(obj);
        }
    }
}
