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
    public class ProjectHandler
    {
        
        public ITemplateDirectory TemplateManager { get; private set; }

        private NodeProviderBroker broker;

        public ProjectHandler(NodeProviderBroker broker, ITemplateDirectory template_manager, List<Tag> tags, List<Filter> filters, string project_directory)
        {
            this.broker = broker;
            TemplateManager = template_manager;
            this.tags = tags;
            this.filters = filters;
            this.project_directory = project_directory;
            template_loader = new TemplateLoader(project_directory);
            parser = InitializeParser();
        }

        ITemplateManager parser;

        TemplateLoader template_loader;
        private List<Tag> tags;
        private List<Filter> filters;
        private string project_directory;

        private ITemplateManager InitializeParser()
        {

            TemplateManagerProvider provider = new TemplateManagerProvider();
            return provider
                    .WithTags(tags)
                    .WithFilters(filters)
                    .WithSetting(NDjango.Constants.EXCEPTION_IF_ERROR, false)
                    .WithLoader(template_loader)
                    .GetNewManager();

        }

        /// <summary>
        /// Retrieves or creates a node provider for a buffer
        /// </summary>
        /// <param name="buffer"></param>
        /// <returns></returns>
        internal NodeProvider GetNodeProvider(string project_directory, ITextBuffer buffer, IVsHierarchy hier, string filename)
        {
            var provider =
                new NodeProvider(
                    this,
                    filename,
                    new TypeResolver(GlobalServices.TypeService.GetContextTypeResolver(hier), GlobalServices.TypeService.GetTypeResolutionService(hier)));
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
            return parser.GetTemplate(filename, resolver).Nodes;
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
    }
}
