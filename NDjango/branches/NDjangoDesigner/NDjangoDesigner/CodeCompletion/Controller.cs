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
    class Controller : IIntellisenseController
    {
        private IList<ITextBuffer> subjectBuffers;
        private ITextView subjectTextView;
        private IWpfTextView WpfTextView;
        private ICompletionBrokerMapService completionBrokerMap;
        private ICompletionSession activeSession;
        private Completion selectedCompletionBeforeCommit;
        private Dictionary<ITextBuffer, NodeProvider> nodeProviders = new Dictionary<ITextBuffer,NodeProvider>();
        private IVsEditorAdaptersFactoryService adaptersFactory;

        public Controller(INodeProviderBroker nodeProviderBroker, IList<ITextBuffer> subjectBuffers, 
            ITextView subjectTextView, ICompletionBrokerMapService completionBrokerMap,
            IVsEditorAdaptersFactoryService adaptersFactory)
        {
            this.subjectBuffers = subjectBuffers;
            this.subjectTextView = subjectTextView;
            this.completionBrokerMap = completionBrokerMap;
            this.adaptersFactory = adaptersFactory;
            subjectBuffers.ToList().ForEach(buffer => nodeProviders.Add(buffer, nodeProviderBroker.GetNodeProvider(buffer)));

            WpfTextView = subjectTextView as IWpfTextView;
            if (WpfTextView != null)
            {
                GetShellCommandDispatcher(subjectTextView);
                WpfTextView.VisualElement.KeyDown += new System.Windows.Input.KeyEventHandler(VisualElement_KeyDown);
                WpfTextView.VisualElement.KeyUp += new System.Windows.Input.KeyEventHandler(VisualElement_KeyUp);
            }
        }
        
        /// <summary>        
        /// Get the SUIHostCommandDispatcher from the shell.  This method is rather ugly, and will (hopefully) be cleaned up        
        /// slightly whenever [Import]ing an IServiceProvider is available.        
        /// </summary>        
        /// <param name="view"></param>        
        /// <returns></returns>        
        IOleCommandTarget GetShellCommandDispatcher(ITextView view)        
        {            
            IOleCommandTarget shellCommandDispatcher;             
            var vsBuffer = adaptersFactory.GetBufferAdapter(view.TextBuffer);            
            if (vsBuffer == null)                
                return null;             
            Guid guidServiceProvider = VSConstants.IID_IUnknown;            
            IObjectWithSite objectWithSite = vsBuffer as IObjectWithSite;            
            IntPtr ptrServiceProvider = IntPtr.Zero;            
            objectWithSite.GetSite(ref guidServiceProvider, out ptrServiceProvider);             
            Microsoft.VisualStudio.OLE.Interop.IServiceProvider serviceProvider = 
                (Microsoft.VisualStudio.OLE.Interop.IServiceProvider)Marshal.GetObjectForIUnknown(ptrServiceProvider);
            Guid guidService = typeof(SUIHostCommandDispatcher).GUID;            
            Guid guidInterface = typeof(IOleCommandTarget).GUID;            
            IntPtr ptrObject = IntPtr.Zero;             
            int hr = serviceProvider.QueryService(ref guidService, ref guidInterface, out ptrObject);
            if (ErrorHandler.Failed(hr) || ptrObject == IntPtr.Zero)
                return null;
            shellCommandDispatcher = (IOleCommandTarget)Marshal.GetObjectForIUnknown(ptrObject);
            Marshal.Release(ptrObject);
            var vsTextView = adaptersFactory.GetViewAdapter(view);
            return shellCommandDispatcher;        
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

            if (activeSession == null)
            {

                // determine which subject buffer is affected by looking at the caret position
                SnapshotPoint? caretPoint = textView.Caret.Position.Point.GetPoint
                                                (textBuffer => (subjectBuffers.Contains(textBuffer) &&
                                                                completionBrokerMap.GetBrokerForTextView(textView, textBuffer) != null),
                                                 PositionAffinity.Predecessor);

                if (caretPoint.HasValue)
                {
                    List<INode> completionNodes = 
                        nodeProviders[caretPoint.Value.Snapshot.TextBuffer].GetNodes(caretPoint.Value);
                    if (completionNodes != null)
                    {
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
            activeSession = null;
        }

        void OnActiveSessionCommitted(object sender, System.EventArgs e)
        {
            //if (selectedCompletionBeforeCommit != null)
            //{
            //    activeSession.TextView.Caret.MoveTo(new VirtualSnapshotPoint(activeSession.TextView.Caret.Position.BufferPosition));
            //}
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
                WpfTextView.VisualElement.KeyDown -= new System.Windows.Input.KeyEventHandler(VisualElement_KeyDown);
                WpfTextView.VisualElement.KeyUp -= new System.Windows.Input.KeyEventHandler(VisualElement_KeyUp);
            }
        }
    }
}
