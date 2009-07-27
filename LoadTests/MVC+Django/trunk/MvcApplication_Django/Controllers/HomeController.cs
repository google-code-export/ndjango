using System;
using System.Collections.Generic;
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

        public ActionResult Index()
        {
            ViewData["Message"] = String.Format("Welcome to ASP.NET MVC!", new Random().Next());

            return View();
        }

        public ActionResult About()
        {
            ViewData["RandomMessage"] = new Random().Next().ToString();
            return View();
        }
    }
}
