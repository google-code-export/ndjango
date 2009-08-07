using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.Text;
using System.ComponentModel.Composition;
using NDjango.Interfaces;

namespace NDjango.Designer.Parsing
{
    internal interface INodeProviderBroker
    {
        NodeProvider GetNodeProvider(ITextBuffer buffer);
        bool IsNDjango(ITextBuffer buffer);
    }

    [Export(typeof(INodeProviderBroker))]
    internal class NodeProviderBroker : INodeProviderBroker
    {

        IParser parser = new NDjango.TemplateManagerProvider()
            .WithSetting(NDjango.Constants.EXCEPTION_IF_ERROR, false); // {get; set;}

     
        public bool IsNDjango(ITextBuffer buffer)
        {
            switch (buffer.ContentType.TypeName)
            {
                case "text":
                case "HTML":
                    return true;
                default: return false;
            }
        }

        public NodeProvider GetNodeProvider(ITextBuffer buffer)
        {
            NodeProvider provider;
            if (!buffer.Properties.TryGetProperty(typeof(NodeProvider), out provider))
                buffer.Properties.AddProperty(typeof(NodeProvider), provider = new NodeProvider(parser, buffer));
            return provider;
        }

    }
}
