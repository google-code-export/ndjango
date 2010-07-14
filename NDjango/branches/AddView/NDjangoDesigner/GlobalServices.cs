using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.Shell;
using System.ComponentModel.Composition;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell.Design;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Utilities;

namespace NDjango.Designer
{
    [Export]
    public class GlobalServices
    {

        private static GlobalServices handler = new GlobalServices();
        
        public static IVsRunningDocumentTable RDT {get; private set;}

        public static TaskProvider TaskList {get; private set;}

        public static DynamicTypeService TypeService { get; private set; }
        public static IVsMonitorSelection SelectionService { get; private set; }

        public static EnvDTE.DTE DTE { get; private set; }


        private static SVsServiceProvider serviceProvider;
        private static ITextBufferFactoryService bufferFactory;
        private static IContentTypeRegistryService contentService;
        [Import]
        private SVsServiceProvider ServiceProvider
        {
            get { return serviceProvider; }
            set
            {
                serviceProvider = value;
                TaskList = new TaskProvider(serviceProvider);
                RDT = GetService<IVsRunningDocumentTable>(typeof(SVsRunningDocumentTable));
                SelectionService = GetService<IVsMonitorSelection>(typeof(SVsShellMonitorSelection));
                DTE = GetService<EnvDTE.DTE>();
                TypeService = GetService<DynamicTypeService>();
            }
        }
        [Import]
        public static ITextBufferFactoryService TextBufferFactory
        {
            get { return bufferFactory; }
            set { bufferFactory = value; }
        }
        [Import]
        public static IContentTypeRegistryService ContentRegistryService
        {
            get { return contentService; }
            set { contentService = value; }
        }

        public T GetService<T>()
        {
            return (T)ServiceProvider.GetService(typeof(T));
        }

        public T GetService<T>(Type serviceType)
        {
            return (T)ServiceProvider.GetService(serviceType);
        }

    }
}
