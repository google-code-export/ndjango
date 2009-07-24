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

namespace NDjango.Template

open System.Text
open System.Collections
open System.Collections.Generic
open System.IO

open NDjango.Interfaces
open NDjango.Filters
open NDjango.Constants
open NDjango.Tags
open NDjango.Tags.Misc

module internal Internals =    
    /// adds the key/value pair into the supplied map (usage: map ++ (key, value))
    let internal (++) map (key: 'a, value: 'b) = Map.add key value map

    let internal standardFilters = 
        new Map<string, ISimpleFilter>([])
            ++ ("date", (new Now.DateFilter() :> ISimpleFilter))
            ++ ("escape", (new EscapeFilter() :> ISimpleFilter))
            ++ ("force_escape", (new ForceEscapeFilter() :> ISimpleFilter))
            ++ ("slugify", (new Slugify() :> ISimpleFilter))
            ++ ("truncatewords" , (new TruncateWords() :> ISimpleFilter))
            ++ ("urlencode", (new UrlEncodeFilter() :> ISimpleFilter))
            ++ ("urlize", (new UrlizeFilter() :> ISimpleFilter))
            ++ ("urlizetrunc", (new UrlizeTruncFilter() :> ISimpleFilter))
            ++ ("wordwrap", (new WordWrapFilter() :> ISimpleFilter))
            ++ ("default_if_none", (new DefaultIfNoneFilter() :> ISimpleFilter))
            ++ ("linebreaks", (new LineBreaksFilter() :> ISimpleFilter))
            ++ ("linebreaksbr", (new LineBreaksBrFilter() :> ISimpleFilter))
            ++ ("striptags", (new StripTagsFilter() :> ISimpleFilter))
            ++ ("join", (new JoinFilter() :> ISimpleFilter))
            ++ ("yesno", (new YesNoFilter() :> ISimpleFilter))
            ++ ("dictsort", (new DictSortFilter() :> ISimpleFilter))
            ++ ("dictsortreversed", (new DictSortReversedFilter() :> ISimpleFilter))
            ++ ("time", (new Now.TimeFilter() :> ISimpleFilter))
            ++ ("timesince", (new Now.TimeSinceFilter() :> ISimpleFilter))
            ++ ("timeuntil", (new Now.TimeUntilFilter() :> ISimpleFilter))
            ++ ("pluralize", (new PluralizeFilter() :> ISimpleFilter))
            ++ ("phone2numeric", (new Phone2numericFilter() :> ISimpleFilter))
            ++ ("filesizeformat", (new FileSizeFormatFilter() :> ISimpleFilter))
            
        
    let internal standardTags =
        new Map<string, ITag>([])
            ++ ("autoescape", (new AutoescapeTag() :> ITag))
            ++ ("block", (new LoaderTags.BlockTag() :> ITag))
            ++ ("comment", (new CommentTag() :> ITag))
            ++ ("cycle", (new Cycle.Tag() :> ITag))
            ++ ("debug", (new DebugTag() :> ITag))
            ++ ("extends", (new LoaderTags.ExtendsTag() :> ITag))
            ++ ("filter", (new Filter.FilterTag() :> ITag))
            ++ ("firstof", (new FirstOfTag() :> ITag))
            ++ ("for", (new For.Tag() :> ITag))
            ++ ("if", (new If.Tag() :> ITag))
            ++ ("ifchanged", (new IfChanged.Tag() :> ITag))
            ++ ("ifequal", (new IfEqual.Tag(true) :> ITag))
            ++ ("ifnotequal", (new IfEqual.Tag(false) :> ITag))
            ++ ("include", (new LoaderTags.IncludeTag() :> ITag))
            ++ ("now", (new Now.Tag() :> ITag))
            ++ ("regroup", (new RegroupTag() :> ITag))
            ++ ("spaceless", (new SpacelessTag() :> ITag))
            ++ ("ssi", (new LoaderTags.SsiTag() :> ITag))
            ++ ("templatetag", (new TemplateTag() :> ITag))
            ++ ("widthratio", (new WidthRatioTag() :> ITag))
            ++ ("with", (new WithTag() :> ITag))
        
    let internal defaultSettings = 
        new Map<string, obj>([])
            ++ ("settings.DEFAULT_AUTOESCAPE", (true :> obj))
            ++ ("settings.TEMPLATE_STRING_IF_INVALID", ("" :> obj))

    type internal DefaultLoader() =
        interface ITemplateLoader with
            member this.GetTemplate source = 
                if not <| File.Exists(source) then
                    raise (FileNotFoundException (sprintf "Could not locate template '%s'" source))
                else
                    (new StreamReader(source) :> TextReader)
                    
            member this.IsUpdated (source, timestamp) = File.GetLastWriteTime(source) > timestamp
            
type Provider (settings, tags, filters, loader) =
    
    /// global lock
    let lockProvider = new obj()

    /// pointer to the most recent template manager. don't want to have an
    /// interface for the sake of an interface, so we'll make it obj to
    /// avoid dealing with circular references
    let active = ref None
    let templates = ref Map.Empty

    public new () =
        new Provider(Internals.defaultSettings, Internals.standardTags, Internals.standardFilters, (new Internals.DefaultLoader() :> ITemplateLoader))
        
    member x.NewWithSettings new_settings = new Provider(new_settings, tags, filters, loader)
    
    member x.NewWithTags new_tags = new Provider(settings, new_tags, filters, loader)

    member x.NewWithFilters new_filters = new Provider(settings, tags, new_filters, loader)
    
    member x.NewWithLoader new_loader = new Provider(settings, tags, filters, new_loader)
    
    member public x.GetNewManager = new Manager(x, !templates, loader) :> ITemplateManager
    
    member internal x.GetTemplate name =
        lock lockProvider 
            (fun() -> 
                match !templates |> Map.tryFind name with
                | None ->
                    x.LoadTemplate name
                | Some (template, ts) -> 
                    if (loader.IsUpdated(name, ts)) then
                        x.LoadTemplate name
                    else
                        ((template :> ITemplate), ts)
            )
            
    member internal x.LoadTemplate name =
        lock lockProvider
            (fun() ->
                let t = ((new Impl(x, loader.GetTemplate(name)) :> ITemplate), System.DateTime.Now)
                templates := Map.add name t !templates  
                t
            )
    
    interface IManagerProvider with

        member x.FindTag name =
            Map.tryFind name tags

        member x.FindFilter name =
            Map.tryFind name filters
            
        member x.Settings = settings

and internal Manager(provider, templates, loader) =
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
            if loader.IsUpdated (name, ts) then
               load_template name true
            else
               template
        | None ->
               load_template name false

    member x.Provider = provider :> IManagerProvider
    
    interface ITemplateContainer with
        member x.RenderTemplate (name, context) =
            (x.GetTemplate name).Walk x context

        member x.GetTemplateReader name = loader.GetTemplate name

        member this.GetTemplateVariables name = 
            Array.of_list (this.GetTemplate name).GetVariables 
    
        
/// Implements the template (ITemplate interface)
and private Impl(provider, template) =

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
            
        member this.WithNewManager(manager) =
            new Context(manager, externalContext, variables, autoescape) :> IContext
            
        member this.Manager =
            manager

//            
//            member this.FindTag name = Map.tryFind name tags
//            
//            member this.FindFilter name = Map.tryFind name filters
            
        

//        /// Retrieves the current active template manager
//        member private x.GetActiveManager = 
//                    lock lockMgr 
//                        (fun () -> 
//                            match !active with
//                            | None -> 
//                                let m = new Manager(x) 
//                                active := Some m
//                                m
//                            | Some m -> m
//                        )
                        
//    and private Manager(provider, filters, tags, templates, loader, settings) =
        
        
//        new (provider) =
//            new Manager(provider, standardFilters, standardTags, (new DefaultLoader() :> ITemplateLoader), defaultSettings)
//            
//        new (provider, filters, tags, loader, settings) =
//            new Manager(provider, filters, tags, Map.Empty, loader, settings)
                    
//        [<OverloadID("filters")>]
//        member x.Register(name, filter: ISimpleFilter) =
//            new Manager(provider, filters ++ (name, filter), tags, !templates, loader, settings)
//
//        [<OverloadID("tags")>]
//        member x.Register(name, tag: ITag) =
//            new Manager(provider, filters, tags ++ (name, tag), !templates, loader, settings)
//
//        [<OverloadID("loader")>]
//        member x.Register(new_loader: ITemplateLoader) =
//            new Manager(provider, filters, tags, !templates, new_loader, settings)
//            
//        member x.GetSettingValue name = settings.TryFind(name)
//            
//        member x.SetSettingValue(name, value) =
//            new Manager(provider, filters, tags, !templates, loader, settings ++ (name,value))
//            
//        member private x.RegisterTemplate name template =
//            let t = new Impl(template, x)
//            templates := Map.add name (t, System.DateTime.Now) !templates
//            t

//        member x.GetTemplateInternal name = 
//            Map.tryFind name !templates
                    
        /// Retrieves a template, along with the instance of the ITemplateManager that contains it.
        /// While any GetTemplate request is guaranteed to retrieve the latest version of the
        /// ITemplate, retaining the instance (the second in the returned tuple) will be more
        /// efficient, as subsequent requests for that template from the returned instance are
        /// guaranteed to be non-blocking
//        member x.GetTemplate name =
//            match Map.tryFind name !templates with
//            | Some (template, ts) -> 
//                if loader.IsUpdated (name, ts) then
//                   provider.GetTemplate(x, name)
////                    loader.GetTemplate name |> x.RegisterTemplate name
//                else
//                   template
//            | None ->
//                provider.GetTemplate(x, name) 
//                lock lockMgr 
//                    (fun () -> 
//                        let mgr = !active
//                        match Map.tryFind full_name mgr.Templates with
//                        | Some (template, ts) -> this.GetAndReloadIfNeeded full_name template ts
//                        | None -> mgr.RegisterTemplate full_name (loader.GetTemplate full_name)
//                    )
//    and oldManager =
//        /// Retrieves the template, and verifies that if a template is currently available,
//        /// that it is still valid. if not valid, causes a reload to occur, and all outstanding
//        /// 
//        member private this.GetAndReloadIfNeeded full_name template ts = 
//            if loader.IsUpdated (full_name, ts) then
//                lock lockMgr 
//                    (fun () -> (!active).RegisterTemplate full_name (loader.GetTemplate full_name) )
//            else
//                ((this:>ITemplateManager), template)

        /// Retrieves a template, along with the instance of the ITemplateManager that contains it.
        /// While any GetTemplate request is guaranteed to retrieve the latest version of the
        /// ITemplate, retaining the instance (the second in the returned tuple) will be more
        /// efficient, as subsequent requests for that template from the returned instance are
        /// guaranteed to be non-blocking
//        member this.GetTemplate full_name =
//            match Map.tryFind full_name templates with
//            | Some (template, ts) -> 
//                this.GetAndReloadIfNeeded full_name template ts
//            | None -> 
//                lock lockMgr 
//                    (fun () -> 
//                        let mgr = !active
//                        match Map.tryFind full_name mgr.Templates with
//                        | Some (template, ts) -> this.GetAndReloadIfNeeded full_name template ts
//                        | None -> mgr.RegisterTemplate full_name (loader.GetTemplate full_name)
//                    )

//        interface ITemplateContainer with
//            member this.RenderTemplate (full_name, context) = 
//                let manager, template = this.GetTemplate full_name
//                (manager, template.Walk context)
//
//            member this.GetTemplateReader full_name = loader.GetTemplate full_name
//                            
//            member this.Settings = settings
//            
//            member this.FindTag name = Map.tryFind name tags
//            
//            member this.FindFilter name = Map.tryFind name filters
//            
//            member this.GetTemplateVariables name = 
//                let t = this.GetTemplate name |> snd
//                Array.of_list t.GetVariables 
//
//        /// Creates a new filter manager with the tempalte registered 
//        member private this.RegisterTemplate name template =
//            // this is called internally, from within a lock statement
//            let t = new Impl(template, this) :> ITemplate
//            
//            // this may be an update call. if that's the case, then we should
//            // add the template into a map that has it already removed
//            let new_templates = 
//                Map.add name (t, System.DateTime.Now) <|
//                match Map.tryFind name this.Templates with
//                | Some (template, ts) -> Map.remove name this.Templates
//                | None -> this.Templates
//                
//            active := new Manager(this.Filters, this.Tags, new_templates, this.Loader, this.Settings)
//            (!active :> ITemplateManager), t
