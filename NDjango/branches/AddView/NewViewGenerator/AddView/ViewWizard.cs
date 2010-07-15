using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Utilities;
using NDjango;
using NDjango.Interfaces;
using NDjango.Designer;
using NDjango.Designer.Parsing;
using EnvDTE;

namespace NewViewGenerator
{
    public class ViewWizard: IVsSelectionEvents
    {
        public ViewWizard()
        {
            SelectionService.AdviseSelectionEvents(this, out SelectionCookie);
            ContextCookie = RegisterContext();
            uint pitemid;
            IVsMultiItemSelect ppMIS;
            IntPtr ppHier,ppSC;
            object directory = "";
            if (ErrorHandler.Succeeded(SelectionService.GetCurrentSelection(out ppHier, out pitemid, out ppMIS, out ppSC)))
            {
                try
                {
                    Hierarchy = (IVsHierarchy)Marshal.GetObjectForIUnknown(ppHier);
                    Hierarchy.GetProperty(VSConstants.VSITEMID_ROOT,(int)__VSHPROPID.VSHPROPID_ProjectDir, out directory);
                    ProjectDir = directory.ToString();
                }
                finally
                {
                    Marshal.Release(ppHier);
                }
            }
            template_loader = new TemplateLoader(ProjectDir);
            parser = InitializeParser();
            templatesDir = new TemplateDirectory();
            templatesDir.selectionTracker = SelectionService;
        }
        
        IVsMonitorSelection SelectionService = (IVsMonitorSelection)Package.GetGlobalService(typeof(SVsShellMonitorSelection));
        DTE  dte = (DTE)Package.GetGlobalService(typeof(DTE));

        public string ProjectDir { get; private set; }
        public string ProjectName { get; private set; }
        public string ViewsFolderName { get; private set; }
        uint ContextCookie;
        uint SelectionCookie;
        INode blockNameNode = null;
        ProjectItems ViewsFolder;
        IVsHierarchy Hierarchy;
        ITemplateManager parser;
        TemplateDirectory templatesDir; 

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
                    Hierarchy = pHierNew;
                    pHierNew.GetProperty(VSConstants.VSITEMID_ROOT,(int)__VSHPROPID.VSHPROPID_ProjectDir, out temp);
                    ProjectDir = temp.ToString();
                    //root = projectFullName.Substring(0, projectFullName.LastIndexOf('\\') + 1);
                    pHierNew.GetProperty(VSConstants.VSITEMID_ROOT, (int)__VSHPROPID.VSHPROPID_ProjectName, out temp);
                    ProjectName = temp.ToString();
                    ViewsFolderName = itemName.ToString();
                    SelectionService.SetCmdUIContext(ContextCookie, 1);
                }
            }
            else
                SelectionService.SetCmdUIContext(ContextCookie, 0);
            return VSConstants.S_OK;

        }
        public void AddNewItemFromVsTemplate(string templateName, string language, string name)
        {
            if (name == null)
                throw new ArgumentException("name");
            int activeProject = GetActiveProject();
            ProjectItems parent = dte.Solution.Projects.Item(activeProject).ProjectItems;
            if (parent == null)
                throw new ArgumentException("project");

            EnvDTE80.Solution2 sol = dte.Solution as EnvDTE80.Solution2;
            string filename = sol.GetProjectItemTemplate(templateName, language);
            parent.AddFromTemplate(filename, name);
        }
        public void AddFromFile(string fileName,string folder,string itemName)
        {
            ViewsFolder = dte.Solution.Projects.Item(GetActiveProject()).ProjectItems; ;//default ViewsFolder is  the root of the project
            SearchFolder(folder, ViewsFolder);//find the real folder the new view must be inserted to
            ViewsFolder.AddFromTemplate(fileName, itemName);
        }
        public List<Assembly> GetReferences()
        {
            Project project = dte.Solution.Projects.Item(1);
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
        public IEnumerable<string> GetTemplates(string root)
        {
            return templatesDir.GetTemplates(root);
        }
        public IEnumerable<string> Recent5Templates
        {
            get { return templatesDir.Recent5Templates; }
        }
        public void RegisterInserted(string inserted)
        {
            templatesDir.RegisterInserted(inserted);
        }
        public List<string> GetTemplateBlocks(string template)
        {
            var nodes = parser.GetTemplate(template, new NDjango.TypeResolver.DefaultTypeResolver()).Nodes;
            List<string> blocks = new List<string>();
            foreach (INode node in nodes)
            {
                if (node.NodeType == NodeType.BlockName)
                    break;
                else
                    for (int i = 0; i < node.Nodes.Count; i++ )
                        FindBlockNameNode(node.Nodes.Values.ElementAt(i));
            }
            if (blockNameNode != null)
            {
                var completion_provider = blockNameNode as ICompletionValuesProvider;
                if (completion_provider != null)
                blocks.AddRange(completion_provider.Values); 

            }
            return blocks;
        }
        private int GetActiveProject()
        {
            int i = 1;
            foreach (Project p in dte.Solution.Projects)
                if (String.Compare(p.Name, ProjectName) == 0)
                    return i;
                else
                    i++;
            return 1;
        }
        private void SearchFolder(string folder, ProjectItems parent)
        {
            if (String.IsNullOrEmpty(folder))
                return;
            foreach (ProjectItem pi in parent)
            {
                if (folder.StartsWith(pi.Name, true, System.Globalization.CultureInfo.CurrentCulture))
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
                        ViewsFolder =  pi.ProjectItems;
                    }
                }
            }

        }
        private uint RegisterContext()
        {
            uint retVal;
            Guid uiContext = GuidList.UICONTEXT_ViewsSelected;
            SelectionService.GetCmdUIContextCookie(ref uiContext, out retVal);
            return retVal;

        }
        void FindBlockNameNode(IEnumerable<INode> nodes)
        {
            foreach (INode subnode in nodes)
            {
                if (subnode.NodeType == NodeType.BlockName)
                {
                    blockNameNode = subnode;
                    break;
                }
                else
                    for (int i = 0; i < subnode.Nodes.Values.Count; i++)
                    {
                        FindBlockNameNode(subnode.Nodes.Values.ElementAt(i));
                    }
            }
        }
        
        TemplateLoader template_loader;
        private List<Tag> tags = new List<Tag>();
        private List<Filter> filters = new List<Filter>();

        private ITemplateManager InitializeParser()
        {

            TemplateManagerProvider provider = new TemplateManagerProvider();
            return provider
                    .WithTags(tags)
                    .WithFilters(filters)
                    .WithSetting(NDjango.Constants.EXCEPTION_IF_ERROR, false)
                    .WithLoader(template_loader)
                    .GetNewManager();

        }

        

        

    }
}
