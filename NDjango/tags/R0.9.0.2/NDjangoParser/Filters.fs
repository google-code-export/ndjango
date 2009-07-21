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
open System.Text
open System.Text.RegularExpressions
open System.Collections
open System.Collections.Generic
open System.Web
open System.Globalization
open NDjango.Interfaces

module internal Filters = 
    
    type IEscapeFilter =
    
    /// Escapes a string's HTML. Specifically, it makes these replacements:
    ///    * < is converted to &lt;
    ///    * > is converted to &gt;
    ///    * ' (single quote) is converted to &#39;
    ///    * " (double quote) is converted to &quot;
    ///    * & is converted to &amp;
    ///
    ///The escaping is only applied when the string is output, so it does not matter
    ///where in a chained sequence of filters you put escape: it will always be applied 
    ///as though it were the last filter. If you want escaping to be applied immediately, 
    ///use the force_escape filter.
    ///
    ///Applying escape to a variable that would normally have auto-escaping applied to the 
    ///result will only result in one round of escaping being done. So it is safe to use 
    ///this function even in auto-escaping environments. If you want multiple escaping 
    ///passes to be applied, use the force_escape filter.
    ///
    ///Changed in Django 1.0: Due to auto-escaping, the behavior of this filter has changed 
    ///slightly. The replacements are only made once, after all other filters are applied -- 
    ///including filters before and after it.
    type EscapeFilter() =
        interface IEscapeFilter
        interface ISimpleFilter with
            member x.Perform value = value
        
    /// Applies HTML escaping to a string (see the escape filter for details). This filter 
    ///is applied immediately and returns a new, escaped string. This is useful in the rare 
    ///cases where you need multiple escaping or want to apply other filters to the escaped 
    ///results. Normally, you want to use the escape filter.
    type ForceEscapeFilter() = 
        interface ISimpleFilter with
            member x.Perform value = OutputHandling.escape (Convert.ToString (value, CultureInfo.InvariantCulture)):> obj
        
    /// Converts to lowercase, removes non-word characters (alphanumerics and underscores) 
    ///and converts spaces to hyphens. Also strips leading and trailing whitespace.
    ///
    ///For example:
    ///{{ value|slugify }}
    ///If value is "Joel is a slug", the output will be "joel-is-a-slug".
    type Slugify() =
        let sl1Regex = Regex("[^\w\s-]",RegexOptions.Compiled)
        let sl2Regex = Regex("[-\s]+",RegexOptions.Compiled)                 
        interface ISimpleFilter with
            member x.Perform value =  
                 let s = Convert.ToString (value, CultureInfo.InvariantCulture) |> Encoding.ASCII.GetBytes |> Encoding.ASCII.GetString
                 let s = s.Normalize NormalizationForm.FormKD |> String.lowercase
                 let s = sl1Regex.Replace(s, String.Empty)
                 let s = s.Trim()
                 sl2Regex.Replace(s, "-") :> obj 
                 
                 

    /// Truncates a string after a certain number of words.
    ///
    ///Argument: Number of words to truncate after
    ///For example:
    ///{{ value|truncatewords:2 }}
    ///If value is "Joel is a slug", the output will be "Joel is ...".                     
    type TruncateWords() =
        let wsRegex = Regex("\S([,\.\s]+|\z)", RegexOptions.Compiled)
        interface IFilter with
            member x.DefaultValue 
                with get() = null
            member x.Perform value = raise (System.Exception("Not implemented."))
            member x.PerformWithParam (value, arg) = 
                let success, limit = Int32.TryParse (Convert.ToString (arg, CultureInfo.InvariantCulture))
                if success then
                    let value = Convert.ToString (value, CultureInfo.InvariantCulture)
                    let rec truncate limit start (words:Match seq) =
                        if Seq.is_empty words then ""
                        elif limit = 0 then "..."
                        else 
                            let m = words |> Seq.hd 
                            let s = String.sub value start (m.Index + m.Length - start)
                            s + truncate (limit-1) (m.Index + m.Length) (Seq.skip 1 words)
                    wsRegex.Matches value |> Seq.cast |> truncate limit 0 :> obj
                else
                    value
                        
    /// Escapes a value for use in a URL.                
    type UrlEncodeFilter() =
        interface ISimpleFilter with
            member x.Perform value = 
                let strVal = value |> Convert.ToString |> HttpUtility.UrlEncode
                strVal :> obj
        

    /// function for Urlize and UrlizeTrunc    
    let processUrlize (url:string) (trimCount) = 
        let ProcessUrlizeString (trimlimit : int option) (str : string) =
            let LEADING_PUNCTUATION  = ["\\("; "<"; "&lt;"]
            let TRAILING_PUNCTUATION = ["\\."; ","; "\\)"; ">"; "\\n"; "&gt;"]
            let punctuation_re_input = Printf.sprintf "^(?<lead>(?:%s)*)(?<middle>.*?)(?<trail>(?:%s)*)$" (String.Join("|",List.to_array LEADING_PUNCTUATION) ) (String.Join("|", List.to_array TRAILING_PUNCTUATION) ) 
            let punctuation_re = new Regex ( punctuation_re_input , RegexOptions.ExplicitCapture  )
            let simple_email_re = new Regex (@"^\S+@[a-zA-Z0-9._-]+\.[a-zA-Z0-9._-]+$")

            let trim_url (url:string) (trimCount:int option) = 
                if (trimCount.IsNone) then url 
                    else
                        let trimCount = trimCount.Value
                        if (trimCount < url.Length) 
                            then 
                                if (trimCount >=3) then (Printf.sprintf "%s..." (url.Substring(0, (trimCount - 3))))
                                else "..."
                            else url
        
            let rec FindSubString (str : string) subsList =
                match subsList with
                    | (sub : string) :: tail -> if str.Contains sub then true else FindSubString str tail
                    | _ -> false
                    
            let rec FindExtension (str: string) extList =
                match extList with
                    | (sub : string) :: tail -> if str.EndsWith sub then true else FindExtension str tail
                    | _ -> false
        
            if (FindSubString str ["."; "@"; ":"]) 
                then
                    let matchStr = punctuation_re.Match str
                    if not matchStr.Success
                        then str 
                        else 
                            let groups = matchStr.Groups
                            let lead = groups.["lead"].Value
                            let middle = groups.["middle"].Value
                            let trail = groups.["trail"].Value
                            let url = 
                                if (middle.StartsWith "http://" || middle.StartsWith "https://") 
                                    then middle
                                elif ((middle.StartsWith "www.") || (not (middle.Contains "@") && (FindExtension middle [".org"; ".net"; ".com"]) ))
                                    then sprintf "http://%s" middle
                                elif middle.Contains "@" && (not (middle.Contains ":")) && simple_email_re.IsMatch middle
                                    then sprintf "mailto:%s" middle
                                else String.Empty
                                        
                            if url = String.Empty 
                                then str
                                else 
                                    let trimmed = trim_url middle trimlimit
                                    let middle = sprintf "<a href=\"%s\">%s</a>" url trimmed
                                    sprintf "%s%s%s" lead middle trail
                else str
    
        let words_split_re = new Regex("\\s+")
        let words = words_split_re.Split(url)
        let wordsList = [for word in words do yield word] |> List.map (ProcessUrlizeString trimCount)
        String.Join (String.Empty,List.to_array wordsList)

    /// Converts URLs in plain text into clickable links.
    ///
    ///Note that if urlize is applied to text that already contains HTML markup, things won't work as expected. Apply this filter only to plain text.
    ///For example:
    ///{{ value|urlize }}
    ///If value is "Check out www.djangoproject.com", the output will be "Check out <a href="http://www.djangoproject.com">www.djangoproject.com</a>".
    type UrlizeFilter() =
        interface ISimpleFilter with
            member x.Perform value = 
                let strVal = Convert.ToString (value, CultureInfo.InvariantCulture)
                processUrlize strVal None :> obj
                
    /// Converts URLs into clickable links, truncating URLs longer than the given character limit.
    ///
    ///As with urlize, this filter should only be applied to plain text.
    ///Argument: Length to truncate URLs to
    ///For example:
    ///{{ value|urlizetrunc:15 }}
    ///If value is "Check out www.djangoproject.com", the output would be 'Check out <a href="http://www.djangoproject.com">www.djangopr...</a>'.                
    type UrlizeTruncFilter() =
        interface IFilter with
            member x.DefaultValue 
                with get() = null
            member x.Perform value = raise (System.Exception("Not implemented."))
            member x.PerformWithParam (value, arg) = 
                let strVal = Convert.ToString (value, CultureInfo.InvariantCulture)
                let success, limit = Int32.TryParse (Convert.ToString (arg, CultureInfo.InvariantCulture))
                if success then
                    processUrlize strVal (Some limit) :> obj
                else 
                    processUrlize strVal None :> obj

    /// Wraps words at specified line length.
    ///
    ///Argument: number of characters at which to wrap the text
    ///For example:
    ///{{ value|wordwrap:5 }}
    ///If value is Joel is a slug, the output would be:
    ///
    ///Joel
    ///is a
    ///slug
    ///
    type WordWrapFilter() =
        let splitWordRe = new Regex (@"((?:\S+)|(?:[\f\t\v\x85\p{Z}]+)|(?:[\r\n]+))", RegexOptions.Compiled)
        let spacesRe = new Regex (@"(\A[\f\t\v\x85\p{Z}]+\z)", RegexOptions.Compiled)
        let crlfRe = new Regex (@"(\A[\r\n]+\z)", RegexOptions.Compiled)
        let ProcessNextString (outStr:string,wrapSize:int,charsAmount:int) (nextStr:string) = 
            let nextSize = String.length nextStr
            let charsAmount, outStr =
                if charsAmount+nextSize > wrapSize then
                    match nextStr with
                    | _ when (spacesRe.IsMatch nextStr) ->
                        0,
                        (outStr.TrimEnd null) + "\r\n"
                    | _ when (crlfRe.IsMatch nextStr) ->
                        0,
                        (outStr.TrimEnd null) + "\r\n"
                    | _ ->
                        nextSize,
                        (outStr.TrimEnd null) + "\r\n" + nextStr
                else 
                    match nextStr with
                    | _ when (crlfRe.IsMatch nextStr) ->
                        0,
                        (outStr.TrimEnd null) + nextStr
                    | _ ->
                        charsAmount + nextSize,
                        outStr + nextStr
            (outStr,wrapSize, charsAmount)     
            
        interface IFilter with
            member x.DefaultValue 
                with get() = null
            member x.Perform value = raise (System.Exception("Not implemented."))
            member x.PerformWithParam (value, arg) = 
                let strVal = Convert.ToString (value, CultureInfo.InvariantCulture)
                let success, limit = Int32.TryParse (Convert.ToString (arg, CultureInfo.InvariantCulture))
                if success then 
                    let splittedText = splitWordRe.Matches strVal
                    let resultWords =
                        [for wordMatch in splittedText do
                             yield wordMatch.Value
                        ]
                    let result, useless1, useless2 = List.fold_left ProcessNextString ("",limit,0) resultWords 
                    result :> obj
                else
                    value
             
    /// Replaces line breaks in plain text with appropriate HTML; a single newline becomes an HTML line break (<br />) and a new line followed by a blank line becomes a paragraph break (</p>).
    ///
    ///For example:
    ///{{ value|linebreaks }}
    ///If value is Joel\nis a slug, the output will be <p>Joel<br />is a slug</p>.
    type LineBreaksFilter() =
        let replaceNRRe = new Regex (@"\r\n|\r|\n", RegexOptions.Compiled)
        let splitRegex = new Regex (@"(?:\r\n){2,}", RegexOptions.Compiled)
        interface ISimpleFilter with
            member x.Perform value = 
                let paragraphs = replaceNRRe.Replace ((Convert.ToString (value, CultureInfo.InvariantCulture)), "\r\n") |> splitRegex.Split
                Seq.fold (fun retLine (paragraph: string) -> retLine + "<p>" + paragraph.Replace("\r\n", "<br />") + "</p>") "" paragraphs :> obj
                
    /// Converts all newlines in a piece of plain text to HTML line breaks (<br />).                
    type LineBreaksBrFilter() =
        let replaceNRRe = new Regex (@"\r\n|\r|\n", RegexOptions.Compiled)
        interface ISimpleFilter with
            member x.Perform value = 
                replaceNRRe.Replace ((Convert.ToString (value, CultureInfo.InvariantCulture)), @"<br />") :> obj
                
    /// Strips all [X]HTML tags.
    ///
    ///For example:
    ///{{ value|striptags }}
    ///If value is "<b>Joel</b> <button>is</button> a <span>slug</span>", the output will be "Joel is a slug".
    type StripTagsFilter() =
        let nextTagItemRegex = new Regex (@"</?[A-Za-z][A-Za-z0-9]*[^<>]*>", RegexOptions.Compiled)
        interface ISimpleFilter with
            member x.Perform value = 
                nextTagItemRegex.Replace((Convert.ToString (value, CultureInfo.InvariantCulture)), "") :> obj
        
    /// Joins a list with a string, like Python's str.join(list)
    ///
    ///For example:
    ///{{ value|join:" // " }}
    ///If value is the list ['a', 'b', 'c'], the output will be the string "a // b // c".
    type JoinFilter() =
        interface IFilter with
            member x.DefaultValue 
                with get() = null        
            member x.Perform value = raise (System.Exception("Not implemented."))
            member x.PerformWithParam (value, arg) = 
                match value with 
                    | :? IEnumerable ->
                        let strArr = Seq.map (fun (item: Object)  -> (Convert.ToString (item, CultureInfo.InvariantCulture)) ) (Seq.cast (value :?> IEnumerable) ) |> Seq.to_array 
                        String.Join (Convert.ToString arg, strArr) :> obj
                    | _ -> raise (System.Exception("Type not supported"))
                    
    /// Given a string mapping values for true, false and (optionally) None, returns one of those strings according to the value:
    ///
    ///Value 	Argument 	Outputs
    ///True 	"yeah,no,maybe" 	yeah
    ///False 	"yeah,no,maybe" 	no
    ///None 	"yeah,no,maybe" 	maybe
    ///None 	"yeah,no" 	"no" (converts None to False if no mapping for None is given)
    ///
    ///Note: Does NOT work for None at this time.
    type YesNoFilter() =
        interface IFilter with
            member x.DefaultValue 
                with get() = "yes,no,maybe" :> obj
            member x.Perform value = raise (System.Exception("Not implemented."))
            member x.PerformWithParam (value,arg) = 
                let strYesNoMaybe = String.split [','] (Convert.ToString (arg, CultureInfo.InvariantCulture))
                if strYesNoMaybe.Length < 2 then
                    value
                else
                    let strYesNoMaybe = 
                        if strYesNoMaybe.Length<3 then
                             (List.append strYesNoMaybe [strYesNoMaybe.[1]])
                        else 
                            strYesNoMaybe
                        
                    let retValue = 
                        match value with
                            | :? Boolean when ((value :?> Boolean) = true) -> strYesNoMaybe.[0]
                            | :? (obj option) when ((value :?> (obj option)) = None) -> strYesNoMaybe.[2]
                            | _ -> strYesNoMaybe.[1]
                    retValue :> obj

          
    /// Takes a list of dictionaries and returns that list sorted by the key given in the argument.
    ///
    ///For example:
    ///
    ///{{ value|dictsort:"name" }}
    ///If value is:
    ///
    ///[
    ///    {'name': 'zed', 'age': 19},
    ///    {'name': 'amy', 'age': 22},
    ///    {'name': 'joe', 'age': 31},
    ///]
    ///
    ///then the output would be:
    ///
    ///[
    ///    {'name': 'amy', 'age': 22},
    ///    {'name': 'joe', 'age': 31},
    ///    {'name': 'zed', 'age': 19},
    ///]
    ///
    type DictSortFilter() = 
        interface IFilter with
            member x.DefaultValue 
                with get() = null
            member x.Perform value = raise (System.Exception("Not implemented."))
            member x.PerformWithParam (value, arg) = 
                let arg = Convert.ToString (arg, CultureInfo.InvariantCulture)
                match value with
                    | :? IEnumerable ->
                        let value = (value :?> IEnumerable) |> Seq.cast
                        let value = Seq.sort_by (fun (elem: IDictionary<string,IComparable>) -> elem.[arg]) value
                        value :> obj
                    | _ -> 
                        value

    /// Takes a list of dictionaries and returns that list sorted in reverse order by the key given 
    ///in the argument. This works exactly the same as the above filter, but the returned value will be 
    ///in reverse order.                    
    type DictSortReversedFilter() = 
        interface IFilter with
            member x.DefaultValue 
                with get() = null
            member x.Perform value = raise (System.Exception("Not implemented."))
            member x.PerformWithParam (value, arg) = 
                let arg = Convert.ToString (arg, CultureInfo.InvariantCulture)
                match value with
                    | :? IEnumerable ->
                        let value = (value :?> IEnumerable) |> Seq.cast
                        let value = Seq.sort_by (fun (elem: IDictionary<string,IComparable>) -> elem.[arg]) value
                        let value = Seq.to_list value |> List.rev
                        value :> obj
                    | _ -> 
                        value
