using System.Collections.Generic;
using Bistro.Controllers.OutputHandling;
using Bistro.Controllers;
using System.IO;
using System.Web;
using System;
using NDjango.Interfaces;
using System.Configuration;
using System.Reflection;
using System.Text;

namespace NDjango.BistroIntegration
{
    /// <summary>
    /// Bistro-specific implementation of the django {% url %} tag.
    /// 
    /// This tag will take a url in a String.Format format, and apply the 
    /// supplied parameters to it.
    /// </summary>
    public class BistroUrlTag : NDjango.Tags.Abstract.UrlTag
    {
        string rootDir;

        public BistroUrlTag(string rootDir)
        {
            // trim to guarantee it's not there, then add to not do it every time
            this.rootDir = rootDir.TrimEnd('/') + '/';
        }

        public override string GenerateUrl(string pathTemplate, string[] parameters, NDjango.Interfaces.IContext context)
        {
            return rootDir + String.Format(pathTemplate.Trim('/'), parameters);
        }
    }

    /// <summary>
    /// Standalone class for Template loader.
    /// </summary>
    internal class IntegrationTemplateLoader : NDjango.Interfaces.ITemplateLoader
    {

        internal IntegrationTemplateLoader()
        {

            rootDir = HttpRuntime.AppDomainAppPath;
        }


        string rootDir;

        #region ITemplateLoader Members

        public TextReader GetTemplate(string name)
        {
            return File.OpenText(Path.Combine(rootDir, name));
        }

        public bool IsUpdated(string name, System.DateTime timestamp)
        {
            return File.GetLastWriteTime(Path.Combine(rootDir, name)) > timestamp;
        }

        #endregion
    }


    

    /// <summary>
    /// Integration point for django into the bistro rendering framework
    /// </summary>
    public class DjangoEngine : TemplateEngine
    {
        readonly string errorTemplate =
@"
<head>
    <title>Exception Processing Request</title>
</head>
<body>
<h1 style=""color: Red"">Exception processing {0}</h1>
<p />
<h2>{1}</h2><p />
<pre>{2}</pre>
</body>
</html>
";

        private static TemplateManagerProvider provider;

        private static object lockObj = new object();

        public static TemplateManagerProvider Provider
        {
            get
            {
                // lock must be here.
                lock (lockObj)
                {
                    if (provider == null)
                    {
                        ITemplateLoader loader = new IntegrationTemplateLoader();
                        provider = new TemplateManagerProvider().WithLoader(loader).WithTag("url", new BistroUrlTag(HttpRuntime.AppDomainAppVirtualPath));
                        provider = NDjango.FiltersCS.FilterManager.Initialize(provider);


                        NDjangoRegisterTemplate NDjangoRegisterTemplate = new NDjangoRegisterTemplate();
                        NDjangoRegisterTemplate.Provider = provider;
                        NDjangoRegisterTemplate.RegisterTemplates();
                        provider = NDjangoRegisterTemplate.Provider;
                    }
                }
                return provider;
            }
            set
            {
                lock (lockObj)
                {
                    provider = value;
                }
            }
        }



        public DjangoEngine(IHttpHandler handler)
        {
            manager = Provider.GetNewManager();
        }
        #region private members
        private NDjango.Interfaces.ITemplateManager manager;
        #endregion

        public override void Render(HttpContextBase httpContext, Bistro.Controllers.IContext requestContext)
        {
            if (httpContext.Session != null)
                foreach (object key in httpContext.Session.Keys)
                {
                    if (requestContext.Contains(key))
                        throw new ApplicationException(String.Format("{0} is present on both the Session and the Request.", key));

                    requestContext.Add(key.ToString(), httpContext.Session[key.ToString()]);
                }

            try
            {
                TextReader reader = manager.RenderTemplate(requestContext.Response.RenderTarget, (IDictionary<string, object>)requestContext);
                char[] buffer = new char[4096];
                int count = 0;
                while ((count = reader.ReadBlock(buffer, 0, 4096)) > 0)
                    httpContext.Response.Write(buffer, 0, count);
            }
            catch (Exception ex)
            {
                httpContext.Response.StatusCode = 500;
                httpContext.Response.Write(RenderException(requestContext.Response.RenderTarget, ex, true));
            }
        }

        public string RenderException(string request, Exception ex, bool showTrace)
        {
            return String.Format(errorTemplate, request, ex.Message, showTrace ? ex.ToString() : String.Empty);
        }



    }


    public class NDjangoRegisterTemplate
    {
        const string NDJangoCommonSection = "NDJangoCommonSection";

        string[] Sections = new string[] { NDJangoCommonSection };
        private TemplateManagerProvider provider;
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
        public NDjangoRegisterTemplate()
        {
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
            //NDjangoSectionHandler section isn't exist
            if (nameValueSection == null)
            {
                return;
            }

            foreach (NameValueClassElement item in nameValueSection.NDJangoTagFilterSectionCollection)
            {
                CreateInstanceByAssemblyName(item.Name);
            }

            foreach (NameValueElementAssembly item in nameValueSection.NDJangoSectionCollection)
            {
                if (item.ImportCollection.Count == 0)
                {
                    //register all tag and filter
                    //RegisterGroupOfTemplates(item.Name);
                    LoadByAssemblyName(item.Name);
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
            provider = provider.WithTag(name, tag);
        }

        //Register Filter by Name
        private void RegisterISimpleFilter(string name, ISimpleFilter filter)
        {
            provider = provider.WithFilter(name, filter);
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

                        provider = provider.WithSetting(name, result);
                        isNewSetting = false;
                    }
                }
            }
            if (isNewSetting)
            {
                provider = provider.WithSetting(name, result);
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

        //Register Group of filters and tags By Full Assembly Name
        private void RegisterGroupOfTagFilterByFullAssemblyName(string value)
        {
            try
            {
                LoadAssembly(String.Empty, value);
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

        //Create instance by full assembly name
        private void CreateInstanceByAssemblyName(string AssemblyName)
        {
            try
            {
                Type type = Type.GetType(AssemblyName);
                if (type.GetInterface(typeof(ITag).Name) != null)
                {
                    ITag tag = (ITag)Activator.CreateInstance(type);
                    RegisterITag(GetTagName(type), tag);
                }
                else if (type.GetInterface(typeof(ISimpleFilter).Name) != null)
                {
                    ISimpleFilter filter = (ISimpleFilter)Activator.CreateInstance(type);
                    RegisterISimpleFilter(GetTagName(type), filter);
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

        //Load assemblies by assembly name
        private void LoadByAssemblyName(string assemblyName)
        {
            try
            {
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
