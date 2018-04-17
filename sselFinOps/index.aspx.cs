using LNF.Cache;
using LNF.Models.Data;
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
                        if (CacheManager.Current.CurrentUser.ClientID != clientId)
                        {
                            Session.Abandon();
                            Response.Redirect("~");
                        }
                    }
                }

                lblName.Text = CacheManager.Current.CurrentUser.DisplayName;
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

        protected void btnLogout_Click(object sender, EventArgs e)
        {
            CacheManager.Current.RemoveCacheData(); //remove anything left in cache
            CacheManager.Current.AbandonSession();
            Response.Redirect("/sselonline/Blank.aspx");
        }
    }
}