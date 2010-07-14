using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Utilities;
using NDjango.Interfaces;
using NDjango.Designer;
using NDjango.Designer.Parsing;
using EnvDTE;

namespace NewViewGenerator.Interaction
{
    public class ProjectManager: IVsSelectionEvents
    {
        public static IVsProject Hierarchy { get; private set; }
        public string ProjectDir { get; private set; }
        public string ProjectName { get; private set; }
        public string ViewsFolderName;
        public uint ViewsFolderId;
        [Import]
        NDjango.Designer.GlobalServices services;
        [Import]
        private NodeProviderBroker broker;
        public ProjectHandler handler;
        private uint ContextCookie;
        private uint SelectionCookie;
        public ProjectManager()
        {
            ContextCookie = RegisterContext();
            handler = CreateHandler();
            GlobalServices.SelectionService.AdviseSelectionEvents(this, out SelectionCookie);
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
                    Hierarchy = (IVsProject)pHierNew;
                    pHierNew.GetProperty(VSConstants.VSITEMID_ROOT,(int)__VSHPROPID.VSHPROPID_ProjectDir, out temp);
                    ProjectDir = temp.ToString();
                    //root = projectFullName.Substring(0, projectFullName.LastIndexOf('\\') + 1);
                    pHierNew.GetProperty(VSConstants.VSITEMID_ROOT, (int)__VSHPROPID.VSHPROPID_ProjectName, out temp);
                    ProjectName = temp.ToString();
                    ViewsFolderId = itemidNew;
                    ViewsFolderName = itemName.ToString();
                    GlobalServices.SelectionService.SetCmdUIContext(ContextCookie, 1);
                }
            }
            else
                GlobalServices.SelectionService.SetCmdUIContext(ContextCookie, 0);
            return VSConstants.S_OK;

        }
        public void AddNewItemFromVsTemplate(string templateName, string language, string name)
        {
            if (name == null)
                throw new ArgumentException("name");
            int activeProject = GetActiveProject();
            ProjectItems parent = GlobalServices.DTE.Solution.Projects.Item(activeProject).ProjectItems;
            if (parent == null)
                throw new ArgumentException("project");

            EnvDTE80.Solution2 sol = GlobalServices.DTE.Solution as EnvDTE80.Solution2;
            string filename = sol.GetProjectItemTemplate(templateName, language);
            parent.AddFromTemplate(filename, name);
        }
        public void AddFromFile(string fileName,string folder,string itemName)
        {
            ProjectItems parent = GlobalServices.DTE.Solution.Projects.Item(GetActiveProject()).ProjectItems;
            SearchFolder(folder, parent);
            parent.AddFromTemplate(fileName, itemName);
        }
        public List<Assembly> GetReferences()
        {
            Project project = GlobalServices.DTE.Solution.Projects.Item(1);
            List<Assembly> list = new List<Assembly>();
            if (project.Object is VSLangProj.VSProject)
            {
                VSLangProj.VSProject vsproject = (VSLangProj.VSProject)project.Object;
                foreach (VSLangProj.Reference reference in vsproject.References)
                {
                    try
                    {
                        if (reference.StrongName)
                            //System.Configuration, Version=2.0.0.0,
                            //Culture=neutral, PublicKeyToken=B03F5F7F11D50A3A
                            list.Add(Assembly.Load(

                                reference.Identity +
                                ", Version=" + reference.Version +
                                ", Culture=" + (string.IsNullOrEmpty(reference.Culture) ?
                                "neutral" : reference.Culture) +
                                ", PublicKeyToken=" + reference.PublicKeyToken));
                        else
                            list.Add(Assembly.Load(reference.Path));
                    }
                    catch (System.IO.FileLoadException ex)
                    {
                    }

                }
            }
            else if (project.Object is VsWebSite.VSWebSite)
            {
                VsWebSite.VSWebSite vswebsite = (VsWebSite.VSWebSite)project.Object;
                foreach (VsWebSite.AssemblyReference reference in vswebsite.References)
                    list.Add(Assembly.Load(reference.StrongName));
            }
            return list;

        }
        public void GetTemplateBlocks(string template)
        {
            var nodes = handler.ParseTemplate(template, new NDjango.TypeResolver.DefaultTypeResolver());
            List<string> result;
            foreach (INode node in nodes)
            {
                //if (node.NodeType == NodeType.BlockName)
                //    result.AddRange(node.Context.BodyContext)
 
                //while (node.Nodes != null)
                //{
                //    foreach (KeyValuePair<string,INode> child in node.Nodes)
                //    {
                //    }
                //}
            }
            //var snapshot = handler.GetSnapshot(template);
            //StreamReader reader = new StreamReader(template);
            //ITextBuffer buffer = GlobalServices.TextBufferFactory.CreateTextBuffer(reader,GlobalServices.ContentRegistryService.GetContentType("html"));
            //NodeProvider provider = broker.GetNodeProvider(buffer);
            //List<DesignerNode> designer_nodes = nodes.Aggregate(
            //                new List<DesignerNode>(),
            //                (list, node) => { list.Add(new DesignerNode(provider, null, snapshot, (INode)node)); return list; }
            //            );
            #region alternative recursive function
            //List<string> blockNames = new List<string>();
            //foreach (var node in nodes)
            //{
            //    switch (((INode)node).NodeType)
            //    {
            //        case NodeType.ParsingContext:
            //            break;
            //        case NodeType.BlockName:
            //            blockNames.Add(node.Token.TextToken.Value);
            //            break;
            //        case NodeType.TemplateName:
            //            string s = node.Token.TextToken.Value;
            //            break;
            //        default:
            //            if (node.Token != null && node.Token.IsBlock)
            //            {
            //                blockNames.Add(node.Token.TextToken.Value);
            //            }
            //            break;

            //    }
            //}
            #endregion
        }
        private int GetActiveProject()
        {
            int i = 1;
            foreach (Project p in GlobalServices.DTE.Solution.Projects)
                if (String.Compare(p.Name, ProjectName) == 0)
                    return i;
                else
                    i++;
            return 1;
        }
        private ProjectItems SearchFolder(string folder, ProjectItems parent)
        {
            if (String.IsNullOrEmpty(folder))
                return parent;
            foreach (ProjectItem pi in parent)
            {
                if (folder.StartsWith(pi.Name, true, CultureInfo.CurrentCulture))
                {
                    if (String.Compare(folder, pi.Name, true) != 0)
                    {
                        folder = folder.Remove(0, pi.Name.Length + 1);
                        if (folder.EndsWith("\\"))
                            folder = folder.Remove(folder.Length - 1, 1);
                        SearchFolder(folder, pi.ProjectItems);
                    }
                    else
                    {
                        return pi.ProjectItems;
                    }
                }
            }
            return parent;

        }
        private ProjectHandler CreateHandler()
        {
            ProjectHandler handler = null;
            broker = new NodeProviderBroker();
            TemplateDirectory templatesDir = new TemplateDirectory();
            templatesDir.selectionTracker = GlobalServices.SelectionService;
            List<NDjango.Tag> tags = new List<NDjango.Tag>() { };
            List<NDjango.Filter> filters = new List<NDjango.Filter>() { };
            IntPtr ppHier;
            uint pitemid;
            IVsMultiItemSelect ppMIS;
            IntPtr ppSC;
            object directory = "";
            if (ErrorHandler.Succeeded(GlobalServices.SelectionService.GetCurrentSelection(out ppHier, out pitemid, out ppMIS, out ppSC)))
            {
                handler = new ProjectHandler(broker, templatesDir, tags, filters, directory.ToString());
                ProjectDir = directory.ToString();
            }
            return handler;

        }
        private uint RegisterContext()
        {
            uint retVal;
            Guid uiContext = GuidList.UICONTEXT_ViewsSelected;
            NDjango.Designer.GlobalServices.SelectionService.GetCmdUIContextCookie(ref uiContext, out retVal);
            return retVal;

        }

    }
}
