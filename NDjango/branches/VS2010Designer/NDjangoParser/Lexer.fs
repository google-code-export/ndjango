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

open System
open System.Collections
open System.Collections.Generic
open System.IO
open System.Text.RegularExpressions

open NDjango.OutputHandling

module Lexer =

    /// Structure describing the token location in the original source string
    type Location = 
        {
            /// 0 based offset of the text from the begining of the source file
            Offset: int
            /// Length of the text fragment from which the token was generated 
            /// may or may not match the length of the actual text
            Length: int
            /// 0 based position of the starting position of the text within the line it belongs to
            Position: int
            /// 0 based line number
            Line: int
        }

    /// base class for text tokens
    type TextToken(text:string, value:string, location: Location) =
  
        let location_ofMatch (m:Match) =
            {location 
                with 
                    Offset = location.Offset + m.Groups.[0].Index;
                    Length = m.Groups.[0].Length;
                    Position = location.Position + m.Groups.[0].Index;
            }
              
        new (text:string, location: Location) =
            new TextToken(text, text, location)
            
        member x.Tokenize (regex:Regex) =
            [for m in regex.Matches(value) -> new TextToken(m.Groups.[0].Value, location_ofMatch m)]

        member x.RawText = text
        
        member x.BlockBody = 
            let body = x.RawText.[2..x.RawText.Length-4].Trim()
            if body = "" then
                raise (SyntaxError("Empty block"))
            x.CreateToken(x.RawText.IndexOf(body), body.Length)

        override x.ToString() = sprintf " in token: \"%s\" at line %d pos %d " text location.Line location.Position
        
        member x.Location = location
        
        member x.CreateToken (capture:Capture) = x.CreateToken (capture.Index,capture.Length)
        
        member x.CreateToken (offset,length) = 
            new TextToken(x.RawText.Substring(offset,length)
                , {location with Offset = location.Offset + offset; Length = length; Position = location.Position + offset}) 
                
        member x.WithValue new_value = new TextToken(text, new_value, location)
        
        member x.Value = value

    /// Exception raised when template syntax errors are encountered
    /// this exception is defined here because it its dependency on the TextToken class
    type SyntaxException (message: string, token: TextToken) =
        inherit System.ApplicationException(message + token.ToString())
        
        member x.Token = token
        member x.ErrorMessage = message  
    
    let private split_tag_re = new Regex(@"(""(?:[^""\\]*(?:\\.[^""\\]*)*)""|'(?:[^'\\]*(?:\\.[^'\\]*)*)'|[^\s]+)", RegexOptions.Compiled)

    type BlockToken(text, location) =
        inherit TextToken(text, location)
        let mutable fragments = None
        
        member private x.Tokenize =
            match fragments with
            | Some _ -> ()
            | None ->
                fragments <- 
                    match x.BlockBody.Tokenize split_tag_re with
                    | [] -> raise (SyntaxError("Empty tag block"))
                    | _ as tokens -> Some tokens
            fragments.Value
            
        member x.Verb = List.hd x.Tokenize
        member x.Args = List.tl x.Tokenize 
    
    type ErrorToken(text, error:string, location) =
        inherit TextToken(text, location)
          
        member x.ErrorMessage = error

    type VariableToken(text:string, location) =
        inherit TextToken(text, location)
            member x.Expression = x.BlockBody
    
    type CommentToken(text, location) =
        inherit TextToken(text, location) 
    
    /// A lexer token produced through the tokenize function
    type Token =
        | Block of BlockToken
        | Variable of VariableToken
        | Comment of CommentToken
        | Error of ErrorToken
        | Text of TextToken
        
        member private x.TextToken = 
            match x with
            | Block b -> b :> TextToken
            | Error e -> e :> TextToken
            | Variable v -> v :> TextToken
            | Comment c -> c :> TextToken
            | Text t -> t

        member x.Position = x.TextToken.Location.Position
        member x.Length = x.TextToken.Location.Length
        member x.CreateToken location = x.TextToken.CreateToken location
//        member x.CreateToken (capture:Capture) = x.TextToken.CreateTokenAt capture.Index capture.Length
//        member x.Value = x.TextToken.Text
            
    let (|MatchToken|) (t:TextToken) = t.RawText
                   

    let get_textToken = function
    | Block b -> b :> TextToken
    | Error e -> e :> TextToken
    | Variable v -> v :> TextToken
    | Comment c -> c :> TextToken
    | Text t -> t
        
    /// <summary> generates sequence of tokens out of template TextReader </summary>
    /// <remarks>the type implements the IEnumerable interface (sequence) of templates
    /// It reads the template text from the text reader one buffer at a time and 
    /// returns tokens in batches - a batch is a sequence of the tokens generated 
    /// off the content of the buffer </remarks>
    type private Tokenizer (template:TextReader) =
        let mutable current: Token list = []
        let mutable line = 0
        let mutable pos = 0
        let mutable linePos = 0
        let mutable tail = ""
        let buffer = Array.create 4096 ' '
        
        let create_token in_tag (text:string) = 
            in_tag := not !in_tag
            let currentPos = pos
            let currentLine = line
            let currentLinePos = linePos
            Seq.iter 
                (fun ch -> 
                    pos <- pos + 1
                    if ch = '\n' then 
                        line <- line + 1 
                        linePos <- 0
                    else linePos <- linePos + 1
                    ) 
                text
            let location = {Offset = currentPos; Length = text.Length; Line = currentLine; Position = currentLinePos}
            if not !in_tag then
                Text(new TextToken(text, location))
            else
                try
                    match text.[0..1] with
                    | "{{" -> Variable (new VariableToken(text, location))
                    | "{%" -> Block (new BlockToken(text, location))
                    | "{#" -> Comment (new CommentToken(text, location))
                    | _ -> Text (new TextToken(text, location))
                with
                | :? SyntaxError as ex -> 
                    Error (new ErrorToken(text, ex.Message, location))
                | _ -> rethrow()
        
        interface IEnumerator<Token seq> with
            member this.Current = Seq.of_list current
        
        interface IEnumerator with
            member this.Current = Seq.of_list current :> obj
            
            member this.MoveNext() =
                match tail with
                | null -> 
                    false
                | _ -> 
                    let count = template.ReadBlock(buffer, 0, buffer.Length)
                    let strings = (Constants.tag_re.Split(tail + new String(buffer, 0, count)))
                    let t, strings = 
                        if (count > 0) then strings.[strings.Length-1], strings.[0..strings.Length - 2]
                        else null, strings
                    
                    tail <- t
                    let in_tag = ref true
                    current <- strings |> List.of_array |> List.map (create_token in_tag)
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
    let internal tokenize (template:TextReader) =
        LazyList.of_seq <| Seq.fold (fun (s:Token seq) (item:Token seq) -> Seq.append s item) (seq []) (new Tokenizer(template))
