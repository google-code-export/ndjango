#light

open Utilities

type Settings (settings: System.Collections.Generic.IDictionary<string, obj>) =
    let settings = Map<string, obj>.of_dictionary settings
    
    let safe_find key = 
        match settings |> Map.tryfind key with
        | Some v -> v
        | None -> System.String.Empty :> obj
    
    member x.TEMPLATE_STRING_IF_INVALID = safe_find "TEMPLATE_STRING_IF_INVALID"
    member x.autoescape = "dj_autoescape"
    abstract member GetTemplate:string -> string
    default this.GetTemplate(template) = template