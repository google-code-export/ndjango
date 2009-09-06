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
using System.Runtime.InteropServices;
using NDjango.Designer.Parsing;

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
                return;

            // if there is a session already leave it be
            if (activeSession != null)
                return;

            // determine which subject buffer is affected by looking at the caret position
            SnapshotPoint? caret = textView.Caret.Position.Point.GetPoint
                (textBuffer => 
                    (
                        subjectBuffers.Contains(textBuffer) 
                        && provider.nodeProviderBroker.IsNDjango(textBuffer, context)
                        && provider.CompletionBrokerMapService.GetBrokerForTextView(textView, textBuffer) != null
                    ),
                    PositionAffinity.Predecessor);

            // return if no suitable buffer found
            if (!caret.HasValue)
                return;

            SnapshotPoint caretPoint = caret.Value;

            var subjectBuffer = caretPoint.Snapshot.TextBuffer;

            string triggerChars = Win32.CharsOfKey(e.Key);

            // return if the key pressed is not a character key
            if (triggerChars == "")
                return;

            if (triggerChars == "%" && caretPoint > 0)
                if (subjectBuffer.CurrentSnapshot[caretPoint.Position-1] == '{')
                    // start of a new tag
                    triggerChars = "{%";
                else
                    return;

            // the invocation occurred in a subject buffer of interest to us
            ICompletionBroker broker = provider.CompletionBrokerMapService.GetBrokerForTextView(textView, subjectBuffer);
            ITrackingPoint triggerPoint = caretPoint.Snapshot.CreateTrackingPoint(caretPoint.Position, PointTrackingMode.Positive);

            List<CompletionSet> completions = 
                provider.nodeProviderBroker.GetNodeProvider(subjectBuffer).GetCompletions(caretPoint, triggerChars);

            // return if there is no information to show
            if (completions.Count == 0)
                return;

            // attach filter to intercept the Enter key
            attachKeyboardFilter();

            // Create a completion session
            activeSession = broker.CreateCompletionSession(triggerPoint, true);

            // Put the list of completion nodes on the session so that it can be used by the completion source
            activeSession.Properties.AddProperty(typeof(Source),completions);

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
            if (activeSession.SelectedCompletionSet.SelectionStatus.Completion.InsertionText.EndsWith("%}"))
            {
                var textView = activeSession.TextView;
                textView.Caret.MoveToPreviousCaretPosition();
                textView.Caret.MoveToPreviousCaretPosition();
            }
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

    internal class Win32
    {
        public static string CharsOfKey(Key key)
        {
            uint vk = (uint)KeyInterop.VirtualKeyFromKey(key);
            byte[] keyState = new byte[256];
            Win32.GetKeyboardState(keyState);
            uint scancode = Win32.MapVirtualKey(vk, (uint)Win32.MapType.MAPVK_VK_TO_VSC);
            char[] buffer = new char[10];
            int count = Win32.ToUnicode(vk, scancode, keyState, buffer, buffer.Length, 0);
            if (count < 0) 
                count = 0;
            char[] result = new char[count];
            Array.Copy(buffer, result, count);
            return new string(result);
        }

        /// <summary>The set of valid MapTypes used in MapVirtualKey
        /// </summary>
        /// <remarks></remarks>
        public enum MapType : uint
        {
            /// <summary>uCode is a virtual-key code and is translated into a scan code.
            /// If it is a virtual-key code that does not distinguish between left- and
            /// right-hand keys, the left-hand scan code is returned.
            /// If there is no translation, the function returns 0.
            /// </summary>
            /// <remarks></remarks>
            MAPVK_VK_TO_VSC = 0x0,

            /// <summary>uCode is a scan code and is translated into a virtual-key code that
            /// does not distinguish between left- and right-hand keys. If there is no
            /// translation, the function returns 0.
            /// </summary>
            /// <remarks></remarks>
            MAPVK_VSC_TO_VK = 0x1,

            /// <summary>uCode is a virtual-key code and is translated into an unshifted
            /// character value in the low-order word of the return value. Dead keys (diacritics)
            /// are indicated by setting the top bit of the return value. If there is no
            /// translation, the function returns 0.
            /// </summary>
            /// <remarks></remarks>
            MAPVK_VK_TO_CHAR = 0x2,

            /// <summary>Windows NT/2000/XP: uCode is a scan code and is translated into a
            /// virtual-key code that distinguishes between left- and right-hand keys. If
            /// there is no translation, the function returns 0.
            /// </summary>
            /// <remarks></remarks>
            MAPVK_VSC_TO_VK_EX = 0x3,

            /// <summary>Not currently documented
            /// </summary>
            /// <remarks></remarks>
            MAPVK_VK_TO_VSC_EX = 0x4,
        }

        [DllImport("user32.dll", CharSet = System.Runtime.InteropServices.CharSet.Auto)]
        public static extern uint MapVirtualKey(uint uCode, uint uMapType);

        [DllImport("user32.dll", CharSet = System.Runtime.InteropServices.CharSet.Auto)]
        public static extern bool GetKeyboardState(byte[] keyState);

        [DllImport("user32.dll", CharSet = System.Runtime.InteropServices.CharSet.Auto)]
        public static extern int 
            ToUnicode(
                uint virtKey, 
                uint scanCode, 
                byte[] keyState,
                char[] resultBuffer,
                int bufSize,
                int flags
            );
    }
}
