using Microsoft.VisualStudio.Text;
using NDjango.Interfaces;

namespace NDjango.Designer.Parsing
{
    class NodeSnapshot
    {
        private SnapshotSpan snapshotSpan;
        private INode node;

        public NodeSnapshot(ITextSnapshot snapshot, INode node)
        {
            this.snapshotSpan = new SnapshotSpan(snapshot, node.Position, node.Length);
            this.node = node;
        }

        public SnapshotSpan SnapshotSpan { get { return snapshotSpan; } }

        public INode Node { get { return node; } }

        public string Type
        {
            get
            {
                switch (node.NodeType)
                {
                    case NodeType.Marker:
                        return Constants.MARKER_CLASSIFIER;
                    default:
                        return Constants.DJANGO_CONSTRUCT;
                }
            }
        }

        internal void TranslateTo(ITextSnapshot snapshot)
        {
            snapshotSpan = snapshotSpan.TranslateTo(snapshot, SpanTrackingMode.EdgeExclusive);
        }
    }
}
