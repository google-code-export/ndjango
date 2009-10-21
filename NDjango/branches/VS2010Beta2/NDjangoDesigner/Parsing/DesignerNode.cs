﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NDjango.Interfaces;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Shell.Interop;

namespace NDjango.Designer.Parsing
{
    /// <summary>
    /// A proxy implementation of a django syntax tree node. 
    /// </summary>
    /// <remarks>Implementation of all properties/methods 
    /// for the INode interface are redirected to underlying 'real' syntax tree node generated by the 
    /// parser. Implements additional methods/properties necessary for designer
    /// </remarks>
    class DesignerNode : INode
    {
        private INode node;
        private SnapshotSpan snapshotSpan;
        private SnapshotSpan extensionSpan;
        private List<DesignerNode> children = new List<DesignerNode>();

        /// <summary>
        /// Creates the designer (proxy) node over the real syntax node passed in as a parameter
        /// Also recursively creates child nodes for all 'real' node children
        /// </summary>
        /// <param name="parent"></param>
        /// <param name="snapshot"></param>
        /// <param name="node"></param>
        public DesignerNode(DesignerNode parent, ITextSnapshot snapshot, INode node)
        {
            Parent = parent;
            this.node = node;
            if (node.NodeType == NodeType.ParsingContext)
            {
                snapshotSpan = new SnapshotSpan(snapshot, node.Position + node.Length, 0);
                extensionSpan = new SnapshotSpan(snapshot, node.Position, node.Length);
            }
            else
            {
                snapshotSpan = new SnapshotSpan(snapshot, node.Position, node.Length);
                int offset = 0;
                if (node.Values.GetEnumerator().MoveNext())
                {
                    ITextSnapshotLine line = snapshot.GetLineFromPosition(node.Position);

                    // if the Value list is not empty, expand the snapshotSpan
                    // to include leading whitespaces, so that when a user
                    // types smth in this space he will get the dropdown
                    for (; node.Position - offset > line.Extent.Start.Position; offset++)
                    {
                        switch (snapshot[node.Position - offset - 1])
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
            }
            foreach (IEnumerable<INode> list in node.Nodes.Values)
                foreach (INode child in list)
                    children.Add(new DesignerNode(this, snapshot, child));
        }

        /// <summary>
        /// Parent node of the current node. For the topmoste nodes returns null
        /// </summary>
        public DesignerNode Parent { get; private set; }

        /// <summary>
        /// Span covering the source the INode was created from
        /// </summary>
        public SnapshotSpan SnapshotSpan { get { return snapshotSpan; } }

        /// <summary>
        /// The extension span for the INode - is empty unless the node has code completion values
        /// if not emoty covers all whitespace to the left of the node 
        /// </summary>
        public SnapshotSpan ExtensionSpan { get { return extensionSpan; } }

        /// <summary>
        /// Translates the NodeSnapshot to a newer snapshot
        /// </summary>
        /// <param name="snapshot"></param>
        public void TranslateTo(ITextSnapshot snapshot)
        {
            snapshotSpan = snapshotSpan.TranslateTo(snapshot, SpanTrackingMode.EdgeExclusive);
            extensionSpan = extensionSpan.TranslateTo(snapshot, SpanTrackingMode.EdgeExclusive);
            foreach (DesignerNode child in children)
                child.TranslateTo(snapshot);
        }

        /// <summary>
        /// Displays a diagnostic message in the error list window as well as in the output pane
        /// </summary>
        /// <param name="djangoDiagnostics"></param>
        /// <param name="filePath"></param>
        public void ShowDiagnostics(IVsOutputWindowPane djangoDiagnostics, string filePath)
        {
            if (node.ErrorMessage.Severity > 0)
            {
                ITextSnapshotLine line = snapshotSpan.Snapshot.GetLineFromPosition(node.Position);
                djangoDiagnostics.OutputTaskItemString(
                    node.ErrorMessage.Message + "\n",
                    VSTASKPRIORITY.TP_HIGH,
                    VSTASKCATEGORY.CAT_BUILDCOMPILE,
                    "something",
                    (int)_vstaskbitmap.BMP_COMPILE,
                    filePath,
                    (uint)line.LineNumber,
                    node.ErrorMessage.Message + "\n"
                    );
            }
            foreach (DesignerNode child in children)
                child.ShowDiagnostics(djangoDiagnostics, filePath);
        }

        /// <summary>
        /// A list of child nodes
        /// </summary>
        public List<DesignerNode> Children { get { return children; } }

        /// <summary>
        /// Parsing context for current node
        /// </summary>
        public ParsingContext ParsingContext 
        {
            get 
            {
                if (node is NDjango.ParserNodes.ParsingContextNode)
                    return (node as NDjango.ParserNodes.ParsingContextNode).Context;
                if (node is NDjango.ParserNodes.TagNameNode)
                    return (node as NDjango.ParserNodes.TagNameNode).Context;
                throw new Exception("Context - not implemented");
            }
        }

        #region INode Members

        public string Description
        {
            get { return node.Description; }
        }

        public Error ErrorMessage
        {
            get { return node.ErrorMessage; }
        }

        public int Length
        {
            get { return node.Length; }
        }

        public NodeType NodeType
        {
            get { return node.NodeType; }
        }

        public IDictionary<string, IEnumerable<INode>> Nodes
        {
            get { return node.Nodes; }
        }

        public int Position
        {
            get { return node.Position; }
        }

        public IEnumerable<string> Values
        {
            get { return node.Values; }
        }

        #endregion
    }
}
