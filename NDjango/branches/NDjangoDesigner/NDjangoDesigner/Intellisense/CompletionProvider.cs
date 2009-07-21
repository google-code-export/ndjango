using System.Collections.Generic;
using Microsoft.VisualStudio.Language.Intellisense;

namespace NDjango.Designer.Intellisense
{
    class CompletionProvider
    {
        internal static string CompletionProviderSessionKey = "ndjango.completionProvider";
        private List<string> completions;

        public CompletionProvider(List<string> completions)
        {
            this.completions = completions;
        }

        internal IEnumerable<Microsoft.VisualStudio.Language.Intellisense.Completion> GetCompletions(Microsoft.VisualStudio.Language.Intellisense.ICompletionSession session)
        {
            foreach (string completion in completions)
                yield return new Completion(completion, completion, completion);
        }
    }
}
