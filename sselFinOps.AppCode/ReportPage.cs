using LNF.Cache;
using LNF.CommonTools;
using LNF.Models.Data;
using LNF.Web;
using System;
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

            if (!CacheManager.Current.CurrentUser.HasPriv(AuthTypes))
            {
                CacheManager.Current.AbandonSession();
                Response.Redirect(CacheManager.Current.GetLoginUrl() + "?Action=Exit");
            }
        }

        protected virtual void Back()
        {
            //CacheManager.Current.Updated(false);
            CacheManager.Current.RemoveCacheData(); //remove anything left in cache
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
