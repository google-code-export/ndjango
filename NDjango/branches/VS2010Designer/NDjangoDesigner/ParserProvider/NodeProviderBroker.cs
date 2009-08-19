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
using Microsoft.VisualStudio.Text;
using System.ComponentModel.Composition;
using NDjango.Interfaces;
using Microsoft.VisualStudio.ApplicationModel.Environments;
using Microsoft.VisualStudio.Editor;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.OLE.Interop;
using System.Runtime.InteropServices;

namespace NDjango.Designer.Parsing
{
    internal interface INodeProviderBroker
    {
        NodeProvider GetNodeProvider(ITextBuffer buffer);
        bool IsNDjango(ITextBuffer buffer, IEnvironment context);
    }

    /// <summary>
    /// Allocates node porviders to text buffers
    /// </summary>
    [Export(typeof(INodeProviderBroker))]
    internal class NodeProviderBroker : INodeProviderBroker
    {

        IParser parser = new NDjango.TemplateManagerProvider()
            .WithSetting(NDjango.Constants.EXCEPTION_IF_ERROR, false); // {get; set;}

        [Import]
        internal IVsEditorAdaptersFactoryService adaptersFactory { get; set; }

        /// <summary>
        /// Determines whether the buffer conatins ndjango code
        /// </summary>
        /// <param name="buffer"></param>
        /// <returns><b>true</b> if this is a ndjango buffer</returns>
        public bool IsNDjango(ITextBuffer buffer, IEnvironment context)
        {
            // we do not need to mess with the text buffers for tooltips
            var formatMap = new VariableDescription();
            formatMap.Name = "FormatMap";
            var formatMapName = context.Get(formatMap);
            if (Convert.ToString(formatMapName) == "tooltip")
                return false;

            switch (buffer.ContentType.TypeName)
            {
                case "text":
                case "HTML":
                    return true;
                default: return false;
            }
        }

        /// <summary>
        /// Retrieves or creates a node provider for a buffer
        /// </summary>
        /// <param name="buffer"></param>
        /// <returns></returns>
        public NodeProvider GetNodeProvider(ITextBuffer buffer)
        {
            if (djangoDiagnostics == null)
                djangoDiagnostics = GetOutputPane(buffer);

            NodeProvider provider;
            if (!buffer.Properties.TryGetProperty(typeof(NodeProvider), out provider))
                buffer.Properties.AddProperty(typeof(NodeProvider), provider 
                    = new NodeProvider(djangoDiagnostics, parser, buffer));
            return provider;
        }
        IVsOutputWindowPane djangoDiagnostics = null;


        public IVsOutputWindowPane GetOutputPane(ITextBuffer textBuffer)
        {
            Guid page = this.GetType().GUID;
            string caption = "Django Templates";

            IVsOutputWindow service = GetService(textBuffer, typeof(SVsOutputWindow)) as IVsOutputWindow;

            IVsOutputWindowPane ppPane = null;
            if ((ErrorHandler.Failed(service.GetPane(ref page, out ppPane)) && (caption != null)) 
                && ErrorHandler.Succeeded(service.CreatePane(ref page, caption, 1, 1)))
            {
                service.GetPane(ref page, out ppPane);
            }
            if (ppPane != null)
            {
                ErrorHandler.ThrowOnFailure(ppPane.Activate());
            }
            return ppPane;
        }

        private object GetService(ITextBuffer textBuffer, Type type)
        {
            var vsBuffer = adaptersFactory.GetBufferAdapter(textBuffer);
            if (vsBuffer == null)
                return null;

            Guid guidServiceProvider = VSConstants.IID_IUnknown;
            IObjectWithSite objectWithSite = vsBuffer as IObjectWithSite;
            IntPtr ptrServiceProvider = IntPtr.Zero;
            objectWithSite.GetSite(ref guidServiceProvider, out ptrServiceProvider);
            Microsoft.VisualStudio.OLE.Interop.IServiceProvider serviceProvider =
                (Microsoft.VisualStudio.OLE.Interop.IServiceProvider)Marshal.GetObjectForIUnknown(ptrServiceProvider);

            Guid guidService = typeof(SVsOutputWindow).GUID;
            Guid guidInterface = typeof(IVsOutputWindow).GUID;
            IntPtr ptrObject = IntPtr.Zero;

            int hr = serviceProvider.QueryService(ref guidService, ref guidInterface, out ptrObject);
            if (ErrorHandler.Failed(hr) || ptrObject == IntPtr.Zero)
                return null;


            IVsOutputWindow taskList = (IVsOutputWindow)Marshal.GetObjectForIUnknown(ptrObject);
            Marshal.Release(ptrObject);

            return taskList;
        }
    }
}
