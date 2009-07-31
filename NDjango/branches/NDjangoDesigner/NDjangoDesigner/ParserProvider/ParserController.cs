using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.Text;
using System.ComponentModel.Composition;
using NDjango.Interfaces;

namespace NDjango.Designer.Parsing
{
    internal interface IParserController
    {
        NodeProvider GetNodeProvider(ITextBuffer buffer);
        bool IsNDjango(ITextBuffer buffer);
        List<INode> Parse(IEnumerable<string> template);
    }

    [Export(typeof(IParserController))]
    internal class ParserController : IParserController
    {

        //[Import]
        IParser parser = new Parser();// {get; set;}

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
            NodeProvider tokenizer;
            if (!buffer.Properties.TryGetProperty(typeof(NodeProvider), out tokenizer))
                buffer.Properties.AddProperty(typeof(NodeProvider), tokenizer = new NodeProvider(this, buffer));
            return tokenizer;
        }

        public List<INode> Parse(IEnumerable<string> template)
        {
            return parser.Parse(template);
        }
    }
}
