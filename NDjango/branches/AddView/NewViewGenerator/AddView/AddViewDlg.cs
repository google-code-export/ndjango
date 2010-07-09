using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Microsoft.VisualStudio.Shell.Interop;
using NewViewGenerator.Interaction;
namespace NewViewGenerator
{
    public partial class AddViewDlg : Form
    {
        private SelectionHandler handler;
        public AddViewDlg(SelectionHandler _handler)
        {
            handler = _handler;
            InitializeComponent();
        }

        private void btnAdd_Click(object sender, EventArgs e)
        {
            VSADDRESULT[] pResult = null;
            string itemName = handler.root + tbViewName.Text + ".django";
            string[] filesToOpen = new string[1];
            filesToOpen[0] = "C:\\base.django";
            handler.curHier.AddItem(handler.viewsfolderId, VSADDITEMOPERATION.VSADDITEMOP_CLONEFILE,
                itemName, 1, filesToOpen, IntPtr.Zero, pResult);
        }
    }
}
