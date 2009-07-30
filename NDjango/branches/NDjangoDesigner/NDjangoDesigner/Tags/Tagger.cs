using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.Text.Tagging;
using Microsoft.VisualStudio.Text;
using NDjango.Designer.Parsing;

namespace NDjango.Designer.Tags
{
    class Tagger : ITagger<ErrorTag>
    {
        private Tokenizer tokenizer;

        public Tagger(IParserController parser, ITextBuffer buffer)
        {
            tokenizer = parser.GetTokenizer(buffer);
            tokenizer.TagsChanged += new Tokenizer.TokenEvent(tokenizer_TagsChanged);
        }

        void tokenizer_TagsChanged(SnapshotSpan snapshotSpan)
        {
            if (TagsChanged != null)
                TagsChanged(this, new SnapshotSpanEventArgs(snapshotSpan));
        }

        /// <summary>
        /// Gets a list of tags related to a span
        /// </summary>
        /// <param name="spans"></param>
        /// <returns></returns>
        public IEnumerable<ITagSpan<ErrorTag>> GetTags(Microsoft.VisualStudio.Text.NormalizedSnapshotSpanCollection spans)
        {
            foreach (SnapshotSpan span in spans)
            {
                foreach (TokenSnapshot token in tokenizer.GetTokens(span))
                {
                    if (token.SnapshotSpan.OverlapsWith(span) && token.Token.Errors.Count > 0)
                        yield return new TagSpan<ErrorTag>(token.SnapshotSpan, new ErrorTag());
                }
            }
        }

        public event EventHandler<SnapshotSpanEventArgs> TagsChanged;

    }
}
