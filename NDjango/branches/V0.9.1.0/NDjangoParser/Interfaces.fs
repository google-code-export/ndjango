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


namespace NDjango.Interfaces

open System.Collections.Generic
open System.IO
open NDjango

/// A no-parameter filter
type ISimpleFilter = 
    abstract member Perform: obj -> obj
    
/// A filter that accepts a single parameter of type 'a
type IFilter =
    inherit ISimpleFilter
    abstract member DefaultValue: obj
    
    abstract member PerformWithParam: obj * obj -> obj    

type ITemplateLoader = 
    abstract member GetTemplate: string -> TextReader
    abstract member IsUpdated: string * System.DateTime -> bool
    
/// Template lifecycle manager. Implementers of this interface are responsible for
/// providing compiled templates by resource name
type ITemplateManager = 
    /// given the template name and context returns the <see cref="System.IO.TextReader"/> 
    /// that will stream out the results of the render.
    abstract member RenderTemplate: string * IDictionary<string, obj> -> TextReader

    /// Looks up the filter in the filter dictionary
    abstract member GetTemplateVariables: string -> string []
    
/// Template lifecycle manager. Implementers of this interface are responsible for
/// providing compiled templates by resource name. This interface is supposed to be internal
/// unfrotunately as of 1.9.6.16 it is not allowed to have public types implement non-public 
/// interfaces
type ITemplateContainer =
    inherit ITemplateManager 
    
    /// Retrieves a TextReader object for the specified file. It is a responsibility of the
    /// caller to make sure it is properly closed and disposed of
    abstract member GetTemplateReader: string -> TextReader

/// Template imeplementation. This interface effectively represents the root-level node
/// in the Django AST.
and ITemplate =
    /// Recursivly "walks" the AST, returning a text reader that will stream out the 
    /// results of the render.
    abstract Walk: ITemplateManager -> IDictionary<string, obj> -> System.IO.TextReader
    
    /// A list of top level sibling nodes
    abstract Nodes: INode list
    
    abstract GetVariables: string list
    
/// An execution context container. This interface defines a set of methods necessary 
/// for templates and external entities to exchange information.
and IContext =
    /// Adds an object to the context
    abstract member add:(string*obj)->IContext
    
    /// Attempts to find an object in the context by the key
    abstract member tryfind: string->obj option
    
    /// Retrieves the requested template along with the containing template manager
    abstract member GetTemplate: string -> ITemplate
    
    // TODO: why is this on the interface definition?
    abstract member TEMPLATE_STRING_IF_INVALID: obj
    
    /// Indicates that this Context is in Autoescape mode
    abstract member Autoescape: bool
    
    /// Returns a new Context with the specified Autoescape mode
    abstract member WithAutoescape: bool -> IContext
    
//    /// Returns a new Context with the specified template manager
//    abstract member WithNewManager: ITemplateManager -> IContext
//
    /// Returns the template manager associated with the context
    abstract member Manager: ITemplateManager

/// Rendering state 
and Walker =
    {
        parent: Walker option
        nodes: INode list
        buffer: string
        bufferIndex: int
        context: IContext
    }
    
/// Parsing interface definition
and IParser =
    /// Produces a commited node list and uncommited token list as a result of parsing until
    /// a block from the string list is encotuntered
    abstract member Parse: LazyList<Lexer.Token> -> string list -> (INode list * LazyList<Lexer.Token>)
   
    /// Produces an uncommited token list as a result of parsing until
    /// a block from the string list is encotuntered
    abstract member Seek: LazyList<Lexer.Token> -> string list -> LazyList<Lexer.Token>

    abstract member Provider: ITemplateManagerProvider
//    abstract member FindFilter: string -> ISimpleFilter option
        
/// A single tag implementation
and ITag = 
    /// Transforms a {% %} tag into a list of nodes and uncommited token list
    abstract member Perform: Lexer.BlockToken -> IParser -> LazyList<Lexer.Token> -> (INode * LazyList<Lexer.Token>)

and INode =

    /// Indicates whether this node must be the first non-text node in the template
    abstract member must_be_first: bool
    
    /// The token that defined the node
    abstract member Token : Lexer.Token

    /// Processes this node and all child nodes
    abstract member walk: Walker -> Walker
    
    /// returns all child nodes contained within this node
    abstract member GetVariables: string list

and ITemplateManagerProvider =
    
    abstract member Tags: Map<string, ITag>
    
    abstract member Filters: Map<string, ISimpleFilter>

    abstract member Settings: Map<string, obj>

    abstract member Loader: ITemplateLoader

    /// Retrieves the requested template checking first the global
    /// dictionary and validating the timestamp
    abstract member GetTemplate: string -> (ITemplate* System.DateTime)

    /// Retrieves the requested template without checking the 
    /// local dictionary and/or timestamp
    /// the retrieved template is placed in the dictionary replacing 
    /// the existing template with the same name (if any)
    abstract member LoadTemplate: string -> (ITemplate* System.DateTime)
