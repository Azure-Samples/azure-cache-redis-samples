using System;

namespace DotNetConsoleAppUsingStackExchangeRedisClient.Commons.Pool
{
    /// <summary>
    /// The interface defines the operations for an object pool.
    /// </summary>
    /// <typeparam name="T">The type of the pooled object.</typeparam>
    public interface IObjectPool<T> : IDisposable where T : class
    {
        /// <summary>
        /// Acquires an object from the pool. 
        /// If there is any object available in the pool, one of the idle objects is returned
        /// from the pool. 
        /// If there is not any idle object in the pool, and the pool size is less than the max value,
        /// the object pool attempts to create an object and returns the object if successful. Pool size is increased.
        /// If there is not any idle object in the pool and the pool size has reached the max value, it waits infinitively.
        /// </summary>
        /// <returns>The object</returns>
        /// <exception cref="InvalidOperationException">
        /// If this pool is using an <see cref="IPooledObjectValidator{T}"/>, configured to validate on acquire and the pool already had acquired and
        /// validated invalid objects. This effectively prevents a potential infinite blocking call.
        /// If for any reason all objects become invalid (e.g there is a network issue and all connections to external resource could not be established).
        /// </exception>
        T Acquire();

        /// <summary>
        /// Acquires an object from the pool. If there is no object available in the pool, it waits until timeout. If the time out is less than or equal to 0, 
        /// it waits for the object infinitively.
        /// </summary>
        /// <param name="timeout">The timeout in milli-seconds.</param>
        /// <param name="obj">The object from the pool.</param>
        /// <returns>True if any idle object is retrieved from the pool, otherwise false.</returns>
        bool TryAcquire(int timeout, out T obj);

        /// <summary>
        /// Returns an <paramref name="obj"/> to the pool. The object becomes idle when it's returned to the pool.
        /// </summary>
        /// <param name="obj">The returned object</param>
        void Return(T obj);

        /// <summary>
        /// The number of the idle objects.
        /// </summary>
        int IdleCount { get; }

        /// <summary>
        /// The number of the active objects.
        /// </summary>
        int ActiveCount { get; }

        /// <summary>
        /// The maximum number of all the objects.
        /// </summary>
        int Capacity { get; }

        /// <summary>
        /// The initial size of the pool. If the pool grows, its size eventually reaches the <see cref="Capacity"/>;
        /// </summary>
        int InitialSize { get; }

        /// <summary>
        /// Invalidates an object from the pool. The pool must destroy and forget the given instance.
        /// </summary>
        /// <remarks>
        /// By contract, <paramref name="obj"/> must have been obtained using <see cref="Acquire"/> or a related method as defined in an implementation or sub-interface.
        /// 
        ///This method should be used when an object that has been borrowed is determined (due to an exception or other problem) to be invalid.
        /// </remarks>
        /// <param name="obj">an acquired instance to be disposed.</param>
        void Invalidate(T obj);
    }
}
