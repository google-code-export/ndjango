using System.Collections.Generic;
using System.ComponentModel.Composition;
using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.Utilities;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.ApplicationModel.Environments;
using NDjango.Designer.Parsing;

namespace NDjango.Designer.Intellisense
{
    [Export(typeof(IIntellisenseControllerProvider))]
    [Name("NDjango Completion Controller")]
    [Order(Before = "Default Completion Controller")]
    [ContentType(Constants.NDJANGO)]
    internal class CompletionControllerProvider : IIntellisenseControllerProvider
    {
        [Import]
        private ICompletionBrokerMapService CompletionBrokerMapService { get; set; }

        [Import]
        internal IParserProviderBorker parser { get; set; }

        public IIntellisenseController TryCreateIntellisenseController(ITextView textView, IList<ITextBuffer> subjectBuffers, IEnvironment context)
        {

            bool brokerCreated = false;
            foreach (ITextBuffer subjectBuffer in subjectBuffers)
            {
                if (parser.IsNDjango(subjectBuffer))
                    brokerCreated |= (CompletionBrokerMapService.GetBrokerForTextView(textView, subjectBuffer) != null);
            }

            // There may not be a broker for any of the subject buffers for this text view.  This can happen if there are no providers available.
            if (brokerCreated)
            {
                return new CompletionController(parser, subjectBuffers, textView, CompletionBrokerMapService);
            }

            return null;
        }
    }
}
