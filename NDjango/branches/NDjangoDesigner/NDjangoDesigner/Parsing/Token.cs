using System.Collections.Generic;
using NDjango;

namespace NDjango.Designer.Parsing
{
    /// <summary>
    /// Provides basic information which is necessary for all kinds of designer features.
    /// </summary>
    public struct Token
    {
        public enum TokenType { Tag, Keyword, Variable, VariableDefined, Reference, Marker, Filter };
        public enum PurposeType { TagList, InnerNodes, Filters, Fields };
        /// <summary>
        /// Type of the token. Depending of type, tokens may provide different functionality,
        /// may have or not have child tokens.
        /// </summary>
        public TokenType Type;
        public int Position;
        public int Length;
        public string TagName;
        public List<string> Values;
        public List<string> Errors;
        /// <summary>
        /// defines the child nodes, distributed by their type (purpose).
        /// Every node at least will have two child nodes lists.
        /// </summary>
        public Dictionary<PurposeType, List<Token>> ChildNodesByPurpose;
        
        /// <summary>
        /// Creates a new token.
        /// </summary>
        /// <param name="position"></param>
        /// <param name="length"></param>
        /// <param name="tokenType">type of the token</param>
        /// <param name="Text">tag name</param>
        public Token(int position, int length, TokenType tokenType, string Text, List<Token> tagList, List<Token> innerNodes)
        {
            // TODO: Complete member initialization
            this.TagName = Text;
            this.Position = position;
            this.Length = length;
            this.Type = tokenType;

            Values = new List<string>();
            Errors = new List<string>();
            ChildNodesByPurpose = new Dictionary<PurposeType, List<Token>>();
            ChildNodesByPurpose.Add(PurposeType.TagList, tagList);
            ChildNodesByPurpose.Add(PurposeType.InnerNodes, innerNodes);
        }

        public void GenerateCompletionValues(List<string> variables)
        {
            Values.Clear();
            switch (Type)
            {
                case TokenType.Keyword:
                    Values.AddRange(new string[] { "for", "if", "block", "endif" });
                    break;
                case TokenType.Tag:
                    Values.AddRange(new string[] { "aaa", "bbb", "ccc", "hhh"});
                    break;
                case TokenType.Variable:
                    Values.AddRange(variables);
                    break;
                case TokenType.Filter:
                    Values.AddRange(new string[] { "add", "addslashes", "default", "default_if_none", "cut" });
                    break;
                default:
                    break;
            }
        }

        public void AddChildNode(Token token, PurposeType purpose)
        {
            ChildNodesByPurpose[purpose].Add(token);
        }
    }
}
