using System;
using System.Web.UI;

namespace sselFinOps
{
    public partial class ConHolidays : Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            Response.Redirect("/finops/configuration/holidays?ReturnTo=/sselfinops");
        }
    }
}