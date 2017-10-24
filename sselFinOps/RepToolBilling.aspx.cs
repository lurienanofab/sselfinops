using System;
using System.Web.UI;

namespace sselFinOps
{
    public partial class RepToolBilling : Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            Response.Redirect("/finops/report/tool-billing?ReturnTo=/sselfinops");
        }
    }
}