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
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.ApplicationModel.Environments;
using System.ComponentModel.Composition;
using Microsoft.VisualStudio.Utilities;

namespace NDjango.Designer.QuickInfo
{
    [Export(typeof(IQuickInfoSourceProvider))]
    [Name("NDjango QuickInfo Source")]
    [Order(Before = "default")]
    [ContentType(Constants.NDJANGO)]
    class SourceProvider : IQuickInfoSourceProvider
    {
        internal static string QuickInfoProviderSessionKey = "ndjango.quickInfoProvider";

        public IQuickInfoSource TryCreateQuickInfoSource(ITextBuffer textBuffer, IEnvironment environment)
        {
            return new Source();
        }
    }
}
