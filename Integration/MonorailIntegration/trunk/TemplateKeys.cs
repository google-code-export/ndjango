using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NDjango.MonorailIntegration
{
    public sealed class TemplateKeys
    {
        private TemplateKeys() { throw new InvalidOperationException(); }

        public static readonly String ChildContent = "childContent";
        public static readonly String Context = "context";
        public static readonly String Request = "request";
        public static readonly String Response = "response";
        public static readonly String Session = "session";
        public static readonly String Controller = "controller";
        public static readonly String SiteRoot = "siteroot";
    }
}
