using System;
using System.Threading.Tasks;
using Couchbase.Core;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Internal;
using Microsoft.Extensions.Options;

namespace Couchbase.Extensions.Caching
{
    public class CouchbaseCache : IDistributedCache
    {
        internal static readonly TimeSpan InfiniteLifetime = TimeSpan.Zero;

        internal IBucket Bucket { get; }

        internal IOptions<CouchbaseCacheOptions> Options { get; }

        private ISystemClock _clock = new SystemClock();

        private class CacheItem
        {
            public TimeSpan CreationTime { get; set; }
        }

        public CouchbaseCache(IOptions<CouchbaseCacheOptions> options) :
            this(ClusterHelper.GetBucket(options.Value.BucketName), options)
        {
        }

        public CouchbaseCache(IBucket bucket, IOptions<CouchbaseCacheOptions> options)
        {
            Bucket = bucket;
            Options = options;
        }

        public byte[] Get(string key)
        {
            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }
            return Bucket.Get<byte[]>(key).Value;
        }

        public async Task<byte[]> GetAsync(string key)
        {
            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }
            return (await Bucket.GetAsync<byte[]>(key)).Value;
        }

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
            Bucket.Insert(key, value, lifeTime);
        }

        public Task SetAsync(string key, byte[] value, DistributedCacheEntryOptions options = null)
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
            return Bucket.InsertAsync(key, value, lifeTime);
        }

        public void Refresh(string key)
        {
            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }

            var lifeTime = GetLifetime();
            Bucket.Touch(key, lifeTime);
        }

        public async Task RefreshAsync(string key)
        {
            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }

            var lifeTime = GetLifetime();
            await Bucket.TouchAsync(key, lifeTime);
        }

        public void Remove(string key)
        {
            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }

            Bucket.Remove(key);
        }

        public async Task RemoveAsync(string key)
        {
            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }

            await Bucket.RemoveAsync(key);
        }

        async Task<byte[]> IDistributedCache.GetAsync(string key)
        {
            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }

            return (await Bucket.GetAsync<byte[]>(key)).Value;
        }

        internal TimeSpan GetLifetime(DistributedCacheEntryOptions options = null)
        {
            if (options?.SlidingExpiration != null)
            {
                return options.SlidingExpiration.Value;
            }

            return Options.Value.LifeSpan ?? InfiniteLifetime;
        }
    }
}
