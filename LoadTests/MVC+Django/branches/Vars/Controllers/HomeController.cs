using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using NDjango.ASPMVCIntegration;
using NDjango.FiltersCS;

namespace MvcApplication_Django.Controllers
{
    [HandleError]
    public class HomeController : Controller
    {
        private string greeting = "hello";
        public ActionResult Index()
        {
            ViewData["Message"] = "Welcome to Django!";
            ViewData["Greet"] = greeting;
            return View();
        }

        public ActionResult About()
        {
            ViewData["RandomMessage"] = "";
            return View();
        }
    }
}
