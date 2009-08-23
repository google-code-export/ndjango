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
open System.Text
open System.Text.RegularExpressions
open System.Collections
open System.Collections.Generic
open System.Reflection

open NDjango.Interfaces
open Lexer
open ParserNodes
open Variables
open OutputHandling
open Utilities

module Expressions =


    type private Filter =
        |Escape 
        |Filter of FilterWrapper

    and private FilterWrapper(filter:ISimpleFilter, args:Variable list)=
        
        member x.Perform (context, input) =
            match filter with
            | :? IFilter as std ->
                let param = 
                    match args with
                    // we don't have to check for the presence of a default value here, as an earlier
                    // check enforces that filters without defaults do not get called without parameters
                    | [] -> std.DefaultValue
                    | _ -> (args |> List.hd).Resolve context |> fst
                std.PerformWithParam(input, param)
                 
            | _ as simple -> simple.Perform input
                
    type FilterExpression (context:ParsingContext, token:Lexer.Token, expression: LexToken) =
        
        let wrap_filter (filter:ISimpleFilter) (args:Variable list) =
            Some  
               <| match filter with
                    | :? NDjango.Filters.IEscapeFilter -> Escape
                    | :? IFilter as f ->
                        match args with
                        | [] -> if (f.DefaultValue = null) 
                                then raise (SyntaxError ("filter requires argument, none provided"))
                                Filter (new FilterWrapper(filter, []))
                        | _ -> Filter (new FilterWrapper(filter, args))
                    | _ -> Filter (new FilterWrapper(filter, args))

        /// unescapes literal quotes. takes '\"value\"' and returns '"value"'
        let flatten (text: string) = text.Replace("\\\"", "\"")
        
        /// unescapes literal quotes. takes '\"value\"' and returns '"value"'
        let flatten_group (group: Group) = flatten group.Value

        let lex_token (capture:#System.Text.RegularExpressions.Capture) offset =
            LexToken (capture.Value,(capture.Index + offset + (fst expression.Location), capture.Length))
            
        let generate_diag_for_filter (ex:System.Exception) variable =
            match ex with
            | :? SyntaxError as e ->
                if (context.Provider.Settings.[Constants.EXCEPTION_IF_ERROR] :?> bool)
                then
                    raise (SyntaxException(e.Message, get_textToken token))
                else
                    Some (new Error(2, e.Message), variable, [])
            |_  -> None
        
        /// Helper function for parsing filter expressions
        /// Parses a variable token and its optional filters (all as a single string),
        /// and return a list of tuples of the filter name and arguments.
        /// Sample:
        ///     >>> token = 'variable|default:\"Default value\"|date:\"Y-m-d\"'
        ///     >>> p = Parser('')
        ///     >>> fe = FilterExpression(token, p)
        ///     >>> len(fe.filters)
        ///     2
        ///     >>> fe.var
        ///     <Variable: 'variable'>
        ///
        let rec parse_var (filter_match: Match) upto (var: LexToken option) (filters: 'a list)=
            if not (filter_match.Success) then
                if not (upto = expression.string.Length) 
                then raise (SyntaxError (sprintf "Could not parse the remainder: '%s' from '%s'" expression.[upto..] expression.string))
                else
                    (upto, new Variable(context, token, var.Value), filters)
            else
                // short-hand for the recursive call. the values for match and upto are always computed the same way
                let fast_call variable filter = 
                        let new_filters = 
                            match filter with
                            | Some f -> filters @ [f]
                            | None -> filters
                        parse_var 
                            (filter_match.NextMatch()) 
                            (filter_match.Index + filter_match.Length) 
                            variable
                            new_filters
            
                if not (upto = filter_match.Index) then
                    raise 
                        (SyntaxError 
                            (sprintf "Could not parse some characters %s|%s|%s" 
                                expression.[..upto] 
                                expression.[upto..filter_match.Index] 
                                expression.[filter_match.Index..]
                            ))
                else
                    // when called from FilterExpression constructor, var = None
                    match var with
                    | None ->
                        // process the first element - which is the variable reference
                        let var_match = filter_match.Groups.["var"]
                        if var_match.Success then
                            fast_call (Some (LexToken(var_match.Value, (var_match.Index, var_match.Length)))) None
                        else
                            raise (SyntaxError (sprintf "Could not find variable at start of %s" expression.string))
                    | Some s ->
                        // all subsequent elements are filters
                        let filter_name = filter_match.Groups.["filter_name"]
                        let arg = filter_match.Groups.["arg"].Captures |> Seq.cast |> Seq.to_list 
                                |> List.map 
                                    (fun (c:System.Text.RegularExpressions.Capture) ->
                                        new Variable(context, token, lex_token c 0)
                                    ) 
                        match Map.tryFind filter_name.Value context.Provider.Filters with
                        | None -> raise (SyntaxError (sprintf "filter %A could not be found" filter_name.Value))
                        | Some filter ->
                            wrap_filter filter arg |> fast_call var 

        // list of filters along with the arguments that they expect
        let error, variable, filters =
            try
                let _, variable, filters =
                        parse_var (Constants.filter_re.Match expression.string) 0 None []
                (new Error(-1, ""), Some variable, filters)
            with
                | _ as ex -> 
                    match generate_diag_for_filter ex None with
                    | Some result -> result
                    | _ -> rethrow()

        /// resolves the filter against the given context. 
        /// the tuple returned consists of the value as the first item in the tuple
        /// and a boolean indicating whether the value needs escaping 
        /// if ignoreFailures is true, None is returned for failed expressions, otherwise an exception is thrown.
        member this.Resolve (context: IContext) ignoreFailures =
            let resolved_value =
                match variable with 
                | Some v -> 
                    try
                        let result = v.Resolve context
                        (Some (fst <| result), snd <| result)
                    with
                        | _ as exc -> 
                            if ignoreFailures then
                                (None, false)
                            else
                                raise (RenderingError((sprintf "Exception occured while processing variable '%s'" v.ExpressionText.string), exc))
                 | None ->
                    raise (SyntaxException(error.Message, get_textToken token))
            
            filters |> List.fold 
                (fun input f ->
                    match fst input with
                    | None -> (None, false)
                    | Some value ->
                        match f with
                        | Escape -> (fst input, true)
                        | Filter(filter) -> (Some (filter.Perform(context, value)), snd input)
                ) 
                resolved_value
            
        /// resolves the filter against the given context and 
        /// converts it to string taking into account escaping. 
        /// This method never fails, if the expression fails to resolve, 
        /// the method returns None
        member this.ResolveForOutput manager walker =
            let result, needsEscape = this.Resolve walker.context false
            match result with 
            | None -> None  // this results in no output from the expression
            | Some o -> 
                match o with 
                | :? INodeImpl as node -> Some (node.walk manager walker) // take output from the node
                | null -> None // this results in no output from the expression
                | _ as v ->
                    match if needsEscape then escape v else string v with
                    | "" -> None
                    | _ as s -> Some {walker with buffer = s}
            

            //TODO: django language spec allows 0 or 1 arguments to be passed to a filter, however the django implementation will handle any number
            //for filter, args in filters do
                
        interface INode with            
                     /// TagNode type = TagName
            member x.NodeType = NodeType.Expression 
            
            /// Position - see above
            member x.Position = (Lexer.get_textToken token).Position
            
            /// Length - see above
            member x.Length = (Lexer.get_textToken token).Position

            /// List of available values empty
            member x.Values =  seq []
            
            /// error message associated with the node
            member x.ErrorMessage = error
            
            /// No description 
            member x.Description = ""
            
            /// node list consists of the variable node and the list of the filter nodes
            member x.Nodes =
                let list = 
                    match variable with
                    | Some v -> [(v :> INode)]
                    | None -> [] 
                new Map<string, IEnumerable<INode>>([]) 
                    |> Map.add Constants.NODELIST_TAG_ELEMENTS (list :> IEnumerable<INode>) 
                        :> IDictionary<string, IEnumerable<INode>>


    // TODO: we still need to figure out the translation piece
    // python code
    //        if self.translate:
    //            return _(value)

