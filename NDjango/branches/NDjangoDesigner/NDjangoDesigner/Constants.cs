/****************************************************************************
 * 
 *  NDjango Parser Copyright © 2009 Hill30 Inc
 *
 *  This file is part of the NDjango Designer.
 *
 *  The NDjango Parser is free software: you can redistribute it and/or modify
 *  it under the terms of the GNU Lesser General Public License as published by
 *  the Free Software Foundation, either version 3 of the License, or
 *  (at your option) any later version.
 *
 *  The NDjango Parser is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 *  GNU Lesser General Public License for more details.
 *
 *  You should have received a copy of the GNU Lesser General Public License
 *  along with NDjango Parser.  If not, see <http://www.gnu.org/licenses/>.
 *  
 ***************************************************************************/

using System.Windows.Media;
using System.ComponentModel.Composition;
using Microsoft.VisualStudio.Utilities;
using Microsoft.VisualStudio.Text.Classification;
using Microsoft.VisualStudio.Text.Tagging;

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

        internal class ErrorTag : SquiggleTag
        {
            public ErrorTag()
                : base("error") { }
        }
    }
}
