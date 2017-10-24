using System;
using System.Web.UI;

namespace sselFinOps
{
    public partial class ConRemoteProcessing : Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            Response.Redirect("/finops/configuration/remote-processing?ReturnTo=/sselfinops");
        }
    }
}