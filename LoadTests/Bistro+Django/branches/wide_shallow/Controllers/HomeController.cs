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
    [Bind("get /home/index/{param1}",Priority=-2)]
    [Bind("get /")]
    [Bind("get /default")]
    [Bind("get /home/notes1")]
    [Bind("get /home/notes2")]
    [Bind("get /home/notes3")]
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
        [Request,DependsOn]
        protected string Message ;
        [Request]
        protected int nDigit = 2;
        /// <summary>
        /// Controller implementation. Since the sole purpose of this controller is to expose
        /// the value of the Message field, no logic is present here. 
        /// </summary>
        /// <param name="context">The context.</param>
        public override void DoProcessRequest(IExecutionContext context) 
        { 
            Message += " Welcome to Bistro!";
        }
    }

    /// <summary>
    /// About controller. This controller services the 'get /home/about' method.
    /// </summary>
    [Bind("get /myHome")]
    [Bind("get /myHome1")]
    [Bind("get /myHome2")]
    [Bind("get /myHome3")]
    [Bind("get /home/index/{param1}", Priority = 5)]
    [RenderWith("Views/Home/index.django")]
    public class IndexController : AbstractController
    {
        /// <summary>
        /// Controller implementation. This controller merely displays the view, so there is no
        /// need for an implementation.
        /// </summary>
        /// <param name="context">The context.</param>
        [Request]
        protected string param1 = "";
        [Request]
        protected string Message;
        public override void DoProcessRequest(IExecutionContext context)
        {
            if (param1 != null)
            {
                Message = param1.Contains('h') ? "hello" : "bye";
            }
        }
    }


    [Bind("get /home/about")]
    [Bind("get /home/notes")]
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

    
    
    public class Index1Controller : AbstractController
    {
        /// <summary>
        /// Controller implementation. This controller merely displays the view, so there is no
        /// need for an implementation.
        /// </summary>
        /// <param name="context">The context.</param>
        [Request,DependsOn]
        protected string Message;
        [Request,DependsOn]
        protected int nDigit;
        public override void DoProcessRequest(IExecutionContext context) 
        {
            if (nDigit > 1)
            {
                Message += "large enough";
            }

        }
    }
 

}
