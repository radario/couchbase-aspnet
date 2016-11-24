using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.DependencyInjection;

namespace Couchbase.Extensions.Caching
{
    public static class CouchbaseCacheServiceCollectionExtensions
    {
        public static IServiceCollection AddDistributedCouchbaseCache(this IServiceCollection services, Action<CouchbaseCacheOptions> setupAction)
        {
            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }
            if (setupAction == null)
            {
                throw new ArgumentNullException(nameof(setupAction));
            }

            services.AddOptions();
            services.Configure(setupAction);
            services.Add(ServiceDescriptor.Singleton<IDistributedCache, CouchbaseCache>());

            return services;
        }
    }
}
