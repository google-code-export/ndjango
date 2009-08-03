using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.ApplicationModel.Environments;
using System.ComponentModel.Composition;
using Microsoft.VisualStudio.Utilities;

namespace NDjango.Designer.QuickInfo
{
    [Export(typeof(IQuickInfoSourceProvider))]
    [Name("NDjango QuickInfo Source")]
    [Order(Before = "default")]
    [ContentType(Constants.NDJANGO)]
    class SourceProvider : IQuickInfoSourceProvider
    {
        internal static string QuickInfoProviderSessionKey = "ndjango.quickInfoProvider";

        public IQuickInfoSource TryCreateQuickInfoSource(ITextBuffer textBuffer, IEnvironment environment)
        {
            return new Source(textBuffer);
        }
    }
}
