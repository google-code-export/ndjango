using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.Text;
using System.Threading;
using NDjango.Interfaces;

namespace NDjango.Designer.Parsing
{
    /// <summary>
    /// Provides ability to retrive tokens out of snapshot objects.
    /// </summary>
    class NodeProvider
    {
        // it can take some time for the parser to build the token list.
        // for now let us initialize it to an empty list
        private List<NodeSnapshot> nodes = new List<NodeSnapshot>();
        
        private object node_lock = new object();
        private IParser parser;
        private ITextBuffer buffer;

        public NodeProvider(IParser parser, ITextBuffer buffer)
        {
            this.parser = parser;
            this.buffer = buffer;
            rebuildNodes(buffer.CurrentSnapshot);
            buffer.Changed += new EventHandler<TextContentChangedEventArgs>(buffer_Changed);
        }

        public delegate void SnapshotEvent (SnapshotSpan snapshotSpan);

        void buffer_Changed(object sender, TextContentChangedEventArgs e)
        {
            rebuildNodes(e.After);
        }

        private void rebuildNodes(ITextSnapshot snapshot)
        {
            ThreadPool.QueueUserWorkItem(rebuildNodesAsynch, snapshot);
        }

        public event SnapshotEvent NodesChanged;

        /// <summary>
        /// Retrieves sequence of tokens out of snapshot object. 
        /// </summary>
        private void rebuildNodesAsynch(object snapshotObject)
        {
            ITextSnapshot snapshot = (ITextSnapshot)snapshotObject;
            List<NodeSnapshot> nodes = parser.Parse(snapshot.Lines.ToList().ConvertAll(line => line.GetTextIncludingLineBreak()))
                .ToList()
                    .ConvertAll<NodeSnapshot>
                        (token => new NodeSnapshot(snapshot, token));
            lock (node_lock)
            {
                this.nodes = nodes;
            }
            if (NodesChanged != null)
                NodesChanged(new SnapshotSpan(snapshot, 0, snapshot.Length));
        }

        internal List<NodeSnapshot> GetNodes(SnapshotSpan snapshotSpan)
        {
            List<NodeSnapshot> nodes;
            lock (node_lock)
            {
                nodes = this.nodes;
            }
            if (nodes.Count == 0)
                return nodes;
            
            // just in case if while the tokens list was being rebuilt
            // another modification was made
            if (this.nodes[0].SnapshotSpan.Snapshot != snapshotSpan.Snapshot)
                this.nodes.ForEach(token => token.TranslateTo(snapshotSpan.Snapshot));

            return nodes;
        }
        
        /// <summary>
        /// Returns the django syntax node based on the point in the text buffer
        /// </summary>
        /// <param name="point">point identifiying the desired node</param>
        /// <returns></returns>
        internal INode GetNode(SnapshotPoint point)
        {
            NodeSnapshot result = GetNodes(new SnapshotSpan(point.Snapshot, point.Position, 0))
                            .FirstOrDefault(token => token.SnapshotSpan.IntersectsWith(new SnapshotSpan(point.Snapshot, point.Position, 0)));
            if (result == null)
                return null;
            return result.Node;
        }
    }
}
