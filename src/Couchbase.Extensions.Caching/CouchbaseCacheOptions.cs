using System;
using Couchbase.Configuration.Client;
using Couchbase.Core;
using Microsoft.Extensions.Options;

namespace Couchbase.Extensions.Caching
{
    /// <summary>
    /// Options for <see cref="CouchbaseCache"/>. Note that if <see cref="Bucket"/> is empty the <see cref="ClusterHelper.GetBucket(string)"/>
    /// will be used to create the bucket.
    /// </summary>
    public class CouchbaseCacheOptions : IOptions<CouchbaseCacheOptions>
    {
        /// <summary>
        /// The current <see cref="CouchbaseCacheOptions"/> instance.
        /// </summary>
        public CouchbaseCacheOptions Value => this;

        /// <summary>
        /// Configuration for the cluster.
        /// </summary>
        public ClientConfiguration Configuration { get; set; }

        /// <summary>
        /// The bucket name of the bucket to open.
        /// </summary>
        public string BucketName { get; set; }

        /// <summary>
        /// The global lifespan for cache items.
        /// </summary>
        public TimeSpan? LifeSpan { get; set; }

        /// <summary>
        /// A bucket reference.
        /// </summary>
        public IBucket Bucket { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to throw an exception if the operation has failed and the result
        /// contains an exception. The using application will then have to handle the exception individually. If the
        /// operation fails and ThrowExceptions is <c>false</c>, then the reason for the exception will not be thrown
        /// and null will be returned. In either case the exception will be logged.
        /// </summary>
        /// <value>
        ///   <c>true</c> if you want to throw exceptions; otherwise, <c>false</c>.
        /// </value>
        public bool ThrowExceptions { get; set; }
    }
}
