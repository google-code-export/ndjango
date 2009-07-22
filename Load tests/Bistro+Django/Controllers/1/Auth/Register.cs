using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Bistro.Controllers;
using Bistro.Controllers.Descriptor;
using Bistro.Controllers.Descriptor.Data;
using MvcSamlpePort.Security;
using System.Globalization;
using System.Web.Security;

namespace MvcSamlpePort.Controllers.Auth
{
    [Bind("get /auth/newuser")]
    [RenderWith("Views/Account/register.django")]
    public class RegisterDisplay: AbstractController
    {
        public override void DoProcessRequest(IExecutionContext context) { }
    }

    [Bind("post /auth/newuser")]
    [RenderWith("Views/Account/register.django")]
    public class DoRegister : AuthBase
    {
        [FormField, Request]
        protected string
            username,
            email,
            password,
            confirmPassword;

        [FormField, Request]
        protected bool rememberMe = false;

        public override void DoProcessRequest(IExecutionContext context)
        {
            if (!ValidateRegistration())
                return;

            MembershipCreateStatus createStatus = MembershipService.CreateUser(username, password, email);

            if (createStatus == MembershipCreateStatus.Success)
            {
                FormsAuth.SignIn(username, false);
                context.Transfer("/home/index");
            }
            else
                ReportError(null, ErrorCodeToString(createStatus));
        }

        private bool ValidateRegistration()
        {
            int currentErrorCount = ErrorCount;
            if (String.IsNullOrEmpty(username))
                ReportError("username", "You must specify a username.");

            if (String.IsNullOrEmpty(email))
                ReportError("email", "You must specify an email address.");

            if (password == null || password.Length < MembershipService.MinPasswordLength)
            {
                ReportError("password",
                    String.Format(CultureInfo.CurrentCulture,
                         "You must specify a password of {0} or more characters.",
                         MembershipService.MinPasswordLength));
            }

            if (!String.Equals(password, confirmPassword, StringComparison.Ordinal))
                ReportError(null, "The new password and confirmation password do not match.");

            return ErrorCount == currentErrorCount;
        }

        private static string ErrorCodeToString(MembershipCreateStatus createStatus)
        {
            // See http://msdn.microsoft.com/en-us/library/system.web.security.membershipcreatestatus.aspx for
            // a full list of status codes.
            switch (createStatus)
            {
                case MembershipCreateStatus.DuplicateUserName:
                    return "Username already exists. Please enter a different user name.";

                case MembershipCreateStatus.DuplicateEmail:
                    return "A username for that e-mail address already exists. Please enter a different e-mail address.";

                case MembershipCreateStatus.InvalidPassword:
                    return "The password provided is invalid. Please enter a valid password value.";

                case MembershipCreateStatus.InvalidEmail:
                    return "The e-mail address provided is invalid. Please check the value and try again.";

                case MembershipCreateStatus.InvalidAnswer:
                    return "The password retrieval answer provided is invalid. Please check the value and try again.";

                case MembershipCreateStatus.InvalidQuestion:
                    return "The password retrieval question provided is invalid. Please check the value and try again.";

                case MembershipCreateStatus.InvalidUserName:
                    return "The user name provided is invalid. Please check the value and try again.";

                case MembershipCreateStatus.ProviderError:
                    return "The authentication provider returned an error. Please verify your entry and try again. If the problem persists, please contact your system administrator.";

                case MembershipCreateStatus.UserRejected:
                    return "The user creation request has been canceled. Please verify your entry and try again. If the problem persists, please contact your system administrator.";

                default:
                    return "An unknown error occurred. Please verify your entry and try again. If the problem persists, please contact your system administrator.";
            }
        }
    }
}
