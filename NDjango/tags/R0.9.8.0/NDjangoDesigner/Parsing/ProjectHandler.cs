﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio;
using System.ComponentModel.Composition;
using Microsoft.VisualStudio.Editor;
using Microsoft.FSharp.Collections;
using NDjango.Interfaces;

namespace NDjango.Designer.Parsing
{
    public class ProjectHandler: IDisposable
    {
        
        public TemplateDirectory TemplateDirectory { get; private set; }

        private NodeProviderBroker broker;

        public ProjectHandler(NodeProviderBroker broker, IVsHierarchy hier, string project_directory)
        {
            this.broker = broker;
            template_loader = new TemplateLoader(project_directory);
            type_resolver = new TypeResolver(hier);
            TemplateDirectory = new TemplateDirectory(project_directory);
            parser = new TemplateManagerProvider()
                    .WithTags(type_resolver.Tags)
                    .WithFilters(type_resolver.Filters)
                    .WithSetting(NDjango.Constants.EXCEPTION_IF_ERROR, false)
                    .WithLoader(template_loader)
                    .GetNewManager();
        }

        ITemplateManager parser;

        TemplateLoader template_loader;
        private List<Tag> tags;
        private List<Filter> filters;
        private string project_directory;
        TypeResolver type_resolver;

        /// <summary>
        /// Retrieves or creates a node provider for a buffer
        /// </summary>
        /// <param name="buffer"></param>
        /// <returns></returns>
        internal NodeProvider GetNodeProvider(ITextBuffer buffer, IVsHierarchy hier, string filename)
        {
            var provider = new NodeProvider(this, filename, type_resolver);
            buffer.Properties.AddProperty(typeof(NodeProvider), provider);
            template_loader.Register(filename, buffer, provider);
            return provider;
        }

        /// <summary>
        /// Parses the template
        /// </summary>
        /// <param name="template">a reader with the template</param>
        /// <returns>A list of the syntax nodes</returns>
        public FSharpList<INodeImpl> ParseTemplate(string filename, ITypeResolver resolver)
        {
            return parser.GetTemplate(filename, resolver, new NDjango.TypeResolver.ModelDescriptor(GetDefaultModel(filename))).Nodes;
        }

        protected virtual IEnumerable<IDjangoType> GetDefaultModel(string filename)
        {
            return new List<IDjangoType>();
        }

        public ITextSnapshot GetSnapshot(string filename)
        {
            return template_loader.GetSnapshot(filename);
        }


        internal void Unregister(string filename)
        {
            template_loader.Unregister(filename);
        }

        internal void RemoveDiagnostics(Microsoft.VisualStudio.Shell.ErrorTask errorTask)
        {
            broker.RemoveDiagnostics(errorTask);
        }

        internal void ShowDiagnostics(Microsoft.VisualStudio.Shell.ErrorTask errorTask)
        {
            broker.ShowDiagnostics(errorTask);
        }

        #region IDisposable Members

        public void Dispose()
        {
            type_resolver.Dispose();
        }

        #endregion
    }
}
