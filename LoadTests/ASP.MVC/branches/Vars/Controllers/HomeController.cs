using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace MvcApplication_Simple.Controllers
{
    [HandleError]
    public class HomeController : Controller
    {
        private string greeting = "hello";
        public ActionResult Index()
        {
            ViewData["Message"] = String.Format("Welcome to ASP.NET MVC!", new Random().Next());
            ViewData["Greet"] = greeting;
            return View();
        }

        public ActionResult About()
        {
            ViewData["RandomMessage"] = new Random().Next().ToString();
            return View();
        }
    }
}
