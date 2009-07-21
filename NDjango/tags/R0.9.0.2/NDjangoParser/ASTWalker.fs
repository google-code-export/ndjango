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

open NDjango.Interfaces

module internal ASTWalker =
    type Reader(walker: Walker) =
        inherit System.IO.TextReader()
        
        let mutable walker = walker
        let buffer = Array.create 4096 ' '
        
        let rec getChar() = 
            match walker.buffer with
            | None ->
                match walker.nodes |> Seq.first (fun node -> Some node)  with
                | Some node ->
                    walker <- {walker with nodes = LazyList.of_seq <| Seq.skip 1 walker.nodes}  // advance node list
                    walker <- node.walk(walker);                             // proceed with whatever was given back to us by the node
                    getChar()
                | None ->
                    match walker.parent with
                    | None -> -1               // we are done - nothing more to walk 
                    | Some p -> 
                        walker <- p;            // resume with the parent walker
                        getChar()
            | Some buffer -> 
                if (buffer = "") then 
                    walker <- {walker with buffer=None}
                    getChar()
                else Char.code buffer.[0]

        let read (buffer: char[]) index count = 
            let mutable transferred = 0;
            while getChar() <> -1 && transferred < count do
                match walker.buffer with
                | Some buf ->
                    let mutable index = 0
                    while index < buf.Length && transferred < count do
                        buffer.[transferred] <- buf.[index]
                        transferred <- transferred+1
                        index <- index+1
                    walker <- 
                        if index < buf.Length 
                        then {walker with buffer=Some (buf.Substring(index))}
                        else {walker with buffer=None}
                // None should never happen after a call to getChar                    
                | None -> raise (System.Exception("None should never happen after a call to getChar"))
            transferred

        let rec read_to_end (buffers:System.Text.StringBuilder) = 
            match read buffer 0 buffer.Length with
            | 0 -> buffers
            | _ as l ->
                if l = buffer.Length then 
                    buffers.Append(new System.String(buffer)) |> ignore
                    read_to_end buffers
                else 
                    buffers.Append(new System.String(buffer), 0, l) |> ignore
                    read_to_end buffers


        override this.Peek() =
            getChar()
            
        override this.Read() =
            let result = getChar()
            if result <> -1 then
                match walker.buffer with
                | Some buffer ->
                    if buffer = "" then
                        walker <- {walker with buffer=None}
                    else 
                        walker <- {walker with buffer=Some (buffer.Substring(1))}
                // None should never happen after a call to getChar                    
                | None -> raise (System.Exception("None should never happen after a call to getChar"))
            result
        
        override this.Read(buffer: char[], index: int, count: int) = read buffer index count

        override this.ReadToEnd() = 
         let sb = new System.Text.StringBuilder()
         (read_to_end sb).ToString()
