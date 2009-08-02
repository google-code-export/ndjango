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

namespace NDjango.Designer.QuickInfo
{
    class Controller : IIntellisenseController
    {
        private IParserProviderBorker parser;
        private IList<ITextBuffer> subjectBuffers;
        private ITextView textView;
        private IQuickInfoBrokerMapService brokerMapService;
        private IQuickInfoSession activeSession;
        private Dictionary<ITextBuffer, NodeProvider> tokenizers = new Dictionary<ITextBuffer,NodeProvider>();

        public Controller(IParserProviderBorker parser, IList<ITextBuffer> subjectBuffers, ITextView textView, IQuickInfoBrokerMapService brokerMapService)
        {
            // TODO: Complete member initialization
            this.parser = parser;
            this.subjectBuffers = subjectBuffers;
            this.textView = textView;
            this.brokerMapService = brokerMapService;
            subjectBuffers.ToList().ForEach(buffer => tokenizers.Add(buffer, parser.GetNodeProvider(buffer)));

            textView.MouseHover += new EventHandler<MouseHoverEventArgs>(textView_MouseHover);

        }

        void textView_MouseHover(object sender, MouseHoverEventArgs e)
        {
            if (activeSession != null)
                activeSession.Dismiss();

            SnapshotPoint? point = e.TextPosition.GetPoint
                                    (textBuffer => (subjectBuffers.Contains(textBuffer) &&
                                         brokerMapService.GetBrokerForTextView(textView, textBuffer) != null),
                                                 PositionAffinity.Predecessor);
            if (point.HasValue)
            {
                INode quickInfoNode = tokenizers[point.Value.Snapshot.TextBuffer].GetQuickInfo(point.Value);
                if (quickInfoNode != null)
                {
                    // the invocation occurred in a subject buffer of interest to us
                    IQuickInfoBroker broker = brokerMapService.GetBrokerForTextView(textView, point.Value.Snapshot.TextBuffer);
                    ITrackingPoint triggerPoint = point.Value.Snapshot.CreateTrackingPoint(point.Value.Position, PointTrackingMode.Positive);

                    // Create a quickInfo session
                    activeSession = broker.CreateQuickInfoSession(triggerPoint, true);
                    activeSession.Properties.AddProperty(Provider.QuickInfoProviderSessionKey, quickInfoNode);
                    activeSession.Start();
                }
            }
        }

        public void ConnectSubjectBuffer(ITextBuffer subjectBuffer)
        {
//            throw new NotImplementedException();
        }

        public void Detach(ITextView textView)
        {
//            throw new NotImplementedException();
        }

        public void DisconnectSubjectBuffer(ITextBuffer subjectBuffer)
        {
//            throw new NotImplementedException();
        }
    }
}
