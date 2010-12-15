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


namespace NDjango.Tags

open System
open System.Collections
open System.Diagnostics

open NDjango.Lexer
open NDjango.Interfaces
open NDjango.Expressions
open NDjango.ParserNodes

module internal If = 
    /// The ``{% if %}`` tag evaluates a variable, and if that variable is "true"
    /// (i.e., exists, is not empty, and is not a false boolean value), the
    /// contents of the block are output:
    /// 
    /// ::
    /// 
    ///     {% if athlete_list %}
    ///         Number of athletes: {{ athlete_list|count }}
    ///     {% else %}
    ///         No athletes.
    ///     {% endif %}
    /// 
    /// In the above, if ``athlete_list`` is not empty, the number of athletes will
    /// be displayed by the ``{{ athlete_list|count }}`` variable.
    /// 
    /// As you can see, the ``if`` tag can take an option ``{% else %}`` clause
    /// that will be displayed if the test fails.
    /// 
    /// ``if`` tags may use ``or``, ``and`` or ``not`` to test a number of
    /// variables or to negate a given variable::
    /// 
    ///     {% if not athlete_list %}
    ///         There are no athletes.
    ///     {% endif %}
    /// 
    ///     {% if athlete_list or coach_list %}
    ///         There are some athletes or some coaches.
    ///     {% endif %}
    /// 
    ///     {% if athlete_list and coach_list %}
    ///         Both atheletes and coaches are available.
    ///     {% endif %}
    /// 
    ///     {% if not athlete_list or coach_list %}
    ///         There are no athletes, or there are some coaches.
    ///     {% endif %}
    /// 
    ///     {% if athlete_list and not coach_list %}
    ///         There are some athletes and absolutely no coaches.
    ///     {% endif %}
    /// 
    /// ``if`` tags do not allow ``and`` and ``or`` clauses with the same tag,
    /// because the order of logic would be ambigous. For example, this is
    /// invalid::
    /// 
    ///     {% if athlete_list and coach_list or cheerleader_list %}
    /// 
    /// If you need to combine ``and`` and ``or`` to do advanced logic, just use
    /// nested if tags. For example::
    /// 
    ///     {% if athlete_list %}
    ///         {% if coach_list or cheerleader_list %}
    ///             We have athletes, and either coaches or cheerleaders!
    ///         {% endif %}
    ///     {% endif %}


    type IfLinkType = 
        | And
        | Or



    /// Represents one operation in operations tree
    /// For example, we have the next tree:
    ///
    ///         and
    ///       /    \
    ///    x < 3  t == "test"
    ///
    /// Here 'and' - parent operation, that has two child operations ('<' and '==').
    ///
    /// Properties of 'and' operation:
    ///     operationType   - 'and'
    ///     firstToken      - None
    ///     secondToken     - None
    ///     Parent          - None
    ///     firstUnit       - operation '<'
    ///     secondUnit      - operation '=='
    ///
    /// Properties of '<' operation:
    ///     operationType   - '<'
    ///     firstToken      - TextToken (x)
    ///     secondToken     - TextToken (3)
    ///     Parent          - operation 'and'
    ///     firstUnit       - None
    ///     secondUnit      - None
    ///
    /// Properties of '==' operation:
    ///     operationType   - '=='
    ///     firstToken      - TextToken (t)
    ///     secondToken     - TextToken ("test")
    ///     Parent          - operation 'and'
    ///     firstUnit       - None
    ///     secondUnit      - None
    type Unit(
                operType: string,
                fstUnit: Unit option,
                sndUnit: Unit option,
                fstToken: (string*FilterExpression) option,
                sndToken: (string*FilterExpression) option,
                parent: Unit option
             ) =
        
        let operation = operType
        let _firstToken = fstToken
        let _secondToken = sndToken
        let mutable _firstUnit = fstUnit
        let mutable _secondUnit = sndUnit
        let mutable _parent = parent
        let mutable _operationResult:bool option = None

        /// Operation's type ('and', 'or', '!=', '<=', 'in', ...)
        member this.operationType = operation

        /// First and second elements in runtime-calculations
        /// (for example: x == 2. Here 'x' - first token and '2' - second token. '==' - operation type)
        member this.firstToken = _firstToken
        member this.secondToken = _secondToken
        
        /// Parent operation
        member this.Parent
            with get() = _parent
            and set(value) = _parent <- value
        
        /// First and second child operations of current operation
        member this.firstUnit
            with get() = _firstUnit
            and set(value) =
                _firstUnit <- value
                _firstUnit.Value.Parent <- Some this
        
        member this.secondUnit
            with get() = _secondUnit
            and set(value) =
                _secondUnit <- value
                _secondUnit.Value.Parent <- Some this
        
        /// Boolean operation result (will calculate in run-time)
        member this.operationResult
            with get():bool option = _operationResult
            and set(value:bool option) = _operationResult <- value

//    /// AST TagNode representing an entire if tag, with all nested and composing tags
//    type TagNode(
//                provider,
//                token,
//                tag,
//                bool_vars: (bool * FilterExpression) list, 
//                node_list_true: NDjango.Interfaces.INodeImpl list, 
//                node_list_false: NDjango.Interfaces.INodeImpl list, 
//                link_type: IfLinkType
//                ) =
//        inherit NDjango.ParserNodes.TagNode(provider, token, tag)
//        
//        /// Evaluates a single filter expression against the context. Results are intepreted as follows: 
//        /// None: false (or invalid values, as FilterExpression.Resolve is called with ignoreFailure = true)
//        /// Any value of type System.Boolean: actual value
//        /// Any IEnumerable: true if at least one element exists, false otherwise
//        /// Any other non-null value: true
//        let eval context (bool_expr: FilterExpression) = 
//            match fst (bool_expr.Resolve context true) with
//            | None -> false
//            | Some v -> 
//                match v with 
//                | :? System.Boolean as b -> b                           // boolean value, take literal
//                | :? IEnumerable as e -> e.GetEnumerator().MoveNext()   // some sort of collection, take if empty
//                | null -> false                                         // null evaluates to false
//                | _ -> true                                             // anything else. true because it's there
//                    
//        /// recursivly evaluates the entire list of expressions and returns the boolean value.
//        let rec eval_expression context (expr: (bool * FilterExpression) list) =
//            match expr with 
//            | h::t ->
//                let ifnot, bool_expr = h
//                let e = eval context bool_expr
//                match link_type with
//                | Or ->
//                    if (e && not ifnot) || (ifnot && not e) then
//                        true
//                    else
//                        eval_expression context t
//                | And ->
//                    if not((e && not ifnot) || (ifnot && not e)) then
//                        false
//                    else
//                        eval_expression context t
//            | [] -> 
//                    match link_type with 
//                    | Or -> false
//                    | And -> true
//                
//        override x.walk manager walker =
//            match eval_expression walker.context bool_vars with
//            | true -> {walker with parent=Some walker; nodes=node_list_true}
//            | false -> {walker with parent=Some walker; nodes=node_list_false}
//            
//        override x.Nodes =
//            base.Nodes 
//                |> Map.add (NDjango.Constants.NODELIST_IFTAG_IFTRUE) (node_list_true |> Seq.map (fun node -> (node :?> INode)))
//                |> Map.add (NDjango.Constants.NODELIST_IFTAG_IFFALSE) (node_list_false |> Seq.map (fun node -> (node :?> INode)))
    
    type TagNode(
                provider,
                token,
                tag,
                tree: Unit, 
                node_list_true: NDjango.Interfaces.INodeImpl list, 
                node_list_false: NDjango.Interfaces.INodeImpl list
                ) =
        inherit NDjango.ParserNodes.TagNode(provider, token, tag)
        
        let rec getNonCalculatedChild (node: Unit) =
            match node.firstUnit with
            | None -> raise (SyntaxError ("invalid operations tree structure"))
            | Some any ->
                match any.operationResult with
                | None -> getNonCalculatedChild any
                | Some any -> node

        /// recursivly evaluates the entire tree of expressions and returns the boolean value.
        let rec eval_expression context (node: Unit) =
            let nodeForCalculate = getNonCalculatedChild node
     //       tree.firstUnit.Value.
            true
//            match expr with 
//            | h::t ->
//                let ifnot, bool_expr = h
//                let e = eval context bool_expr
//                match link_type with
//                | Or ->
//                    if (e && not ifnot) || (ifnot && not e) then
//                        true
//                    else
//                        eval_expression context t
//                | And ->
//                    if not((e && not ifnot) || (ifnot && not e)) then
//                        false
//                    else
//                        eval_expression context t
//            | [] -> 
//                    match link_type with 
//                    | Or -> false
//                    | And -> true
                
        override x.walk manager walker =
            match eval_expression walker.context tree with
            | true -> {walker with parent=Some walker; nodes=node_list_true}
            | false -> {walker with parent=Some walker; nodes=node_list_false}
            
        override x.Nodes =
            base.Nodes 
                |> Map.add (NDjango.Constants.NODELIST_IFTAG_IFTRUE) (node_list_true |> Seq.map (fun node -> (node :?> INode)))
                |> Map.add (NDjango.Constants.NODELIST_IFTAG_IFFALSE) (node_list_false |> Seq.map (fun node -> (node :?> INode)))



    [<NDjango.ParserNodes.Description("Outputs the content of enclosed tags based on expression evaluation result.")>]
    type Tag() =
        /// builds a list of FilterExpression objects for the variable components of an if statement. 
        /// The tuple returned is (not flag, FilterExpression), where not flag is true when the value
        /// is modified by the "not" keyword, and false otherwise.
        let rec build_vars token notFlag (tokens: TextToken list) parser (vars:(IfLinkType option)*(bool*FilterExpression) list) =
            match tokens with
            | MatchToken("not")::var::tail -> build_vars token true (var::tail) parser vars
            | var::[] -> 
                match fst vars with
                | None -> IfLinkType.Or, [(notFlag, new FilterExpression(parser, var))]
                | Some any -> any, snd vars @ [(notFlag, new FilterExpression(parser, var))]
            | var::MatchToken("and")::var2::tail -> 
                append_vars IfLinkType.And var token notFlag (var2::tail) parser vars 
            | var::MatchToken("or")::var2::tail -> 
                append_vars IfLinkType.Or var token notFlag (var2::tail) parser vars 
            | _ -> raise (SyntaxError ("invalid conditional expression in 'if' tag"))
            
        and append_vars linkType var
            token notFlag (tokens: TextToken list) parser vars =
            match fst vars with
            | Some any when any <> linkType -> raise (SyntaxError ("'if' tags can't mix 'and' and 'or'"))
            | _ -> ()
            build_vars token false tokens parser (Some linkType, snd vars @ [(notFlag, new FilterExpression(parser, var))])
        
        /// Get operation priority index
        let getPriorityLevel operation =
            match operation with
            | "==" | "!=" | ">" | "<" | ">=" | "<=" -> 1
            | "in" -> 2
            | "not" -> 3
            | "and" -> 4
            | "or" -> 5
            | _ -> 0
        
        /// Compare indexes of two operations
        let priority operation1 operation2 =
            getPriorityLevel operation1 > getPriorityLevel operation2

        let priorityOrEqual operation1 operation2 =
            getPriorityLevel operation1 >= getPriorityLevel operation2

        /// Getting parent for newNode in conformity with operation priorities
        let rec getNeedParent (node: Unit) (newNode: Unit) =
            match node.Parent with
            | None -> node
            | Some any ->
                if priorityOrEqual newNode.operationType any.operationType then
                    getNeedParent any newNode
                else node
        
        /// Getting the root node of operations tree
        let rec getParent (node: Unit) =
            match node.Parent with
            | None -> node
            | Some any -> getParent any

        /// Check for !=, ==, <, >, <=, >= and return the tree of operations in our If tag
        let rec build_tree token (tokens: TextToken list) parser (node: Unit option) = 
            match tokens with
            | var::MatchToken("==")::var2::tail ->
                append_tree token tail parser node "==" (Some ("", new FilterExpression(parser, var)))
                    (Some ("", new FilterExpression(parser, var2)))
            | var::MatchToken("!=")::var2::tail ->
                append_tree token tail parser node "!=" (Some ("", new FilterExpression(parser, var)))
                    (Some ("", new FilterExpression(parser, var2)))
            | var::MatchToken(">")::var2::tail ->
                append_tree token tail parser node ">" (Some ("", new FilterExpression(parser, var)))
                    (Some ("", new FilterExpression(parser, var2)))
            | var::MatchToken("<")::var2::tail ->
                append_tree token tail parser node "<" (Some ("", new FilterExpression(parser, var)))
                    (Some ("", new FilterExpression(parser, var2)))
            | var::MatchToken(">=")::var2::tail ->
                append_tree token tail parser node ">=" (Some ("", new FilterExpression(parser, var)))
                    (Some ("", new FilterExpression(parser, var2)))
            | var::MatchToken("<=")::var2::tail ->
                append_tree token tail parser node "<=" (Some ("", new FilterExpression(parser, var)))
                    (Some ("", new FilterExpression(parser, var2)))
            | MatchToken("and")::var::tail ->
                append_tree token (var::tail) parser node "and" None None
            | MatchToken("or")::var::tail ->
                append_tree token (var::tail) parser node "or" None None
            | [] ->
                match node with
                | None -> raise (SyntaxError("Empty If tag"))
                | Some any -> getParent any
            | _ -> raise (SyntaxError ("Error"))
        
        and append_tree token (tokens: TextToken list) parser (node: Unit option) operation
            (firstToken: (string*FilterExpression) option) (secondToken: (string*FilterExpression) option) =
            let newUnit = Unit(operation, None, None, firstToken, secondToken, None)
            
            match node with
            | None -> build_tree token tokens parser (Some newUnit)
            | Some any -> setNewUnit token tokens parser any newUnit

        and setNewUnit token (tokens: TextToken list) parser (node: Unit) (newNode: Unit) =
            if priority node.operationType newNode.operationType then
                node.secondUnit <- Some newNode
            else insert_node newNode node
            build_tree token tokens parser (Some newNode)

        and insert_node (newNode: Unit) (node: Unit) =
            let tempNode = getNeedParent node newNode
            if tempNode.Parent <> None then tempNode.Parent.Value.secondUnit <- Some newNode
            newNode.firstUnit <- Some tempNode


        

        interface ITag with 
            member x.is_header_tag = false
            member this.Perform token context tokens =

                let node_list_true, remaining = (context.Provider :?> IParser).Parse (Some token) tokens (context.WithClosures(["else"; "endif"]))
                let node_list_false, remaining2 =
                    match node_list_true.[node_list_true.Length-1].Token with
                    | NDjango.Lexer.Block b -> 
                        if b.Verb.RawText = "else" then
                            (context.Provider :?> IParser).Parse (Some token) remaining (context.WithClosures(["endif"]))
                        else
                            [], remaining
                    | _ -> [], remaining
                   
                let tree =
                    try
                        build_tree token token.Args context None
                    with
                    | :? SyntaxError as e ->
                            raise (SyntaxError(e.Message, 
                                    node_list_true @ node_list_false,
                                    remaining2))
                    |_ -> reraise()

//                (({
//                    new TagNode(context, token, this, tree, node_list_true, node_list_false)
//                        with
//                            override this.elements
//                                with get()=
//                                    List.append (bool_vars |> List.map(fun (_, element) -> (element :> INode))) base.elements
//                    } :> NDjango.Interfaces.INodeImpl),
//                    context, remaining2)
                ((
                    new TagNode(context, token, this, tree, node_list_true, node_list_false)
                    :> NDjango.Interfaces.INodeImpl),
                    context, remaining2)
                    
//                let link_type, bool_vars = 
//                    try
//                        build_vars token false token.Args context (None,[])
//                    with
//                    | :? SyntaxError as e ->
//                            raise (SyntaxError(e.Message, 
//                                    node_list_true @ node_list_false,
//                                    remaining2))
//                    |_ -> reraise()
                  
//                (({
//                    new TagNode(context, token, this, bool_vars, node_list_true, node_list_false, link_type)
//                        with
//                            override this.elements
//                                with get()=
//                                    List.append (bool_vars |> List.map(fun (_, element) -> (element :> INode))) base.elements
//                    } :> NDjango.Interfaces.INodeImpl),
//                    context, remaining2)


