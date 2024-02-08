using System.Buffers;
using System.Text.Json;

namespace Microsoft.Extensions.Caching.Distributed;

/// <summary>
/// Provides a simple convenience wrapper around <see cref="IDistributedCache"/>; note that this implementation
/// does not attempt to avoid problems with multiple callers all invoking the "get" method at once when
/// data becomes evicted for cache ("stampeding"), or any other concerns such as returning stale data while
/// refresh occurs in the background - these are future considerations for the cache implementation.
/// </summary>
/// <remarks>The overloads taking <c>TState</c> are useful when used with <c>static</c> get methods, to avoid
/// "capture" overheads, but in most everyday scenarios, it may be more convenient to use the simpler stateless
/// version.</remarks>
public static class DistributedCacheExtensions
{
    /// <summary>
    /// Gets a value from cache, with a caller-supplied <paramref name="getMethod"/> (async, stateless) that is used if the value is not yet available
    /// </summary>
    public static ValueTask<T> GetAsync<T>(this IDistributedCache cache, string key, Func<CancellationToken, ValueTask<T>> getMethod,
        DistributedCacheEntryOptions? options = null, CancellationToken cancellation = default)
        => GetAsyncShared<int, T>(cache, key, state: 0, getMethod, options, cancellation); // use dummy state

    /// <summary>
    /// Gets a value from cache, with a caller-supplied <paramref name="getMethod"/> (sync, stateless) that is used if the value is not yet available
    /// </summary>
    public static ValueTask<T> GetAsync<T>(this IDistributedCache cache, string key, Func<T> getMethod,
        DistributedCacheEntryOptions? options = null, CancellationToken cancellation = default)
        => GetAsyncShared<int, T>(cache, key, state: 0, getMethod, options, cancellation); // use dummy state

    /// <summary>
    /// Gets a value from cache, with a caller-supplied <paramref name="getMethod"/> (async, stateful) that is used if the value is not yet available
    /// </summary>
    public static ValueTask<T> GetAsync<TState, T>(this IDistributedCache cache, string key, TState state, Func<TState, CancellationToken, ValueTask<T>> getMethod,
        DistributedCacheEntryOptions? options = null, CancellationToken cancellation = default)
        => GetAsyncShared<TState, T>(cache, key, state, getMethod, options, cancellation);

    /// <summary>
    /// Gets a value from cache, with a caller-supplied <paramref name="getMethod"/> (sync, stateful) that is used if the value is not yet available
    /// </summary>
    public static ValueTask<T> GetAsync<TState, T>(this IDistributedCache cache, string key, TState state, Func<TState, T> getMethod,
        DistributedCacheEntryOptions? options = null, CancellationToken cancellation = default)
        => GetAsyncShared<TState, T>(cache, key, state, getMethod, options, cancellation);

    /// <summary>
    /// Provides a common implementation for the public-facing API, to avoid duplication
    /// </summary>
    private static ValueTask<T> GetAsyncShared<TState, T>(IDistributedCache cache, string key, TState state, Delegate getMethod,
        DistributedCacheEntryOptions? options, CancellationToken cancellation)
    {
        var pending = cache.GetAsync(key, cancellation);
        if (!pending.IsCompletedSuccessfully)
        {
            // async-result was not available immediately; go full-async
            return Awaited(cache, key, pending, state, getMethod, options, cancellation);
        }

        // GetAwaiter().GetResult() here is *not* "sync-over-async" - we've already
        // validated that this data was available synchronously, and we're eliding
        // the state machine overheads in the (hopefully high-hit-rate) success case
        var bytes = pending.GetAwaiter().GetResult();
        if (bytes is null)
        {
            // async-result was available but data is missing; go async for everything else
            return Awaited(cache, key, null, state, getMethod, options, cancellation);
        }

        // data was available synchronously; deserialize
        return new(Deserialize<T>(bytes));

        static async ValueTask<T> Awaited(
            IDistributedCache cache, // the underlying cache
            string key, // the key on the cache
            Task<byte[]?>? pending, // incomplete "get bytes" operation, if any
            TState state, // state possibly used by the get-method
            Delegate getMethod, // the get-method supplied by the caller
            DistributedCacheEntryOptions? options, // cache expiration, etc
            CancellationToken cancellation)
        {
            byte[]? bytes;
            if (pending is not null)
            {
                bytes = await pending;
                if (bytes is not null)
                {   // data was available asynchronously
                    return Deserialize<T>(bytes);
                }
            }
            var result = getMethod switch
            {
                // we expect 4 use-cases; sync/async, with/without state
                Func<TState, CancellationToken, ValueTask<T>> get => await get(state, cancellation),
                Func<TState, T> get => get(state),
                Func<CancellationToken, ValueTask<T>> get => await get(cancellation),
                Func<T> get => get(),
                _ => throw new ArgumentException(nameof(getMethod)),
            };
            bytes = Serialize<T>(result);
            if (options is null)
            {   // not recommended; cache expiration should be considered
                // important, usually
                await cache.SetAsync(key, bytes, cancellation);
            }
            else
            {
                await cache.SetAsync(key, bytes, options, cancellation);
            }
            return result;
        }
    }

    // The current cache API is byte[]-based, but a wide range of
    // serializer choices are possible; here we use the inbuilt
    // System.Text.Json.JsonSerializer, which is a fair compromise
    // between being easy to configure and use on general types,
    // versus raw performance. Alternative (non-byte[]) storage
    // mechanisms are under consideration.
    //
    // If it is likely that you will change serializers during
    // upgrades (and you are using out-of-process storage), then
    // you may wish to use a sentinel prefix before the payload,
    // to allow you to safely switch between serializers;
    // alternatively, you may choose to use a key-prefix so that
    // the old data is simply not found (and expires naturally)
    private static T Deserialize<T>(byte[] bytes)
    {
        var payload = new Utf8JsonReader(bytes);
        return JsonSerializer.Deserialize<T>(ref payload)!;
    }

    private static byte[] Serialize<T>(T value)
    {
        var buffer = new ArrayBufferWriter<byte>();
        using var writer = new Utf8JsonWriter(buffer);
        JsonSerializer.Serialize(writer, value);
        return buffer.WrittenSpan.ToArray();
    }
}
