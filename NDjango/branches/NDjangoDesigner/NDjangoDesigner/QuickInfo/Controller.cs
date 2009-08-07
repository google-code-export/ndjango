using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.Text.Editor;
using NDjango.Designer.Parsing;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Language.Intellisense;
using System.Windows.Input;
using NDjango.Interfaces;
using Microsoft.VisualStudio.Text.Projection;

namespace NDjango.Designer.QuickInfo
{
    class Controller : IIntellisenseController
    {
        private IList<ITextBuffer> subjectBuffers;
        private ITextView textView;
        private IQuickInfoBrokerMapService brokerMapService;
        private IQuickInfoSession activeSession;
        private INodeProviderBroker nodeProviderBroker;

        public Controller(INodeProviderBroker nodeProviderBroker, IList<ITextBuffer> subjectBuffers, ITextView textView, IQuickInfoBrokerMapService brokerMapService)
        {
            this.nodeProviderBroker = nodeProviderBroker;
            this.subjectBuffers = subjectBuffers;
            this.textView = textView;
            this.brokerMapService = brokerMapService;

            textView.MouseHover += new EventHandler<MouseHoverEventArgs>(textView_MouseHover);

        }

        void textView_MouseHover(object sender, MouseHoverEventArgs e)
        {
            if (activeSession != null)
                activeSession.Dismiss();

            SnapshotPoint? point = e.TextPosition.GetPoint(
                textBuffer => 
                    (
                        subjectBuffers.Contains(textBuffer)
                        && nodeProviderBroker.IsNDjango(textBuffer)
                        && brokerMapService.GetBrokerForTextView(textView, textBuffer) != null
                        && !(textBuffer is IProjectionBuffer)
                    )
                ,PositionAffinity.Predecessor);
            
            
            if (point.HasValue)
            {
                NodeProvider nodeProvider = nodeProviderBroker.GetNodeProvider(point.Value.Snapshot.TextBuffer);
                List<INode> quickInfoNodes = nodeProvider.GetNodes(point.Value);
                if (quickInfoNodes != null)
                {
                    // the invocation occurred in a subject buffer of interest to us
                    IQuickInfoBroker broker = brokerMapService.GetBrokerForTextView(textView, point.Value.Snapshot.TextBuffer);
                    ITrackingPoint triggerPoint = point.Value.Snapshot.CreateTrackingPoint(point.Value.Position, PointTrackingMode.Positive);

                    activeSession = broker.CreateQuickInfoSession(triggerPoint, true);
                    activeSession.Properties.AddProperty(SourceProvider.QuickInfoProviderSessionKey, quickInfoNodes);
                    activeSession.Start();
                }
            }
        }

        public void ConnectSubjectBuffer(ITextBuffer subjectBuffer)
        { }

        public void Detach(ITextView textView)
        { }

        public void DisconnectSubjectBuffer(ITextBuffer subjectBuffer)
        { }
    }
}
