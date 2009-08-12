using System.Collections.Generic;
using Microsoft.VisualStudio.Language.Intellisense;
using NDjango.Interfaces;

namespace NDjango.Designer.Intellisense
{
    class CompletionProvider
    {
        internal static string CompletionProviderSessionKey = "ndjango.completionProvider";
        private List<INode> completionNodes;

        public CompletionProvider(List<INode> completionNodes)
        {
            this.completionNodes = completionNodes;
        }

        internal IEnumerable<Microsoft.VisualStudio.Language.Intellisense.Completion> GetCompletions(Microsoft.VisualStudio.Language.Intellisense.ICompletionSession session)
        {
            foreach (INode node in completionNodes)
                foreach (string value in node.Values)
                    yield return new Completion(value, value, value);
        }
    }
}
