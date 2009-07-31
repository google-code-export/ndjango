using System.Windows.Media;
using System.ComponentModel.Composition;
using Microsoft.VisualStudio.Utilities;
using Microsoft.VisualStudio.Text.Classification;

namespace NDjango.Designer
{
    static class Constants
    {
        /// NDJANGO content type is defined to be just text - pretty much any text
        /// the actual filtering of the content types is done in the IsNDjango method 
        /// on the parser

        internal const string NDJANGO = "text"; //"ndjango";

        /// <summary>
        /// Classifier definition for django tags 
        /// </summary>
        internal const string DJANGO_CONSTRUCT = "ndjango.tag";
        [Export]
        [Name(DJANGO_CONSTRUCT)]
        private static ClassificationTypeDefinition DjangoConstruct;

        [Export(typeof(EditorFormatDefinition))]
        [Name("ndjango.tag.format")]
        [DisplayName("NDjango Tag Format")]
        [UserVisible(true)]
        [ClassificationType(ClassificationTypeNames = DJANGO_CONSTRUCT)]
        [Order]
        internal sealed class NDjangoTagFormat : ClassificationFormatDefinition
        {
            public NDjangoTagFormat()
            {
                BackgroundColor = Colors.Yellow;
            }
        }

        internal const string MARKER_CLASSIFIER = "ndjango.marker";
        [Export]
        [Name(MARKER_CLASSIFIER)]
        internal static ClassificationTypeDefinition NDjangoMarker;

        [Export(typeof(EditorFormatDefinition))]
        [Name("ndjango.marker.format")]
        [DisplayName("ndjango marker format")]
        [UserVisible(false)]
        [ClassificationType(ClassificationTypeNames = MARKER_CLASSIFIER)]
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
