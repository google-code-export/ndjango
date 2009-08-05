namespace NDjango.Designer.Parsing

open NDjango.Interfaces

type Node(nodeType, position, length, values, error, description, nodes) =
    interface INode with
        member x.NodeType = nodeType 
        member x.Position = position
        member x.Length = length
//        string Text { get; }
        member x.Values = values
        member x.ErrorMessage = error
        member x.Description = description
        member x.Nodes = nodes

