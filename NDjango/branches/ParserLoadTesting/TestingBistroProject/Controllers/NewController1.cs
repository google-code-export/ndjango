using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Bistro.Controllers;
using Bistro.Controllers.Descriptor;
using Bistro.Controllers.Descriptor.Data;

namespace Controllers
{
    /// <summary>
    /// Home controller. This controller services the 'get /home/index' method.
    /// </summary>
    [Bind("get /index")]
    [RenderWith("Views/index.django")]
    public class HomeController : AbstractController
    {
        [Request]
        protected string Message = "Welcome to Bistro!!!";

        [Request]
        protected bool foo = true;

        [Request]
        protected bool bar = true;

        [Request]
        protected int value = 1;

        [Request]
        protected int value2 = 2;

        [Request]
        protected string value3 = "";

        [Request]
        protected string[] heroes = new string[] { "Cooper", "Benjamin", "Bob", "Trueman" };

        [Request]
        protected string lt = "<";

        [Request]
        protected string gt = ">";

        [Request]
        protected int v1 = 1;

        [Request]
        protected int v2 = 2;

        [Request]
        protected string value4 = "\"double quotes\"";

        [Request]
        protected int v3 = 3;

        [Request]
        protected DateTime now = DateTime.Now;

        [Request]
        protected int[] range = new int[] { 1, 2, 3, 4, 5 };

        /// <summary>
        /// Controller implementation.
        /// </summary>
        /// <param name="context">The context.</param>
        public override void DoProcessRequest(IExecutionContext context)
        {
        }
    }
}
