using System.Text;
using System.Threading.Tasks;
using Couchbase.Extensions.Caching.IntegrationTests.Infrastructure;
using Newtonsoft.Json;
using NUnit.Framework;

namespace Couchbase.Extensions.Caching.IntegrationTests
{
    [TestFixture]
    public class CouchbaseCacheTests
    {
        [Test]
        public async Task Test_Set()
        {
            var cache = new CouchbaseCache(new CouchbaseCacheOptions
            {
                BucketName = "default",
                Configuration = TestConfiguration.GetCurrentConfiguration()
            });

            var poco = new Poco {Name = "poco1", Age = 12};
            const string key = "CouchbaseCacheTests.Test_Set";
            var bucket = ClusterHelper.GetBucket("default");
            bucket.Remove(key);

            cache.Set(key, GetBytes(poco), null);

            var result = bucket.Get<Poco>(key);
            Assert.AreEqual(result.Value.Age, poco.Age);

            await Task.Delay(10);
        }

        static byte[] GetBytes(Poco poco)
        {
            return Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(poco));
        }

        public class Poco
        {
            public string Name { get; set; }

            public int Age { get; set; }
        }
    }
}
