namespace DotNetConsoleAppUsingStackExchangeRedisClient.Commons.Pool
{
    /// <summary>
    /// Validator used to test if objects are still 'usable', as defined by the client's provided implementations, before acquiring or returning.
    /// </summary>
    /// <remarks>
    /// If a validation fails the object could be invalidated by the pool, by calling <see cref="IObjectPool{T}.Invalidate(T)"/>.
    /// </remarks>
    /// <typeparam name="T">The object type.</typeparam>
    public interface IPooledObjectValidator<in T>
    {
        /// <summary>
        /// Indicates if the pool must validate the object just before returning it to the client. If the validation fails the object must be discarded.
        /// </summary>
        bool ValidateOnAcquire { get; }

        /// <summary>
        /// Validates the given instance to check if it is still good to use. This method should never throw exceptions because of regular validation logic.
        /// </summary>
        /// <param name="obj">The instance to validate.</param>
        /// <returns>Return <see langword="true"/> if the object is still valid, <see langword="false"/> otherwise.</returns>
        bool Validate(T obj);
    }

}
