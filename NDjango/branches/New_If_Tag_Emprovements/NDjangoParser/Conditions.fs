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

open NDjango.Expressions
open NDjango.Interfaces

module ExpressionNodes =

    /// Represents one operation in operations tree
    /// For example, we have the next tree:
    ///
    ///         and
    ///       /    \
    ///    x < 3  t == "test"
    ///
    /// Here 'and' - parent operation, that has two child operations ('<' and '==').
    ///
    /// Properties of 'and' operation:
    ///     operationType   - 'and'
    ///     firstToken      - None
    ///     secondToken     - None
    ///     Parent          - None
    ///     firstUnit       - operation '<'
    ///     secondUnit      - operation '=='
    ///
    /// Properties of '<' operation:
    ///     operationType   - '<'
    ///     firstToken      - TextToken (x)
    ///     secondToken     - TextToken (3)
    ///     Parent          - operation 'and'
    ///     firstUnit       - None
    ///     secondUnit      - None
    ///
    /// Properties of '==' operation:
    ///     operationType   - '=='
    ///     firstToken      - TextToken (t)
    ///     secondToken     - TextToken ("test")
    ///     Parent          - operation 'and'
    ///     firstUnit       - None
    ///     secondUnit      - None
    
    let compare context (left:FilterExpression) (right:FilterExpression) =
            let left = 
                match fst (left.Resolve context true) with
                | Some o -> o
                | None -> null
            let right = 
                match fst (right.Resolve context true) with
                | Some o -> o
                | None -> null
            System.Collections.Comparer.Default.Compare(left,right)

    type Node(
                resolver: IContext -> bool
             ) =

        member x.Resolve context =
            resolver context

    type AndNode (left:Node, right:Node)=
        inherit Node(fun context -> left.Resolve(context) && right.Resolve(context))

    type OrNode (left:Node, right:Node)=
        inherit Node(fun context -> left.Resolve(context) || right.Resolve(context))

    type NotNode (right:Node)=
        inherit Node(fun context -> not <| right.Resolve(context))

    type ComparerNode (left:FilterExpression, right:FilterExpression, resolver)=
        inherit Node(resolver)

    type LessThan (left, right)=
        inherit ComparerNode(left, right, fun context -> (compare context left right) < 0)

    type GreaterThan (left, right)=
        inherit ComparerNode(left, right, fun context -> (compare context left right) > 0)
