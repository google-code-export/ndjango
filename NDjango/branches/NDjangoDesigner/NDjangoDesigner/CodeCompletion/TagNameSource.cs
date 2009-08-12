﻿using System;
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
                    IndexOfAny(new char[] {' ', '\t', '%'} );

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