using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Bistro.Controllers;
using Bistro.Controllers.Descriptor;
using System.Security.Principal;
using Bistro.Controllers.Descriptor.Data;

namespace MvcSamplePort.Controllers
{
    /// <summary>
    /// Default controller. This controller services all requests, and provides values to all of them.
    /// </summary>
    [Bind("?")]
    public class DefaultController: AbstractController
    {
        /// <summary>
        /// The current user. This field exposes the currently logged on user as a context variable.
        /// </summary>
        [Request]
        IPrincipal user;

        /// <summary>
        /// The application root. This field exposes the application root as a context variable. This 
        /// will be used in the view to generate absolute URLs
        /// </summary>
        [Request]
        string root;

        /// <summary>
        /// Controller implementation.
        /// </summary>
        /// <param name="context">The context.</param>
        public override void DoProcessRequest(IExecutionContext context)
        {
            user = context.CurrentUser;

            if (HttpContext.Current.Request.ApplicationPath == "/")
                root = String.Empty;
            else
                root = '/' + HttpContext.Current.Request.ApplicationPath.Trim('/');
        }
    }
}
