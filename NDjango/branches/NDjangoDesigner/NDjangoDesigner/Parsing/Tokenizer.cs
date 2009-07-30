using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.Text;
using System.Threading;

namespace NDjango.Designer.Parsing
{
    /// <summary>
    /// Provides ability to retrive tokens out of snapshot objects.
    /// </summary>
    class Tokenizer
    {
        // it can take some time for the parser to build the token list.
        // for now let us initialize it to an empty list
        private List<TokenSnapshot> tokens = new List<TokenSnapshot>();
        
        private object token_lock = new object();
        private IParserController parser;
        private ITextBuffer buffer;

        public Tokenizer(IParserController parser, ITextBuffer buffer)
        {
            this.parser = parser;
            this.buffer = buffer;
            rebuildTokens(buffer.CurrentSnapshot);
            buffer.Changed += new EventHandler<TextContentChangedEventArgs>(buffer_Changed);
        }

        public delegate void TokenEvent (SnapshotSpan snapshotSpan);

        void buffer_Changed(object sender, TextContentChangedEventArgs e)
        {
            rebuildTokens(e.After);
        }

        private void rebuildTokens(ITextSnapshot snapshot)
        {
            ThreadPool.QueueUserWorkItem(rebuildTokensAsynch, snapshot);
        }

        public event TokenEvent TagsChanged;

        /// <summary>
        /// Retrieves sequence of tokens out of snapshot object. 
        /// </summary>
        private void rebuildTokensAsynch(object snapshotObject)
        {
            ITextSnapshot snapshot = (ITextSnapshot)snapshotObject;
            List<TokenSnapshot> tokens = parser.Parse(snapshot.Lines.ToList().ConvertAll(line => line.GetTextIncludingLineBreak())).ConvertAll<TokenSnapshot>
                (token => new TokenSnapshot(snapshot, token));
            lock (token_lock)
            {
                this.tokens = tokens;
            }
            if (TagsChanged != null)
                TagsChanged(new SnapshotSpan(snapshot, 0, snapshot.Length));
        }

        internal List<TokenSnapshot> GetTokens(SnapshotSpan snapshotSpan)
        {
            List<TokenSnapshot> tokens;
            lock (token_lock)
            {
                tokens = this.tokens;
            }
            if (tokens.Count == 0)
                return tokens;
            
            // just in case if while the tokens list was being rebuilt
            // another modification was made
            if (this.tokens[0].SnapshotSpan.Snapshot != snapshotSpan.Snapshot)
                this.tokens.ForEach(token => token.TranslateTo(snapshotSpan.Snapshot));

            return tokens;
        }
        
        /// <summary>
        /// Gets a list of intellisence values of selected token.
        /// </summary>
        /// <param name="point">Mouse cursor destination</param>
        /// <returns></returns>
        internal List<string> GetCompletions(SnapshotPoint point)
        {
            TokenSnapshot result = GetTokens(new SnapshotSpan(point.Snapshot, point.Position, 0))
                .FirstOrDefault(token => token.SnapshotSpan.IntersectsWith(new SnapshotSpan(point.Snapshot, point.Position, 0)));
            if (result == null)
                return new List<string>();
            result.Token.GenerateCompletionValues(new List<string>());
            return result.Token.Values;
        }
    }
}
