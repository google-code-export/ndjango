using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using System.IO;
using System.Web;
using System.Web.Routing;
using NDjango.FiltersCS;
using System.Configuration;
using System.Web.Configuration;
using System.Reflection;
using NDjango.TagSimpleTests;
using NDjango.Interfaces;

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

            NDjangoRegisterTemplate NDjangoRegisterTemplate = new NDjangoRegisterTemplate(this);
            NDjangoRegisterTemplate.Provider = provider;
            NDjangoRegisterTemplate.RegisterTemplates();
            
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

    public class NDjangoRegisterTemplate
    {
        const string NDJangoTagSection = "NDJangoGroup/NDJangoTagSection";
        const string NDJangoFilterSection = "NDJangoGroup/NDJangoFilterSection";
        const string NDJangoSettingsSection = "NDJangoGroup/NDJangoSettingsSection";
        string[] Sections = new string[] { NDJangoTagSection, NDJangoFilterSection, NDJangoSettingsSection };
        private TemplateManagerProvider provider;
        private NDjangoViewEngine templateLoader;
        internal TemplateManagerProvider Provider
        {
            get
            {
                return provider;
            }
            set
            {
                provider = value;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="NDjangoRegisterTemplate"/> class.
        /// </summary>
        public NDjangoRegisterTemplate(NDjangoViewEngine NDjangoViewEngine)
        {
            templateLoader = NDjangoViewEngine;
        }

        public void RegisterTemplates()
        {
            foreach (string item in Sections)
            {
                RegisterCurrentTemplate(GetCurrentSectionName(item), GetCollectionOfSectionByPath(item));
            }
        }

        private NameValueConfigurationCollection GetCollectionOfSectionByPath(string path)
        {
            NDjangoSectionHandler nameValueSection = new NDjangoSectionHandler();
            try
            {
                nameValueSection = ConfigurationManager.GetSection(path) as NDjangoSectionHandler;
            }
            catch (ConfigurationException cfgEx) {
                //TODO Exception
            }

            if (nameValueSection != null)
            {
                return nameValueSection.NDJangoSectionCollection;
            }
            return null; 
        }

        private void RegisterCurrentTemplate(TypeOfSection type, NameValueConfigurationCollection nVConfCollection)
        {
            if (nVConfCollection != null)
            {
                foreach (string key in nVConfCollection.AllKeys)
                {
                    string name = nVConfCollection[key].Name;
                    string value = nVConfCollection[key].Value;
                    Type myType = Type.GetType(value.ToString());
                    if (myType != null || type == TypeOfSection.NDJangoSettingsSection)
                    {
                        switch (type)
                        {
                            case TypeOfSection.NDJangoTagSection:
                                ITag tag = (ITag)System.Activator.CreateInstance(myType);
                                RegisterITag(name, tag);
                                break;
                            case TypeOfSection.NDJangoFilterSection:
                                ISimpleFilter filter = (ISimpleFilter)System.Activator.CreateInstance(myType);
                                RegisterISimpleFilter(name, filter);
                                break;
                            case TypeOfSection.NDJangoSettingsSection:
                                ValidateSettings(name, value);
                                break;
                            default:
                                break;
                        }
                    }
                }
            }
        }

        private void RegisterITag(string name,ITag tag) {
            provider = provider.WithLoader(templateLoader).WithTag(name, tag);
        }

        private void RegisterISimpleFilter(string name, ISimpleFilter filter) {
            provider = provider.WithLoader(templateLoader).WithFilter(name, filter);
        }

        private void ValidateSettings(string name, string value) {
            object result = null;
            bool r1;
            if (Boolean.TryParse(value, out r1)) {
                result = r1;
            }
            int r2;
            if (int.TryParse(value, out r2)) {
                result = r2;
            }

            Type type = result.GetType();
            foreach (var item in ((ITemplateManagerProvider)provider).Settings)
            {
            }
        }

        private  TypeOfSection GetCurrentSectionName(string value) {
            TypeOfSection type = TypeOfSection.NDJangoNothing;
            if (value.Contains(NDJangoTagSection)) {
                type = TypeOfSection.NDJangoTagSection;
            }
            else if (value.Contains(NDJangoFilterSection)) { 
                type = TypeOfSection.NDJangoFilterSection;
            }
            else if(value.Contains(NDJangoSettingsSection)){
                 type = TypeOfSection.NDJangoSettingsSection;
           }
            return type;   
        }

        private enum TypeOfSection
        {
            NDJangoTagSection = 0,
            NDJangoFilterSection = 1,
            NDJangoSettingsSection = 2,
            NDJangoNothing = 3,
        }


    }
}
