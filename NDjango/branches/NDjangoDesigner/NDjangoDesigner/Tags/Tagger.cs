using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.Text.Tagging;
using Microsoft.VisualStudio.Text;
using NDjango.Designer.Parsing;

namespace NDjango.Designer.Tags
{
    class Tagger : ITagger<Constants.ErrorTag>
    {
        private NodeProvider tokenizer;

        public Tagger(IParserProviderBorker parser, ITextBuffer buffer)
        {
            tokenizer = parser.GetNodeProvider(buffer);
            tokenizer.NodesChanged += new NodeProvider.SnapshotEvent(tokenizer_TagsChanged);
        }

        void tokenizer_TagsChanged(SnapshotSpan snapshotSpan)
        {
            if (TagsChanged != null)
                TagsChanged(this, new SnapshotSpanEventArgs(snapshotSpan));
        }

        /// <summary>
        /// Gets a list of tags related to a span
        /// </summary>
        /// <param name="spans"></param>
        /// <returns></returns>
        public IEnumerable<ITagSpan<Constants.ErrorTag>> GetTags(Microsoft.VisualStudio.Text.NormalizedSnapshotSpanCollection spans)
        {
            foreach (SnapshotSpan span in spans)
            {
                foreach (NodeSnapshot token in tokenizer.GetNodes(span))
                {
                    if (token.SnapshotSpan.OverlapsWith(span) && token.Node.ErrorMessage.Severity > 0)
                        yield return new TagSpan<Constants.ErrorTag>(token.SnapshotSpan, new Constants.ErrorTag());
                }
            }
        }

        public event EventHandler<SnapshotSpanEventArgs> TagsChanged;

    }
}
