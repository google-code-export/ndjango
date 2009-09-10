using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Bistro.Controllers.OutputHandling;
using Bistro.Controllers.Descriptor;

namespace NDjango.BistroIntegration
{
    [Bind("?", ControllerBindType=BindType.After)]
    [TemplateMapping(".django")]
    public class DjangoController : RenderingController
    {
        protected override Type EngineType
        {
            get { return typeof(DjangoEngine); }
        }
    }
}
