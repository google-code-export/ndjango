using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NDjango.MonorailIntegration
{
    /// <summary>
    /// Class containing constants for use in the context dictionary and in the template.
    /// </summary>
    public static class TemplateKeys
    {
        public const String Context = "context";
        public const String Request = "request";
        public const String Response = "response";
        public const String Session = "session";
        public const String Controller = "controller";
    }
}
