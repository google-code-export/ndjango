using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Reflection;
using System.Windows.Forms;
using Microsoft.VisualStudio.Shell.Interop;
using NewViewGenerator.Interaction;
namespace NewViewGenerator
{
    public partial class AddViewDlg : Form
    {
        public AddViewDlg()
        {
            InitializeComponent();
            ProjectData.SetProjectHandler();
            FillModelList();
            FillBaseTemplates();
        }

        private void FillModelList()
        {
            try
            {
                List<Assembly> assmlist = ProjectData.GetReferences();
                foreach (Assembly assm in assmlist)
                {
                    var types= assm.GetTypes();
                    int i = 0;
                    int j = types.Length;
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
            IEnumerable<string> allTemplates =  ProjectData.handler.TemplateManager.GetTemplates("");
            IEnumerable<string> recentTemplates = ProjectData.handler.TemplateManager.Recent5Templates;
            foreach(string item in recentTemplates)
                checkedListBase.Items.Add(item);
            foreach (string item in allTemplates)
                if (!checkedListBase.Items.Contains(item))
                    checkedListBase.Items.Add(item);
        }
        private void btnAdd_Click(object sender, EventArgs e)
        {
            //VSADDRESULT[] pResult = null;
            //string[] filesToOpen = new string[1];
            //filesToOpen[0] = "General\\Text File";
            //handler.curHier.AddItem(handler.viewsfolderId, VSADDITEMOPERATION.VSADDITEMOP_CLONEFILE,
            //    itemName, 1, filesToOpen, IntPtr.Zero, pResult);
            foreach (string item in checkedListBase.CheckedItems)
            {
                ProjectData.GetTemplateBlocks(item);
                ProjectData.handler.TemplateManager.RegisterInserted(item);
            }
            string itemName = ProjectData.projectDir + "\\" + ProjectData.viewsFolderName + "\\" + tbViewName.Text + ".django";
            //ProjectData.AddNewItemFromVsTemplate("NDjangoTemplate2010.zip", "CSharp/Web", itemName);
            Close();
        }

        private void chkInheritance_CheckStateChanged(object sender, EventArgs e)
        {
            checkedListBase.Enabled = lblBaseTemplate.Enabled = lblBlocks.Visible = 
                checkedListBlocks.Visible = chkInheritance.Checked;
        }
    }
}
