using LNF.Data;
using LNF.Impl.Repository.Data;
using sselFinOps.AppCode;
using System;
using System.Data;
using System.Linq;

namespace sselFinOps
{
    public partial class MscBillingChecks : ReportPage
    {
        public override ClientPrivilege AuthTypes
        {
            get { return ClientPrivilege.Administrator; }
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!Page.IsPostBack)
            {
                LoadBillingChecks();
                ddlClient.DataSource = DataSession.Query<Client>().Where(x => x.Active).OrderBy(x => x.DisplayName).ToArray();
                ddlClient.DataBind();
            }
        }

        protected void BtnReport_Click(object sender, EventArgs e)
        {
            LoadBillingChecks();
        }

        private void LoadBillingChecks()
        {
            BillingChecks bc = new BillingChecks(pp1.SelectedPeriod);

            DataView dvDaysWithData = bc.DaysWithData();
            rptDaysWithData.DataSource = dvDaysWithData;
            rptDaysWithData.DataBind();
            panDaysWithDataNoData.Visible = dvDaysWithData.Count.Equals(0);

            DataView dvSubsidyComparison = bc.SubsidyComparison();
            rptSubsidyComparison.DataSource = dvSubsidyComparison;
            rptSubsidyComparison.DataBind();
            panSubsidyComparisonNoData.Visible = dvSubsidyComparison.Count.Equals(0);
        }
    }
}