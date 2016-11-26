using System;
using Couchbase.Configuration.Client;
using Microsoft.Extensions.Options;

namespace Couchbase.Extensions.Caching
{
    public class CouchbaseCacheOptions : IOptions<CouchbaseCacheOptions>
    {
        public CouchbaseCacheOptions Value => this;

        public ClientConfiguration Configuration { get; set; }

        public string BucketName { get; set; }

        public TimeSpan? LifeSpan { get; set; }
    }
}
