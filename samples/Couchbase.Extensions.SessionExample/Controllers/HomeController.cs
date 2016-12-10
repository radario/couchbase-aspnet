using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;

namespace Couchbase.Extensions.SessionExample.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            HttpContext.Session.SetString("Test", "Session stored in couchbase!");
            return View();
        }

        public IActionResult About()
        {
            ViewData["Message"] = HttpContext.Session.GetString("Test");

            return View();
        }

        public IActionResult Contact()
        {
            // Note does not work: DistributedSession never delegates the call to the underlying IDistributedCache implementation
            // opened issue on github https://github.com/aspnet/Session/issues/139
            HttpContext.Session.Remove("Test");
            ViewData["Message"] = "Your contact page.";

            return View();
        }

        public IActionResult Error()
        {
            return View();
        }
    }
}
