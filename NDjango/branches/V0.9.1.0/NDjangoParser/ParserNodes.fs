(****************************************************************************
 * 
 *  NDjango Parser Copyright © 2009 Hill30 Inc
 *
 *  This file is part of the NDjango Parser.
 *
 *  The NDjango Parser is free software: you can redistribute it and/or modify
 *  it under the terms of the GNU Lesser General Public License as published by
 *  the Free Software Foundation, either version 3 of the License, or
 *  (at your option) any later version.
 *
 *  The NDjango Parser is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 *  GNU Lesser General Public License for more details.
 *
 *  You should have received a copy of the GNU Lesser General Public License
 *  along with NDjango Parser.  If not, see <http://www.gnu.org/licenses/>.
 *  
 ***************************************************************************)

namespace NDjango

open System.Collections.Generic
open OutputHandling
open Lexer
open NDjango.Interfaces

module internal ParserNodes =

    type private BracketType = 
        |Open 
        |Close

    /// Base class of the Django AST 
    [<AbstractClass>]
    type Node(token: Token) =
    
        /// Indicates whether this node must be the first non-text node in the template
        abstract member must_be_first: bool
        default this.must_be_first = false
        
        abstract member node_type: NodeType
        
        /// The token that defined the node
        member this.Token with get() = token

        /// Processes this node and all child nodes
        abstract member walk: ITemplateManager -> Walker -> Walker
        default  this.walk manager walker = walker
        
        abstract member nodes: INodeImpl list
        default x.nodes with get() = []

        /// returns a list of immediate child nodes contained within this node
        abstract member Nodes: Map<string, IEnumerable<INode>>
        default x.Nodes 
            with get() =
                new Map<string, IEnumerable<INode>>([]) 
                    |> Map.add Constants.NODELIST_TAG_CHILDREN (x.nodes |> Seq.map (fun node -> (node :?> INode)))
                    |> Map.add Constants.NODELIST_TAG_ELEMENTS (x.elements :> IEnumerable<INode>)
        
        /// returns a list of nodes representing tag elements
        abstract member elements: INode list
        default x.elements 
            with get() = 
                [
                    (new TagBracketNode(get_textToken token, Open) :> INode); 
                    (new TagBracketNode(get_textToken token, Close) :> INode)
                ]
        
        abstract member Values: string list
        default x.Values = []
            
        abstract member ErrorMessage: Error
        default x.ErrorMessage = new Error(-1,"")
            
        abstract member Description: string
        default x.Description = ""

        interface INode with

             /// TagNode type
            member x.NodeType = x.node_type 
            
            /// Position of the first character of the node text
            member x.Position = (get_textToken token).Position
            
            /// Length of the node text
            member x.Length = (get_textToken token).Length

            /// a list of values allowed for the node
            member x.Values = x.Values
            
            /// message associated with the node
            member x.ErrorMessage = x.ErrorMessage
            
            /// TagNode description (will be shown in the tooltip)
            member x.Description = x.Description
            
            /// node lists
            member x.Nodes = x.Nodes :> IDictionary<string, IEnumerable<INode>>

        interface INodeImpl with
            member this.must_be_first = this.must_be_first
            member this.Token = this.Token
            member this.walk manager walker = this.walk manager walker
            
    and private TagBracketNode(token: TextToken, bracketType: BracketType) =

        interface INode with
             /// TagNode type
            member x.NodeType = NodeType.TagName 
            
            /// Position of the first character of the node text
            member x.Position = 
                match bracketType with
                | Open -> token.Position
                | Close -> token.Position + token.Text.Length - 2
            
            /// Length of the node text
            member x.Length = 2

            /// a list of values allowed for the node
            member x.Values = []
            
            /// message associated with the node
            member x.ErrorMessage = new Error(-1,"")
            
            /// TagNode description (will be shown in the tooltip)
            member x.Description = ""
            
            /// node lists
            member x.Nodes = Map.empty :> IDictionary<string, IEnumerable<INode>>

    type TagNameNode(token: BlockToken) =

        interface INode with
             /// TagNode type
            member x.NodeType = NodeType.TagName 
            
            /// Position of the first character of the node text
            member x.Position = token.Position + token.Text.IndexOf(token.Verb)
            
            /// Length of the node text
            member x.Length = token.Verb.Length

            /// a list of values allowed for the node
            member x.Values = []
            
            /// message associated with the node
            member x.ErrorMessage = new Error(-1,"")
            
            /// TagNode description (will be shown in the tooltip)
            member x.Description = ""
            
            /// node lists
            member x.Nodes = Map.empty :> IDictionary<string, IEnumerable<INode>>

