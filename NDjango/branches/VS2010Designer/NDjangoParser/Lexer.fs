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

    /// Base class for text tokens. Its main purpose is to keep track of bits of text
    /// participating in the template parsing in their relationship with the original 
    /// location of the text in the template. As the text is going through various 
    /// stages of parsing it may or may not differ from the original text in the source
    type TextToken(text:string, value:string, location: Location) =
  
        new (text:string, location: Location) =
            new TextToken(text, text, location)
            
        /// The original, unmodified text as it is in the source
        member x.RawText = text
        
        /// The value after modifications
        member x.Value = value

        /// Creates a token with the text of the tag body by stripping off
        /// the first two and the last two characters
        member x.BlockBody = 
            let body = x.RawText.[2..x.RawText.Length-3].Trim()
            x.CreateToken(x.RawText.IndexOf(body), body.Length)

        /// Token location
        member x.Location = location
        
        /// Creates a new token from the existing one 
        /// Use this method when you need to create a new token from a part of the text of an existing one                 
        member x.CreateToken (capture:Capture) = x.CreateToken (capture.Index, capture.Length)
        
        /// Creates a new token from the existing one 
        /// Use this method when you need to create a new token from a part of the text of an existing one                 
        member x.CreateToken (offset, length) = 
            new TextToken(x.Value.Substring(offset,length)
                , {location with Offset = location.Offset + offset; Length = length; Position = location.Position + offset}) 
        
        /// Creates a new token bound to the same location in the source, but with a different value
        /// Use this method when you need to modify the token value but keep its binding to the source                
        member x.WithValue new_value = new TextToken(text, new_value, location)

    /// generates a list of tokens by applying the regexp
    let tokenize_for_token location (regex:Regex) value =
            let location_ofMatch (m:Match) =
                {location 
                    with 
                        Offset = location.Offset + m.Groups.[0].Index;
                        Length = m.Groups.[0].Length;
                        Position = location.Position + m.Groups.[0].Index;
                }
            [for m in regex.Matches(value) -> new TextToken(m.Groups.[0].Value, location_ofMatch m)]
            
    /// Locates the tag fragments the tag name and tag arguments by splitting the text into pieces by whitespaces
    /// Whitespaces inside strings in double- or single quotes remain unaffected
    let private split_tag_re = new Regex(@"(""(?:[^""\\]*(?:\\.[^""\\]*)*)""|'(?:[^'\\]*(?:\\.[^'\\]*)*)'|[^\s]+)", RegexOptions.Compiled)
    
    /// Represents a tag block 
    type BlockToken(text, location) =
        inherit TextToken(text, location)
        
        let verb,args =
            match tokenize_for_token location split_tag_re text with
            | [] -> raise (SyntaxError("Empty tag block"))
            | verb::args -> verb,args
        
        /// A.K.A. tag name
        member x.Verb = verb
        /// List of arguments - can be empty
        member x.Args = args 
    
    /// Represents an syntax error in the syntax node tree. These tokens are generated in response
    /// to exceptions thrown during lexing of the template, so that the actual exception throwing can be delayed
    /// till parsing stage
    type ErrorToken(text, error:string, location) =
        inherit TextToken(text, location)
          
        member x.ErrorMessage = error

    /// Represents a variable block
    type VariableToken(text:string, location) =
        inherit TextToken(text, location)
            member x.Expression = 
                let body = x.BlockBody
                if (body.RawText = "") then raise (SyntaxError("Empty variable block"))
                body
    
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
            
        /// provides additional diagnostic information for the token 
        member x.DiagInfo = 
            sprintf " in token: \"%s\" at line %d pos %d " 
                x.TextToken.RawText x.TextToken.Location.Line x.TextToken.Location.Position

   /// Active Pattern matching the Token to a string constant. Uses the Token.RawText to do the match
    let (|MatchToken|) (t:TextToken) = t.RawText
    
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
