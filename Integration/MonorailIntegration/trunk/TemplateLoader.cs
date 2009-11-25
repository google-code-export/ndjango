using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NDjango.Interfaces;
using System.IO;
using System.Web;

namespace NDjango.MonorailIntegration
{
    /// <summary>
    /// Class, which implements ITemplateLoader interface - used by NDjango engine to load templates 
    /// and check - whether template was updated.
    /// </summary>
    internal class TemplateLoader : ITemplateLoader
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TemplateLoader"/> class.
        /// Sets initial directory, where templates are.
        /// </summary>
        internal TemplateLoader()
        {
            rootDir = HttpRuntime.AppDomainAppPath + "Views\\";
        }

        /// <summary>
        /// Views directory - templates are stored here.
        /// </summary>
        private string rootDir;

        #region ITemplateLoader Members

        /// <summary>
        /// Gets the template.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <returns></returns>
        public TextReader GetTemplate(string name)
        {
            return File.OpenText(Path.Combine(rootDir, name));
        }

        /// <summary>
        /// Determines whether the specified name is updated.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="timestamp">The timestamp.</param>
        /// <returns>
        /// 	<c>true</c> if the specified name is updated; otherwise, <c>false</c>.
        /// </returns>
        public bool IsUpdated(string name, System.DateTime timestamp)
        {
            return File.GetLastWriteTime(Path.Combine(rootDir, name)) > timestamp;
        }

        #endregion
    }
}
