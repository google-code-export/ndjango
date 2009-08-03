using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.Text.Tagging;
using Microsoft.VisualStudio.Text;
using NDjango.Designer.Parsing;

namespace NDjango.Designer.Tags
{
    class Tagger : ITagger<Constants.ErrorTag>
    {
        private NodeProvider nodeProvider;

        public Tagger(IParserProviderBorker parser, ITextBuffer buffer)
        {
            nodeProvider = parser.GetNodeProvider(buffer);
            nodeProvider.NodesChanged += new NodeProvider.SnapshotEvent(provider_TagsChanged);
        }

        void provider_TagsChanged(SnapshotSpan snapshotSpan)
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
                foreach (NodeSnapshot node in nodeProvider.GetNodes(span))
                {
                    if (node.SnapshotSpan.OverlapsWith(span) && node.Node.ErrorMessage.Severity > 0)
                        yield return new TagSpan<Constants.ErrorTag>(node.SnapshotSpan, new Constants.ErrorTag());
                }
            }
        }

        public event EventHandler<SnapshotSpanEventArgs> TagsChanged;

    }
}
