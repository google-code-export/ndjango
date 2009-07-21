﻿(****************************************************************************
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

#light

namespace NDjango.Tags

open System
open System.Collections
open System.Linq;

open NDjango.Lexer
open NDjango.Interfaces
open NDjango.Expressions
open NDjango.OutputHandling

module internal For =

///    for
///    Loop over each item in an array. For example, to display a list of athletes provided in athlete_list:
///    
///    <ul>
///    {% for athlete in athlete_list %}
///        <li>{{ athlete.name }}</li>
///    {% endfor %}
///    </ul>
///    You can loop over a list in reverse by using {% for obj in list reversed %}.
///    
///    New in Django 1.0. 
///    If you need to loop over a list of lists, you can unpack the values in each sub-list into individual variables. For example, if your context contains a list of (x,y) coordinates called points, you could use the following to output the list of points:
///    
///    {% for x, y in points %}
///        There is a point at {{ x }},{{ y }}
///    {% endfor %}
///    This can also be useful if you need to access the items in a dictionary. For example, if your context contained a dictionary data, the following would display the keys and values of the dictionary:
///    
///    {% for key, value in data.items %}
///        {{ key }}: {{ value }}
///    {% endfor %}
///    The for loop sets a number of variables available within the loop:
///    
///    Variable Description 
///    forloop.counter The current iteration of the loop (1-indexed) 
///    forloop.counter0 The current iteration of the loop (0-indexed) 
///    forloop.revcounter The number of iterations from the end of the loop (1-indexed) 
///    forloop.revcounter0 The number of iterations from the end of the loop (0-indexed) 
///    forloop.first True if this is the first time through the loop 
///    forloop.last True if this is the last time through the loop 
///    forloop.parentloop For nested loops, this is the loop "above" the current one 
///    
///    for ... empty¶
///    New in Django 1.1. 
///    The for tag can take an optional {% empty %} clause that will be displayed if the given array is empty or could not be found:
///    
///    <ul>
///    {% for athlete in athlete_list %}
///        <li>{{ athlete.name }}</li>
///    {% empty %}
///        <li>Sorry, no athlete in this list!</li>
///    {% endfor %}
///    <ul>
///    The above is equivalent to -- but shorter, cleaner, and possibly faster than -- the following:
///    
///    <ul>
///      {% if athlete_list %}
///        {% for athlete in athlete_list %}
///          <li>{{ athlete.name }}</li>
///        {% endfor %}
///      {% else %}
///        <li>Sorry, no athletes in this list.</li>
///      {% endif %}
///    </ul>

    type ForContext =
        {
        counter:     int
        counter0:    int
        revcounter:  int
        revcounter0: int
        first:       bool
        last:        bool
        parentloop:  ForContext option
        }
        
            
    type Node(
                token : BlockToken,
                enumerator : FilterExpression, 
                variables : string list, 
                bodyNodes : NDjango.Interfaces.Node list, 
                emptyNodes: NDjango.Interfaces.Node list,
                reversed: bool
                ) =
        inherit NDjango.Interfaces.Node(Block token)

        /// Creates a new ForContext object to represent the current iteration
        /// for the loop. The third parameter (context) is a context for the 
        /// parent loop if this is the first iteration of the loop
        /// or the conetxt from the previous iteration
        let createVars (first:bool) (size:int option) (context:obj option) =
            let context = 
                match context with
                | None -> None
                | Some o -> 
                    match o with
                    | :? ForContext as c -> Some c
                    | _ -> None 

            match first with
            | true -> 
                { 
                    counter = 1; 
                    counter0 = 0;
                    revcounter = 
                        match size with
                        | Some s -> s
                        | None -> 0
                    revcounter0 = -1 +
                        match size with
                        | Some s -> s
                        | None -> 0
                    first = true;
                    last = false;
                    parentloop = context
                }
            | false ->
                match context with
                | None -> failwith "Context should always be available after the first iteration"
                | Some context ->
                    {
                        counter = context.counter+1;
                        counter0 = context.counter0+1;
                        revcounter = context.revcounter-1 ;
                        revcounter0 = context.revcounter0-1;
                        first = false;
                        last = 
                            match size with
                            | Some s -> context.counter = s-1;
                            | None -> false
                        parentloop = context.parentloop
                    }
               
        override this.walk walker = 
                
            match fst <| enumerator.Resolve walker.context false with
            |Some o ->
                match o with
                | :? IEnumerable as loop when not (loop |> Seq.cast |> Seq.isEmpty) -> 
//                    let size =
//                        match loop with
//                        | :? System.Array as array -> Some (array.GetLength(0))
//                        | :? System.Collections.IList as list -> Some (list.Count)
//                        | _ -> None
//                    let size = Some ((loop.OfType<Object>()).Count<Object>())
                    let size = Some (Seq.length(loop |> Seq.cast))
                    
                    /// create context with the loop variables and the forloop 
                    /// record reflecting the current loop interation
                    let createContext (first:bool) (walker:Walker) (item:obj) =
                        // add the loop variable(s) to the context
                        let c = 
                            if List.length variables = 1 then
                                walker.context.add(variables.[0], item)
                            else
                                match item with
                                | :? IEnumerable as list ->
                                    Seq.zip (Seq.cast list) variables |> Seq.fold 
                                        (fun context var -> context.add(snd var, fst var)) 
                                        walker.context
                                | _ ->
                                    variables |> List.fold 
                                        (fun context var -> 
                                            let value = match resolve_lookup item [var] with | Some o -> o | None -> None :> obj
                                            context.add(var, value))
                                        walker.context
                        
                        c.add("forloop", (walker.context.tryFind "forloop" |> createVars first size :> obj))
                        
                    let rec createWalker (first:bool) (walker:Walker) (enumerator: obj seq) =
                        {walker with 
                            parent = Some walker; 
                            nodes = bodyNodes @ [(Repeater(token, Seq.skip 1 enumerator, createWalker false) :> NDjango.Interfaces.Node)];
                            context = enumerator |> Seq.hd |> createContext first walker
                            }
                    
                    if reversed then
                        loop |> Seq.cast |> Seq.to_list |> List.rev |> createWalker true walker
                    else
                        loop |> Seq.cast |> createWalker true walker
                
                | _ -> {walker with parent=Some walker; nodes=emptyNodes}
            | None -> {walker with parent=Some walker; nodes=emptyNodes}
    
    /// this is a for loop helper node. The real loop node <see cref="Node"/> places a list of nodes
    /// for the loop body into the walker, it adds the Repeater as the last one. The repeater checks for
    /// the endloop condition and if another iteration is necessary re-adds the list of nodes and itself to 
    /// the walker
    and Repeater(token:BlockToken, enumerator:obj seq, createWalker) =
        inherit NDjango.Interfaces.Node(Block token)
        
        override this.walk(walker) =
            if Seq.isEmpty enumerator then
                walker
            else 
                createWalker walker enumerator

    type Tag() =

        interface NDjango.Interfaces.ITag with 
            member this.Perform token parser tokens =
                let enumerator, variables, reversed = 
                    match List.rev token.Args with
                        | var::"in"::syntax -> 
                            var,
                            syntax,
                            false
                        | "reversed"::var::"in"::syntax -> 
                            var,
                            syntax,
                            true
                        | _ -> raise (TemplateSyntaxError ("malformed 'for' tag", Some (token:>obj)))
                let enumExpr = FilterExpression(Block token, enumerator, (parser :?> IFilterManager))
                let variables = variables |> List.rev |>  List.fold (fun l item -> (List.append l (Array.to_list( item.Split([|','|], StringSplitOptions.RemoveEmptyEntries))))) []  
                let node_list_body, remaining = parser.Parse tokens ["empty"; "endfor"]
                let node_list_empty, remaining2 =
                    match node_list_body.[node_list_body.Length-1].Token with
                    | NDjango.Lexer.Block b -> 
                        if b.Verb = "empty" then
                            parser.Parse remaining ["endfor"]
                        else
                            [], remaining
                    | _ -> [], remaining

                ([(Node(token, enumExpr, variables, node_list_body, node_list_empty, reversed) :> NDjango.Interfaces.Node)], remaining2)

