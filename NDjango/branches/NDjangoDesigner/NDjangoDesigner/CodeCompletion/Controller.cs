using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Language.Intellisense;
using NDjango.Designer.Parsing;
using NDjango.Interfaces;
using System;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio.Editor;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell.Interop;

namespace NDjango.Designer.Intellisense
{
    class Controller : IIntellisenseController, IOleCommandTarget
    {
        private IList<ITextBuffer> subjectBuffers;
        private ITextView subjectTextView;
        private IWpfTextView WpfTextView;
        private ICompletionBrokerMapService completionBrokerMap;
        private INodeProviderBroker nodeProviderBroker;
        private ICompletionSession activeSession;
        private IVsEditorAdaptersFactoryService adaptersFactory;

        public Controller(INodeProviderBroker nodeProviderBroker, IList<ITextBuffer> subjectBuffers, 
            ITextView subjectTextView, ICompletionBrokerMapService completionBrokerMap,
            IVsEditorAdaptersFactoryService adaptersFactory)
        {
            this.nodeProviderBroker = nodeProviderBroker;
            this.subjectBuffers = subjectBuffers;
            this.subjectTextView = subjectTextView;
            this.completionBrokerMap = completionBrokerMap;
            this.adaptersFactory = adaptersFactory;

            WpfTextView = subjectTextView as IWpfTextView;
            if (WpfTextView != null)
            {
                WpfTextView.VisualElement.KeyDown += new System.Windows.Input.KeyEventHandler(VisualElement_KeyDown);
                WpfTextView.VisualElement.KeyUp += new System.Windows.Input.KeyEventHandler(VisualElement_KeyUp);
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
                    if (this.activeSession.SelectedCompletionSet.SelectionStatus != null )
                    {
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

            if (activeSession == null)
            {

                // determine which subject buffer is affected by looking at the caret position
                SnapshotPoint? caretPoint = textView.Caret.Position.Point.GetPoint
                    (textBuffer => 
                        (
                            subjectBuffers.Contains(textBuffer) 
                            && nodeProviderBroker.IsNDjango(textBuffer)
                            && completionBrokerMap.GetBrokerForTextView(textView, textBuffer) != null
                        ),
                        PositionAffinity.Predecessor);

                if (caretPoint.HasValue)
                {
                    List<INode> completionNodes = 
                        nodeProviderBroker.GetNodeProvider(caretPoint.Value.Snapshot.TextBuffer).GetNodes(caretPoint.Value)
                            .FindAll(node => node.Values.Count() > 0);
                    if (completionNodes.Count > 0)
                    {
                        string prefix = caretPoint.Value.Snapshot.GetText(0, caretPoint.Value.Position);
                        for (int i = prefix.Length - 1; i >= 0; i--)
                            if (prefix[i] == ' ' || prefix[i] == '\t')
                                continue;
                            else
                                if (i > 0 || prefix[i] == '%' || prefix[i - 1] == '{')
                                    break;
                                else
                                    return;

                        ErrorHandler.ThrowOnFailure(adaptersFactory.GetViewAdapter(subjectTextView).AddCommandFilter(this, out oldFilter));

                        // the invocation occurred in a subject buffer of interest to us
                        ICompletionBroker broker = completionBrokerMap.GetBrokerForTextView(textView, caretPoint.Value.Snapshot.TextBuffer);
                        ITrackingPoint triggerPoint = caretPoint.Value.Snapshot.CreateTrackingPoint(caretPoint.Value.Position, PointTrackingMode.Positive);

                        // Create a completion session
                        activeSession = broker.CreateCompletionSession(triggerPoint, true);

                        // Set the completion provider that will be used by the completion source
                        activeSession.Properties.AddProperty(CompletionProvider.CompletionProviderSessionKey, 
                            new CompletionProvider(completionNodes));
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
            ErrorHandler.ThrowOnFailure(adaptersFactory.GetViewAdapter(subjectTextView).RemoveCommandFilter(this));
            activeSession = null;
        }

        void OnActiveSessionCommitted(object sender, System.EventArgs e)
        {
            ErrorHandler.ThrowOnFailure(adaptersFactory.GetViewAdapter(subjectTextView).RemoveCommandFilter(this));
        }

        public void ConnectSubjectBuffer(ITextBuffer subjectBuffer)
        { }

        public void Detach(Microsoft.VisualStudio.Text.Editor.ITextView textView)
        {
            ErrorHandler.ThrowOnFailure(adaptersFactory.GetViewAdapter(subjectTextView).RemoveCommandFilter(this));
        }

        public void DisconnectSubjectBuffer(ITextBuffer subjectBuffer)
        {
            WpfTextView = subjectTextView as IWpfTextView;
            if (WpfTextView != null)
            {
                WpfTextView.VisualElement.KeyDown -= new System.Windows.Input.KeyEventHandler(VisualElement_KeyDown);
                WpfTextView.VisualElement.KeyUp -= new System.Windows.Input.KeyEventHandler(VisualElement_KeyUp);
                ErrorHandler.ThrowOnFailure(adaptersFactory.GetViewAdapter(subjectTextView).RemoveCommandFilter(this));
            }
        }

        public IOleCommandTarget oldFilter;

        private static readonly Guid CMDSETID_StandardCommandSet2k = new Guid("1496a755-94de-11d0-8c3f-00c04fc2aae2");
        private static readonly uint ECMD_RETURN = 3;

        public int Exec(ref Guid pguidCmdGroup, uint nCmdID, uint nCmdexecopt, IntPtr pvaIn, IntPtr pvaOut)
        {
            if (pguidCmdGroup == CMDSETID_StandardCommandSet2k && nCmdID == ECMD_RETURN)
                return VSConstants.S_OK;
            return oldFilter.Exec(pguidCmdGroup, nCmdID, nCmdexecopt, pvaIn, pvaOut);
        }

        public int QueryStatus(ref Guid pguidCmdGroup, uint cCmds, OLECMD[] prgCmds, IntPtr pCmdText)
        {
            return oldFilter.QueryStatus(pguidCmdGroup, cCmds, prgCmds, pCmdText);
        }
    }
}
