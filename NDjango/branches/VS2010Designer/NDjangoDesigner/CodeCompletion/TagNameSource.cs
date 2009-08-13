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
using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.Text;
using System.Collections.ObjectModel;

namespace NDjango.Designer.Intellisense
{

    /// <summary>
    /// Supplies a list of tag names registered with parser
    /// </summary>
    internal class TagNameSource : ICompletionSource
    {
        internal static string TagCompletionSetName = "ndjango.tag.names";

        public System.Collections.ObjectModel.ReadOnlyCollection<CompletionSet> GetCompletionInformation(ICompletionSession session)
        {
            CompletionProvider completionProvider = session.Properties[CompletionProvider.CompletionProviderSessionKey] as CompletionProvider;
            if (completionProvider != null)
            {
                ITextSnapshot snapshot = session.SubjectBuffer.CurrentSnapshot;
                int triggerPoint = session.TriggerPoint.GetPosition(snapshot);
                ITextSnapshotLine line = snapshot.GetLineFromPosition(triggerPoint);
                string lineString = line.GetText();
                // position of the first non-space character before the tag name
                int start = lineString.Substring(0, triggerPoint - line.Start.Position).
                    LastIndexOfAny(new char[] {' ', '\t', '%'})
                    + line.Start.Position + 1;
                // length of the word currently in the tag name position in the tag
                int length = lineString.Substring(triggerPoint - line.Start.Position).
                    IndexOfAny(new char[] {' ', '\t', '%'} )
                    + triggerPoint - start;

                CompletionSet completionSet = new CompletionSet(
                    "TagCompletion", 
                    session.SubjectBuffer.CurrentSnapshot.CreateTrackingSpan(
                    start, length, SpanTrackingMode.EdgeInclusive),
                    completionProvider.GetCompletions(session),
                    null);
                return new ReadOnlyCollection<CompletionSet>(new List<CompletionSet> { completionSet });
            }

            return null;
        }
    }
}
