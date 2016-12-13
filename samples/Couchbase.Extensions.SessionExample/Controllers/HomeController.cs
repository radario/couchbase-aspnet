using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Couchbase.Extensions.Session;
using Couchbase.Extensions.SessionExample.Models;

namespace Couchbase.Extensions.SessionExample.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            HttpContext.Session.SetObject("Test", "{ \"name\" : \"Session stored in couchbase!\"}");
            HttpContext.Session.SetObject("PocoTest", new Poco {Age = 19, Name = "PocoLoco"});
            return View();
        }

        public IActionResult About()
        {
            ViewData["Message"] = HttpContext.Session.GetObject<string>("Test");

            return View();
        }

        public IActionResult Contact()
        {
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
