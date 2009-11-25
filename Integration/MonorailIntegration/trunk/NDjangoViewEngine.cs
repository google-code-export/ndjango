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
        internal const String cTemplateExtension = ".django";

        private TemplateManagerProvider managerProvider;

        #region IInitializable Members
        /// <summary>
        /// 
        /// </summary>
        public void Initialize()
        {
            ITemplateLoader loader = new TemplateLoader();
            managerProvider =
                new TemplateManagerProvider()
                    .WithLoader(loader)
                    //.WithFilters(loader.GetFilters())
                    //.WithTags(loader.GetTags())
                    //.WithTag("url", new AspMvcUrlTag())
                    .WithFilters(FilterManager.GetFilters());
        }

        #endregion

        #region HttpApplication and IRailsEngineContext
        

        /// <summary>
        /// This method extracts HttpApplicationState from the context and adds ITemplateManager to it.
        /// </summary>
        /// <param name="engineContext"></param>
        /// <returns></returns>
        private ITemplateManager GetManagerByRailsContext(IRailsEngineContext engineContext)
        {
            HttpApplicationState app = engineContext.UnderlyingContext.Application;
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
            IDictionary<string,object> newContext = new Dictionary<string,object>();

            if (controller.Resources != null)
            {
                foreach (String key in controller.Resources.Keys)
                {
                    newContext[key] = controller.Resources[key];
                }
            }

            foreach (String key in context.Params.AllKeys)
            {
                if (key == null) continue; 
                object value = context.Params[key];
                if (value == null) continue;
                newContext[key] = value;
            }


            if (controller.PropertyBag != null)
            {
                foreach (DictionaryEntry entry in controller.PropertyBag)
                {
                    if (entry.Value == null) continue;
                    newContext[(string)(entry.Key)] = entry.Value;
                }
            }
            return newContext;

        }

        #endregion

        #region IViewEngine implementation
        public override object CreateJSGenerator(IRailsEngineContext context)
        {
            throw new NotImplementedException();
        }

        public override void GenerateJS(System.IO.TextWriter output, IRailsEngineContext context, Controller controller, string templateName)
        {
            throw new NotImplementedException();
        }

        public override bool HasTemplate(string templateName)
        {
            throw new NotImplementedException();
        }

        public override string JSGeneratorFileExtension
        {
            get { throw new NotImplementedException(); }
        }

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

        public override void Process(IRailsEngineContext context, Controller controller, string templateName)
        {
            Process(context.Response.Output, context, controller, templateName);
        }

        /// <summary>
        /// Processes the contents.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="controller">The controller.</param>
        /// <param name="contents">The contents.</param>
        public override void ProcessContents(IRailsEngineContext context, Controller controller, string contents)
        {
            throw new NotImplementedException();
        }

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
