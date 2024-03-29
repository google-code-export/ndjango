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
open System.Text.RegularExpressions

open NDjango.Lexer
open NDjango.Interfaces
open NDjango.Expressions
open NDjango.OutputHandling

module internal Now =

    ///Display the date, formatted according to the given string.
    ///
    ///Uses the same format as PHP's date() function (http://php.net/date) with some custom extensions.
    ///
    ///Available format strings:
    ///
    ///django Sample       .NET               Description  
    /// char  output  
    /// a    'a.m.'         tt *)   'a.m.' or 'p.m.' (Note that this is slightly different than PHP's output, because this includes periods to match Associated Press style.)  
    /// A    'AM'           tt      'AM' or 'PM'.  
    /// b    'jan'          MMM *)  Month, textual, 3 letters, lowercase.  
    /// B Not implemented.   
    /// d    '01' to'31'    dd      Day of the month, 2 digits with leading zeros.   
    /// D    'Fri'          ddd     Day of the week, textual, 3 letters.  
    /// f    '1','1:30'      ?      Time, in 12-hour hours and minutes, with minutes left off if they're zero. Proprietary extension.   
    /// F    'January'      MMMM    Month, textual, long.  
    /// g    '1' to '12'    h (%h)  Hour, 12-hour format without leading zeros.  
    /// G    '0' to '23'    H (%H)  Hour, 24-hour format without leading zeros.  
    /// h    '01' to '12'   hh      Hour, 12-hour format.  
    /// H    '00' to '23'   HH      Hour, 24-hour format.  
    /// i    '00' to '59'   mm      Minutes.
    /// I Not implemented.   
    /// j    '1' to '31'    d (%d)  Day of the month without leading zeros. 
    /// l    'Friday'       dddd    Day of the week, textual, long. 
    /// L     True or False  ?      Boolean for whether it's a leap year. 
    /// m    '01' to '12'   MM      Month, 2 digits with leading zeros. 
    /// M    'Jan'          MMM     Month, textual, 3 letters. 
    /// n    '1' to '12'    M (%M)  Month without leading zeros. 
    /// N  'Jan.', 'Feb.', 
    ///    'March', 'May'   MMM *)  Month abbreviation in Associated Press style. Proprietary extension. 
    /// O    '+0200'        zzz     Difference to Greenwich time in hours. 
    ///                         *** this will return '+02:00' - note an extra colon in the middle
    /// P  '1 a.m.', 
    ///    '1:30 p.m.', 
    ///    'midnight', 
    ///    'noon', 
    ///    '12:30 p.m.'      ?      Time, in 12-hour hours, minutes and 'a.m.'/'p.m.', with minutes left off if they're zero and the special-case strings 'midnight' and 'noon' if appropriate. Proprietary extension.  
    /// r  'Thu, 21 Dec 2000 16:01:07 +0200'
    ///                             RFC 2822 formatted date. 
    /// s    '00' to '59'   ss      Seconds, 2 digits with leading zeros. 
    /// S    'st', 'nd', 
    ///      'rd' or 'th'    ?      English ordinal suffix for day of the month, 2 characters. 
    /// t    28 to 31        ?      Number of days in the given month.  
    /// T   'EST', 'MDT'     ?      Time zone of this machine.  
    /// U Not implemented.   
    /// w  '0' (Sunday) to 
    ///    '6' (Saturday)   ddd *)  Day of the week, digits without leading zeros.  
    /// W    1, 53           ?      ISO-8601 week number of year, with weeks starting on Monday.  
    /// y    '99'           yy      Year, 2 digits. 
    /// Y    '1999'         yyyy    Year, 4 digits. 
    /// z    0 to 365        ?      Day of the year. 
    /// Z -43200 to 43200    ?      Time zone offset in seconds. The offset for timezones west of UTC is always negative, and for those east of UTC is always positive.  
    ///
    ///Example:
    ///
    ///It is {% now "jS F Y H:i" %}
    ///Note that you can backslash-escape a format string if you want to use the "raw" value. In this example, "f" is backslash-escaped, because otherwise "f" is a format string that displays the time. The "o" doesn't need to be escaped, because it's not a format character:
    ///
    ///It is the {% now "jS o\f F" %}
    ///This would display as "It is the 4th of September".
    ///
    /// Implementation details:
    /// Currently now tag is implemented by mapping the django formatting characters to the corresponding .NET Date formatting sequences
    /// The mapping is shown in the specs above. The django formatting letters which can not be mapped are marked in the table with '?'
    /// The formatting letters which can be mapped by tweaking the DateFormatInfo structure are marked with additional *). Please keep in mind 
    /// that if several formatting letters within the same format string require such modifications, only the last one will take effect

    let format_parser = new Regex(@"([^\\]|\\.)+?", RegexOptions.Compiled)
    let format_map = 
        new Map<char, string>(
            [
/// a    'a.m.'         tt *)   'a.m.' or 'p.m.' (Note that this is slightly different than PHP's output, because this includes periods to match Associated Press style.)  
/// A    'AM'           tt      'AM' or 'PM'.  
                ('a',"tt")  
/// b    'jan'          MMM *)  Month, textual, 3 letters, lowercase.  
/// B Not implemented.   
/// d    '01' to'31'    dd      Day of the month, 2 digits with leading zeros.   
                ('d',"dd")  
/// D    'Fri'          ddd     Day of the week, textual, 3 letters.  
                ('D',"ddd")  
/// f    '1','1:30'             Time, in 12-hour hours and minutes, with minutes left off if they're zero. Proprietary extension.   
/// F    'January'      MMMM    Month, textual, long.  
                ('F',"MMMM")  
/// g    '1' to '12'    h (%h)  Hour, 12-hour format without leading zeros.  
                ('g',"%h")  
/// G    '0' to '23'    H (%H)  Hour, 24-hour format without leading zeros.  
                ('G',"%H")  
/// h    '01' to '12'   hh      Hour, 12-hour format.  
                ('h',"hh")  
/// H    '00' to '23'   HH      Hour, 24-hour format.  
                ('H',"HH")  
/// i    '00' to '59'   mm      Minutes.
                ('i',"mm")  
/// I Not implemented.   
/// j    '1' to '31'    d (%d)  Day of the month without leading zeros. 
                ('j',"%d")  
/// l    'Friday'       dddd    Day of the week, textual, long. 
                ('l',"dddd")  
/// L     True or False  ?      Boolean for whether it's a leap year. 
/// m    '01' to '12'   MM      Month, 2 digits with leading zeros. 
                ('m',"MM")  
/// M    'Jan'          MMM     Month, textual, 3 letters. 
                ('M',"MMM")  
/// n    '1' to '12'    M (%M)  Month without leading zeros. 
                ('n',"%M")  
/// N  'Jan.', 'Feb.', 
///    'March', 'May'   MMM *)  Month abbreviation in Associated Press style. Proprietary extension. 
/// O    '+0200'        zzz     Difference to Greenwich time in hours. 
///                         *** this will return '+02:00' - note an extra colon in the middle
                ('O',"zzz")  
/// P  '1 a.m.', 
///    '1:30 p.m.', 
///    'midnight', 
///    'noon', 
///    '12:30 p.m.'      ?      Time, in 12-hour hours, minutes and 'a.m.'/'p.m.', with minutes left off if they're zero and the special-case strings 'midnight' and 'noon' if appropriate. Proprietary extension.  
/// r  'Thu, 21 Dec 2000 16:01:07 +0200'
///                             RFC 2822 formatted date. 
/// s    '00' to '59'   ss      Seconds, 2 digits with leading zeros. 
                ('s',"ss")  
/// S    'st', 'nd', 
///      'rd' or 'th'    ?      English ordinal suffix for day of the month, 2 characters. 
/// t    28 to 31        ?      Number of days in the given month.  
/// T   'EST', 'MDT'     ?      Time zone of this machine.  
/// U Not implemented.   
/// w  '0' (Sunday) to 
///    '6' (Saturday)   ddd *)  Day of the week, digits without leading zeros.  
/// W    1, 53           ?      ISO-8601 week number of year, with weeks starting on Monday.  
/// y    '99'           yy      Year, 2 digits. 
                ('y',"YY")  
/// Y    '1999'         yyyy    Year, 4 digits. 
                ('Y',"yyyy")  
/// z    0 to 365        ?      Day of the year. 
/// Z -43200 to 43200    ?      Time zone offset in seconds. The offset for timezones west of UTC is always negative, and for those east of UTC is always positive.  
            ])
    let format format = 
        format_parser.Replace(format, 
            (fun (mtch: Match) 
                ->  let m = mtch
                    match format_map.TryFind mtch.Value.[0] with
                    | Some f -> f
                    | None -> if mtch.Value = "\"" then "" else mtch.Value
                    ))
    
    type Tag() =
        interface ITag with
            member this.Perform token parser tokens =
                match token.Args with
                    | f::[] ->
                        {
                            new Node(Block token)
                            with
                                override this.walk walker = 
                                    {walker with buffer = f |> format |> System.DateTime.Now.ToString }
                        }, tokens
                    | _ -> raise (TemplateSyntaxError ("malformed 'now' tag", Some (token:>obj)))
                        

    /// Formats a date according to the given format (same as the now tag).
    ///
    ///For example:
    ///{{ value|date:"D d M Y" }}
    ///If value is a datetime object (e.g., the result of datetime.datetime.now()), the output will be the string 'Wed 09 Jan 2008'.
    ///When used without a format string:
    ///{{ value|date }}
    ///...the DateTime.ToString with empty format string will be used (that's the difference from Django implementation).
    type DateFilter() =
        interface IFilter with
            member x.DefaultValue = null
            member x.Perform value = raise (System.Exception("Not implemented."))
            member x.PerformWithParam (value, args) =
                let format = args |> string |> format 
                let dt =
                    match value with 
                    | :? DateTime as dt1 -> dt1
                    | _ as o -> DateTime.TryParse(o |> string) |> snd
                
                format |> dt.ToString :> obj
                           