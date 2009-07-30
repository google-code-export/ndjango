using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.Text.Classification;
using Microsoft.VisualStudio.Text;
using NDjango.Designer.Parsing;

namespace NDjango.Designer.Classifiers
{
    /// <summary>
    /// Supplies a list of <see cref="ClassificationSpan"/> according to the specs of <see cref="IClassifier"/> interface
    /// </summary>
    /// <remarks>
    /// When an instance of the <see cref="Classifier"/> is created for a buffer it requests from the 
    /// <see cref="Parser"/> an instance of the <see cref="Tokenizer"/> for the specified buffer and subscribes
    /// to the TagsChanged event of the tokenizer. From this moment on work of the <see cref="Classifier"/> is 
    /// controlled by the recieved instance of the <see cref="Tokenizer"/>.
    /// </remarks>
    internal class Classifier : IClassifier
    {
        private IClassificationTypeRegistryService classificationTypeRegistry;
        private Tokenizer tokenizer;

        /// <summary>
        /// Creates a new instance of the <see cref="Classifier"/>
        /// </summary>
        /// <param name="parser"></param>
        /// <param name="classificationTypeRegistry"></param>
        /// <param name="buffer"></param>
        public Classifier(IParserController parser, IClassificationTypeRegistryService classificationTypeRegistry, ITextBuffer buffer)
        {
            this.classificationTypeRegistry = classificationTypeRegistry;
            tokenizer = parser.GetTokenizer(buffer);
            tokenizer.TagsChanged += new Tokenizer.TokenEvent(tokenizer_TagsChanged);
        }

        private void tokenizer_TagsChanged(SnapshotSpan snapshotSpan)
        {
            if (ClassificationChanged != null)
                ClassificationChanged(this, new ClassificationChangedEventArgs(snapshotSpan));
        }

        /// <summary>
        /// Provides a list of <see cref="ClassificationSpan"/> objects for the specified span
        /// </summary>
        /// <param name="span">span for which the list is requested</param>
        /// <returns></returns>
        /// <remarks>The list is generated based on the list of <see cref="TokenSnapshots"/> recieved
        /// from the tokenizer</remarks> 
        public IList<ClassificationSpan> GetClassificationSpans(SnapshotSpan span)
        {
            List<ClassificationSpan> classifications = new List<ClassificationSpan>();

            foreach (TokenSnapshot token in tokenizer.GetTokens(span))
            {
                if (token.SnapshotSpan.OverlapsWith(span))
                    classifications.Add(
                        new ClassificationSpan(
                            token.SnapshotSpan,
                            classificationTypeRegistry.GetClassificationType(token.Type)
                            ));
            }
            return classifications;
        }

        public event EventHandler<ClassificationChangedEventArgs> ClassificationChanged;

    }
}
