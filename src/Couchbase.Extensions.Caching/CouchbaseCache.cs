using System;
using System.Threading.Tasks;
using Couchbase.Core;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Options;

namespace Couchbase.Extensions.Caching
{
    public class CouchbaseCache : IDistributedCache
    {
        private readonly IBucket _bucket;
        private readonly IOptions<CouchbaseCacheOptions> _options;

        public CouchbaseCache(IOptions<CouchbaseCacheOptions> options) :
            this(ClusterHelper.GetBucket(options.Value.BucketName), options)
        {
        }

        public CouchbaseCache(IBucket bucket, IOptions<CouchbaseCacheOptions> options)
        {
            _bucket = bucket;
            _options = options;
        }

        public byte[] Get(string key)
        {
            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }
            return _bucket.Get<byte[]>(key).Value;
        }

        public async Task<byte[]> GetAsync(string key)
        {
            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }
            return (await _bucket.GetAsync<byte[]>(key)).Value;
        }

        public void Set(string key, byte[] value, DistributedCacheEntryOptions options)
        {
            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }
            _bucket.Insert(key, value);
        }

        public Task SetAsync(string key, byte[] value, DistributedCacheEntryOptions options)
        {
            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }
            return _bucket.InsertAsync(key, value);
        }

        public void Refresh(string key)
        {
            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }
            _bucket.Touch(key, _options.Value.LifeSpan ?? TimeSpan.FromDays(180));
        }

        public async Task RefreshAsync(string key)
        {
            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }
            await _bucket.TouchAsync(key, _options.Value.LifeSpan ?? TimeSpan.FromDays(180));
        }

        public void Remove(string key)
        {
            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }
            _bucket.Remove(key);
        }

        public async Task RemoveAsync(string key)
        {
            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }
            await _bucket.RemoveAsync(key);
        }

        async Task<byte[]> IDistributedCache.GetAsync(string key)
        {
            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }
            return (await _bucket.GetAsync<byte[]>(key)).Value;
        }
    }
}
