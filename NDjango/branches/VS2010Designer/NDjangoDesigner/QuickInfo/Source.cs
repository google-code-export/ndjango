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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.Language.Intellisense;
using NDjango.Interfaces;
using Microsoft.VisualStudio.Text;

namespace NDjango.Designer.QuickInfo
{
    /// <summary>
    /// Provides the tooltip content
    /// </summary>
    class Source : IQuickInfoSource
    {
        /// <summary>
        /// Generates the tooltip text 
        /// </summary>
        /// <param name="session"></param>
        /// <param name="applicableToSpan"></param>
        /// <returns></returns>
        /// <remarks>the quick info session will be automatically dismissed
        /// when mouse cursor leaves the 'applicable to' span. The size of the span is
        /// calculated based on the size of the nodes supplying the info to be shown
        /// </remarks>
        public object GetToolTipContent(IQuickInfoSession session, out ITrackingSpan applicableToSpan)
        {
            StringBuilder message = new StringBuilder();
            int position = session.SubjectBuffer.CurrentSnapshot.Length;
            int length = 0;
            string errorSeparator = "\nError:";
            List<INode> nodes;
            if (session.Properties.TryGetProperty<List<INode>>(typeof(SourceProvider), out nodes))
            {
                nodes.ForEach(
                    node => 
                    {
                        // include the node description at the top of the list
                        if (!String.IsNullOrEmpty(node.Description))
                            message.Insert(0, node.Description + "\n");
                        if (node.ErrorMessage.Severity >= 0)
                        {
                            // include the error message text at the bottom
                            message.Append(errorSeparator + "\n\t" + node.ErrorMessage.Message);
                            errorSeparator = "";
                        }
                        if (node.Length > length)
                            length = node.Length;
                        if (node.Position < position)
                            position = node.Position;
                    }
                        );
            }

            applicableToSpan = session.SubjectBuffer.CurrentSnapshot.CreateTrackingSpan(
                position,
                length,
                Microsoft.VisualStudio.Text.SpanTrackingMode.EdgeExclusive);

            if (message.Length > 0)
                return message.ToString();
            else
                return null;
        }
    }
}
