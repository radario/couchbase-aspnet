using System;
using System.Threading.Tasks;
using Couchbase.Core;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Options;

namespace Couchbase.Extensions.Caching
{
    public static class CouchbaseCacheExtensions
    {
        public static void Set<T>(this IDistributedCache cache, string key, T value)
        {
            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }
            if (value == null)
            {
                throw new ArgumentNullException(nameof(value));
            }

            IOptions<CouchbaseCacheOptions> options;
            var bucket = GetBucket(cache, out options);

            bucket.Insert(key, value, GetLifetime(cache));
        }

        public static Task SetAsync<T>(this IDistributedCache cache, string key, T value)
        {
            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }
            if (value == null)
            {
                throw new ArgumentNullException(nameof(value));
            }

            IOptions<CouchbaseCacheOptions> options;
            var bucket = GetBucket(cache, out options);

            return bucket.InsertAsync(key, value, GetLifetime(cache));
        }

        public static T Get<T>(this IDistributedCache cache, string key)
        {
            IOptions<CouchbaseCacheOptions> options;
            var bucket = GetBucket(cache, out options);

            return bucket.Get<T>(key).Value;
        }

        public static async Task<T> GetAsync<T>(this IDistributedCache cache, string key)
        {
            IOptions<CouchbaseCacheOptions> options;
            var bucket = GetBucket(cache, out options);

            return (await bucket.GetAsync<T>(key)).Value;
        }

        public static T Get<T>(this IDistributedCache cache, string key, DistributedCacheEntryOptions itemOptions)
        {
            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }

            IOptions<CouchbaseCacheOptions> cacheOptions;
            var bucket = GetBucket(cache, out cacheOptions);

            return bucket.GetAndTouch<T>(key, GetLifetime(cache, itemOptions)).Value;
        }

        public static async Task<T> GetAsync<T>(this IDistributedCache cache, string key, DistributedCacheEntryOptions itemOptions)
        {
            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }

            IOptions<CouchbaseCacheOptions> cacheOptions;
            var bucket = GetBucket(cache, out cacheOptions);

            return (await bucket.GetAndTouchAsync<T>(key, GetLifetime(cache, itemOptions))).Value;
        }

        public static byte[] Get(this IDistributedCache cache, string key, DistributedCacheEntryOptions itemOptions)
        {
            return Get<byte[]>(cache, key, itemOptions);
        }

        public static async Task<byte[]> GetAsync(this IDistributedCache cache, string key, DistributedCacheEntryOptions itemOptions)
        {
            return (await GetAsync<byte[]>(cache, key, itemOptions));
        }

        static IBucket GetBucket(IDistributedCache cache, out IOptions<CouchbaseCacheOptions> options)
        {
            var couchbaseCache = cache as CouchbaseCache;
            if (couchbaseCache == null)
            {
                throw new NotSupportedException("The IDistributedCache must be a CouchbaseCache.");
            }
            options = couchbaseCache.Options;
            return couchbaseCache.Bucket;
        }

        internal static TimeSpan GetLifetime(IDistributedCache cache, DistributedCacheEntryOptions itemOptions = null)
        {
            var couchbaseCache = cache as CouchbaseCache;
            if (couchbaseCache == null)
            {
                throw new NotSupportedException("The IDistributedCache must be a CouchbaseCache.");
            }

            if (itemOptions?.SlidingExpiration != null)
            {
                return itemOptions.SlidingExpiration.Value;
            }

            return couchbaseCache.Options.Value.LifeSpan ?? CouchbaseCache.InfiniteLifetime;
        }
    }
}
