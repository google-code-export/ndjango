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

open System.IO
open NDjango.Interfaces
open NDjango.Filters
open NDjango.Tags
open NDjango.Tags.Misc

type Filter = {name:string; filter:ISimpleFilter}

type Tag = {name:string; tag:ITag}

type Setting = {name:string; value:obj}

module Defaults = 

    let private (++) map (key: 'a, value: 'b) = Map.add key value map

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
            ++ (Constants.DEFAULT_AUTOESCAPE, (true :> obj))
            ++ (Constants.RELOAD_IF_UPDATED, (true :> obj))
            ++ (Constants.TEMPLATE_STRING_IF_INVALID, ("" :> obj))

type private DefaultLoader() =
    interface ITemplateLoader with
        member this.GetTemplate source = 
            if not <| File.Exists(source) then
                raise (FileNotFoundException (sprintf "Could not locate template '%s'" source))
            else
                (new StreamReader(source) :> TextReader)
                
        member this.IsUpdated (source, timestamp) = File.GetLastWriteTime(source) > timestamp
            
///
/// Manager Provider object serves as a container for all the environment variables controlling 
/// template processing. Various methods of the object provide different ways to change them, 
/// namely add tag and/or filter definitions, change settings or switch the loader. All such 
/// DO NOT affect the provider they are called on, but rather create a new clean provider 
/// instance with modified variables. 
/// Method GetManager on the provider can be used to create an instance of TemplateManager
/// TemplateManagers can be used to render templates. 
/// All methods and properties of the Manager Provider use locking as necessary and are thread safe.
type TemplateManagerProvider (settings:Map<string,obj>, tags, filters, loader:ITemplateLoader) =
    
    let (++) map (key: 'a, value: 'b) = Map.add key value map

    /// global lock
    let lockProvider = new obj()

    let templates = ref Map.Empty

    let validate_template = 
        if (settings.[Constants.RELOAD_IF_UPDATED] :?> bool) then loader.IsUpdated
        else (fun (name,ts) -> false) 
    
    public new () =
        new TemplateManagerProvider(Defaults.defaultSettings, Defaults.standardTags, Defaults.standardFilters, new DefaultLoader())
        
    member x.WithSetting name value = new TemplateManagerProvider( settings++(name, value), tags, filters, loader)
    
    member x.WithTag name tag = new TemplateManagerProvider(settings, tags++(name, tag) , filters, loader)

    member x.WithFilter name filter = new TemplateManagerProvider(settings, tags, filters++(name, filter) , loader)
    
    member x.WithSettings new_settings = 
        new TemplateManagerProvider(
            new_settings |> Seq.fold (fun settings setting -> settings++(setting.name, setting.value) ) settings
            , tags, filters, loader)
    
    member x.WithTags (new_tags : Tag seq) = 
        new TemplateManagerProvider(settings, 
            new_tags |> Seq.fold (fun tags tag -> tags++(tag.name, tag.tag)) tags
            , filters, loader)

    member x.WithFilters (new_filters : Filter seq)  = 
        new TemplateManagerProvider(settings, tags, 
            new_filters |> Seq.fold (fun filters filter -> Map.add filter.name filter.filter filters) filters
            , loader)
    
    member x.WithLoader new_loader = new TemplateManagerProvider(settings, tags, filters, new_loader)
    
    member public x.GetNewManager() = new Template.Manager(x, !templates) :> ITemplateManager
    
    interface ITemplateManagerProvider with

        member x.GetTemplate name =
            lock lockProvider 
                (fun() -> 
                    match !templates |> Map.tryFind name with
                    | None ->
                        (x :> ITemplateManagerProvider).LoadTemplate name
                    | Some (template, ts) -> 
                        if (validate_template(name, ts)) then
                            (x :> ITemplateManagerProvider).LoadTemplate name
                        else
                            (template, ts)
                )
                
        member x.LoadTemplate name =
            lock lockProvider
                (fun() ->
                    let t = ((new NDjango.Template.Impl((x :> ITemplateManagerProvider), loader.GetTemplate(name)) :> ITemplate), System.DateTime.Now)
                    templates := Map.add name t !templates  
                    t
                )
    
        member x.Tags = tags

        member x.Filters = filters
            
        member x.Settings = settings
        
        member x.Loader = loader

