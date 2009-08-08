using System;
using System.Collections.Generic;
using System.Text;
using System.Web.Mvc;
using System.IO;

namespace NDjango.ASPMVCIntegration
{
    /// <summary>
    /// Implementation of IView interface for the NDjango engine
    /// </summary>
    public class NDjangoView: IView
    {
        /// <summary>
        /// Simple wrapper class to satisfy IViewDataContainer requirements
        /// </summary>
        class ViewDataContainerWrapper : IViewDataContainer
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="ViewDataContainerWrapper"/> class.
            /// </summary>
            /// <param name="data">The data.</param>
            public ViewDataContainerWrapper(ViewDataDictionary data)
            {
                this.ViewData = data;
            }

            public ViewDataDictionary ViewData { get; set; }
        }


        public const string aspmvcContextKey = "__asp.mvc.context.key__";

        /// <summary>
        /// Template path
        /// </summary>
        protected string path;

        /// <summary>
        /// Initializes a new instance of the <see cref="NDjangoView"/> class.
        /// </summary>
        /// <param name="path">The path.</param>
        public NDjangoView(string path)
        {
            this.path = path;
        }

        /// <summary>
        /// Gets the ndjango manager supplied by the NDjangoHandle.
        /// </summary>
        /// <param name="viewContext">The view context.</param>
        /// <returns></returns>
        protected virtual NDjango.Interfaces.ITemplateManager GetManager(ViewContext viewContext)
        {
            return (NDjango.Interfaces.ITemplateManager)viewContext.HttpContext.Items[NDjangoHandle.MANAGER_HANDLE];
        }

        /// <summary>
        /// Updates the manager with the one returned by rendering operations
        /// </summary>
        /// <param name="manager">The manager.</param>
        /// <param name="viewContext">The view context.</param>
        protected virtual void ReturnManager(NDjango.Interfaces.ITemplateManager manager, ViewContext viewContext)
        {
            viewContext.HttpContext.Items[NDjangoHandle.MANAGER_HANDLE] = manager;
        }

        /// <summary>
        /// Renders the specified view context.
        /// </summary>
        /// <param name="viewContext">The view context.</param>
        /// <param name="writer">The writer.</param>
        public void Render(ViewContext viewContext, TextWriter writer)
        {
            var manager = GetManager(viewContext);
            var requestContext = new Dictionary<string, object>();

            foreach (string key in viewContext.ViewData.Keys)
                requestContext.Add(key, viewContext.ViewData[key]);

            requestContext.Add("model", viewContext.ViewData.Model);

            var modelState = new Dictionary<string, object>();
            foreach (string key in viewContext.ViewData.ModelState.Keys)
            {
                var value = viewContext.ViewData.ModelState[key].Value;

                if (value != null)
                    modelState.Add(key, value.AttemptedValue);
            }

            requestContext.Add("modelState", modelState);
            requestContext.Add("fullModelState", viewContext.ViewData.ModelState);
            requestContext.Add("htmlHelper", 
                new HtmlHelper(viewContext, new ViewDataContainerWrapper(viewContext.ViewData)));

            if (viewContext.HttpContext.Session != null)
                foreach (object key in viewContext.HttpContext.Session.Keys)
                {
                    if (requestContext.ContainsKey(key.ToString()))
                        throw new ApplicationException(String.Format("{0} is present on both the Session and the Request.", key));

                    requestContext.Add(key.ToString(), viewContext.HttpContext.Session[key.ToString()]);
                }

            requestContext.Add(aspmvcContextKey, viewContext.RequestContext);

            var templateTuple = manager.RenderTemplate(path, requestContext);
            manager = templateTuple.Item1;
            TextReader reader = templateTuple.Item2;
            char[] buffer = new char[4096];
            int count = 0;
            while ((count = reader.ReadBlock(buffer, 0, 4096)) > 0)
                writer.Write(buffer, 0, count);

            ReturnManager(manager, viewContext);
        }
    }
}
