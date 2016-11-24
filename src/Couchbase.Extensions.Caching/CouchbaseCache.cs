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

        public CouchbaseCache(IOptions<CouchbaseCacheOptions> options) : this(ClusterHelper.GetBucket(options.Value.BucketName), options)
        {
        }

        public CouchbaseCache(IBucket bucket, IOptions<CouchbaseCacheOptions> options)
        {
            _bucket = bucket;
            _options = options;
        }

        public byte[] Get(string key)
        {
            return _bucket.Get<byte[]>(key).Value;
        }

        public Task GetAsync(string key)
        {
            return _bucket.GetAsync<byte[]>(key);
        }

        public void Set(string key, byte[] value, DistributedCacheEntryOptions options)
        {
            _bucket.Insert(key, value);
        }

        public Task SetAsync(string key, byte[] value, DistributedCacheEntryOptions options)
        {
            return _bucket.InsertAsync(key, value);
        }

        public void Refresh(string key)
        {
            _bucket.Touch(key, new TimeSpan(0, 0, 10));
        }

        public Task RefreshAsync(string key)
        {
            return _bucket.TouchAsync(key, new TimeSpan(0, 0, 10));
        }

        public void Remove(string key)
        {
            _bucket.RemoveAsync(key);
        }

        public Task RemoveAsync(string key)
        {
            return _bucket.RemoveAsync(key);
        }

        async Task<byte[]> IDistributedCache.GetAsync(string key)
        {
            return (await _bucket.GetAsync<byte[]>(key)).Value;
        }
    }
}
