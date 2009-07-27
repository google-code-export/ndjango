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
    /// LogonDisplay controller. This controller services get requests on the '/auth/logon' method
    /// by providing the view.
    /// </summary>
    [Bind("get /auth/logon")]
    [RenderWith("Views/Account/logon.django")]
    public class LogonDisplay : AbstractController
    {
        /// <summary>
        /// Controller implementation
        /// </summary>
        /// <param name="context">The context.</param>
        public override void DoProcessRequest(IExecutionContext context) { }
    }

    /// <summary>
    /// DoLogon controller. This controller performs the requested logon action via posts to the
    /// '/auth/logon' method
    /// </summary>
    [Bind("post /auth/logon")]
    [RenderWith("Views/Account/logon.django")]
    public class DoLogon : AuthBase
    {
        /// <summary>
        /// The user name and password fields. These fields are retrieved from the form via the
        /// FormField attribute, but also made available to the request context via the Request
        /// attribute. If not modified by DoProcessRequest, the value submitted on the form
        /// will be passed back out to the request context under the same key.
        /// </summary>
        [FormField, Request]
        protected string
            username,
            password;

        /// <summary>
        /// Remember me flag. This field is both a form field and a request field. Note that the 
        /// data type is "bool". The inbound string value is parsed, and then made available to the
        /// request context as a Boolean
        /// </summary>
        [FormField, Request]
        protected bool rememberMe = false;

        /// <summary>
        /// Controller implementation
        /// </summary>
        /// <param name="context">The context.</param>
        public override void DoProcessRequest(IExecutionContext context)
        {
            if (!ValidateLogon())
                return;

            context.Authenticate(FormsAuth.SignIn(username, rememberMe));

            context.Transfer("/home/index");
        }

        /// <summary>
        /// Validates the logon information.
        /// </summary>
        /// <returns></returns>
        private bool ValidateLogon()
        {
            int currentErrorCount = ErrorCount;
            if (String.IsNullOrEmpty(username))
                ReportError("username", "You must specify a username.");

            if (String.IsNullOrEmpty(password))
                ReportError("password", "You must specify a password.");

            if (!MembershipService.ValidateUser(username, password))
                ReportError(null, "The username or password provided is incorrect.");

            return ErrorCount == currentErrorCount;
        }
    }
}
