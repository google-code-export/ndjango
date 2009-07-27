using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Bistro.Controllers;
using Bistro.Controllers.Descriptor;
using Bistro.Controllers.Descriptor.Data;
using MvcSamlpePort.Security;
using System.Globalization;
using Bistro.Controllers.Security;

namespace MvcSamlpePort.Controllers.Auth
{
    [Bind("/auth/changepassword")]
    [Deny("?", OnFailure = FailAction.Redirect, Target = "/auth/logon")]
    public class ChangePasswordSecurity : SecurityController { }

    [Bind("get /auth/changepassword")]
    [RenderWith("Views/Account/changePassword.django")]
    public class ChangePasswordDisplay: AbstractController
    {
        public override void DoProcessRequest(IExecutionContext context) { }
    }

    [Bind("post /auth/changepassword")]
    [RenderWith("Views/Account/changePassword.django")]
    public class DoChangePassword : AuthBase
    {
        [FormField, Request]
        protected string
            currentPassword,
            newPassword,
            confirmPassword;

        [FormField, Request]
        protected bool rememberMe = false;

        public override void DoProcessRequest(IExecutionContext context)
        {
            if (!ValidateChangePassword())
                return;

            try
            {
                if (!MembershipService.ChangePassword(context.CurrentUser.Identity.Name, currentPassword, newPassword))
                {
                    ReportError(null, "The current password is incorrect or the new password is invalid.");
                    return;
                }
            }
            catch
            {
                ReportError(null, "The current password is incorrect or the new password is invalid.");
                return;
            } 
            
            context.Response.RenderWith("Views/Account/changePasswordSuccess.django");
        }

        private bool ValidateChangePassword()
        {
            int currentErrorCount = ErrorCount;
            if (String.IsNullOrEmpty(currentPassword))
                ReportError("currentPassword", "You must specify a current password.");

            if (newPassword == null || newPassword.Length < MembershipService.MinPasswordLength)
            {
                ReportError("newPassword",
                    String.Format(CultureInfo.CurrentCulture,
                         "You must specify a new password of {0} or more characters.",
                         MembershipService.MinPasswordLength));
            }

            if (!String.Equals(newPassword, confirmPassword, StringComparison.Ordinal))
                ReportError("_FORM", "The new password and confirmation password do not match.");

            return ErrorCount == currentErrorCount;
        }
    }
}
