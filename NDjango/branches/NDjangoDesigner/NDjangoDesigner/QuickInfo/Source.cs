using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.Language.Intellisense;
using NDjango.Interfaces;

namespace NDjango.Designer.QuickInfo
{
    class Source : IQuickInfoSource
    {

        public object GetToolTipContent(IQuickInfoSession session, out Microsoft.VisualStudio.Text.ITrackingSpan applicableToSpan)
        {
            StringBuilder message = new StringBuilder();
            int position = session.SubjectBuffer.CurrentSnapshot.Length;
            int length = 0;
            List<INode> nodes;
            if (session.Properties.TryGetProperty<List<INode>>(SourceProvider.QuickInfoProviderSessionKey, out nodes))
            {
                nodes.ForEach(
                    node => 
                    {
                        if (!String.IsNullOrEmpty(node.Description))
                            message.Insert(0, node.Description + "\n");
                        if (node.ErrorMessage.Severity >= 0)
                            message.Append("\n" + node.ErrorMessage.Message);
                        if (node.Length > length)
                            length = node.Length;
                        if (node.Position < position)
                            position = node.Position;
                    }
                        );
            }

            applicableToSpan = session.SubjectBuffer.CurrentSnapshot.CreateTrackingSpan(
                position,
                length,
                Microsoft.VisualStudio.Text.SpanTrackingMode.EdgeExclusive);

            if (message.Length > 0)
                return message.ToString();
            else
                return null;
        }

        private string GetToolTipMessage(List<INode> nodes)
        {
            StringBuilder result = new StringBuilder();
            foreach (INode node in nodes)
            {
                result.Append(node.ErrorMessage.Message);
                result.AppendLine();
            }

            if (nodes.Exists(someNode => someNode.NodeType == NodeType.Tag))
                result.Append(nodes.Find(someNode => someNode.NodeType == NodeType.Tag).Description);
                
            return result.ToString();
        }
    }
}
