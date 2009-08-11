using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.Text;
using System.Collections.ObjectModel;

namespace NDjango.Designer.Intellisense
{

    internal class CompletionSource : ICompletionSource
    {
        internal static string CompletionSetName = "ndjango.tags";

        public System.Collections.ObjectModel.ReadOnlyCollection<CompletionSet> GetCompletionInformation(ICompletionSession session)
        {
            CompletionProvider completionProvider = session.Properties[CompletionProvider.CompletionProviderSessionKey] as CompletionProvider;

            int triggerPointPosition = session.TriggerPoint.GetPosition(session.SubjectBuffer.CurrentSnapshot);
            ITrackingSpan trackingSpan = session.SubjectBuffer.CurrentSnapshot.CreateTrackingSpan(
                completionProvider.Position, completionProvider.Length, SpanTrackingMode.EdgeInclusive);

            if (completionProvider != null)
            {
                CompletionSet completionSet = new CompletionSet("TagCompletion", trackingSpan,
                    completionProvider.GetCompletions(session),
                    null);
                completionSet.SelectionStatusChanged += new EventHandler<ValueChangedEventArgs<CompletionSelectionStatus>>(completionSet_SelectionStatusChanged);
                return new ReadOnlyCollection<CompletionSet>(new List<CompletionSet> { completionSet });
            }

            return null;
        }

        void completionSet_SelectionStatusChanged(object sender, ValueChangedEventArgs<CompletionSelectionStatus> e)
        {
//            if (e.NewValue.IsSelected)
//                return;
//            ((CompletionSet)sender).SelectionStatus = new CompletionSelectionStatus(e.NewValue.Completion, true, false);
        }
    }
}
