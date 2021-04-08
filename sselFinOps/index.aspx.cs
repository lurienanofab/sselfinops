using LNF.Data;
using LNF.Web;
using LNF.Web.Content;
using System;
using System.Web.UI.WebControls;

namespace sselFinOps
{
    public partial class Index : LNFPage
    {
        public override ClientPrivilege AuthTypes
        {
            get { return 0; }
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!Page.IsPostBack)
            {
                if (Request.QueryString["AbandonSession"] == "1")
                {
                    Session.Abandon();
                    Response.Redirect("~");
                }

                // check to see if session is valid
                if (!string.IsNullOrEmpty(Request.QueryString["ClientID"])) //probably coming from sselonline
                {
                    if (int.TryParse(Request.QueryString["ClientID"].Trim(), out int clientId))
                    {
                        if (CurrentUser.ClientID != clientId)
                        {
                            Session.Abandon();
                            Response.Redirect("~");
                        }
                    }
                }

                lblName.Text = CurrentUser.DisplayName;
            }
        }

        protected void Button_Command(object sender, CommandEventArgs e)
        {
            if (e.CommandName == "navigate")
            {
                string location = e.CommandArgument.ToString();
                Response.Redirect(location);
            }
        }

        protected void BtnLogout_Click(object sender, EventArgs e)
        {
            ContextBase.RemoveCacheData(); //remove anything left in cache
            Session.Abandon();
            Response.Redirect("/sselonline/Blank.aspx");
        }
    }
}