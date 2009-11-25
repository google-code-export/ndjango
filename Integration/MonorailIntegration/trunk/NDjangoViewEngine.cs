using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Castle.MonoRail.Framework;
using Castle.Core;
using NDjango;
using NDjango.Interfaces;
using NDjango.FiltersCS;
using System.Web;
using System.IO;
using System.Collections;

namespace NDjango.MonorailIntegration
{
    public class NDjangoViewEngine : ViewEngineBase, IInitializable
    {
        /// <summary>
        /// Key to use in HttpApplicationState for storing ITemplateManager.
        /// </summary>
        internal const String cDjangoManagerKey = "_djangoManagerKey";
        /// <summary>
        /// Template extension
        /// </summary>
        internal const String cTemplateExtension = ".django";

        /// <summary>
        /// Template Manager Provider will store here.
        /// </summary>
        private TemplateManagerProvider managerProvider;

        #region IInitializable Members
        /// <summary>
        /// initializing managerProvider and loader.
        /// </summary>
        public void Initialize()
        {
            managerProvider =
                new TemplateManagerProvider()
                    .WithLoader(new TemplateLoader())
                    //.WithFilters(loader.GetFilters())
                    //.WithTags(loader.GetTags())
                    //.WithTag("url", new AspMvcUrlTag())
                    .WithFilters(FilterManager.GetFilters());
        }

        #endregion

        #region HttpApplication and IRailsEngineContext
        

        /// <summary>
        /// This method extracts HttpApplicationState from the context and adds ITemplateManager to it.
        /// This should be done, because we need new instance of the ITemplateManager for each thread
        /// </summary>
        /// <param name="engineContext"></param>
        /// <returns></returns>
        private ITemplateManager GetManagerByRailsContext(IRailsEngineContext engineContext)
        {
            HttpApplicationState app = engineContext.UnderlyingContext.Application;
            // If there's no manager - managerProvider will return new one for us.
            if (app[cDjangoManagerKey] == null)
            {
                // Since one HttpApplication processed by a single thread - we don't need no locking here.
                app[cDjangoManagerKey] = managerProvider.GetNewManager();
            }
            ITemplateManager mgr = app[cDjangoManagerKey] as ITemplateManager;
            if (mgr == null)
            {
                if (Logger.IsErrorEnabled)
                {
                    Logger.Error("Couldn't get ITemplateManager from the HttpApplicationState");
                }
                else
                {
                    throw new ApplicationException("Couldn't get ITemplateManager from the HttpApplicationState");
                }
            }
            return mgr;
        }

        #endregion

        #region Template names
        /// <summary>
        /// Resolves the template name into a velocity template file name.
        /// </summary>
        protected string ResolveTemplateName(string templateName)
        {
            if (Path.HasExtension(templateName))
            {
                return templateName;
            }
            else
            {
                return templateName + cTemplateExtension;
            }
        }

        #endregion

        #region Context creation
        /// <summary>
        /// Creates the context form the Rails Context and the controller.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="controller">The controller.</param>
        /// <returns></returns>
        protected IDictionary<string, object> CreateContext(IRailsEngineContext context, Controller controller)
        {
            IDictionary<string,object> ndjangoContext = new Dictionary<string,object>();
            ndjangoContext.Add(TemplateKeys.Controller, controller);
            ndjangoContext.Add(TemplateKeys.Context, context);
            ndjangoContext.Add(TemplateKeys.Request, context.Request);
            ndjangoContext.Add(TemplateKeys.Response, context.Response);
            ndjangoContext.Add(TemplateKeys.Session, context.Session);

            if (controller.Resources != null)
            {
                foreach (String key in controller.Resources.Keys)
                {
                    ndjangoContext[key] = controller.Resources[key];
                }
            }

            foreach (String key in context.Params.AllKeys)
            {
                if (key == null) continue; // Copied from nvelocity.
                object value = context.Params[key];
                if (value == null) continue;
                ndjangoContext[key] = value;
            }


            if (controller.PropertyBag != null)
            {
                foreach (DictionaryEntry entry in controller.PropertyBag)
                {
                    if (entry.Value == null) continue;
                    ndjangoContext[(string)(entry.Key)] = entry.Value;
                }
            }

            ndjangoContext[TemplateKeys.SiteRoot] = context.ApplicationPath;
            return ndjangoContext;

        }

        #endregion

        #region IViewEngine implementation
        /// <summary>
        /// Not Implemented - Implementors should return a generator instance if
        /// the view engine supports JS generation.
        /// </summary>
        /// <param name="context">The request context.</param>
        /// <returns>A JS generator instance</returns>
        public override object CreateJSGenerator(IRailsEngineContext context)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Not Implemented - Processes the js generation view template - using the templateName
        /// to obtain the correct template, and using the specified <see cref="T:System.IO.TextWriter"/>
        /// to output the result.
        /// </summary>
        /// <param name="output">The output.</param>
        /// <param name="context">The request context.</param>
        /// <param name="controller">The controller.</param>
        /// <param name="templateName">Name of the template.</param>
        public override void GenerateJS(System.IO.TextWriter output, IRailsEngineContext context, Controller controller, string templateName)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Not Implemented - Evaluates whether the specified template exists.
        /// </summary>
        /// <param name="templateName"></param>
        /// <returns><c>true</c> if it exists</returns>
        public override bool HasTemplate(string templateName)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Not Implemented - Gets the JS generator file extension.
        /// </summary>
        /// <value>The JS generator file extension.</value>
        public override string JSGeneratorFileExtension
        {
            get { throw new NotImplementedException(); }
        }

        /// <summary>
        /// Processes the view - using the templateName
        /// to obtain the correct template
        /// and writes the results to the System.IO.TextWriter.
        /// </summary>
        /// <param name="output"></param>
        /// <param name="context"></param>
        /// <param name="controller"></param>
        /// <param name="templateName"></param>
        public override void Process(System.IO.TextWriter output, IRailsEngineContext context, Controller controller, string templateName)
        {
            ITemplateManager mgr = GetManagerByRailsContext(context);
            AdjustContentType(context);

            string resolvedName = ResolveTemplateName(templateName);

            var djangoContext = CreateContext(context,controller);


            try
            {
                TextReader reader = mgr.RenderTemplate(resolvedName, djangoContext);
                char[] buffer = new char[4096];
                int count = 0;
                while ((count = reader.ReadBlock(buffer, 0, 4096)) > 0)
                    output.Write(buffer, 0, count);
            }
            catch (Exception ex)
            {
                if (Logger.IsErrorEnabled)
                {
                    Logger.Error("Could not render view", ex);
                }

                throw new RailsException("Could not render view: " + resolvedName, ex);
            }
        }

        /// <summary>
        /// Processes the view - using the templateName
        /// to obtain the correct template,
        /// and using the context's response's output to output the result. 
        /// </summary>
        /// <param name="context"></param>
        /// <param name="controller"></param>
        /// <param name="templateName"></param>
        public override void Process(IRailsEngineContext context, Controller controller, string templateName)
        {
            Process(context.Response.Output, context, controller, templateName);
        }

        /// <summary>
        /// Not Implemented - Processes the contents.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="controller">The controller.</param>
        /// <param name="contents">The contents.</param>
        public override void ProcessContents(IRailsEngineContext context, Controller controller, string contents)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Not Implemented - Should process the specified partial. The partial name must contains
        /// the path relative to the views folder.
        /// </summary>
        /// <param name="output">The output.</param>
        /// <param name="context">The request context.</param>
        /// <param name="controller">The controller.</param>
        /// <param name="partialName">The partial name.</param>
        public override void ProcessPartial(System.IO.TextWriter output, IRailsEngineContext context, Controller controller, string partialName)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Gets a value indicating whether [supports JS generation].
        /// </summary>
        /// <value>
        /// 	<c>true</c> if [supports JS generation]; otherwise, <c>false</c>.
        /// </value>
        public override bool SupportsJSGeneration
        {
            get { return false; }
        }

        /// <summary>
        /// Gets the view file extension for ndjango templates.
        /// </summary>
        /// <value>The view file extension for ndjango templates.</value>
        public override string ViewFileExtension
        {
            get { return cTemplateExtension; }
        }
        #endregion
    }
}
