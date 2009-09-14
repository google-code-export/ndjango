using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NDjango.Designer.Parsing;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Language.Intellisense;

namespace NDjango.Designer.CodeCompletion
{
    class TagNameCompletionSet : CompletionSet
    {
        ParserNodes.TagNameNode node;
        internal TagNameCompletionSet(IDjangoSnapshot node, SnapshotPoint point)
            : base(node, point)
        {
            this.node = node.Node as NDjango.ParserNodes.TagNameNode;
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
            return BuildCompletions(values, "", "");
        }

    }
}
