using System;
using System.Threading.Tasks;
using Couchbase.Core;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Options;

namespace Couchbase.Extensions.Caching
{
    public class CouchbaseCache : IDistributedCache
    {
        internal IBucket Bucket { get; }

        internal IOptions<CouchbaseCacheOptions> Options { get; }

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

        public void Set(string key, byte[] value, DistributedCacheEntryOptions options)
        {
            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }
            if (value == null)
            {
                throw new ArgumentNullException(nameof(value));
            }

            Bucket.Insert(key, value, Options.Value.LifeSpan ?? TimeSpan.FromDays(180));
        }

        public Task SetAsync(string key, byte[] value, DistributedCacheEntryOptions options)
        {
            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }
            if (value == null)
            {
                throw new ArgumentNullException(nameof(value));
            }

            return Bucket.InsertAsync(key, value, Options.Value.LifeSpan ?? TimeSpan.FromDays(180));
        }

        public void Refresh(string key)
        {
            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }

            Bucket.Touch(key, Options.Value.LifeSpan ?? TimeSpan.FromDays(180));
        }

        public async Task RefreshAsync(string key)
        {
            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }

            await Bucket.TouchAsync(key, Options.Value.LifeSpan ?? TimeSpan.FromDays(180));
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
    }
}
