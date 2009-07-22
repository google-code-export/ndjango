using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using NDjango.ASPMVCIntegration;

namespace MvcApplication_Django
{
    // Note: For instructions on enabling IIS6 or IIS7 classic mode, 
    // visit http://go.microsoft.com/?LinkId=9394801

    public class MvcApplication : System.Web.HttpApplication
    {
        protected NDjango.ASPMVCIntegration.NDjangoHandle ndjangoHandle;
        public string root;// = "/MvcApplication_Django/";
        public MvcApplication()
        {
            ndjangoHandle = new NDjangoHandle(this);
        }
        public static void RegisterRoutes(RouteCollection routes)
        {
            
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            routes.MapRoute(
                "Default",                                              // Route name
                "{controller}/{action}/{id}",                           // URL with parameters
                new { controller = "Home", action = "Index", id = "" }  // Parameter defaults
            );
            routes.MapRoute(
                "FormRoot",
                "*",
                new { controller = "Default", action = "Load", id = "" }
                );

        }

        protected void Application_Start()
        {
            RegisterRoutes(RouteTable.Routes);
            root = "/MvcApplication_Django/";
        }
    }
}