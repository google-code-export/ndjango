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
using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.Text;
using System.Collections.ObjectModel;
using NDjango.Interfaces;
using NDjango.Designer.Parsing;
using VSCompletionSet = Microsoft.VisualStudio.Language.Intellisense.CompletionSet;

namespace NDjango.Designer.CodeCompletion
{

    /// <summary>
    /// Supplies a list of completion values
    /// </summary>
    internal class Source : ICompletionSource
    {
        private INodeProviderBroker nodeProviderBroker;

        public Source(INodeProviderBroker nodeProviderBroker)
        {
            // TODO: Complete member initialization
            this.nodeProviderBroker = nodeProviderBroker;
        }
        /// <summary>
        /// Gets the completion information
        /// </summary>
        /// <param name="session"></param>
        /// <returns></returns>
        /// <remarks>
        /// The location of the textspan to be replaced with 
        /// the selection so that the entire entire word would be replaced
        /// </remarks>
        public ReadOnlyCollection<VSCompletionSet> GetCompletionInformation(ICompletionSession session)
        {
            CompletionContext context;
            if (!session.Properties.TryGetProperty<CompletionContext>(typeof(CompletionContext), out context))
                return null;

            SnapshotPoint point = session.TriggerPoint.GetPoint(session.TriggerPoint.TextBuffer.CurrentSnapshot);
            
            List<IDjangoSnapshot> nodes =
                nodeProviderBroker.GetNodeProvider(session.TriggerPoint.TextBuffer)
                    .GetNodes(point, n => true).FindAll(n => n.Values.Count > 0);

            IDjangoSnapshot node = null;
            switch (context)
            {
                case CompletionContext.Tag:
                    node = nodes.FindLast(n => n.ContentType == ContentType.Context);
                    break;

                case CompletionContext.FilterName:
                    node = nodes.FindLast(n => n.ContentType == ContentType.FilterName);
                    break;

                case CompletionContext.Other:
                    node = nodes.FindLast(n => n.ContentType != ContentType.Context);
                    break;

            }

            if (node == null)
                return null;

            Span span = new Span(point.Position, 0);
            if (node.SnapshotSpan.IntersectsWith(span))
                span = node.SnapshotSpan.Span;
            var applicableTo = point.Snapshot.CreateTrackingSpan(span, SpanTrackingMode.EdgeInclusive);
            
            return 
                new ReadOnlyCollection<VSCompletionSet>
                    (new CompletionSet[] {new CompletionSet(session, applicableTo, node, context)});

        }
    }
}
