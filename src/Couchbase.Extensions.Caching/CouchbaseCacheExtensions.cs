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
            IOptions<CouchbaseCacheOptions> options;
            var bucket = GetBucket(cache, out options);

            bucket.Insert(key, value, options.Value.LifeSpan ?? TimeSpan.FromDays(180));
        }

        public static Task SetAsync<T>(this IDistributedCache cache, string key, T value)
        {
            IOptions<CouchbaseCacheOptions> options;
            var bucket = GetBucket(cache, out options);

            return bucket.InsertAsync(key, value, options.Value.LifeSpan ?? TimeSpan.FromDays(180));
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
    }
}
