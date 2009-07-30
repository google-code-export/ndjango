using System.ComponentModel.Composition;
using Microsoft.VisualStudio.Text.Classification;
using Microsoft.VisualStudio.Utilities;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.ApplicationModel.Environments;
using NDjango.Designer.Parsing;

namespace NDjango.Designer.Classifiers
{
    /// <summary>
    /// Provides classifiers for TextBuffers
    /// </summary>
    /// <remarks>Imports the Parser object and passes it on to the new classifiers so that 
    /// classifiers can generate tokenzers</remarks>
    [Export(typeof(IClassifierProvider))]
    [ContentType(Constants.NDJANGO)]
    [Name("NDjango Classifier")]
    internal class ClassifierProvider : IClassifierProvider
    {
        [Import]
        internal IClassificationTypeRegistryService classificationTypeRegistry { get; set; }

        [Import]
        internal IParserController parser { get; set; }

        public IClassifier GetClassifier(ITextBuffer textBuffer, IEnvironment context)
        {
            if (parser.IsNDjango(textBuffer))
                return new Classifier(parser, classificationTypeRegistry, textBuffer);
            else
                return null;
        }
    }
}
