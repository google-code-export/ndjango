using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NDjango.Designer.Parsing;

namespace NDjango.Interfaces
{
    public interface IParser
    {
        List<INode> Parse(IEnumerable<string> template);
    }

    public enum NodeType
    {
        Tag,
        Marker,
        TagName,
        Keyword,
        Variable,
        Expression,
        Reference,
        Filter,
        FilterName,
        FilterParam
    };

    public static class Constants
    {
        public const string NODELIST_TAG_ELEMENTS = "standard.elements";
        public const string NODELIST_TAG_CHILDREN = "standard.children";
        public const string NODELIST_IFTAG_IFTRUE = "if.true.children";
        public const string NODELIST_IFTAG_IFFALSE = "if.false.children";
    }

    public struct Error
    {
        public Error(int severity, string message)
        {
            this.message = message;
            this.severity = severity;
        }
        public int Severity { get { return severity; } }
        public string Message { get { return message; } }
        private int severity;
        private string message;
    }

    public interface INode
    {
        NodeType NodeType { get; }
        int Position { get; }
        int Length { get; }
        string Text { get; }
        IEnumerable<string> Values { get; }
        Error ErrorMessage { get; }
        Dictionary<string, IEnumerable<INode>> Nodes { get; }
    }
}
