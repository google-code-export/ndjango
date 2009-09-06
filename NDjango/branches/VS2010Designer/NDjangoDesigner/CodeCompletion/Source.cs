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
        public ReadOnlyCollection<CompletionSet> GetCompletionInformation(ICompletionSession session)
        {
            Tuple<SnapshotPoint, string> origin;
            if (session.Properties.TryGetProperty<Tuple<SnapshotPoint, string>>(typeof(Source), out origin))
            {
                var caretPoint = origin.Item1;
                var triggerChars = origin.Item2;
                List<CompletionSet> completions =
                    GetCompletions(caretPoint, triggerChars);
                if (completions.Count == 0)
                    return null;

                return new ReadOnlyCollection<CompletionSet>(completions);
            }
            return null;
        }

        private List<CompletionSet> GetCompletions(SnapshotPoint point, string trigger)
        {

            List<IDjangoSnapshot> nodes = 
                nodeProviderBroker.GetNodeProvider(point.Snapshot.TextBuffer)
                    .GetNodes(point, node => true).FindAll(node => node.Values.Count > 0);
            List<CompletionSet> result = new List<CompletionSet>();
            if (trigger.Length > 0)
                switch (trigger)
                {
                    case "{%":
                        CreateCompletionSet(nodes, result, point,
                                node => node.ContentType == ContentType.Context,
                                "% ",
                                " %}");
                        break;
                    case ":":
                    case "|":
                        CreateCompletionSet(nodes, result, point,
                                node => node.ContentType == ContentType.FilterName,
                                trigger,
                                "");
                        break;
                    default:
                        if (Char.IsLetterOrDigit(trigger[0]))
                            CreateCompletionSet(nodes, result, point,
                                    node => node.ContentType != ContentType.Context,
                                    "",
                                    "");
                        break;
                }
            return result;
        }

        private void CreateCompletionSet(
                List<IDjangoSnapshot> nodes,
                List<CompletionSet> sets,
                SnapshotPoint point,
                Predicate<IDjangoSnapshot> selector,
                string prefix,
                string suffix
            )
        {
            var node = nodes.FindLast(selector);
            if (node == null)
                return;
            Span span = new Span(point.Position, 0);
            if (node.SnapshotSpan.IntersectsWith(span))
                span = node.SnapshotSpan.Span;
            var applicableTo = point.Snapshot.CreateTrackingSpan(span, SpanTrackingMode.EdgeInclusive);
            sets.Add(new CompletionSet(
                "NDjango Completions",
                applicableTo,
                CompletionsForNode(node.Values, prefix, suffix),
                null
                ));
        }

        private IEnumerable<Completion> CompletionsForNode(IEnumerable<string> values, string prefix, string suffix)
        {
            foreach (string value in values)
                yield return new Completion(value, prefix + value + suffix, value);
        }
    }
}
