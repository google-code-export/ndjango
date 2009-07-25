using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.Language.Intellisense;

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
            return "ToolTip";
        }
    }
}
