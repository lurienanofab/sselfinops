using LNF.Data;
using LNF.Repository;
using LNF.Web;
using System;
using System.Data;
using System.Web.UI.WebControls;

namespace sselFinOps.AppCode
{
    public abstract class ReportPage : LNF.Web.Content.LNFPage
    {
        public override ClientPrivilege AuthTypes
        {
            get { return ClientPrivilege.Administrator; }
        }

        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);

            if (!CurrentUser.HasPriv(AuthTypes))
            {
                ContextBase.Session.Abandon();
                Response.Redirect(Provider.LoginUrl() + "?Action=Exit");
            }
        }

        protected virtual void Back()
        {
            ContextBase.RemoveCacheData(); //remove anything left in cache
            Response.Redirect("~", !IsAsync);
        }

        protected void BackButton_Click(object sender, EventArgs e)
        {
            Back();
        }

        protected void FillYearSelect(DropDownList ddl, int startYear = 2003)
        {
            WebUtility.BindYearData(ddl, startYear);
        }

        public virtual bool ShowButton
        {
            get { return true; }
        }
    }
}
