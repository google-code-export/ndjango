using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Bistro.Controllers;
using Bistro.Controllers.Descriptor;
using Bistro.Controllers.Descriptor.Data;
using MvcSamlpePort.Security;

namespace MvcSamlpePort.Controllers.Auth
{
    /// <summary>
    /// Logoff controller. This controller has no view, and simply redirects to the '/home/index' method
    /// upon completion.
    /// </summary>
    [Bind("get /auth/logoff")]
    public class DoLogoff: AuthBase
    {
        /// <summary>
        /// Controller implementation. User signout is performed by calling Authenticate with null as the 
        /// user. 
        /// </summary>
        /// <param name="context">The context.</param>
        public override void DoProcessRequest(IExecutionContext context)
        {
            FormsAuth.SignOut();
            context.Authenticate(null);

            context.Transfer("home/index");
        }
    }
}
