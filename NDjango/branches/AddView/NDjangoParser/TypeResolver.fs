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

namespace NDjango

open NDjango.Interfaces
open NDjango.Expressions
open System.Reflection

module TypeResolver =
    
    type ValueDjangoType(name) =
        interface IDjangoType with
            member x.Name = name
            member x.Type = DjangoType.Value
            member x.Members = Seq.empty

    type CLRTypeMember(expression: FilterExpression, member_name:string) =
        interface IDjangoType with
            member x.Name = member_name
            member x.Type = DjangoType.Value
            member x.Members = Seq.empty

    type TypedValueDjangoType(name, _type) =
        
        let resolve (_type:System.Type) =

            let build_descriptor name _type mbrs =
                let result = TypedValueDjangoType(name, _type) :> IDjangoType
                [result] |> List.toSeq |> Seq.append mbrs


            let validate_method (_method:MethodInfo) = 
                if _method.ContainsGenericParameters then false
                else if _method.IsGenericMethodDefinition then false
                else if _method.IsGenericMethod then false
                else if _method.IsConstructor then false
                else if _method.GetParameters().Length > 0 then false
                else if _method.ReturnType = null then false
                else true
                    

            if _type = null then Seq.empty
            else
                _type.GetMembers() |>
                Array.toSeq |>
                Seq.fold 
                    (fun mbrs mbr ->
                        match mbr.MemberType with
                        | MemberTypes.Field -> build_descriptor mbr.Name (mbr :?> FieldInfo).FieldType mbrs
                        | MemberTypes.Property -> build_descriptor mbr.Name (mbr :?> PropertyInfo).PropertyType mbrs
                        | MemberTypes.Method when validate_method (mbr :?> MethodInfo) -> 
                            build_descriptor mbr.Name (mbr :?> MethodInfo).ReturnType mbrs
                        | _ -> mbrs
                        ) 
                    Seq.empty

        interface IDjangoType with
            member x.Name = name
            member x.Type = DjangoType.Value
            member x.Members = resolve _type

    type AbstractTypeResolver() =
        
        abstract member GetType: string -> System.Type
        default x.GetType type_name : System.Type = null
        
        interface ITypeResolver with
            member x.Resolve name = 
               (TypedValueDjangoType("", x.GetType(name)) :> IDjangoType).Members

    type DefaultTypeResolver() =
        interface ITypeResolver with
            member x.Resolve type_name = [] |> List.toSeq
            
