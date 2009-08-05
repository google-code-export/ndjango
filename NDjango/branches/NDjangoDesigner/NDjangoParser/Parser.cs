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
                    result.AddRange(CreateInnerChildNodes(tagToken));
                    
                    pos = end;
                }
                line_start += line.Length;
            }
            return result;
        }

        private List<INode> CreateInnerChildNodes(INode node)
        {
            List<INode> result = new List<INode>();
            if (node.Text.Contains(" "))
            {
                node.NodeType = NodeType.Tag;
                string[] lexemes = node.Text.Split(' ');
                foreach (string lexeme in lexemes)
                {
                    if (lexeme != string.Empty)
                    {
                        Node childNode = new Node(node.Position, node.Length, NodeType.TagName, lexeme);
                        ((Node)node).AddChildNode(childNode, Constants.NODELIST_TAG_ELEMENTS);
                        result.Add(childNode);
                        result.AddRange(CreateInnerChildNodes(childNode));
                    }
                }
            }
            else if(node.Text.Contains("."))
            {
                node.NodeType = NodeType.Expression;
                string[] lexemes = node.Text.Split('.');
                foreach (string lexeme in lexemes)
                {
                    if (lexeme != string.Empty)
                    {
                        Node childNode = new Node(node.Position, node.Length, NodeType.Variable, lexeme);
                        ((Node)node).AddChildNode(childNode, Constants.NODELIST_TAG_CHILDREN);
                        result.Add(childNode);
                    }
                }
            }
            else if (node.Text.Contains("|"))
            {
                node.NodeType = NodeType.Filter;
                string[] lexemes = node.Text.Split('|');
                foreach (string lexeme in lexemes)
                {
                    if (lexeme != string.Empty)
                    {
                        Node childNode = new Node(node.Position, node.Length, NodeType.FilterName, lexeme);
                        ((Node)node).AddChildNode(childNode, Constants.NODELIST_TAG_CHILDREN);
                        result.Add(childNode);
                    }
                }
            }
            return result;
        }

        private NodeType IdentifyTokenType(Node parentToken, string lexeme)
        {
            //not implemented
            return NodeType.TagName;
        }

        static Regex r = new Regex("{{.*}}", RegexOptions.Compiled);
    }
}
