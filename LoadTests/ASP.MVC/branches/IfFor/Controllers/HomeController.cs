using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace MvcApplication_Simple.Controllers
{
    [HandleError]
    public class HomeController : Controller
    {
        private string greeting = "helloo!!!";
        private int number = 1;
        ArrayList arrList = new ArrayList(5);
        public ActionResult Index()
        {
            /*if(arrList.Count == 0)
            {
                for (int i = 0; i < 5; i++)
                {
                   arrList.Add("smth");
                }
            }*/
            ViewData["Message"] = String.Format("Welcome to ASP.NET MVC!", new Random().Next());
            ViewData["greeting"] = greeting;
            ViewData["number"] = number;
            ViewData["arrList"] = arrList;
     
            return View();
        }

        public ActionResult About()
        {
            ViewData["RandomMessage"] = new Random().Next().ToString();
            return View();
        }
        public ActionResult Context()
        {
            ViewData["arrList"] = arrList;
          
            return View();
        }
    }
}
