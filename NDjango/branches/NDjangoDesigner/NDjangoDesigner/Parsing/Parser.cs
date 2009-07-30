using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Text.RegularExpressions;
using Microsoft.VisualStudio.Text;

namespace NDjango.Designer.Parsing
{
    internal interface IParser
    {
        Tokenizer GetTokenizer(ITextBuffer buffer);
        bool IsNDjango(ITextBuffer buffer);
    }

    [Export(typeof(IParser))]
    internal class Parser : IParser
    {

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
            var result = new List<Token>();
            int line_start = 0;
            foreach (string line in template)
            {
                int pos = 0;
                while (pos < line.Length)
                {
                    int start = line.IndexOf("{%", pos);
                    if (start < 0)
                        break;
                    pos = start;
                    int token_start = line_start + start;
                    int end = line.IndexOf("%}", pos);
                    if (end < 0)
                        break;
                    result.Add(new Token(token_start, 2, Token.TokenType.Marker, string.Empty, new List<Token>(), new List<Token>()));
                    result.Add(new Token(line_start + end, 2, Token.TokenType.Marker, string.Empty, new List<Token>(), new List<Token>()));
                    Token tagToken = new Token(token_start, end + 2 - start, Token.TokenType.Tag,
                        line.Substring(start, end + 2 - start).Replace("{%", string.Empty).Replace("%}", string.Empty), new List<Token>(), new List<Token>());
                    result.Add(tagToken);
                    CreateInnerChildNodes(tagToken);
                    
                    pos = end;
                }
                line_start += line.Length;
            }
            return result;
        }

        private void CreateInnerChildNodes(Token tagToken)
        {
            if (tagToken.Type != Token.TokenType.Tag)
                return;

            string[] lexemes = tagToken.TagName.Split(' ');
            foreach (string lexeme in lexemes)
            {
                if (lexeme != string.Empty)
                {
                    tagToken.AddChildNode(new Token(0, 0, IdentifyTokenType(tagToken, lexeme), lexeme, new List<Token>(), new List<Token>()), Token.PurposeType.InnerNodes);
                }
            }
        }

        private Token.TokenType IdentifyTokenType(Token parentToken, string lexeme)
        {
            //not implemented
            return Token.TokenType.Keyword;
        }

        static Regex r = new Regex("{{.*}}", RegexOptions.Compiled);
    }
}
