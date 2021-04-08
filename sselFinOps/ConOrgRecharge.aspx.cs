using LNF.Billing;
using LNF.Data;
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
            if (string.IsNullOrEmpty(Request.QueryString["show_all_accounts"]))
                return false;
            else
                return Request.QueryString["show_all_accounts"] == "yes";
        }

        protected void LoadOrganizations()
        {
            var query = Provider.Data.Org.GetActiveOrgs().OrderBy(x => x.OrgName).ToArray();
            ddlOrg.DataSource = query;
            ddlOrg.DataBind();
        }

        protected void LoadAccounts()
        {
            IAccount[] query;

            if (ShowAllAccounts())
                query = Provider.Data.Account.GetAccounts().ToArray();
            else
                query = Provider.Data.Account.GetActiveAccounts().ToArray();

            ddlAccount.DataSource = query.Where(x => x.ChargeTypeID == 5).Select(CreateAccountSelectItem);
            ddlAccount.DataBind();
        }

        protected void LoadOrgRechargeItems()
        {
            var util = new OrgRechargeUtility(DateTime.Now, Provider);
            IOrgRecharge[] query = Provider.Billing.OrgRecharge.GetActiveOrgRecharges().ToArray();
            rptOrgRecharge.DataSource = query.Select(CreateRepeaterItem);
            rptOrgRecharge.DataBind();
        }

        protected void Row_Command(object sender, CommandEventArgs e)
        {
            switch (e.CommandName)
            {
                case "disable":
                    int id = Convert.ToInt32(e.CommandArgument);
                    var util = new OrgRechargeUtility(DateTime.Now, Provider);
                    util.Disable(id);
                    LoadOrgRechargeItems();
                    break;
            }
        }

        protected object CreateRepeaterItem(IOrgRecharge x)
        {
            var acct = Provider.Data.Account.GetAccount(x.AccountID);
            AccountChartFields fields = new AccountChartFields(acct);

            return new
            {
                x.OrgRechargeID,
                x.OrgName,
                x.AccountName,
                fields.ShortCode,
                fields.Project,
                EnableDate = x.EnableDate.ToString("M/d/yyyy h:mm:ss tt"),
                AccountCssClass = x.AccountActive ? "active" : "inactive"
            };
        }

        protected object CreateAccountSelectItem(IAccount x)
        {
            return new
            {
                x.AccountID,
                AccountName = x.NameWithShortCode
            };
        }

        protected void btnAddOrUpdate_Click(object sender, EventArgs e)
        {
            int orgId = Convert.ToInt32(ddlOrg.SelectedValue);
            int accountId = Convert.ToInt32(ddlAccount.SelectedValue);
            IOrg org = Provider.Data.Org.GetOrg(orgId);
            IAccount acct = Provider.Data.Account.GetAccount(accountId);
            if (org != null && acct != null)
            {
                var util = new OrgRechargeUtility(DateTime.Now, Provider);
                util.Enable(org, acct);
            }
            LoadOrgRechargeItems();
        }
    }
}