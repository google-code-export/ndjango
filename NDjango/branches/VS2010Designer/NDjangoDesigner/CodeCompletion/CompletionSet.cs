using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.Text;
using NDjango.Designer.Parsing;
using System.Windows.Input;
using System.Runtime.InteropServices;

namespace NDjango.Designer.CodeCompletion
{

    class CompletionSet : Microsoft.VisualStudio.Language.Intellisense.CompletionSet
    {


        private ITrackingSpan completionSpan;
        private int offset;
        private ICompletionSession session;
        private List<Completion> completions;

        public CompletionSet(ICompletionSession session, ITrackingSpan applicableTo, IDjangoSnapshot node, CompletionContext context)
            : base("Django Completions", applicableTo, null, null)
        {
            this.session = session;
            ITextSnapshot snapshot = session.TriggerPoint.TextBuffer.CurrentSnapshot;

            string prefix = "";
            string suffix = "";

            completionSpan = snapshot.CreateTrackingSpan
                (session.TriggerPoint.GetPosition(snapshot), 0, SpanTrackingMode.EdgeInclusive);

            switch (context)
            {
                case CompletionContext.Tag:
                    prefix = "% ";
                    suffix = " %}";
                    offset = 1;
                    break;
                case CompletionContext.FilterName:
                    prefix = "|";
                    suffix = "";
                    offset = 1;
                    break;
                case CompletionContext.Other:
                    prefix = "";
                    suffix = "";
                    offset = 0;
                    break;
            }
            completions = new List<Completion>(CompletionsForNode(node.Values, prefix, suffix));
//            WritableCompletions.AddRange(completions);
        }

        private static IEnumerable<Completion> CompletionsForNode(IEnumerable<string> values, string prefix, string suffix)
        {
            foreach (string value in values)
                yield return new Completion(value, prefix + value + suffix, value);
        }
        
        public override void Filter(CompletionMatchType matchType, bool caseSensitive)
        {
            base.Filter(matchType, caseSensitive);
        }

        private string getPrefix()
        {
            var prefix = completionSpan.GetText(completionSpan.TextBuffer.CurrentSnapshot);
            if (prefix.Length > offset)
                return prefix.Substring(offset);
            return prefix;
        }

        public override void SelectBestMatch()
        {
            string prefix = getPrefix();
            Completion completion = Completions.FirstOrDefault(c => c.DisplayText.CompareTo(prefix) >= 0);
            if (completion != null)
                SelectionStatus = new CompletionSelectionStatus(completion, true, completion.DisplayText == prefix);
        }

        public override IList<Completion> Completions
        {
            get
            {
                string prefix = getPrefix();
                //if (prefix.Length > 1)
                //    return completions.Where(c => c.DisplayText.StartsWith(prefix.Substring(0, prefix.Length - 1))).ToList();
                //else
                    return completions;
            }
        }

        public override IList<Completion> CompletionBuilders
        {
            get
            {
                return base.CompletionBuilders;
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

        public static CompletionContext GetCompletionContext(Key key, ITextBuffer buffer, int position)
        {
            string triggerChars = Win32.CharsOfKey(key);

            // return if the key pressed is not a character key
            if (triggerChars == "")
                return CompletionContext.None;

            switch (triggerChars[0])
            {
                case '%':
                    if (position > 0 && buffer.CurrentSnapshot[position - 1] == '{')
                        // it is start of a new tag
                        return CompletionContext.Tag;
                    else
                        // if it is not we can ignore it
                        return CompletionContext.None;

                case '|':
                    return CompletionContext.FilterName;

                default:
                    return CompletionContext.Other;
            }

        }

    }

    /// <summary>
    /// A list of various contexts a list of completions can be requested from
    /// </summary>
    public enum CompletionContext
    {
        /// <summary>
        /// A new tag context - triggered if a '%' is entered right after '{'
        /// </summary>
        Tag,
        /// <summary>
        /// A filter name context - triggered by '|'
        /// </summary>
        FilterName,

        /// <summary>
        /// Other is a context covering typeing inside a word - a tag name, a filter name a keyword, etc
        /// </summary>
        Other,

        /// <summary>
        /// This is not a code completion context no list will be provided
        /// </summary>
        None
    }
}
