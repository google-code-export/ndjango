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

        string Type { get; }

        void TranslateTo(ITextSnapshot snapshot);

        void ShowDiagnostics(IVsOutputWindowPane djangoDiagnostics, string filePath);


        bool IsPlaceholder { get; }

        ContentType ContentType { get; }

        IList<string> Values { get; }

        string Description { get; }

        Error ErrorMessage { get; }
    }
}
