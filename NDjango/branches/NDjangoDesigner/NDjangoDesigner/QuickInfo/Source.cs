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
        private Microsoft.VisualStudio.Text.ITextBuffer textBuffer;

        public Source(Microsoft.VisualStudio.Text.ITextBuffer textBuffer)
        {
            // TODO: Complete member initialization
            this.textBuffer = textBuffer;
        }

        public object GetToolTipContent(IQuickInfoSession session, out Microsoft.VisualStudio.Text.ITrackingSpan applicableToSpan)
        {
            applicableToSpan = session.SubjectBuffer.CurrentSnapshot.CreateTrackingSpan(0, 10, Microsoft.VisualStudio.Text.SpanTrackingMode.EdgeExclusive);

            object node;
            if (session.Properties.TryGetProperty<object>(SourceProvider.QuickInfoProviderSessionKey, out node))
            {
                List<INode> quickInfoNodes = (List<INode>)session.Properties.GetProperty(SourceProvider.QuickInfoProviderSessionKey);
                //quickInfoNodes.ConvertAll(infoNode => infoNode.Info);
                //TODO: define what exactly node's info to be displayed
                return GetToolTipMessage(quickInfoNodes);
            }

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
