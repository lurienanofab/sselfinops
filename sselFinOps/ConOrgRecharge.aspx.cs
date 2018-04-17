using LNF.Billing;
using LNF.Data;
using LNF.Repository;
using LNF.Repository.Billing;
using LNF.Repository.Data;
using sselFinOps.AppCode;
using System;
using System.Linq;
using System.Web;
using System.Web.UI.WebControls;

namespace sselFinOps
{
    public partial class ConOrgRecharge : ReportPage
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            hidAjaxUrl.Value = VirtualPathUtility.ToAbsolute("~/Ajax/OrgRecharge.ashx");
            if (!Page.IsPostBack)
            {
                LoadOrgRechargeItems();
                LoadOrganizations();
                LoadAccounts();
            }
        }

        protected bool ShowAllAccounts()
        {
            if (String.IsNullOrEmpty(Request.QueryString["show_all_accounts"]))
                return false;
            else
                return Request.QueryString["show_all_accounts"] == "yes";
        }

        protected void LoadOrganizations()
        {
            var query = DA.Current.Query<Org>().Where(x => x.Active).OrderBy(x => x.OrgName).ToArray();
            ddlOrg.DataSource = query;
            ddlOrg.DataBind();
        }

        protected void LoadAccounts()
        {
            Account[] query;

            if (ShowAllAccounts())
                query = DA.Current.Query<Account>().OrderBy(x => x.Name).ToArray();
            else
                query = AccountManager.ActiveAccounts().ToArray();

            ddlAccount.DataSource = query.Where(x => x.Org.OrgType.ChargeType.ChargeTypeID == 5).Select(CreateAccountSelectItem);
            ddlAccount.DataBind();
        }

        protected void LoadOrgRechargeItems()
        {
            OrgRecharge[] query = DA.Current.Query<OrgRecharge>().Where(x => x.DisableDate == null).ToArray();
            rptOrgRecharge.DataSource = query.Select(CreateRepeaterItem);
            rptOrgRecharge.DataBind();
        }

        protected void Row_Command(object sender, CommandEventArgs e)
        {
            switch (e.CommandName)
            {
                case "disable":
                    int id = Convert.ToInt32(e.CommandArgument);
                    OrgRecharge item = DA.Current.Single<OrgRecharge>(id);
                    OrgRechargeUtility.Disable(item);
                    LoadOrgRechargeItems();
                    break;
            }
        }

        protected object CreateRepeaterItem(OrgRecharge x)
        {
            AccountChartFields fields = new AccountChartFields(x.Account);
            return new
            {
                OrgRechargeID = x.OrgRechargeID,
                OrgName = x.Org.OrgName,
                AccountName = x.Account.Name,
                ShortCode = fields.ShortCode,
                Project = fields.Project,
                EnableDate = x.EnableDate.ToString("M/d/yyyy h:mm:ss tt"),
                AccountCssClass = x.Account.Active ? "active" : "inactive"
            };
        }

        protected object CreateAccountSelectItem(Account x)
        {
            return new
            {
                AccountID = x.AccountID,
                AccountName = x.GetNameWithShortCode()
            };
        }

        protected void btnAddOrUpdate_Click(object sender, EventArgs e)
        {
            int orgId = Convert.ToInt32(ddlOrg.SelectedValue);
            int accountId = Convert.ToInt32(ddlAccount.SelectedValue);
            Org org = DA.Current.Single<Org>(orgId);
            Account acct = DA.Current.Single<Account>(accountId);
            if (org != null && acct != null)
                OrgRechargeUtility.Enable(org, acct);
            LoadOrgRechargeItems();
        }
    }
}