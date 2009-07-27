using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace ASP.Net_Simple
{
    public partial class About : System.Web.UI.Page
    {
        public string hello = Convert.ToString(new Random().Next());
        protected void Page_Load(object sender, EventArgs e)
        {

        }
    }
}
