using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Text.RegularExpressions;
using NDjango.Interfaces;

namespace NDjango.Designer.Parsing
{
    [Export(typeof(IParser))]
    public class Parser : IParser
    {
        public List<INode> Parse(IEnumerable<string> template)
        {
            var result = new List<INode>();
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
                    result.Add(new Node(token_start, 2, NodeType.Marker, string.Empty));
                    result.Add(new Node(line_start + end, 2, NodeType.Marker, string.Empty));
                    Node tagToken = new Node(token_start, end + 2 - start, NodeType.Tag,
                        line.Substring(start, end + 2 - start).Replace("{%", string.Empty).Replace("%}", string.Empty));
                    result.Add(tagToken);
                    CreateInnerChildNodes(tagToken);
                    
                    pos = end;
                }
                line_start += line.Length;
            }
            return result;
        }

        private void CreateInnerChildNodes(Node tagToken)
        {
            if (tagToken.Type != NodeType.Tag)
                return;

            string[] lexemes = tagToken.TagName.Split(' ');
            foreach (string lexeme in lexemes)
            {
                if (lexeme != string.Empty)
                {
                    tagToken.AddChildNode(new Node(0, 0, IdentifyTokenType(tagToken, lexeme), lexeme), Node.PurposeType.InnerNodes);
                }
            }
        }

        private NodeType IdentifyTokenType(Node parentToken, string lexeme)
        {
            //not implemented
            return NodeType.Keyword;
        }

        static Regex r = new Regex("{{.*}}", RegexOptions.Compiled);
    }
}
