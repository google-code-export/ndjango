using System.Collections.Generic;
using NDjango;

namespace NDjango.Designer.Parsing
{
    /// <summary>
    /// Provides basic information which is necessary for all kinds of designer features.
    /// </summary>
    public struct Token
    {
        public enum TokenType { Tag, Keyword, Variable, Reference, Marker };
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
        public List<Token> ChildTokens;
        
        /// <summary>
        /// Creates a new token.
        /// </summary>
        /// <param name="position"></param>
        /// <param name="length"></param>
        /// <param name="tokenType">type of the token</param>
        /// <param name="Text">tag name</param>
        public Token(int position, int length, TokenType tokenType, string Text)
        {
            // TODO: Complete member initialization
            this.TagName = Text;
            this.Position = position;
            this.Length = length;
            this.Type = tokenType;

            Values = new List<string>(new string[] {"for","if","block", "endif"});
            Errors = new List<string>();
            ChildTokens = new List<Token>();
        }

        public void AddChild(Token token)
        {
            ChildTokens.Add(token);
        }
    }
}
