using System.ComponentModel.Composition;
using Microsoft.VisualStudio.Text.Tagging;
using Microsoft.VisualStudio.Utilities;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.ApplicationModel.Environments;
using NDjango.Designer.Parsing;

namespace NDjango.Designer.Tags
{
    /// <summary>
    /// Provides tags for text buffrers
    /// </summary>
    /// <remarks> Imports parser object in order to generate tokenizer for tagger</remarks>
    [Export(typeof(ITaggerProvider))]
    [ContentType(Constants.NDJANGO)]
    [TagType(typeof(SquiggleTag))]
    class TaggerProvider : ITaggerProvider
    {
        [Import]
        internal IParserController parser { get; set; }

        public ITagger<T> CreateTagger<T>(ITextBuffer buffer, IEnvironment context) where T : ITag
        {
            if (parser.IsNDjango(buffer))
                return (ITagger<T>)new Tagger(parser, buffer);
            else
                return null;
        }

    }
}
