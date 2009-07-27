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
    /// Base class for authentication controllers. This class provides basic functionality
    /// and exposes default validation components used in authentication and user management
    /// screens.
    /// </summary>
    public abstract class AuthBase : AbstractController
    {
        /// <summary>
        /// A form field-specific errors dictionary. This value is exposed to the request 
        /// context.
        /// </summary>
        [Request]
        protected Dictionary<string, string> errors;

        /// <summary>
        /// The minimum password allowed by the membership service. This property will be
        /// made available to the request context. Invoking it from the context will \not\
        /// invoke the property, but rather access the value the property held when the 
        /// DoProcessRequest method finished.
        /// </summary>
        /// <value>The minimum length of the password.</value>
        [Request]
        protected int PasswordLength { get { return MembershipService.MinPasswordLength; } }

        /// <summary>
        /// Gets or sets the forms authentication provider.
        /// </summary>
        /// <value>The forms auth.</value>
        protected IFormsAuthentication FormsAuth
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets or sets the membership service.
        /// </summary>
        /// <value>The membership service.</value>
        protected IMembershipService MembershipService
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the error count.
        /// </summary>
        /// <value>The error count.</value>
        protected int ErrorCount { get { return errors == null ? 0 : errors.Count; } }

        /// <summary>
        /// Places an error onto the errors dictionary
        /// </summary>
        /// <param name="element">The element.</param>
        /// <param name="error">The error.</param>
        protected void ReportError(string element, string error)
        {
            if (errors == null)
                errors = new Dictionary<string, string>();

            string errorKey = element ?? errors.Count.ToString();

            if (errors.ContainsKey(errorKey))
                errors[errorKey] += error;
            else
                errors.Add(errorKey, error);
        }

        /// <summary>
        /// Initializes this instance.
        /// </summary>
        public override void Initialize()
        {
            base.Initialize();

            FormsAuth = new FormsAuthenticationService();
            MembershipService = new AccountMembershipService();
        }
    }
}
