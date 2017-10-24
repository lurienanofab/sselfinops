using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Data;
using LNF.Repository;
using LNF.CommonTools;
using sselFinOps.AppCode;

namespace sselFinOps
{
    public partial class MscExp : ReportPage
    {
        protected void Page_Load(object sender, EventArgs e)
        {

        }

        protected void ConfigureCost_Command(object sender, CommandEventArgs e)
        {
            Response.Redirect(string.Format("~/ConCost.aspx?ItemType={0}&Exp=Exp", e.CommandArgument));
        }

        protected void btnConFormula_Click(object sender, EventArgs e)
        {
            Response.Redirect("~/ConFormula.aspx?Exp=Exp");
        }

        protected void btnReport_Click(object sender, EventArgs e)
        {
            int numMonths = 0;
            
            if (!int.TryParse(txtNumMonths.Text, out numMonths)) return;

            if (numMonths == 0 || numMonths > 12) return;

            // get time frame
            DateTime sd = pp1.SelectedPeriod;
            DateTime ed = sd.AddMonths(1);

            // create table of clients along with associated orgs
            // add columns for room/store/tool for each month that report is run and set all values to 0
            DataTable dtOrg = null;
            DataTable dtClientOrg = null;

            using (var dba = DA.Current.GetAdapter())
            {
                dba.AddParameter("@Action", "ForExpCost");
                dtClientOrg = dba.FillDataTable("ClientOrg_Select");

                for (int i = 0; i < numMonths; i++)
                {
                    dtClientOrg.Columns.Add(string.Format("mn{0}Room", i), typeof(double));
                    dtClientOrg.Columns.Add(string.Format("mn{0}Tool", i), typeof(double));
                }

                foreach (DataRow dr in dtClientOrg.Rows)
                {
                    for (int i = 0; i < numMonths; i++)
                    {
                        dr.SetField(string.Format("mn{0}Room", i), 0);
                        dr.SetField(string.Format("mn{0}Tool", i), 0);
                    }
                }
            }

            using (var dba = DA.Current.GetAdapter())
            {
                dba.AddParameter("@Action", "AllActive");
                dba.AddParameter("@sDate", sd);
                dba.AddParameter("@eDate", ed);
                dtOrg = dba.MapSchema().FillDataTable("Org_Select");
            }

            // loop by month, then org - follow example in invoice to sum and cap

            DataRow[] drClientOrg;
            DataTable dtAggCost;
            string[] CostType = {"Room", "Tool"};
            Compile mCompile = new Compile();
            DataTable dtClientWithCharges;
            double CapCost;
            double TotalCharges;

            for(int mnOff = 0; mnOff < numMonths; mnOff++)
            {
                foreach (DataRow drOrg in dtOrg.Rows)
                {
                    for (int i = 0; i <  CostType.Length; i++)
                    {
                        dtAggCost = mCompile.CalcCost(CostType[i], string.Empty, "OrgID", drOrg.Field<int>("OrgID"), sd.AddMonths(mnOff), 0, 0, Compile.AggType.CliAcct, true, "Exp");
                        dtClientWithCharges = mCompile.GetTable(1);
                        CapCost = mCompile.CapCost;

                        foreach (DataRow drCWC in dtClientWithCharges.Rows)
                        {
                            object temp = dtAggCost.Compute("SUM(TotalCalcCost)", string.Format("ClientID = {0}", drCWC["ClientID"]));

                            if (temp == DBNull.Value)
                                TotalCharges = 0.0;
                            else
                                TotalCharges = Convert.ToDouble(temp);

                            if (TotalCharges > CapCost)
                            {
                                DataRow[] fdr = dtAggCost.Select(string.Format("ClientID = {0}", drCWC["ClientID"]));
                                for (int j = 0; j < fdr.Length; j++)
                                {
                                    fdr[j].SetField("TotalCalcCost", fdr[j].Field<double>("TotalCalcCost") * CapCost / TotalCharges);
                                }
                            }

                            // now, add this to the clientOrg table
                            drClientOrg = dtClientOrg.Select(string.Format("ClientID = {0} AND OrgID = {1}", drCWC["ClientID"], drOrg["OrgID"]));
                            if (drClientOrg.Length > 0)  // should always be 1
                                drClientOrg[0].SetField(string.Format("mn{0}{1}", mnOff, CostType[i]), Math.Min(TotalCharges, CapCost));
                        }
                    }
                }
            }

            // now, create output table and show results
            DataTable dtResults = new DataTable();
            dtResults.Columns.Add("DisplayName", typeof(string));
            dtResults.Columns.Add("OrgName", typeof(string));
            dtResults.Columns.Add("RoomCost", typeof(double));
            dtResults.Columns.Add("ToolCost", typeof(double));

            DataRow ndr;
            Double RoomCost, ToolCost;

            foreach (DataRow dr in dtClientOrg.Rows)
            {
                RoomCost = 0;
                ToolCost = 0;

                for (int i = 0; i < numMonths; i++)
                {
                    RoomCost += dr.Field<double>(string.Format("mn{0}Room", i));
                    ToolCost += dr.Field<double>(string.Format("mn{0}Tool", i));
                }

                if (RoomCost > 0 || ToolCost > 0)
                {
                    ndr = dtResults.NewRow();
                    ndr["DisplayName"] = dr["DisplayName"];
                    ndr["OrgName"] = dtOrg.Rows.Find(dr["OrgID"]).Field<string>("OrgName");
                    ndr["RoomCost"] = RoomCost;
                    ndr["ToolCost"] = ToolCost;
                    dtResults.Rows.Add(ndr);
                }
            }

            dtResults.DefaultView.Sort = "OrgName, DisplayName";
            dgCost.DataSource = dtResults;
            dgCost.DataBind();

            Table1.Visible = true;
        }
    }
}