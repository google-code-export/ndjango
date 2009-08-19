using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using System.IO;
using System.Web;
using System.Web.Routing;
using NDjango.FiltersCS;

namespace NDjango.ASPMVCIntegration
{
    /// <summary>
    /// Bistro-specific implementation of the django {% url %} tag.
    /// 
    /// This tag will take an action name, and an optional controller name (as the second parameter)
    /// and return the result of calling UrlHelper.Action(action, [controller]);
    /// </summary>
    public class AspMvcUrlTag : NDjango.Tags.Abstract.UrlTag
    {
        public override string GenerateUrl(string pathTemplate, string[] parameters, NDjango.Interfaces.IContext context)
        {
            var contextOption = context.tryfind(NDjangoView.aspmvcContextKey);

            if (contextOption == null || contextOption.Value == null)
                throw new ApplicationException("Unable to locate asp mvc request context. Did someone modify djangocontext." + NDjangoView.aspmvcContextKey + "?");

            RequestContext requestContext = contextOption.Value as RequestContext;

            if (parameters.Length > 1)
                throw new ApplicationException("Only 0 or 1 parameters are supported by the asp.mvc version of the URL tag");
            else if (parameters.Length == 1)
                return new UrlHelper(requestContext).Action(pathTemplate, parameters[0]);
            else
                return new UrlHelper(requestContext).Action(pathTemplate);
        }
    }

    /// <summary>
    /// VirtualPathProviderViewEngine implementation for the NDjango engine
    /// </summary>
    public class NDjangoViewEngine : VirtualPathProviderViewEngine, NDjango.Interfaces.ITemplateLoader
    {
        /// <summary>
        /// Application root directory
        /// </summary>
        string rootDir;

        private TemplateManagerProvider provider;

        internal TemplateManagerProvider Provider
        {
            get
            {
                return provider;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="NDjangoViewEngine"/> class.
        /// </summary>
        public NDjangoViewEngine()
        {

            provider = new TemplateManagerProvider().WithLoader(this).WithTag("url", new AspMvcUrlTag());
            provider = FilterManager.Initialize(provider);
            base.ViewLocationFormats = new string[] { "~/Views/{1}/{0}.django" };

            base.PartialViewLocationFormats = base.ViewLocationFormats;
            rootDir = HttpRuntime.AppDomainAppPath;

            ViewEngines.Engines.Clear();
            ViewEngines.Engines.Add(this);
        }

        /// <summary>
        /// Gets or sets the ndjango manager obtained by registering the engine as a Loader.
        /// </summary>
        /// <value>The initial manager.</value>
//        public NDjango.Interfaces.ITemplateManager InitialManager { get; private set; }

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
        /// If string contains tilde - it's a relative path, so leading tilde and slashes should be removed.
        /// Such path can come only from ASP.MVC itself, there should be no tilde when we load templates from extends or include tags
        /// </summary>
        /// <param name="strWithTilde"></param>
        /// <returns></returns>
        private string RemoveTilde(string strWithTilde)
        {
            return strWithTilde.StartsWith("~/") ? strWithTilde.TrimStart('~', '/') : strWithTilde;
        }

        /// <summary>
        /// Gets the template source from the app-relative path.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <returns></returns>
        public TextReader GetTemplate(string name)
        {

            return File.OpenText(Path.Combine(rootDir, RemoveTilde(name)));
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
            return File.GetLastWriteTime(Path.Combine(rootDir, RemoveTilde(name))) > timestamp;
        }
    }
}
