using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.Text;
using System.ComponentModel.Composition;

namespace NDjango.Designer.Parsing
{
    internal interface IParserController
    {
        Tokenizer GetTokenizer(ITextBuffer buffer);
        bool IsNDjango(ITextBuffer buffer);
        List<Token> Parse(IEnumerable<string> template);
    }

    [Export(typeof(IParserController))]
    internal class ParserProvider : IParserController
    {
        [Import]
        internal IParser parser { get; set; }

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

        public Tokenizer GetTokenizer(ITextBuffer buffer)
        {
            Tokenizer tokenizer;
            if (!buffer.Properties.TryGetProperty(typeof(Tokenizer), out tokenizer))
                buffer.Properties.AddProperty(typeof(Tokenizer), tokenizer = new Tokenizer(this, buffer));
            return tokenizer;
        }

        public List<Token> Parse(IEnumerable<string> template)
        {
            return parser.Parse(template);
        }
    }
}
