using LNF.Models.Data;
using LNF.Repository;
using sselFinOps.AppCode;
using System;

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

        protected void Pp1_SelectedPeriodChanged(object sender, EventArgs e)
        {
            ShowCharges();
        }

        private void ShowCharges()
        {
            dgTool.DataSource = null;
            dgTool.DataBind();

            DateTime sd = pp1.SelectedPeriod;
            DateTime ed = sd.AddMonths(1);

            // get client data
            var dt = DA.Command()
                .Param("Action", "ExtUserToolUsage")
                .Param("sDate", sd)
                .Param("eDate", ed)
                .FillDataTable("dbo.sselScheduler_Select");

            dgTool.DataSource = dt;
            dgTool.DataBind();

            Table1.Visible = true;
        }
    }
}