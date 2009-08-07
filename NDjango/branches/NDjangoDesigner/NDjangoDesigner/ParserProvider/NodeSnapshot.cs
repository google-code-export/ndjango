using Microsoft.VisualStudio.Text;
using NDjango.Interfaces;
using System.Collections.Generic;

namespace NDjango.Designer.Parsing
{
    class NodeSnapshot
    {
        private SnapshotSpan snapshotSpan;
        private INode node;
        private List<NodeSnapshot> children = new List<NodeSnapshot>();

        public NodeSnapshot(ITextSnapshot snapshot, INode node)
        {
            this.snapshotSpan = new SnapshotSpan(snapshot, node.Position, node.Length);
            this.node = node;
            foreach (IEnumerable<INode> list in node.Nodes.Values)
                foreach (INode child in list)
                    children.Add(new NodeSnapshot(snapshot, child));
        }

        public SnapshotSpan SnapshotSpan { get { return snapshotSpan; } }

        public IEnumerable<NodeSnapshot> Children { get { return children; } }

        public INode Node { get { return node; } }

        public string Type
        {
            get
            {
                switch (node.NodeType)
                {
                    case NodeType.Marker:
                        return Constants.MARKER_CLASSIFIER;
                    case NodeType.Text:
                        return "text";
                    default:
                        return Constants.DJANGO_CONSTRUCT;
                }
            }
        }

        internal void TranslateTo(ITextSnapshot snapshot)
        {
            snapshotSpan = snapshotSpan.TranslateTo(snapshot, SpanTrackingMode.EdgeExclusive);
            foreach (NodeSnapshot child in children)
                child.TranslateTo(snapshot);
        }
    }
}
