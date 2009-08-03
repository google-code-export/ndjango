using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel.Composition;
using Microsoft.VisualStudio.Utilities;
using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using NDjango.Designer.Parsing;
using Microsoft.VisualStudio.ApplicationModel.Environments;

namespace NDjango.Designer.QuickInfo
{
    [Export(typeof(IIntellisenseControllerProvider))]
    [Name("NDjango Completion Controller")]
    [Order(Before = "Default Completion Controller")]
    [ContentType(Constants.NDJANGO)]
    internal class CompletionControllerProvider : IIntellisenseControllerProvider
    {

        [Import(typeof(IQuickInfoBrokerMapService))]
        private IQuickInfoBrokerMapService brokerMapService { get; set; }

        [Import]
        internal INodeProviderBroker parser { get; set; }

        public IIntellisenseController TryCreateIntellisenseController(ITextView textView, IList<ITextBuffer> subjectBuffers, IEnvironment context)
        {

            bool brokerCreated = false;
            foreach (ITextBuffer subjectBuffer in subjectBuffers)
            {
                if (parser.IsNDjango(subjectBuffer))
                    brokerCreated |= (brokerMapService.GetBrokerForTextView(textView, subjectBuffer) != null);
            }

            // There may not be a broker for any of the subject buffers for this text view.  This can happen if there are no providers available.
            if (brokerCreated)
            {
                return new Controller(parser, subjectBuffers, textView, brokerMapService);
            }

            return null;
        }
    }
}
