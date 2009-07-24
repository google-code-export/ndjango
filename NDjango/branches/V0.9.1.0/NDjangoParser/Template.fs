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
open System.Collections
open System.Collections.Generic
open System.IO

open NDjango.Interfaces
open NDjango.Filters
open NDjango.Constants
open NDjango.Tags
open NDjango.Tags.Misc

module internal Template =    

    type internal Manager(provider:ITemplateManagerProvider, templates) =
        
        let templates = ref(templates)
        
        let load_template name validated =
            let tr = 
                if validated then provider.LoadTemplate name
                else provider.GetTemplate name
            templates := Map.add name tr !templates
            fst tr

        member internal x.GetTemplate name =
            match Map.tryFind name !templates with
            | Some (template, ts) -> 
                if provider.Loader.IsUpdated (name, ts) then
                   load_template name true
                else
                   template
            | None ->
                   load_template name false

        member x.Provider = provider
        
        interface ITemplateContainer with
            member x.RenderTemplate (name, context) =
                (x.GetTemplate name).Walk x context

            member x.GetTemplateReader name = provider.Loader.GetTemplate name

            member this.GetTemplateVariables name = 
                Array.of_list (this.GetTemplate name).GetVariables 
        
            
    /// Implements the template (ITemplate interface)
    and internal Impl(provider : ITemplateManagerProvider, template: TextReader) =

        let node_list =

            let parser = new NDjango.Parser.DefaultParser(provider) :> IParser
            // this will cause the TextReader to be closed when the template goes out of scope
            use template = template
            fst <| parser.Parse (NDjango.Lexer.tokenize template) []
        
        interface ITemplate with
            member this.Walk manager context=
                new NDjango.ASTWalker.Reader (
                    {parent=None; 
                     nodes=node_list; 
                     buffer="";
                     bufferIndex = 0; 
                     context=new Context(manager, context, (new Map<string,obj>(context |> Seq.map (fun item-> (item.Key, item.Value)))))
                    }) :> System.IO.TextReader
                
            member this.Nodes = node_list
            
            member this.GetVariables = node_list |> List.fold (fun result node -> result @ node.GetVariables) []
            
    and
        private Context (manager, externalContext, variables, ?autoescape: bool) =

        let settings = (manager :?> Manager).Provider.Settings
            
        let autoescape = match autoescape with | Some v -> v | None -> settings.["settings.DEFAULT_AUTOESCAPE"] :?> bool
        
        override this.ToString() =
            
            let autoescape = "autoescape = " + autoescape.ToString() + "\r\n"
            let vars =
                variables |> Microsoft.FSharp.Collections.Map.fold
                    (fun result name value -> 
                        result + name + 
                            if (value = null) then " = NULL\r\n"
                            else " = \"" + value.ToString() + "\"\r\n" 
                        ) "" 
                        
            externalContext.ToString() + "\r\n---- NDjango Context ----\r\nSettings:\r\n" + autoescape + "Variables:\r\n" + vars

        interface IContext with
            member this.add(pair) =
                new Context(manager, externalContext, Map.add (fst pair) (snd pair) variables, autoescape) :> IContext
                
            member this.tryfind(name) =
                match variables.TryFind(name) with
                | Some v -> Some v
                | None -> None 
                
            member this.GetTemplate(template) = 
                (manager :?> Manager).GetTemplate(template)

            member this.TEMPLATE_STRING_IF_INVALID = settings.["settings.TEMPLATE_STRING_IF_INVALID"]
            
            member this.Autoescape = autoescape

            member this.WithAutoescape(value) =
                new Context(manager, externalContext, variables, value) :> IContext
