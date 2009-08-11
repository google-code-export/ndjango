using System.Collections.Generic;
using Microsoft.VisualStudio.Language.Intellisense;
using NDjango.Interfaces;

namespace NDjango.Designer.Intellisense
{
    class CompletionProvider
    {
        internal static string CompletionProviderSessionKey = "ndjango.completionProvider";
        private List<string> completions;
        int position;
        public int Position { get { return position; } }

        int length;
        public int Length { get { return length; } }

        public CompletionProvider(List<INode> completionNodes)
        {
            this.completions = new List<string>();
            completionNodes.ForEach(node => this.completions.AddRange(node.Values));
            if (completionNodes.Exists(node => node.NodeType == NodeType.TagName))
            {
                INode actualNode = completionNodes.Find(node => node.NodeType == NodeType.TagName);
                this.position = actualNode.Position;
                this.length = actualNode.Length;
            }
        }

        internal IEnumerable<Microsoft.VisualStudio.Language.Intellisense.Completion> GetCompletions(Microsoft.VisualStudio.Language.Intellisense.ICompletionSession session)
        {
            foreach (string completion in completions)
                yield return new Completion(completion, completion, completion);
        }
    }
}
