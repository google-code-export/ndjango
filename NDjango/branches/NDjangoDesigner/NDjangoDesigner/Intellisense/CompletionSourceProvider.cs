using System.ComponentModel.Composition;
using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.ApplicationModel.Environments;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Utilities;
using NDjango.Designer.Parsing;

namespace NDjango.Designer.Intellisense
{
    [Export(typeof(ICompletionSourceProvider))]
    [Name("NDjango Completion Source")]
    [Order(Before = "default")]
    [ContentType(Constants.NDJANGO)]
    internal class CompletionSourceProvider : ICompletionSourceProvider
    {
        [Import]
        internal IParser parser { get; set; }

        public ICompletionSource TryCreateCompletionSource(ITextBuffer textBuffer, IEnvironment environment)
        {
            return new CompletionSource();
        }
    }
}
