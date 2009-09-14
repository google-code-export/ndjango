using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NDjango.Designer.Parsing;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Language.Intellisense;

namespace NDjango.Designer.CodeCompletion
{
    class FilterCompletionSet : CompletionSet
    {
        new internal static CompletionSet Create(List<IDjangoSnapshot> nodes, SnapshotPoint point)
        {
            IDjangoSnapshot node = nodes.FindLast(n => n.ContentType == ContentType.FilterName);
            if (node == null)
                return null;
            return new FilterCompletionSet(node, point);
        }

        IDjangoSnapshot node;
        private FilterCompletionSet(IDjangoSnapshot node, SnapshotPoint point)
            : base(node, point)
        {
            this.node = node;
        }

        protected override List<Completion> NodeCompletions
        {
            get { return new List<Completion>(BuildCompletions(node.Values, "|", "")); }
        }

        protected override int FilterOffset { get { return 1; } }

    }
}
