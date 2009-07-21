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

#light

namespace NDjango

open System
open System.IO
open System.Collections
open System.Collections.Generic

open NDjango.OutputHandling

module Lexer =

    type TextToken(text:string, line:int, pos:int) =
        member this.Text = text
        override this.ToString() = sprintf " in token: \"%s\" at line %d pos %d " text line pos

    type BlockToken(text, line, pos) =
        inherit TextToken(text, line, pos)
        let verb, args = 
            match smart_split (text.[Constants.BLOCK_TAG_START.Length..text.Length-Constants.BLOCK_TAG_END.Length-1].Trim()) with
            | verb::args -> verb, args 
            | _ -> raise (TemplateSyntaxError ("Empty tag block", Some (TextToken(text, line, pos):>obj)))
        member this.Verb = verb 
        member this.Args = args
    
    type VariableToken(text:string, line, pos) =
        inherit TextToken(text, line, pos)
        let expression = text.[Constants.VARIABLE_TAG_START.Length..text.Length-Constants.VARIABLE_TAG_END.Length-1].Trim()
            
        member this.Expression = 
            if expression.Equals("") then
                raise (TemplateSyntaxError ("Empty variable block", Some (TextToken(text, line, pos):>obj)))
            expression 
    
    type CommentToken(text, line, pos) =
        inherit TextToken(text, line, pos) 
    
    /// A lexer token produced through the tokenize function
    type public Token =
        | Block of BlockToken
        | Variable of VariableToken
        | Comment of CommentToken
        | Text of TextToken
        | Nothing
        
    let get_textToken = function
    | Block b -> Some (b :> obj)
    | Variable v -> Some (v :> obj)
    | Comment c -> Some (c :> obj)
    | Text t -> Some (t :> obj)
    | _ -> None
        
    /// <summary> generates sequence of tokens out of template TextReader </summary>
    /// <remarks>the type implements the IEnumerable interface (sequence) of templates
    /// It reads the template text from the text reader one buffer at a time and 
    /// returns tokens in batches - a batch is a sequence of the tokens cenerated 
    /// off the content of the buffer </remarks>
    type private Tokenizer (template:TextReader) =
        let mutable current: Token array = [||]
        let mutable line = 0
        let mutable pos = 0
        let buffer = Array.create 4096 ' '
        
        interface IEnumerator<Token seq> with
            member this.Current = Seq.of_array current
        
        interface IEnumerator with
            member this.Current = Seq.of_array current :> obj
            member this.MoveNext() =
                if current.Length > 0 && current.[0] = Nothing then false
                else
                    let count = template.ReadBlock(buffer, 0, buffer.Length)
                    if count = 0 then current <- [|Nothing|]
                    else 
                        let in_tag = ref true
                        current <- 
                            Array.map 
                                (fun (token:string) ->
                                    in_tag := not !in_tag
                                    let currentLine = line
                                    let currentPos = pos
                                    Seq.iter 
                                        (fun ch -> 
                                            if ch = '\n' then 
                                                line <- line + 1 
                                                pos <- 0
                                            else pos <- pos + 1
                                            ) 
                                        token
                                    if !in_tag then
                                        match token.[0..1] with
                                        | "{{" -> Variable (new VariableToken(token, currentLine, currentPos))
                                        | "{%" -> Block (new BlockToken(token, currentLine, currentPos))
                                        | "{#" -> Comment (new CommentToken(token, currentLine, currentPos))
                                        | _ -> Text (new TextToken(token, currentLine, currentPos))
                                    else
                                        Text(new TextToken(token, currentLine, currentPos))
                                 )
                                 (Constants.tag_re.Split(new String(buffer, 0, count)))
                    true
                
            member this.Reset() = failwith "Reset is not supported by Tokenizer"
        
        interface IEnumerable<Token seq> with
            member this.GetEnumerator():IEnumerator<Token seq> =
                 this :> IEnumerator<Token seq>

        interface IEnumerable with
            member this.GetEnumerator():IEnumerator =
                this :> IEnumerator

        interface IDisposable with
            member this.Dispose() = ()

    /// Produces a sequence of token objects based on the template text
    let tokenize (template:TextReader) =
        Seq.fold (fun (s:Token seq) (item:Token seq) -> Seq.append s item) (seq []) (new Tokenizer(template))
