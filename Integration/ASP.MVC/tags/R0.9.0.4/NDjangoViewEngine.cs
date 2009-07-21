using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using System.IO;
using System.Web;

namespace NDjango.ASPMVCIntegration
{
    /// <summary>
    /// VirtualPathProviderViewEngine implementation for the NDjango engine
    /// </summary>
    public class NDjangoViewEngine : VirtualPathProviderViewEngine, NDjango.Interfaces.ITemplateLoader
    {
        /// <summary>
        /// Application root directory
        /// </summary>
        string rootDir;

        /// <summary>
        /// Initializes a new instance of the <see cref="NDjangoViewEngine"/> class.
        /// </summary>
        public NDjangoViewEngine()
        {
            NDjango.FiltersCS.FilterManager.Instance.Initialize();
            base.ViewLocationFormats = new string[] { "~/Views/{1}/{0}.django" };

            base.PartialViewLocationFormats = base.ViewLocationFormats;
            rootDir = HttpRuntime.AppDomainAppPath;
            InitialManager = NDjango.Template.Manager.RegisterLoader(this);

            ViewEngines.Engines.Clear();
            ViewEngines.Engines.Add(this);
        }

        /// <summary>
        /// Gets or sets the ndjango manager obtained by registering the engine as a Loader.
        /// </summary>
        /// <value>The initial manager.</value>
        public NDjango.Interfaces.ITemplateManager InitialManager { get; private set; }

        /// <summary>
        /// Creates the partial view.
        /// </summary>
        /// <param name="controllerContext">The controller context.</param>
        /// <param name="partialPath">The partial path.</param>
        /// <returns></returns>
        protected override IView CreatePartialView(ControllerContext controllerContext, string partialPath)
        {
            return new NDjangoView(partialPath);
        }

        /// <summary>
        /// Creates the view.
        /// </summary>
        /// <param name="controllerContext">The controller context.</param>
        /// <param name="viewPath">The view path.</param>
        /// <param name="masterPath">The master path.</param>
        /// <returns></returns>
        protected override IView CreateView(ControllerContext controllerContext, string viewPath, string masterPath)
        {
            return new NDjangoView(viewPath);
        }

        /// <summary>
        /// Gets the template source from the app-relative path.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <returns></returns>
        public TextReader GetTemplate(string name)
        {
            return File.OpenText(Path.Combine(rootDir, name.TrimStart('~', '/')));
        }

        /// <summary>
        /// Determines whether the specified template has been updated since the supplied time stamp
        /// </summary>
        /// <param name="name">The template name.</param>
        /// <param name="timestamp">The timestamp.</param>
        /// <returns>
        /// 	<c>true</c> if the template has since changed; otherwise, <c>false</c>.
        /// </returns>
        public bool IsUpdated(string name, System.DateTime timestamp)
        {
            return File.GetLastWriteTime(Path.Combine(rootDir, name)) > timestamp;
        }
    }
}
