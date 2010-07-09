using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;

namespace NewViewGenerator.Interaction
{
    public class SelectionHandler : IVsSelectionEvents
    {

        public static EnvDTE.DTE dte;
        public IVsProject curHier;
        public string root;
        public  uint viewsfolderId;
        private static IVsMonitorSelection SelectionService;
        private static uint ContextCookie = RegisterContext();
        private static uint RegisterContext()
        {
            SelectionService = (IVsMonitorSelection)Package.GetGlobalService(typeof(SVsShellMonitorSelection));
            dte = (EnvDTE.DTE)Package.GetGlobalService(typeof(EnvDTE.DTE));
            uint retVal;
            Guid uiContext = GuidList.UICONTEXT_ViewsSelected;
            SelectionService.GetCmdUIContextCookie(ref uiContext, out retVal);
            return retVal;

        }
        int IVsSelectionEvents.OnCmdUIContextChanged(uint dwCmdUICookie, int fActive)
        {
            return VSConstants.S_OK;
        }

        int IVsSelectionEvents.OnElementValueChanged(uint elementid, object varValueOld, object varValueNew)
        {
              return VSConstants.S_OK;
        }


        int IVsSelectionEvents.OnSelectionChanged(IVsHierarchy pHierOld, uint itemidOld,IVsMultiItemSelect pMISOld, ISelectionContainer pSCOld,IVsHierarchy pHierNew, uint itemidNew,IVsMultiItemSelect pMISNew, ISelectionContainer pSCNew)
        {

            if (pHierNew != null)
            {
                object itemName;
                string projectName;
                pHierNew.GetProperty(itemidNew, (int)__VSHPROPID.VSHPROPID_Name, out itemName);
                if (itemName != null && itemName.ToString().Contains("Views"))
                {
                    curHier = (IVsProject)pHierNew;
                    pHierNew.GetCanonicalName(VSConstants.VSITEMID_ROOT, out projectName);
                    root = projectName.Substring(0, projectName.LastIndexOf('\\') + 1);
                    viewsfolderId = itemidNew;
                    SelectionService.SetCmdUIContext(ContextCookie, 1);
                }
            }
            else
                SelectionService.SetCmdUIContext(ContextCookie, 0);
            return VSConstants.S_OK;

        }

    }

}
