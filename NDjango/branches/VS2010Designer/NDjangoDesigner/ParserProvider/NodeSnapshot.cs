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
using Microsoft.VisualStudio.Shell.Interop;

namespace NDjango.Designer.Parsing
{
    /// <summary>
    /// Maps a ndjango syntax node to the corresponding snapshotspan in the snapshot
    /// </summary>
    class NodeSnapshot
    {
        private SnapshotSpan snapshotSpan;
        private SnapshotSpan extensionSpan;
        private INode node;
        private List<NodeSnapshot> children = new List<NodeSnapshot>();

        public NodeSnapshot(ITextSnapshot snapshot, INode node)
        {

            this.node = node; 
            this.snapshotSpan = new SnapshotSpan(snapshot, node.Position, node.Length);
            
            int offset = 0;
            if (node.Values.GetEnumerator().MoveNext())
            {
                ITextSnapshotLine line = snapshot.GetLineFromPosition(node.Position);

                // if the Value list is not empty, expand the snapshotSpan
                // to include leading whitespaces, so that when a user
                // types smth in this space he will get the dropdown
                for (; node.Position - offset > line.Extent.Start.Position; offset++)
                {
                    switch (snapshot[node.Position - offset-1])
                    {
                        case ' ':
                        case '\t':
                            continue;
                        default:
                            break;
                    }
                    break;
                }
            }
            extensionSpan = new SnapshotSpan(snapshot, node.Position - offset, offset);
            foreach (IEnumerable<INode> list in node.Nodes.Values)
                foreach (INode child in list)
                    children.Add(new NodeSnapshot(snapshot, child));
        }

        /// <summary>
        /// Span covering the source the INode was created from
        /// </summary>
        public SnapshotSpan SnapshotSpan { get { return snapshotSpan; } }

        /// <summary>
        /// The extension span for the INode - is empty unless the node has code completion values
        /// if not emoty covers all whitespace to the left of the node 
        /// </summary>
        public SnapshotSpan ExtensionSpan { get { return extensionSpan; } }

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
            extensionSpan = extensionSpan.TranslateTo(snapshot, SpanTrackingMode.EdgeExclusive);
            foreach (NodeSnapshot child in children)
                child.TranslateTo(snapshot);
        }

        /// <summary>
        /// Displays a diagnostic message in the error list window as well as in the output pane
        /// </summary>
        /// <param name="djangoDiagnostics"></param>
        /// <param name="filePath"></param>
        internal void ShowDiagnostics(IVsOutputWindowPane djangoDiagnostics, string filePath)
        {
            if (node.ErrorMessage.Severity > 0)
            {
                ITextSnapshotLine line = snapshotSpan.Snapshot.GetLineFromPosition(node.Position);
                djangoDiagnostics.OutputTaskItemString(
                    node.ErrorMessage.Message + "\n",
                    VSTASKPRIORITY.TP_HIGH,
                    VSTASKCATEGORY.CAT_BUILDCOMPILE,
                    "",
                    (int)_vstaskbitmap.BMP_COMPILE,
                    filePath,
                    (uint)line.LineNumber,
                    node.ErrorMessage.Message + "\n"
                    );
            }
            foreach (NodeSnapshot child in children)
                child.ShowDiagnostics(djangoDiagnostics, filePath);
        }
    }
}
