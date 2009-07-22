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
        public int number = 1;
        private ArrayList arrList = new ArrayList(5);
        private string greeting = "hello!!!";
        public ActionResult Index()
        {
            if (arrList.Count == 0)
            {
                for (int i = 0; i < 5; i++)
                {
                    arrList.Add("smth");
                }
            }
            ViewData["Message"] = "Welcome to Django!";
            ViewData["Greeting"] = greeting;
            ViewData["arrList"] = arrList;
            ViewData["number"] = number;
            return View();
        }

        public ActionResult About()
        {
            ViewData["RandomMessage"] = "";
            return View();
        }
    }
}
