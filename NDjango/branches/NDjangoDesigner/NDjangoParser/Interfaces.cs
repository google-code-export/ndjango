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
        /// The keyword, as required by some tags.
        /// <example>"and", "as"</example>
        /// </summary>
        Keyword,

        /// <summary>
        /// The variable definition used in tags which introduce new variables i.e. 
        /// loop variable in the For tag.
        /// <example>loop_item</example>
        /// </summary>
        Variable,

        /// <summary>
        /// Expression, which consists of a reference followed by 0 or more filters
        /// <example>User.DoB|date:"D d M Y"</example>
        /// </summary>
        Expression,
        
        /// <summary>
        /// Reference to a value in the current context.
        /// <example>User.DoB</example>
        /// </summary>
        Reference,

        /// <summary>
        /// Filter with o without a parameter. Parameter can be a constant or a reference
        /// <example>default:"nothing"</example>
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
        /// <summary>
        /// List nodes representing the elements of the tag itself, including 
        /// markers, tag name, tag paremeters, etc
        /// </summary>
        public const string NODELIST_TAG_ELEMENTS = "standard.elements";
        /// <summary>
        /// Stadard list of nodes representing child tags
        /// </summary>
        public const string NODELIST_TAG_CHILDREN = "standard.children";
        /// <summary>
        /// List of nodes representing the <b>true</b> branch of the if tag and similar tags
        /// </summary>
        public const string NODELIST_IFTAG_IFTRUE = "if.true.children";
        /// <summary>
        /// List of nodes representing the <b>false</b> branch of the if tag and similar tags
        /// </summary>
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
        NodeType NodeType { get; set; }
        int Position { get; }
        int Length { get; }
        string Text { get; }
        /// <summary>
        /// List of allowed values
        /// </summary>
        IEnumerable<string> Values { get; }
        Error ErrorMessage { get; }
        /// <summary>
        /// Text to be shown as the node description.
        /// </summary>
        string Info { get; }
        int Priority { get; }
        Dictionary<string, IEnumerable<INode>> Nodes { get; }
    }
}
