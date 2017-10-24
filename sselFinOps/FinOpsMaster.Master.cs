using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace sselFinOps
{
    public partial class FinOpsMaster : LNF.Web.Content.LNFMasterPage
    {
        public override bool ShowMenu
        {
            get { return false; }
        }

        protected void Page_Load(object sender, EventArgs e)
        {

        }
    }
}