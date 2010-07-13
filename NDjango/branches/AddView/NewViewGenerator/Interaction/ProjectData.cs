using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using EnvDTE;
using EnvDTE80;
using NDjango.Designer.Parsing;


namespace NewViewGenerator.Interaction
{
    public static class ProjectData
    {
        public static IVsMonitorSelection SelectionService;
        public static DTE dte;
        public static IVsProject curHier;
        public static string projectDir;
        public static string projectName;
        public static string viewsFolderName;
        public static uint viewsFolderId;
        public static ProjectHandler handler;

        public static void SetProjectHandler()
        {
            NodeProviderBroker broker = new NodeProviderBroker();
            TemplateDirectory templatesDir = new TemplateDirectory();
            templatesDir.selectionTracker = ProjectData.SelectionService;

            List<NDjango.Tag> tags = new List<NDjango.Tag>() { };
            List<NDjango.Filter> filters = new List<NDjango.Filter>() { };
            IntPtr ppHier;
            uint pitemid;
            IVsMultiItemSelect ppMIS;
            IntPtr ppSC;
            object directory = "";
            if (ErrorHandler.Succeeded(ProjectData.SelectionService.GetCurrentSelection(out ppHier, out pitemid, out ppMIS, out ppSC)))
            {
                handler = new ProjectHandler(broker, templatesDir, tags, filters, directory.ToString());
            }


        }
        public static void AddNewItemFromVsTemplate(string templateName, string language, string name)
        {
            if (name == null)
                throw new ArgumentException("name");
            int activeProject = GetActiveProject();
            ProjectItems parent = dte.Solution.Projects.Item(activeProject).ProjectItems;
            if (parent == null)
                throw new ArgumentException("project");

            Solution2 sol = dte.Solution as Solution2;
            string filename = sol.GetProjectItemTemplate(templateName, language);
            parent.AddFromTemplate(filename, name);
        }
        private static void AddFromFile(string fileName)
        {
            ProjectItems parent =  dte.Solution.Projects.Item(GetActiveProject()).ProjectItems;
            parent.AddFromFile(fileName);
        }
        public static void GetTemplateBlocks(string baseTemplate)
        {
            var nodes =  handler.ParseTemplate(projectDir + "\\" + baseTemplate, new NDjango.TypeResolver.DefaultTypeResolver());
        }
        public static int GetActiveProject()
        {
            int i = 1;
            foreach (Project p in dte.Solution.Projects)
                if (String.Compare(p.Name, projectName) == 0)
                    return i;
                else
                    i++;
            return 1;
        }
        public static List<Assembly> GetReferences()
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
    }
}
