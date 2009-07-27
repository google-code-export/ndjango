using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Bistro.Controllers;
using Bistro.Controllers.Descriptor;
using Bistro.Controllers.Descriptor.Data;

namespace MvcSamplePort.Controllers
{
    /// <summary>
    /// Home controller. This controller services the 'get /home/index' method.
    /// </summary>
    [Bind("get /home/index")]
    [Bind("get /")]
    [Bind("get /default")]
    [RenderWith("Views/Home/index.django")]
    public class HomeController : AbstractController
    {
        /// <summary>
        /// Message sent to the context. This field is marked with the 'Request' attribute,
        /// making it available to the request context. Since it is not marked with any other
        /// attribute, it defaults to a 'Provides' behavior, meaning that it places this value
        /// onto the context, and does not depend on other controllers to provide it. This 
        /// means that other controllers can depend on it.
        /// </summary>
        [Request]
        protected string Message = "Welcome to Bistro!";

        /// <summary>
        /// Controller implementation. Since the sole purpose of this controller is to expose
        /// the value of the Message field, no logic is present here. 
        /// </summary>
        /// <param name="context">The context.</param>
        public override void DoProcessRequest(IExecutionContext context) { }
    }

    /// <summary>
    /// About controller. This controller services the 'get /home/about' method.
    /// </summary>
    [Bind("get /home/about")]
    [RenderWith("Views/Home/about.django")]
    public class AboutController : AbstractController
    {
        /// <summary>
        /// Controller implementation. This controller merely displays the view, so there is no
        /// need for an implementation.
        /// </summary>
        /// <param name="context">The context.</param>
        public override void DoProcessRequest(IExecutionContext context) { }
    }
    [Bind("get /home/about1")]
    [RenderWith("Views/Home/about1.django")]
    public class About1Controller : AbstractController
    {
        /// <summary>
        /// Controller implementation. This controller merely displays the view, so there is no
        /// need for an implementation.
        /// </summary>
        /// <param name="context">The context.</param>
        public override void DoProcessRequest(IExecutionContext context) { }
    }
    [Bind("get /home/about2")]
    [RenderWith("Views/Home/about2.django")]
    public class About2Controller : AbstractController
    {
        /// <summary>
        /// Controller implementation. This controller merely displays the view, so there is no
        /// need for an implementation.
        /// </summary>
        /// <param name="context">The context.</param>
        public override void DoProcessRequest(IExecutionContext context) { }
    }

}
