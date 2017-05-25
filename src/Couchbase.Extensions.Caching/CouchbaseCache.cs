using System;
using System.Threading.Tasks;
using Couchbase.Core;
using Couchbase.Logging;
using Couchbase.Utils;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Internal;
using Microsoft.Extensions.Options;

namespace Couchbase.Extensions.Caching
{
    /// <summary>
    /// A <see cref="IDistributedCache"/> implementation for Couchbase Server.
    /// </summary>
    public class CouchbaseCache : IDistributedCache
    {
        internal readonly ILog Log = LogManager.GetLogger<CouchbaseCache>();

        internal static readonly TimeSpan InfiniteLifetime = TimeSpan.Zero;

        internal IBucket Bucket { get; }

        internal IOptions<CouchbaseCacheOptions> Options { get; }

        private ISystemClock _clock = new SystemClock();

        private class CacheItem<T>
        {
            public TimeSpan CreationTime { get; set; }

            public T Body;
        }

        /// <summary>
        /// Constructor for <see cref="CouchbaseCache"/> - if the <see cref="CouchbaseCacheOptions.Bucket"/> field is null,
        /// the bucket will attempted to be retrieved from <see cref="ClusterHelper"/>. If <see cref="ClusterHelper"/> has
        /// not been initialized, then an exception will be thrown.
        /// </summary>
        /// <param name="options"></param>
        public CouchbaseCache(IOptions<CouchbaseCacheOptions> options)
        {
            Options = options;
            Bucket = options.Value.Bucket ?? ClusterHelper.GetBucket(Options.Value.BucketName);
        }

        /// <summary>
        /// Gets a cache item by its key, returning null if the item does not exist within the Cache.
        /// </summary>
        /// <param name="key">The key to lookup the item.</param>
        /// <returns>The cache item if found, otherwise null.</returns>
        public byte[] Get(string key)
        {
            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }
            var result = Bucket.Get<byte[]>(key);
            HandleIfError(result);
            return result.Value;
        }

        /// <summary>
        /// Gets a cache item by its key asynchronously, returning null if the item does not exist within the Cache.
        /// </summary>
        /// <param name="key">The key to lookup the item.</param>
        /// <returns>The cache item if found, otherwise null.</returns>
        async Task<byte[]> IDistributedCache.GetAsync(string key)
        {
            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }

            var result = await Bucket.GetAsync<byte[]>(key).ContinueOnAnyContext();
            HandleIfError(result);
            return result.Value;
        }

        /// <summary>
        /// Gets a cache item by its key asynchronously, returning null if the item does not exist within the Cache.
        /// </summary>
        /// <param name="key">The key to lookup the item.</param>
        /// <returns>The cache item if found, otherwise null.</returns>
        public Task<byte[]> GetAsync(string key)
        {
            return ((IDistributedCache)this).GetAsync(key);
        }

        /// <summary>
        /// Sets a cache item using its key. If the key exists, it will not be updated.
        /// </summary>
        /// <param name="key">The key for the cache item.</param>
        /// <param name="value">An array of bytes representing the item.</param>
        /// <param name="options">The <see cref="DistributedCacheEntryOptions"/> for the item; note that only sliding expiration is currently supported.</param>
        public void Set(string key, byte[] value, DistributedCacheEntryOptions options = null)
        {
            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }
            if (value == null)
            {
                throw new ArgumentNullException(nameof(value));
            }

            var lifeTime = GetLifetime(options);
            var result = Bucket.Upsert(key, value, lifeTime);
            HandleIfError(result);
        }

        /// <summary>
        /// Sets a cache item using its key asynchronously. If the key exists, it will not be updated.
        /// </summary>
        /// <param name="key">The key for the cache item.</param>
        /// <param name="value">An array of bytes representing the item.</param>
        /// <param name="options">The <see cref="DistributedCacheEntryOptions"/> for the item; note that only sliding expiration is currently supported.</param>
        public async Task SetAsync(string key, byte[] value, DistributedCacheEntryOptions options = null)
        {
            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }
            if (value == null)
            {
                throw new ArgumentNullException(nameof(value));
            }

            var lifeTime = GetLifetime(options);
            var result = await Bucket.UpsertAsync(key, value, lifeTime);
            HandleIfError(result);
        }

        /// <summary>
        /// Refreshes or "touches" a key updating it's lifetime expiration.
        /// </summary>
        /// <param name="key">The key for the cache item.</param>
        public void Refresh(string key)
        {
            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }

            var lifeTime = GetLifetime();
            var result = Bucket.Touch(key, lifeTime);
            HandleIfError(result);
        }

        /// <summary>
        /// Refreshes or "touches" a key updating it's lifetime expiration asynchronously.
        /// </summary>
        /// <param name="key">The key for the cache item.</param>
        public async Task RefreshAsync(string key)
        {
            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }

            var lifeTime = GetLifetime();
            var result = await Bucket.TouchAsync(key, lifeTime);
            HandleIfError(result);
        }

        /// <summary>
        /// Removes an item from the cache by it's key.
        /// </summary>
        /// <param name="key">The key for the cache item.</param>
        public void Remove(string key)
        {
            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }

            var result = Bucket.Remove(key);
            HandleIfError(result);
        }

        /// <summary>
        /// Removes an item from the cache by it's key asynchonously.
        /// </summary>
        /// <param name="key">The key for the cache item.</param>
        public async Task RemoveAsync(string key)
        {
            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }

            var result = await Bucket.RemoveAsync(key);
            HandleIfError(result);
        }

        /// <summary>
        /// Gets the lifetime or expiration from the <see cref="DistributedCacheEntryOptions"/>. Only
        /// sliding expiration is currently supported. If <see cref="DistributedCacheEntryOptions.SlidingExpiration"/>
        /// if not set, then the <see cref="CouchbaseCacheOptions.LifeSpan"/> will be used. If it is empty then the
        /// default lifespan of zero (0) will be used which is infinite expiration.
        /// </summary>
        /// <param name="options"></param>
        /// <returns></returns>
        internal TimeSpan GetLifetime(DistributedCacheEntryOptions options = null)
        {
            if (options?.SlidingExpiration != null)
            {
                return options.SlidingExpiration.Value;
            }

            return Options.Value.LifeSpan ?? InfiniteLifetime;
        }

        /// <summary>
        /// Handles an error if the operation has failed.
        /// </summary>
        /// <param name="result">The result.</param>
        internal void HandleIfError(IOperationResult result)
        {
            if (!result.Success)
            {
                Log.Debug("Operation {0} failed: {1}", result.OpCode, result.Status);
                if (result.Exception != null)
                {
                    Log.Warn(result.Exception);
                    if (Options.Value.ThrowExceptions)
                    {
                        throw result.Exception;
                    }
                }
            }
        }
    }
}
