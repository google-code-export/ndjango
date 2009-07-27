using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using NDjango.ASPMVCIntegration;
namespace MvcApplication_Django.Controllers
{
    [HandleError]
    public class DefaultController: Controller
    {
        public string root;
        public void Load()
        {
            root = "/MvcApplication_Django";
        }
    }
}
