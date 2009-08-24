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
open OutputHandling
open Utilities

module Variables =

    /// Finds and invokes the first property, field or 0-parameter method in the list.
    let rec private find_and_invoke_member (members: MemberInfo list) current bit =
        match members with 
        | h::t ->
            match h with
            | :? MethodInfo as mtd -> 
                // only call methods that don't have any parameters
                match mtd.GetParameters().Length with 
                | 0 -> Some <| mtd.Invoke(current, null)
                | _ -> find_and_invoke_member t current bit
            | :? FieldInfo as fld -> Some <| fld.GetValue(current)
            | :? PropertyInfo as prop -> 
                match prop.GetIndexParameters().Length with
                | 0 -> Some <| prop.GetValue(current, null)     // non-indexed property
                | 1 -> Some <| prop.GetValue(current, [|bit|])  // indexed property
                | _ -> None                                     // indexed property with more indeces that we can handle
            | _ -> failwith <| sprintf "%A is unexpected." current // this shouldn't happen, as all other types would be filtered out
        | [] -> None
    
    /// recurses through the supplied list and attempts to navigate the "current" object graph using
    /// name elements provided by the list. This function will invoke method and evaluate properties and members
    let rec internal resolve_lookup (current: obj) = function
        | h::t ->
            // tries to invoke a member in the members list, calling f if find_and_invoke_member returned None
            let try_invoke = fun (members: array<MemberInfo>) bit (f: unit -> obj option) ->
                match find_and_invoke_member (List.of_array members) current bit with
                | Some v -> Some v
                | None -> f()
                
            let find_intermediate = fun bit (current: obj) ->        
                match current with 
                | :? IDictionary as dict ->
                    match dict with
                    | Utilities.Contains (Some bit) v -> Some v
                    | Utilities.Contains (Some (String.concat System.String.Empty [OutputHandling.django_ns; bit])) v -> Some v
                    | Utilities.Contains (Utilities.try_int bit) v -> Some v
                    | _ ->
                        let (dict_members: array<MemberInfo>) = current.GetType().GetMember(bit, MemberTypes.Field ||| MemberTypes.Method ||| MemberTypes.Property, BindingFlags.Public ||| BindingFlags.NonPublic ||| BindingFlags.Instance)
                        find_and_invoke_member (List.of_array dict_members) current bit
                | :? IList as list ->
                    match bit with 
                    | Utilities.Int i when list.Count > i -> Some list.[i]
                    | _ -> None
                | null -> None
                | _ ->
                    let (members: array<MemberInfo>) = current.GetType().GetMember(bit, MemberTypes.Field ||| MemberTypes.Method ||| MemberTypes.Property, BindingFlags.Public ||| BindingFlags.NonPublic ||| BindingFlags.Instance)
                    let indexed_members = lazy ( current.GetType().GetMember("Item", MemberTypes.Property, BindingFlags.Public ||| BindingFlags.NonPublic ||| BindingFlags.Instance))
                    let as_array = fun () -> 
                        try 
                            // bit is an index into an array
                            if (Utilities.is_int bit) && current.GetType().IsArray && Array.length (current :?> array<obj>) > (int bit) then
                                Some <| Array.get (current :?> array<obj>) (int bit)
                            // no clue
                            else
                                None
                        // any number of issues as the result trying to parse an unknown value
                        with | _ -> None
                                           
                    // this bit of trickery needs explanation. try_invoke tries to find and invoke a member in the
                    // members array. if it doesn't, it will call the supplied function f. here, we're chaining together
                    // a search across all members, followed by a search for indexed members. the second search is 
                    // supplied as the "f" to the first search. also, indexed members are defined as lazy so that we
                    // don't take the reflection hit if we dont' need it
                    try_invoke members bit (fun () -> try_invoke (indexed_members.Force()) bit (fun () -> as_array()))

            if not (t = []) then
                match find_intermediate h current with
                | Some v -> resolve_lookup v t
                | None -> None
            else
                find_intermediate h current
        | [] ->  None
            
    /// A template variable, resolvable against a given context. The variable may be
    /// a hard-coded string (if it begins and ends with single or double quote
    /// marks)::
    /// 
    ///     >>> c = {'article': {'section':u'News'}}
    ///     >>> Variable('article.section').resolve(c)
    ///     u'News'
    ///     >>> Variable('article').resolve(c)
    ///     {'section': u'News'}
    ///     >>> class AClass: pass
    ///     >>> c = AClass()
    ///     >>> c.article = AClass()
    ///     >>> c.article.section = u'News'
    ///     >>> Variable('article.section').resolve(c)
    ///     u'News'
    /// (The example assumes VARIABLE_ATTRIBUTE_SEPARATOR is '.')
    type Variable(context: ParsingContext, token:Lexer.TextToken) =
//        let (|ComponentStartsWith|_|) chr (text: LexToken) =
//            if text.StartsWith(chr) || text.Contains(Constants.VARIABLE_ATTRIBUTE_SEPARATOR + chr) then
//                Some chr
//            else
//                None
//        
        let fail_syntax v = 
            raise (
                SyntaxError (
                    sprintf "Variables and attributes may not be empty, begin with underscores or minus (-) signs: '%s', '%s'" token.RawText v)
                    )
//
//        do match variable with 
//            | ComponentStartsWith "-" v->
//                match variable.string with
//                | Int i -> () 
//                | Float f -> ()
//                | _ -> fail_syntax v
//            | ComponentStartsWith "_" v when not <| variable.StartsWith(Constants.I18N_OPEN) ->
//                fail_syntax v
//            | _ -> () // need this to show the compiler that all cases are covered. 

        /// Returns a tuple of (var * value * needs translation)
        let find_literal = function
            | Utilities.Int i -> (None, Some (i :> obj), false)
            | Utilities.Float f -> (None, Some (f :> obj), false)
            | _ as v -> OutputHandling.strip_markers v

        let var, literal, translate = find_literal token.Value
        
        let lookups = if var.IsSome then Some <| List.of_array (var.Value.Split(Constants.VARIABLE_ATTRIBUTE_SEPARATOR.ToCharArray())) else None
        
        let clean_nulls  = function
        | Some v as orig -> if v = null then None else orig
        | None -> None
        
        let template_string_if_invalid = context.Provider.Settings.TryFind(Constants.TEMPLATE_STRING_IF_INVALID)

        /// Resolves this variable against a given context
        member this.Resolve (context: IContext) =
            match lookups with
            | None -> (literal.Value, false)
            | Some lkp ->
                try
                    let result =
                        match 
                            match context.tryfind (List.hd <| lkp) with
                            | Some v -> 
                                match lkp |> List.tl with
                                | h::t -> 
                                    // make sure we don't end up with a 'Some null'
                                    resolve_lookup v (h::t) |> clean_nulls
                                | _ -> Some v |> clean_nulls
                            | None -> None
                            with
                            | Some v1 -> v1
                            | None -> 
                                match template_string_if_invalid with
                                | Some o -> o
                                | None -> "" :> obj

                    (result, context.Autoescape)
                with
                    | _ as exc -> 
                        raise (RenderingError((sprintf "Exception occured while processing variable '%s'" token.RawText), exc))

        member this.IsLiteral with get() = lookups.IsNone

        interface INode with            
                     /// TagNode type = TagName
            member x.NodeType = NodeType.Reference 
            
            /// Position - see above
            member x.Position = token.Location.Position
            
            /// Length - see above
            member x.Length = token.Location.Length

            /// List of available values empty
            member x.Values =  seq []
            
            /// No message associated with the node
            member x.ErrorMessage = new Error(-1,"")
            
            /// No description 
            member x.Description = ""
            
            /// node list is empty
            member x.Nodes = Map.empty :> IDictionary<string, IEnumerable<INode>>
