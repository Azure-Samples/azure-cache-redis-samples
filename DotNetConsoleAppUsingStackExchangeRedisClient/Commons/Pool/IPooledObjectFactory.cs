namespace DotNetConsoleAppUsingStackExchangeRedisClient.Commons.Pool
{
    /// <summary>
    /// The abstract factory is called by the object pool to create and destroy the objects which are pooled.
    /// </summary>
    /// <typeparam name="T">The object type.</typeparam>
    public interface IPooledObjectFactory<T>
    {
        /// <summary>
        /// Creates an object to be pooled.
        /// </summary>
        /// <returns>The object created by the factory</returns>
        T Create();

        /// <summary>
        /// Destroys the object in the pool. When overriding this method, exceptions shall be caught, as it will break the 
        /// process and remaining pooled object cannot be destroyed.
        /// </summary>
        /// <param name="obj">The object to be destroyed.</param>
        void Destroy(T obj);

        // Heal the broken object into healthy state
        void Heal(T obj);
    }
}
