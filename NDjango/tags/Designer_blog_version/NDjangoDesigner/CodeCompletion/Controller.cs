/****************************************************************************
 * 
 *  NDjango Parser Copyright © 2009 Hill30 Inc
 *
 *  This file is part of the NDjango Designer.
 *
 *  The NDjango Parser is free software: you can redistribute it and/or modify
 *  it under the terms of the GNU Lesser General Public License as published by
 *  the Free Software Foundation, either version 3 of the License, or
 *  (at your option) any later version.
 *
 *  The NDjango Parser is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 *  GNU Lesser General Public License for more details.
 *
 *  You should have received a copy of the GNU Lesser General Public License
 *  along with NDjango Parser.  If not, see <http://www.gnu.org/licenses/>.
 *  
 ***************************************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.ApplicationModel.Environments;
using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using NDjango.Interfaces;

namespace NDjango.Designer.CodeCompletion
{
    /// <summary>
    /// Controls the flow of the codecompletion sessions for the given TextView
    /// </summary>
    class Controller : IIntellisenseController, IOleCommandTarget
    {
        private IList<ITextBuffer> subjectBuffers;
        private ITextView subjectTextView;
        private IWpfTextView WpfTextView;
        private ICompletionSession activeSession;
        private IEnvironment context;
        private ControllerProvider provider;

        /// <summary>
        /// Given a text view creates a new instance of the code completion controller and subscribes 
        /// to the text view keyboard events
        /// </summary>
        /// <param name="provider"></param>
        /// <param name="subjectBuffers"></param>
        /// <param name="subjectTextView"></param>
        /// <param name="context"></param>
        public Controller(ControllerProvider provider, IList<ITextBuffer> subjectBuffers, ITextView subjectTextView, IEnvironment context)
        {
            this.provider = provider;
            this.subjectBuffers = subjectBuffers;
            this.subjectTextView = subjectTextView;
            this.context = context;

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
        /// Pressing Enter key commits the session
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

            // we only start the session when an alphanumeric key is pressed
            if (!(e.Key >= Key.A && e.Key <= Key.Z))
                return;

            // if there is a session already leave it be
            if (activeSession != null)
                return;

            // determine which subject buffer is affected by looking at the caret position
            SnapshotPoint? caretPoint = textView.Caret.Position.Point.GetPoint
                (textBuffer => 
                    (
                        subjectBuffers.Contains(textBuffer) 
                        && provider.nodeProviderBroker.IsNDjango(textBuffer, context)
                        && provider.CompletionBrokerMapService.GetBrokerForTextView(textView, textBuffer) != null
                    ),
                    PositionAffinity.Predecessor);

            // return if no suitable buffer found
            if (!caretPoint.HasValue)
                return;

            List<INode> completionNodes = 
                provider.nodeProviderBroker.GetNodeProvider(caretPoint.Value.Snapshot.TextBuffer).GetNodes(caretPoint.Value)
                    .FindAll(node => node.Values.Count() > 0);

            // return if there is no information to show
            if (completionNodes.Count == 0)
                return;

            // attach filter to intercept the Enter key
            attachKeyboardFilter();

            // the invocation occurred in a subject buffer of interest to us
            ICompletionBroker broker = provider.CompletionBrokerMapService.GetBrokerForTextView(textView, caretPoint.Value.Snapshot.TextBuffer);
            ITrackingPoint triggerPoint = caretPoint.Value.Snapshot.CreateTrackingPoint(caretPoint.Value.Position, PointTrackingMode.Positive);

            // Create a completion session
            activeSession = broker.CreateCompletionSession(triggerPoint, true);

            // Put the list of completion nodes on the session so that it can be used by the completion source
            activeSession.Properties.AddProperty(typeof(Source), completionNodes);

            // Attach to the session events
            activeSession.Dismissed += new System.EventHandler(OnActiveSessionDismissed);
            activeSession.Committed += new System.EventHandler(OnActiveSessionCommitted);

            // Start the completion session. The intellisense will be triggered.
            activeSession.Start();
        }

        void OnActiveSessionDismissed(object sender, System.EventArgs e)
        {
            detachKeyboardFilter();
            activeSession = null;
        }

        void OnActiveSessionCommitted(object sender, System.EventArgs e)
        {
            detachKeyboardFilter();
            activeSession = null;
        }

        public void ConnectSubjectBuffer(ITextBuffer subjectBuffer)
        { }

        public void Detach(Microsoft.VisualStudio.Text.Editor.ITextView textView)
        {
            detachKeyboardFilter();
        }

        public void DisconnectSubjectBuffer(ITextBuffer subjectBuffer)
        {
            WpfTextView = subjectTextView as IWpfTextView;
            if (WpfTextView != null)
            {
                WpfTextView.VisualElement.KeyDown -= new System.Windows.Input.KeyEventHandler(VisualElement_KeyDown);
                WpfTextView.VisualElement.KeyUp -= new System.Windows.Input.KeyEventHandler(VisualElement_KeyUp);
                detachKeyboardFilter();
            }
        }

        private void attachKeyboardFilter()
        {
            ErrorHandler.ThrowOnFailure(provider.adaptersFactory.GetViewAdapter(subjectTextView).AddCommandFilter(this, out oldFilter));
        }

        private void detachKeyboardFilter()
        {
            ErrorHandler.ThrowOnFailure(provider.adaptersFactory.GetViewAdapter(subjectTextView).RemoveCommandFilter(this));
        }

        // The ugly COM code below is the heavy heritage from the "old style" editor integration
        // the reason we need it is that when the user presses Enter, this keypress in addition to being 
        // sent to our KeyUp event is also sent to the editor window itself. As a result, the window gets 
        // updated which causes the update events to fire which in turn causes the current selection
        // in the CompletionSet to be reset according to the matching rules. In other words, if the user 
        // selects something from the completion dropdown using arrows and presses enter, before the 
        // selection is applied to the code it is reset to whatever is considered to be a match to the 
        // letters keyed in before that.
        // The code below intercepts the ECMD_RETURN command before it is sent to the editor window. 
        private IOleCommandTarget oldFilter;

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
