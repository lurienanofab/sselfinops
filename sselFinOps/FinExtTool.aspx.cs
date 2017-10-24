using LNF.Models.Data;
using LNF.Repository;
using sselFinOps.AppCode;
using System;
using System.Data;

namespace sselFinOps
{
    public partial class FinExtTool : ReportPage
    {
        public override ClientPrivilege AuthTypes
        {
            get { return ClientPrivilege.Administrator; }
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            lblWarning.Visible = false;
            if (!Page.IsPostBack)
                ShowCharges();
        }

        protected void pp1_SelectedPeriodChanged(object sender, EventArgs e)
        {
            ShowCharges();
        }

        private void ShowCharges()
        {
            dgTool.DataSource = null;
            dgTool.DataBind();

            DateTime sd = pp1.SelectedPeriod;
            DateTime ed = sd.AddMonths(1);

            using (var dba = DA.Current.GetAdapter())
            {
                // get client data
                dba.AddParameter("@Action", "ExtUserToolUsage");
                dba.AddParameter("@sDate", sd);
                dba.AddParameter("@eDate", ed);
                DataTable dt = dba.FillDataTable("sselScheduler_Select");

                dgTool.DataSource = dt;
                dgTool.DataBind();
            }

            Table1.Visible = true;
        }
    }
}