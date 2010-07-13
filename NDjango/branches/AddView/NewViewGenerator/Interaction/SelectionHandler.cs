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

        private static uint ContextCookie = RegisterContext();
        private static uint RegisterContext()
        {
            ProjectData.SelectionService = (IVsMonitorSelection)Package.GetGlobalService(typeof(SVsShellMonitorSelection));
            ProjectData.dte = (EnvDTE.DTE)Package.GetGlobalService(typeof(EnvDTE.DTE));
            uint retVal;
            Guid uiContext = GuidList.UICONTEXT_ViewsSelected;
            ProjectData.SelectionService.GetCmdUIContextCookie(ref uiContext, out retVal);
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
                string itemName;
                //pHierNew.GetProperty(itemidNew, (int)__VSHPROPID.VSHPROPID_Name, out itemName);
                pHierNew.GetCanonicalName(itemidNew, out itemName);
                if (itemName != null )//&& itemName.ToString().Contains("Views"))
                {
                    object temp;
                    ProjectData.curHier = (IVsProject)pHierNew;
                    pHierNew.GetProperty(VSConstants.VSITEMID_ROOT,(int)__VSHPROPID.VSHPROPID_ProjectDir, out temp);
                    ProjectData.projectDir = temp.ToString();
                    //root = projectFullName.Substring(0, projectFullName.LastIndexOf('\\') + 1);
                    pHierNew.GetProperty(VSConstants.VSITEMID_ROOT, (int)__VSHPROPID.VSHPROPID_ProjectName, out temp);
                    ProjectData.projectName = temp.ToString();
                    ProjectData.viewsFolderId = itemidNew;
                    ProjectData.viewsFolderName = itemName.ToString();
                    ProjectData.SelectionService.SetCmdUIContext(ContextCookie, 1);
                }
            }
            else
                ProjectData.SelectionService.SetCmdUIContext(ContextCookie, 0);
            return VSConstants.S_OK;

        }

    }

}
