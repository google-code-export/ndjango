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
            NDjangoRegisterTemplate NDjangoRegisterTemplate = new NDjangoRegisterTemplate(this);
            NDjangoRegisterTemplate.Provider = provider;
            NDjangoRegisterTemplate.RegisterTemplates();
            provider = NDjangoRegisterTemplate.Provider;

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
        const string NDJangoCommonSection = "NDJangoCommonSection";

        string[] Sections = new string[] { NDJangoCommonSection };
        private TemplateManagerProvider provider;
        private NDjangoViewEngine templateLoader;
        public TemplateManagerProvider Provider
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
            RegisterCurrentTemplate(GetCollectionOfSectionByPath(NDJangoCommonSection));
        }

        private NDjangoSectionHandler GetCollectionOfSectionByPath(string path)
        {
            NDjangoSectionHandler nameValueSection = new NDjangoSectionHandler();
            try
            {
                nameValueSection = ConfigurationManager.GetSection(path) as NDjangoSectionHandler;
            }
            catch (ConfigurationException cfgEx)
            {
            }

            if (nameValueSection != null)
            {
                return nameValueSection;
            }
            return null;
        }

        private void RegisterCurrentTemplate(NDjangoSectionHandler nameValueSection)
        {
            foreach (NameValueElementAssembly item in nameValueSection.NDJangoSectionCollection)
            {
                if (item.ImportCollection.Count == 0)
                {
                    //register all tag and filter
                    RegisterGroupOfTemplates(item.Name);
                }
                else
                {
                    //register only defined tag and filter
                    foreach (NameValueElementImport impItem in item.ImportCollection)
                    {
                        RegisterGroupOfTemplates(impItem.Name, item.Name);
                    }
                }
            }

            foreach (NameValueElement item in nameValueSection.NDJangoSettingsSectionCollection)
            {
                string name = item.Name;
                string value = item.Value;
                ValidateSettings(name, value);
            }

        }

        //Register Tag by Name
        private void RegisterITag(string name, ITag tag)
        {
            provider = provider.WithLoader(templateLoader).WithTag(name, tag);
        }

        //Register Filter by Name
        private void RegisterISimpleFilter(string name, ISimpleFilter filter)
        {
            provider = provider.WithLoader(templateLoader).WithFilter(name, filter);
        }

        //Validate Settings by Type of provider and current Name
        private void ValidateSettings(string name, string value)
        {
            object result = null;
            bool r1;

            bool isNewSetting = true;
            foreach (var item in ((ITemplateManagerProvider)provider).Settings)
            {
                if (name.Contains(item.Key))
                {
                    Type setType = item.Value.GetType();
                    if (setType == typeof(Boolean))
                    {
                        if (Boolean.TryParse(value, out r1))
                        {
                            result = r1;
                        }

                    }
                    else if (setType == typeof(int))
                    {
                        int r2;
                        if (int.TryParse(value, out r2))
                        {
                            result = r2;
                        }
                    }
                    else if (setType == typeof(string))
                    {
                        result = value.ToString();

                        provider = provider.WithLoader(templateLoader).WithSetting(name, result);
                        isNewSetting = false;
                    }
                }
            }
            if (isNewSetting)
            {
                provider = provider.WithLoader(templateLoader).WithSetting(name, result);
            }
        }

        //Register Group of filters and tags by name
        private void RegisterGroupOfTemplates(string name, string value)
        {
            string assemblyPath = String.Empty;
            assemblyPath = GetAssemblyPath(value);

            try
            {
                LoadAssembly(name, assemblyPath);
            }
            catch (Exception e)
            {
            }

        }

        //Register Group of filters and tags
        private void RegisterGroupOfTemplates(string value)
        {
            string assemblyPath = String.Empty;
            assemblyPath = GetAssemblyPath(value);

            try
            {
                LoadAssembly(assemblyPath);
            }
            catch (Exception e)
            {
            }

        }

        //Load only assemblies by name
        private void LoadAssembly(string name, string assemblyPath)
        {
            try
            {
                AssemblyName assemblyName = new AssemblyName();
                assemblyName.CodeBase = assemblyPath;

                Assembly assembly = Assembly.Load(assemblyName);
                foreach (Type myType in assembly.GetTypes())
                {
                    if (myType.GetInterface(typeof(ITag).Name) != null)
                    {
                        ITag tag = (ITag)System.Activator.CreateInstance(myType);
                        if (GetTagName(myType) == name)
                        {
                            RegisterITag(name, tag);
                        }
                    }

                    if (myType.GetInterface(typeof(ISimpleFilter).Name) != null)
                    {
                        ISimpleFilter filter = (ISimpleFilter)System.Activator.CreateInstance(myType);
                        if (GetTagName(myType) == name)
                        {
                            RegisterISimpleFilter(GetTagName(myType), filter);
                        }
                    }
                }
            }
            catch (ReflectionTypeLoadException ex)
            {
                StringBuilder sb = new StringBuilder();
                sb.AppendLine(ex.Message);
                for (int i = 0; i < ex.Types.Length; i++)
                    if (ex.Types[i] != null)
                        sb.AppendFormat("\t{0} loaded\r\n", ex.Types[i].Name);

                for (int i = 0; i < ex.LoaderExceptions.Length; i++)
                    if (ex.LoaderExceptions[i] != null)
                        sb.AppendFormat("\texception {0}\r\n", ex.LoaderExceptions[i].Message);
            }
        }

        //Load all assemblies
        private void LoadAssembly(string assemblyPath)
        {
            try
            {
                AssemblyName assemblyName = new AssemblyName();
                assemblyName.CodeBase = assemblyPath;

                Assembly assembly = Assembly.Load(assemblyName);
                foreach (Type myType in assembly.GetTypes())
                {
                    if (myType.GetInterface(typeof(ITag).Name) != null)
                    {
                        ITag tag = (ITag)System.Activator.CreateInstance(myType);
                        RegisterITag(GetTagName(myType), tag);
                    }

                    if (myType.GetInterface(typeof(ISimpleFilter).Name) != null)
                    {
                        ISimpleFilter filter = (ISimpleFilter)System.Activator.CreateInstance(myType);
                        RegisterISimpleFilter(GetTagName(myType), filter);
                    }
                }
            }
            catch (ReflectionTypeLoadException ex)
            {
                StringBuilder sb = new StringBuilder();
                sb.AppendLine(ex.Message);
                for (int i = 0; i < ex.Types.Length; i++)
                    if (ex.Types[i] != null)
                        sb.AppendFormat("\t{0} loaded\r\n", ex.Types[i].Name);

                for (int i = 0; i < ex.LoaderExceptions.Length; i++)
                    if (ex.LoaderExceptions[i] != null)
                        sb.AppendFormat("\texception {0}\r\n", ex.LoaderExceptions[i].Message);
            }
        }

        private string GetTagName(Type type)
        {
            string name = String.Empty;
            object[] tagName = type.GetCustomAttributes(typeof(NDjango.Interfaces.NameAttribute), false);
            foreach (NDjango.Interfaces.NameAttribute item in tagName)
            {
                name = item.Name;
            }
            return name;
        }

        private string GetAssemblyPath(string value)
        {
            StringBuilder baseBaseDirectory = new StringBuilder(AppDomain.CurrentDomain.BaseDirectory);
            baseBaseDirectory.Append("bin");
            return CombinePaths(baseBaseDirectory.ToString(), value);
        }

        private string CombinePaths(string p1, string p2)
        {
            string combination = String.Empty;
            try
            {
                combination = Path.Combine(p1, p2);
            }
            catch (Exception e)
            {
            }
            return combination;
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
