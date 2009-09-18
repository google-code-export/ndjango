using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NDjango.Designer.Parsing;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Language.Intellisense;

namespace NDjango.Designer.CodeCompletion
{
    class TagCompletionSet : CompletionSet
    {
        internal static CompletionSet Create(List<IDjangoSnapshot> nodes, SnapshotPoint point)
        {
            IDjangoSnapshot node = nodes.FindLast(n => n.ContentType == ContentType.Context);
            if (node == null)
                return null;
            return new TagCompletionSet(node, point);
        }

        ParserNodes.ParsingContextNode node;
        private TagCompletionSet(IDjangoSnapshot node, SnapshotPoint point)
            : base (node, point)
        {
            this.node = node.Node as NDjango.ParserNodes.ParsingContextNode;
        }

        protected override List<Completion> NodeCompletions
        {
            get { return new List<Completion>(BuildCompletions(node.Context.Tags)); }
        }

        protected override List<Completion> NodeCompletionBuilders
        {
            get { return new List<Completion>(BuildCompletions(node.Context.TagClosures)); }
        }

        private IEnumerable<Completion> BuildCompletions(IEnumerable<string> values)
        {
            return BuildCompletions(values, "% ", " %}");
        }


        protected override int FilterOffset { get { return 1; } }

    }
}
