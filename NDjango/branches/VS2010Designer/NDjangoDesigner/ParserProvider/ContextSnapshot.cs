using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NDjango.Interfaces;
using Microsoft.VisualStudio.Text;

namespace NDjango.Designer.Parsing
{
    class ContextSnapshot : IDjangoSnapshot
    {
        private SnapshotSpan snapshotSpan;

        public SnapshotSpan SnapshotSpan
        {
            get { return snapshotSpan; }
        }

        public SnapshotSpan ExtensionSpan
        {
            get { return snapshotSpan; }
        }

        public IEnumerable<IDjangoSnapshot> Children
        {
            get { return new List<IDjangoSnapshot>(); }
        }

        public string Type
        {
            get { return "text"; }
        }

        public void TranslateTo(ITextSnapshot snapshot)
        {
            snapshotSpan = snapshotSpan.TranslateTo(snapshot, SpanTrackingMode.EdgeExclusive);
        }

        public void ShowDiagnostics(Microsoft.VisualStudio.Shell.Interop.IVsOutputWindowPane djangoDiagnostics, string filePath)
        { 
        }

        public bool IsPlaceholder
        {
            get { return false; }
        }

        public ContentType ContentType
        {
            get { return ContentType.Context; }
        }

        public IList<string> Values
        {
            get { throw new NotImplementedException(); }
        }

        public string Description
        {
            get { return null; }
        }

        public NDjango.Interfaces.Error ErrorMessage
        {
            get { return Error.None; }
        }

    }
}
