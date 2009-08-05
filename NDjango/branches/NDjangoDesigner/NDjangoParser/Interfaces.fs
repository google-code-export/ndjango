// Learn more about F# at http://fsharp.net

namespace NDjango.Interfaces

open System.Collections.Generic

//[<System.Flags>]
type NodeType =
            
        /// <summary>
        /// The whole tag.
        /// <example>{% if somevalue %}</example>
        /// </summary>
        | Tag = 0x0001
        
        /// <summary>
        /// The markers, which frame django tag. 
        /// <example>{%, %}</example>
        /// </summary>
        | Marker = 0x0002
        
        /// <summary>
        /// Django template tag.
        /// <example> "with", "for", "ifequal"</example>
        /// </summary>
        | TagName = 0x0003

        /// <summary>
        /// The keyword, as required by some tags.
        /// <example>"and", "as"</example>
        /// </summary>
        | Keyword = 0x0004

        /// <summary>
        /// The variable definition used in tags which introduce new variables i.e. 
        /// loop variable in the For tag.
        /// <example>loop_item</example>
        /// </summary>
        | Variable = 0x0005

        /// <summary>
        /// Expression, which consists of a reference followed by 0 or more filters
        /// <example>User.DoB|date:"D d M Y"</example>
        /// </summary>
        | Expression = 0x0006
        
        /// <summary>
        /// Reference to a value in the current context.
        /// <example>User.DoB</example>
        /// </summary>
        | Reference = 0x0007

        /// <summary>
        /// Filter with o without a parameter. Parameter can be a constant or a reference
        /// <example>default:"nothing"</example>
        /// </summary>
        | Filter = 0x0008

        /// <summary>
        /// The name of the filter.
        /// <example>"length", "first", "default"</example>
        /// </summary>
        | FilterName = 0x0009
        
        /// <summary>
        /// Filter parameter.
        /// <example>any valid value</example>
        /// </summary>
        | FilterParam = 0x000a

type Error(severity:int, message:string) =
    member x.Severity = severity
    member x.Message = message

type IParser =
    abstract member Parse:
        template: IEnumerable<string> -> IEnumerable<INode>
        
and INode =
    abstract member NodeType: NodeType 
    abstract member Position: int
    abstract member Length: int
//        string Text { get; }
    /// <summary>
    /// List of allowed values
    /// </summary>
    abstract member Values: IEnumerable<string>
    abstract member ErrorMessage: Error
    /// <summary>
    /// Text to be shown as the node description.
    /// </summary>
    abstract member Description: string
    abstract member Nodes: Dictionary<string, IEnumerable<INode>>
