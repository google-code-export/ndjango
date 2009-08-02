using System.Collections.Generic;
using NDjango;
using NDjango.Interfaces;

namespace NDjango.Designer.Parsing
{
    /// <summary>
    /// Provides basic information which is necessary for all kinds of designer features.
    /// </summary>
    public struct Node : INode
    {
        //public enum TokenType { Tag, Marker, TagName, Keyword, Variable, Expression, Reference, Filter, FilterName, FilterParam };

        public enum PurposeType { TagList, InnerNodes, Filters, Fields };
        /// <summary>
        /// Type of the node. Depending of type, nodes may provide different functionality
        /// </summary>
        public NodeType Type;
        public int Position;
        public int Length;
        public string TagName;
        public List<string> Values;
        public string Info;
        public Error Error;
        /// <summary>
        /// defines the child nodes, distributed by their type (purpose).
        /// Every node at least will have two child nodes lists.
        /// </summary>
        public Dictionary<string, IEnumerable<INode>> ChildNodesByPurpose;
        
        /// <summary>
        /// Creates a new node.
        /// </summary>
        /// <param name="position"></param>
        /// <param name="length"></param>
        /// <param name="tokenType">type of the token</param>
        /// <param name="Text">tag name</param>
        public Node(int position, int length, NodeType tokenType, string Text)
        {
            // TODO: Complete member initialization
            this.TagName = Text;
            this.Position = position;
            this.Length = length;
            this.Type = tokenType;
            this.Info = "Quick info about " + Text + " tag";

            Values = new List<string>();
            Error = new Error();
            ChildNodesByPurpose = new Dictionary<string, IEnumerable<INode>>();
            //ChildNodesByPurpose.Add(Constants.NODELIST_TAG_CHILDREN, tagList);
            //ChildNodesByPurpose.Add(Constants.NODELIST_TAG_CHILDREN, innerNodes);
        }

        public void GenerateCompletionValues(List<string> variables)
        {
            Values.Clear();
            switch (Type)
            {
                case NodeType.Keyword:
                    Values.AddRange(new string[] { "for", "if", "block", "endif" });
                    break;
                case NodeType.Tag:
                    Values.AddRange(new string[] { "aaa", "aba", "aca", "bbb", "ccc", "hhh" });
                    break;
                case NodeType.Variable:
                    Values.AddRange(variables);
                    break;
                case NodeType.Filter:
                    Values.AddRange(new string[] { "add", "addslashes", "default", "default_if_none", "cut" });
                    break;
                default:
                    break;
            }
        }

        public void AddChildNode(Node token, PurposeType purpose)
        {
            //ChildNodesByPurpose[purpose].Add(token);
        }

        NodeType INode.NodeType
        {
            get { return Type; }
        }

        int INode.Position
        {
            get { return Position; }
        }

        int INode.Length
        {
            get { return Length; }
        }

        string INode.Text
        {
            get { return TagName; }
        }

        IEnumerable<string> INode.Values
        {
            get 
            {
                GenerateCompletionValues(new List<string>());
                return Values; 
            }
        }

        Error INode.ErrorMessage
        {
            get { return this.Error; }
        }

        string INode.Info
        {
            get
            {
                return this.Info;
            }
        }

        Dictionary<string, IEnumerable<INode>> INode.Nodes
        {
            get { return ChildNodesByPurpose; }
        }
    }
}
