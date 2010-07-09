using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio;
using System.ComponentModel.Design;
using System.Xml;
using System.Runtime.InteropServices;
using OleConstants = Microsoft.VisualStudio.OLE.Interop.Constants;

namespace NewViewGenerator
{
    public class AddViewCommand : OleMenuCommand
    {
        public AddViewCommand()
            : base(Execute, new CommandID(GuidList.guidNewViewGeneratorCmdSet, (int)GuidList.cmdidNewViewGenerator))
        {
            BeforeQueryStatus += new EventHandler(QueryStatus);
        }

        private const string command_text = "Add Django View";
        void QueryStatus(object sender, EventArgs e)
        {
        }

        private static void Execute(object sender, EventArgs e)
        {
        }
    }

}
