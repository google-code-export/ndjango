using System.Collections.Generic;

namespace NDjango.Designer.Parsing
{
    public struct Token
    {
        public enum TokenType { Tag, Keyword, Variable, Reference, Marker };
        public TokenType Type;
        public int Position;
        public int Length;
        public List<string> Values;
        public List<string> Errors;

        public Token(int position, int length, TokenType tokenType)
        {
            // TODO: Complete member initialization
            this.Position = position;
            this.Length = length;
            this.Type = tokenType;
            Values = new List<string>(new string[] {"a","aa","b", "bb"});
            Errors = new List<string>();
        }
    }
}
