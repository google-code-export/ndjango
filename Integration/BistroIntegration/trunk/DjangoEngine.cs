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

        private static ConfigurationLoader loader;

        public static TemplateManagerProvider Provider
        {
            get
            {
                lock (lockObj)
                {
                    if (provider == null)
                    {
                        loader = new ConfigurationLoader();
                        ITemplateLoader templateLoader = new IntegrationTemplateLoader();
                        provider =
                            new TemplateManagerProvider()
                                .WithLoader(templateLoader)
                                .WithFilters(loader.GetFilters())
                                .WithTags(loader.GetTags())
                                .WithTag("url", new BistroUrlTag(HttpRuntime.AppDomainAppVirtualPath))
                                .WithFilters(NDjango.FiltersCS.FilterManager.GetFilters());

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
}