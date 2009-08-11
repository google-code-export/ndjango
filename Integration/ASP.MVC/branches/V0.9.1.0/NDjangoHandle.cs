using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;

namespace NDjango.ASPMVCIntegration
{
    /// <summary>
    /// Associates a ndjango template manager with an instance of an HttpApplication.
    /// This lets the system take advantage of the smart locking employed by bistro,
    /// and minimize global locks past startup.
    /// </summary>
    public class NDjangoHandle
    {
        public const string MANAGER_HANDLE = "ndjangoManagerHandle";

        /// <summary>
        /// Application object instance we're bound to
        /// </summary>
        HttpApplication application;

        /// <summary>
        /// The most current manager we're aware of
        /// </summary>
        NDjango.Interfaces.ITemplateManager manager;

        /// <summary>
        /// Global lock handle
        /// </summary>
        static object handleLock = new object();

        /// <summary>
        /// Initialization flag
        /// </summary>
        static bool initialized;

        /// <summary>
        /// Global engine reference
        /// </summary>
        static NDjangoViewEngine engine;

        /// <summary>
        /// Initializes a new instance of the <see cref="NDjangoHandle"/> class.
        /// </summary>
        /// <param name="application">The application.</param>
        public NDjangoHandle(HttpApplication application)
        {
            this.application = application;
            application.BeginRequest += new EventHandler(application_BeginRequest);

            lock (handleLock)
            {
                if (!initialized)
                {
                    engine = new NDjangoViewEngine();
                    initialized = true;
                }
            }

            manager = engine.Provider.GetNewManager();
        }

        /// <summary>
        /// Handles the BeginRequest event of the application control. This event handler is
        /// used to place the manager onto the http context.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        void application_BeginRequest(object sender, EventArgs e)
        {
            application.Context.Items[MANAGER_HANDLE] = manager;
        }

    }
}
