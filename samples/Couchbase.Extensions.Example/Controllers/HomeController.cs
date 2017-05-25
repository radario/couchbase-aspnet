using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Distributed;

namespace Couchbase.Extensions.Example.Controllers
{
    public class HomeController : Controller
    {
        private IDistributedCache _cache;

        public HomeController(IDistributedCache cache)
        {
            _cache = cache;
        }

        public IActionResult Index()
        {
            _cache.Set("CacheTime", System.Text.Encoding.UTF8.GetBytes(DateTime.Now.ToString()));
            return View();
        }

        public IActionResult About()
        {
            ViewData["Message"] = "Your application description page. " + System.Text.Encoding.UTF8.GetString(_cache.Get("CacheTime"));

            return View();
        }

        public IActionResult Contact()
        {
            ViewData["Message"] = "Your contact page.";

            return View();
        }

        public IActionResult Error()
        {
            return View();
        }
    }
}
