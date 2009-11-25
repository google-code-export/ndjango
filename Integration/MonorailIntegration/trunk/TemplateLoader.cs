using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NDjango.Interfaces;
using System.IO;
using System.Web;

namespace NDjango.MonorailIntegration
{
    internal class TemplateLoader : ITemplateLoader
    {
        internal TemplateLoader()
        {
            rootDir = HttpRuntime.AppDomainAppPath;
        }

        private string rootDir;

        #region ITemplateLoader Members

        #region ITemplateLoader Members

        public TextReader GetTemplate(string name)
        {
            return File.OpenText(Path.Combine(rootDir, "Views\\" + name));
        }

        public bool IsUpdated(string name, System.DateTime timestamp)
        {
            return File.GetLastWriteTime(Path.Combine(rootDir, "Views\\" + name)) > timestamp;
        }

        #endregion

        #endregion
    }
}
