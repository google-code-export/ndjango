using System.Windows.Media;
using System.ComponentModel.Composition;
using Microsoft.VisualStudio.Utilities;
using Microsoft.VisualStudio.Text.Classification;

namespace NDjango.Designer
{
    static class Constants
    {
        internal const string NDJANGO = "ndjango";
        [Export]
        [Name(NDJANGO)]
        [DisplayName("NDjango template")]
        [BaseDefinition("text")]
        //[BaseDefinition("HTML")]
        internal static ContentTypeDefinition NDjangoContentType;

        [Export]
        [FileExtension(".txt")]
        [ContentTypeAttribute(NDJANGO)]
        internal static FileExtensionToContentTypeDefinition NDjangoExtensionTxt;

        //[Export]
        //[FileExtension(".htm")]
        //[ContentTypeAttribute(NDJANGO)]
        //internal static FileExtensionToContentTypeDefinition NDjangoExtensionHtm;

        internal const string TAG_CLASSIFIER = "ndjango.tag";
        [Export]
        [Name(TAG_CLASSIFIER)]
        [BaseDefinition("text")]
        internal static ClassificationTypeDefinition NDjangoTag;

        internal const string MARKER_CLASSIFIER = "ndjango.marker";
        [Export]
        [Name(MARKER_CLASSIFIER)]
        [BaseDefinition("text")]
        internal static ClassificationTypeDefinition NDjangoMarker;

        [Export(typeof(EditorFormatDefinition))]
        [Name("ndjango.tag.format")]
        [DisplayName("ndjango tag format")]
        [UserVisible(false)]
        [ClassificationType(ClassificationTypeNames = "ndjango.tag")]
        [Order(Before = Priority.High)]
        internal sealed class NDjangoTagFormat : ClassificationFormatDefinition
        {
            public NDjangoTagFormat()
            {
                BackgroundColor = Colors.Yellow;
            }
        }

        [Export(typeof(EditorFormatDefinition))]
        [Name("ndjango.marker.format")]
        [DisplayName("ndjango marker format")]
        [UserVisible(false)]
        [ClassificationType(ClassificationTypeNames = "ndjango.marker")]
        [Order]
        internal sealed class NDjangoMarkerFormat : ClassificationFormatDefinition
        {
            public NDjangoMarkerFormat()
            {
                ForegroundColor = Colors.Red;
            }
        }
    }
}
