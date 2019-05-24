using LNF;
using LNF.Billing;
using LNF.Cache;
using LNF.Data;
using LNF.Models.Billing;
using LNF.Models.Data;
using LNF.Web;
using StructureMap.Attributes;
using System;
using System.Web.UI.WebControls;

namespace sselFinOps.AppCode
{
    public abstract class ReportPage : LNF.Web.Content.LNFPage
    {
        [SetterProperty]
        public IAccountManager AccountManager { get; set; }

        [SetterProperty]
        public IBillingTypeManager BillingTypeManager { get; set; }

        public override ClientPrivilege AuthTypes
        {
            get { return ClientPrivilege.Administrator; }
        }

        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);

            ServiceProvider.Current.BuildUp(this);

            if (!CurrentUser.HasPriv(AuthTypes))
            {
                ContextBase.Session.Abandon();
                Response.Redirect(ServiceProvider.Current.Context.LoginUrl + "?Action=Exit");
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
