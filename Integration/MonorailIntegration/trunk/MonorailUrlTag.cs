using System;
using System.Collections.Generic;
using System.Text;

namespace NDjango.MonorailIntegration
{
    /// <summary>
    /// Implementation of the django {% url %} tag.
    /// 
    /// This tag will take a url in a String.Format format, and apply the 
    /// supplied parameters to it.
    /// </summary>
    internal class MonorailUrlTag : NDjango.Tags.Abstract.UrlTag
    {
        private string rootDir;

        /// <summary>
        /// Initializes a new instance of the <see cref="MonorailUrlTag"/> class.
        /// </summary>
        /// <param name="rootDir">The application virtual path.</param>
        internal MonorailUrlTag(string rootDir)
        {
            // trim to guarantee it's not there, then add to not do it every time
            this.rootDir = rootDir.TrimEnd('/') + '/';
        }

        /// <summary>
        /// Generates the URL.
        /// </summary>
        /// <param name="pathTemplate">The path template.</param>
        /// <param name="parameters">The parameters.</param>
        /// <param name="context">The context.</param>
        /// <returns></returns>
        public override string GenerateUrl(string pathTemplate, string[] parameters, NDjango.Interfaces.IContext context)
        {
            return rootDir + String.Format(pathTemplate.Trim('/'), parameters);
        }
    }
}
