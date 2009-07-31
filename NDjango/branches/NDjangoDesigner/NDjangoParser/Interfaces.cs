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

    /// <summary>
    /// The type of the node. Depending on it, the node may provide different functionality
    /// </summary>
    public enum NodeType
    {
        /// <summary>
        /// The whole tag.
        /// <example>{% if somevalue %}</example>
        /// </summary>
        Tag,
        /// <summary>
        /// The markers, which frame django tag. 
        /// <example>{%, %}</example>
        /// </summary>
        Marker,
        /// <summary>
        /// Django template tag.
        /// <example> "with", "for", "ifequal"</example>
        /// </summary>
        TagName,
        /// <summary>
        /// The keyword, that is necessary for some tags.
        /// <example>"and", "as"</example>
        /// </summary>
        Keyword,
        /// <summary>
        /// The variable of expression.
        /// <example>In "User.DoB" expression "DoB" is a variable</example>
        /// </summary>
        Variable,
        /// <summary>
        /// Expression, which may contain variables and filters
        /// <example>User.DoB|date:"D d M Y"</example>
        /// </summary>
        Expression,
        /// <summary>
        /// 
        /// </summary>
        Reference,
        /// <summary>
        /// The filter or a group of filters. May contain parameters.
        /// <example>first|length|default:"nothing"</example>
        /// </summary>
        Filter,
        /// <summary>
        /// The name of the filter.
        /// <example>"length", "first", "default"</example>
        /// </summary>
        FilterName,
        /// <summary>
        /// Filter parameter.
        /// <example>any valid value</example>
        /// </summary>
        FilterParam
    };

    public static class Constants
    {
        public const string NODELIST_TAG_ELEMENTS = "standard.elements";
        public const string NODELIST_TAG_CHILDREN = "standard.children";
        public const string NODELIST_IFTAG_IFTRUE = "if.true.children";
        public const string NODELIST_IFTAG_IFFALSE = "if.false.children";
    }

    /// <summary>
    /// An error message consists of message, and severity level as well.
    /// </summary>
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
        /// <summary>
        /// Indicates the quick info message.
        /// </summary>
        string Info { get; }
        Dictionary<string, IEnumerable<INode>> Nodes { get; }
    }
}
