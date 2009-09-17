/****************************************************************************
 * 
 *  NDjango Parser Copyright © 2009 Hill30 Inc
 *
 *  This file is part of the NDjango Designer.
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
 ***************************************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Shell.Interop;
using NDjango.Interfaces;

namespace NDjango.Designer.Parsing
{
    enum ContentType
    {
        Default,
        Tag,
        CloseTag,
        FilterName,
        TagName,
        Context
    }

    interface IDjangoSnapshot
    {
        /// <summary>
        /// Span covering the source the INode was created from
        /// </summary>
        SnapshotSpan SnapshotSpan { get; }

        /// <summary>
        /// The extension span for the INode - is empty unless the node has code completion values
        /// if not emoty covers all whitespace to the left of the node 
        /// </summary>
        SnapshotSpan ExtensionSpan { get; }

        IEnumerable<IDjangoSnapshot> Children { get; }

        void TranslateTo(ITextSnapshot snapshot);

        void ShowDiagnostics(IVsOutputWindowPane djangoDiagnostics, string filePath);

        ContentType ContentType { get; }

        INode Node { get; }

        IDjangoSnapshot Parent { get; }
    }
}
