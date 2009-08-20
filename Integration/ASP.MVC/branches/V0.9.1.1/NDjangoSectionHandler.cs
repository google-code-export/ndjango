using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;

namespace NDjango.ASPMVCIntegration
{
    class NDjangoSectionHandler : ConfigurationSection
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SectionHandler"/> class.
        /// </summary>
        public NDjangoSectionHandler() { }

        [ConfigurationProperty("", IsDefaultCollection = true)]
        public NameValueConfigurationCollection NDJangoSectionCollection
        {
            get
            {
                return (NameValueConfigurationCollection)base[""];
            }

        }
    }
}
