using System;

namespace DotNetConsoleAppUsingStackExchangeRedisClient.Commons.Pool
{
    class PoolBuilder<T> where T : class

    {
        private int initialSize;
        private int maxSize;
        private Func<T> creator;
        private Action<T> destroyer;
        private IPooledObjectFactory<T> objectFactory;
        private IPooledObjectValidator<T> validator;

        private PoolBuilder()
        {
        }

        public static PoolBuilder<T> NewBuilder()
        {
            return new PoolBuilder<T>();
        }

        public PoolBuilder<T> InitialSize(int initialSize)
        {
            if (initialSize < 0)
            {
                throw new ArgumentException("The initial size value is invalid.");
            }
            this.initialSize = initialSize;
            return this;
        }

        public PoolBuilder<T> MaxSize(int maxSize)
        {
            if (maxSize < -1)
            {
                throw new ArgumentException("The max size value is invalid.");
            }
            if (maxSize < initialSize)
            {
                throw new ArgumentException("The maximum size of the pool shall not be smaller than the initial size.");
            }
            this.maxSize = maxSize;
            return this;
        }

        public PoolBuilder<T> WithFactory(IPooledObjectFactory<T> factory)
        {
            objectFactory = factory;
            return this;
        }

        public PoolBuilder<T> WithValidator(IPooledObjectValidator<T> validator)
        {
            this.validator = validator;
            return this;
        }

        public IObjectPool<T> NewPool()
        {
            if (objectFactory == null)
            {
                throw new InvalidOperationException(
                    "The object pool cannot be instantiated as the object creation method is not defined.");
            }

            if (maxSize > 0 && maxSize < initialSize)
            {
                throw new ArgumentException("The maximum size of the pool shall not be smaller than its initial size.");
            }

            var newPool = new GenericObjectPool<T>(initialSize, maxSize, objectFactory, validator);

            return newPool;
        }
    }

}