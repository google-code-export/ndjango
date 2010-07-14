using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Windows.Forms;
using Microsoft.VisualStudio.Shell.Interop;
using NewViewGenerator.Interaction;
namespace NewViewGenerator
{
    public partial class AddViewDlg : Form
    {
        ProjectManager manager;
        public AddViewDlg()
        {
            manager = new ProjectManager();
            InitializeComponent();
            FillModelList();
            FillBaseTemplates();
        }

        private void FillModelList()
        {
            try
            {
                List<Assembly> assmlist = manager.GetReferences();
                foreach (Assembly assm in assmlist)
                {
                    var types= assm.GetTypes();
                    foreach (Type t in types)
                        comboModel.Items.Add(t.FullName);
                }
            }
            catch (ReflectionTypeLoadException ex)
            {
                Debug.WriteLine(ex);
            }

        }
        private void FillBaseTemplates()
        {
            IEnumerable<string> allTemplates =  manager.handler.TemplateManager.GetTemplates("");
            IEnumerable<string> recentTemplates = manager.handler.TemplateManager.Recent5Templates;
            foreach(string item in recentTemplates)
                checkedListBase.Items.Add(item);
            foreach (string item in allTemplates)
                if (!checkedListBase.Items.Contains(item))
                    checkedListBase.Items.Add(item);
        }
        private void btnAdd_Click(object sender, EventArgs e)
        {
            int rootLen = manager.ProjectDir.Length;
            string folderName = manager.ViewsFolderName.Substring(rootLen + 1, manager.ViewsFolderName.Length - rootLen - 1);
            string itemName = tbViewName.Text + ".django";
            string templateFile = Path.GetTempFileName();
            StreamWriter sw = new StreamWriter(templateFile);
            if (checkedListBase.CheckedItems.Count > 0)
            {
                foreach (string item in checkedListBase.CheckedItems)
                {
                    sw.WriteLine("{% extends \"" + item + "\" %}");
                    manager.handler.TemplateManager.RegisterInserted(item);
                }
            }
            if (comboModel.SelectedItem != null)
                sw.WriteLine("{% model " + comboModel.SelectedItem.ToString() + " %}");
            sw.WriteLine("{% block A %}");
            sw.WriteLine("{% endblock %}");
            sw.Close();
            manager.GetTemplateBlocks(templateFile);
            //VSADDRESULT[] pResult = null;
            //string[] filesToOpen = new string[1];
            //filesToOpen[0] = templateFile;
            
            //ProjectData.curHier.AddItem(ProjectData.viewsFolderId, VSADDITEMOPERATION.VSADDITEMOP_CLONEFILE,
            //    itemName, 1, filesToOpen, IntPtr.Zero, pResult);

            //ProjectData.AddNewItemFromVsTemplate("NDjangoTemplate2010.zip", "CSharp/Web", itemName);
            manager.AddFromFile(templateFile,folderName,itemName);
            File.Delete(templateFile);
            this.Close();
        }

        private void chkInheritance_CheckStateChanged(object sender, EventArgs e)
        {
            checkedListBase.Enabled = checkedListBlocks.Enabled = chkInheritance.Checked;
        }
    }
}
