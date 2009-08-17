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

using Microsoft.VisualStudio.Text;
using NDjango.Interfaces;
using System.Collections.Generic;

namespace NDjango.Designer.Parsing
{
    /// <summary>
    /// Maps a ndjango syntax node to the corresponding snapshotspan in the snapshot
    /// </summary>
    class NodeSnapshot
    {
        private SnapshotSpan snapshotSpan;
        private INode node;
        private List<NodeSnapshot> children = new List<NodeSnapshot>();

        public NodeSnapshot(ITextSnapshot snapshot, INode node)
        {
            int offset = 0;
            if (node.Values.GetEnumerator().MoveNext())
            {
                // if Value list is not empty, expand the snapshotSPan
                // to include leading whitespaces
                  ITextSnapshotLine line = snapshot.GetLineFromPosition(node.Position);
                for (; node.Position - offset > line.Extent.Start.Position; offset++)
                    if (
                        snapshot[node.Position - offset-1] != ' ' &&
                        snapshot[node.Position - offset-1] != '\t')
                        break;
            }

            this.snapshotSpan = new SnapshotSpan(snapshot, node.Position - offset, node.Length + offset);
            this.node = node;
            foreach (IEnumerable<INode> list in node.Nodes.Values)
                foreach (INode child in list)
                    children.Add(new NodeSnapshot(snapshot, child));
        }

        public SnapshotSpan SnapshotSpan { get { return snapshotSpan; } }

        public IEnumerable<NodeSnapshot> Children { get { return children; } }

        public INode Node { get { return node; } }

        /// <summary>
        /// Returns the classification type for the node
        /// </summary>
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

        /// <summary>
        /// Translates the NodeSnapshot to a newer snapshot
        /// </summary>
        /// <param name="snapshot"></param>
        internal void TranslateTo(ITextSnapshot snapshot)
        {
            snapshotSpan = snapshotSpan.TranslateTo(snapshot, SpanTrackingMode.EdgeExclusive);
            foreach (NodeSnapshot child in children)
                child.TranslateTo(snapshot);
        }
    }
}
