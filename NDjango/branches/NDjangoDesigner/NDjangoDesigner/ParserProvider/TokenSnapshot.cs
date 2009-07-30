using Microsoft.VisualStudio.Text;

namespace NDjango.Designer.Parsing
{
    class TokenSnapshot
    {
        private SnapshotSpan snapshotSpan;
        private Token token;

        public TokenSnapshot(ITextSnapshot snapshot, Token token)
        {
            this.snapshotSpan = new SnapshotSpan(snapshot, token.Position, token.Length);
            this.token = token;
        }

        public SnapshotSpan SnapshotSpan { get { return snapshotSpan; } }

        public Token Token { get { return token; } }

        public string Type
        {
            get
            {
                switch (token.Type)
                {
                    case Token.TokenType.Marker:
                        return "ndjango.marker";
                    default:
                        return "ndjango.tag";
                }
            }
        }

        internal void TranslateTo(ITextSnapshot snapshot)
        {
            snapshotSpan = snapshotSpan.TranslateTo(snapshot, SpanTrackingMode.EdgeExclusive);
        }
    }
}
