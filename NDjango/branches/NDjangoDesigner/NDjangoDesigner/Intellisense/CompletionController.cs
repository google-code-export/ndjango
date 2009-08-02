using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Language.Intellisense;
using NDjango.Designer.Parsing;

namespace NDjango.Designer.Intellisense
{
    class CompletionController : IIntellisenseController
    {
        private IList<ITextBuffer> subjectBuffers;
        private ITextView subjectTextView;
        private IWpfTextView WpfTextView;
        private ICompletionBrokerMapService completionBrokerMap;
        private ICompletionSession activeSession;
        private Completion selectedCompletionBeforeCommit;
        private Dictionary<ITextBuffer, NodeProvider> tokenizers = new Dictionary<ITextBuffer,NodeProvider>();

        public CompletionController(IParserProviderBorker parser, IList<ITextBuffer> subjectBuffers, ITextView subjectTextView, ICompletionBrokerMapService completionBrokerMap)
        {
            this.subjectBuffers = subjectBuffers;
            this.subjectTextView = subjectTextView;
            this.completionBrokerMap = completionBrokerMap;
            subjectBuffers.ToList().ForEach(buffer => tokenizers.Add(buffer, parser.GetNodeProvider(buffer)));

            WpfTextView = subjectTextView as IWpfTextView;
            if (WpfTextView != null)
            {
                KeyProcessor.KeyDownEvent += new System.Windows.Input.KeyEventHandler(VisualElement_KeyDown);
                KeyProcessor.KeyUpEvent += new System.Windows.Input.KeyEventHandler(VisualElement_KeyUp);
                //WpfTextView.VisualElement.KeyDown += new System.Windows.Input.KeyEventHandler(VisualElement_KeyDown);
                //WpfTextView.VisualElement.KeyUp += new System.Windows.Input.KeyEventHandler(VisualElement_KeyUp);
            }
        }

        /// <summary>
        /// Handles the key up event.
        /// The intellisense window is dismissed when one presses ESC key
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void VisualElement_KeyUp(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (activeSession != null)
            {
                if (e.Key == Key.Escape)
                {
                    activeSession.Dismiss();
                    e.Handled = true;
                }

                if (e.Key == Key.Enter)
                {
                    if (this.activeSession.SelectedCompletionSet.SelectionStatus != null /*&& this.activeSession.SelectedCompletionSet.SelectionStatus.IsSelected*/ )
                    {
                        selectedCompletionBeforeCommit = this.activeSession.SelectedCompletionSet.SelectionStatus.Completion as Completion;
                        activeSession.Commit();
                    }
                    else
                    {
                        activeSession.Dismiss();
                    }
                    e.Handled = true;
                }

                if (e.Key == Key.Tab)
                {
                    if (this.activeSession.SelectedCompletionSet.SelectionStatus != null)
                    {
                        selectedCompletionBeforeCommit = this.activeSession.SelectedCompletionSet.SelectionStatus.Completion as Completion;
                        activeSession.Commit();
                    }
                    else
                    {
                        activeSession.Dismiss();
                    }
                    e.Handled = true;
                }
            }
        }

        /// <summary>
        /// Triggers Statement completion when appropriate keys are pressed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void VisualElement_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            // Make sure that this event happened on the same text view to which we're attached.
            ITextView textView = sender as ITextView;
            if (this.subjectTextView != textView)
            {
                return;
            }

            if (!(e.Key >= Key.A && e.Key <= Key.Z))
                return;

            if (activeSession != null)
            {
//                activeSession.SelectedCompletionSet.Filter(CompletionMatchType.MatchDisplayText, true);
//                activeSession.SelectedCompletionSet.SelectBestMatch();
            }
            else
            {

                // determine which subject buffer is affected by looking at the caret position
                SnapshotPoint? caretPoint = textView.Caret.Position.Point.GetPoint
                                                (textBuffer => (subjectBuffers.Contains(textBuffer) &&
                                                                completionBrokerMap.GetBrokerForTextView(textView, textBuffer) != null),
                                                 PositionAffinity.Predecessor);

                if (caretPoint.HasValue)
                {
                    List<string> completions = tokenizers[caretPoint.Value.Snapshot.TextBuffer]
                        .GetCompletions(caretPoint.Value);
                    if (completions.Count > 0)
                    {
                        // the invocation occurred in a subject buffer of interest to us
                        ICompletionBroker broker = completionBrokerMap.GetBrokerForTextView(textView, caretPoint.Value.Snapshot.TextBuffer);
                        ITrackingPoint triggerPoint = caretPoint.Value.Snapshot.CreateTrackingPoint(caretPoint.Value.Position, PointTrackingMode.Positive);

                        // Create a completion session
                        activeSession = broker.CreateCompletionSession(triggerPoint, false);

                        // Set the completion provider that will be used by the completion source
                        activeSession.Properties.AddProperty(CompletionProvider.CompletionProviderSessionKey, new CompletionProvider(completions));
                        // Attach to the session events
                        activeSession.Dismissed += new System.EventHandler(OnActiveSessionDismissed);
                        activeSession.Committed += new System.EventHandler(OnActiveSessionCommitted);

                        // Start the completion session. The intellisense will be triggered.
                        activeSession.Start();
                    }
                }
            }
        }

        void OnActiveSessionDismissed(object sender, System.EventArgs e)
        {
            activeSession = null;
        }

        void OnActiveSessionCommitted(object sender, System.EventArgs e)
        {
            //if (selectedCompletionBeforeCommit != null)
            //{
            //    activeSession.TextView.Caret.MoveTo(new VirtualSnapshotPoint(activeSession.TextView.Caret.Position.BufferPosition));
            //}
        }

        private CompletionSet GetCompletionSet()
        {
            return activeSession.CompletionSets.FirstOrDefault(set => set.DisplayName == CompletionSource.CompletionSetName);
        }

        public void ConnectSubjectBuffer(ITextBuffer subjectBuffer)
        { }

        public void Detach(Microsoft.VisualStudio.Text.Editor.ITextView textView)
        { }

        public void DisconnectSubjectBuffer(ITextBuffer subjectBuffer)
        {
            WpfTextView = subjectTextView as IWpfTextView;
            if (WpfTextView != null)
            {
                KeyProcessor.KeyDownEvent -= new System.Windows.Input.KeyEventHandler(VisualElement_KeyDown);
                KeyProcessor.KeyUpEvent -= new System.Windows.Input.KeyEventHandler(VisualElement_KeyUp);
                //WpfTextView.VisualElement.KeyDown -= new System.Windows.Input.KeyEventHandler(VisualElement_KeyDown);
                //WpfTextView.VisualElement.KeyUp -= new System.Windows.Input.KeyEventHandler(VisualElement_KeyUp);
            }
        }
    }
}
