using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.Text;
using System.Threading;
using NDjango.Interfaces;
using System.IO;

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

        class SnapshotReader : TextReader
        {
            ITextSnapshot snapshot;
            string line;
            int pos = 0;
            public SnapshotReader(ITextSnapshot snapshot)
            {
                this.snapshot = snapshot;
            }

            public override int Read(char[] buffer, int index, int count)
            {
                int actual = snapshot.Length - pos;
                if (actual > count)
                    actual = count;
                if (actual > 0)
                    snapshot.ToCharArray(pos, actual).CopyTo(buffer, index);
                pos += actual;
                return actual;
            }
        }

        /// <summary>
        /// Retrieves sequence of tokens out of snapshot object. 
        /// </summary>
        private void rebuildNodesAsynch(object snapshotObject)
        {
            ITextSnapshot snapshot = (ITextSnapshot)snapshotObject;
            List<NodeSnapshot> nodes = parser.ParseTemplate(new SnapshotReader(snapshot))
                .ToList()
                    .ConvertAll<NodeSnapshot>
                        (token => new NodeSnapshot(snapshot, (INode)token));
            lock (node_lock)
            {
                this.nodes = nodes;
            }
            if (NodesChanged != null)
                NodesChanged(new SnapshotSpan(snapshot, 0, snapshot.Length));
        }

        /// <summary>
        /// Returns a list of nodes in the specified snapshot span
        /// </summary>
        /// <param name="snapshotSpan"></param>
        /// <returns></returns>
        internal List<NodeSnapshot> GetNodes(SnapshotSpan snapshotSpan)
        {
            List<NodeSnapshot> nodes;
            lock (node_lock)
            {
                nodes = this.nodes;

                // just in case if while the tokens list was being rebuilt
                // another modification was made
                if (nodes.Count > 0 && this.nodes[0].SnapshotSpan.Snapshot != snapshotSpan.Snapshot)
                    this.nodes.ForEach(token => token.TranslateTo(snapshotSpan.Snapshot));
            }

            return GetNodes(snapshotSpan, nodes);
        }

        private List<NodeSnapshot> GetNodes(SnapshotSpan snapshotSpan, IEnumerable<NodeSnapshot> nodes)
        {
            List<NodeSnapshot> result = new List<NodeSnapshot>();
            foreach (NodeSnapshot node in nodes)
            {
                if (node.SnapshotSpan.IntersectsWith(snapshotSpan))
                    result.Add(node);
                result.AddRange(GetNodes(snapshotSpan, node.Children));
            }
            return result;
        }
        
        /// <summary>
        /// Returns a list of django syntax nodes based on the point in the text buffer
        /// </summary>
        /// <param name="point">point identifiying the desired node</param>
        /// <returns></returns>
        internal List<INode> GetNodes(SnapshotPoint point)
        {
            List<NodeSnapshot> result = GetNodes(new SnapshotSpan(point.Snapshot, point.Position, 0))
                            .FindAll(node => node.SnapshotSpan.IntersectsWith(new SnapshotSpan(point.Snapshot, point.Position, 0)));
            if (result == null)
                return null;
            return result.ConvertAll(node => node.Node);
        }
    }
}
