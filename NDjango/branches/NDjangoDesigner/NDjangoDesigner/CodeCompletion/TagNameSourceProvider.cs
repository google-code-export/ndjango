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
    internal class TagNameSourceProvider : ICompletionSourceProvider
    {
        [Import]
        internal INodeProviderBroker nodeProviderBroker { get; set; }

        public ICompletionSource TryCreateCompletionSource(ITextBuffer textBuffer, IEnvironment environment)
        {
            if (nodeProviderBroker.IsNDjango(textBuffer))
                return new TagNameSource();
            return null;
        }
    }
}
