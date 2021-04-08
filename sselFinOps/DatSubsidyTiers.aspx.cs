using sselFinOps.AppCode;
using System;
using System.Data;

namespace sselFinOps
{
    public partial class DatSubsidyTiers : ReportPage
    {
        private DataSet dsTieredSubsidyBilling = null;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!Page.IsPostBack)
            {
                if (!string.IsNullOrEmpty(Request.QueryString["period"]))
                {
                    if (DateTime.TryParse(Request.QueryString["period"], out DateTime period))
                    {
                        pp1.SelectedPeriod = period;
                    }
                }

                LoadTiers();
            }
        }

        protected void BtnReport_Click(object sender, EventArgs e)
        {
            LoadTiers();
        }

        private void LoadTiers()
        {
            litPeriod.Text = pp1.SelectedPeriod.ToString("MMM yyyy");

            var dt0 = SubsidyTiersData(0);
            rptSubsidyTiersGroup0.DataSource = dt0;
            rptSubsidyTiersGroup0.DataBind();

            if (dt0.Rows.Count == 0)
                panNoData0.Visible = true;

            var dt1 = SubsidyTiersData(1);
            rptSubsidyTiersGroup1.DataSource = dt1;
            rptSubsidyTiersGroup1.DataBind();

            if (dt1.Rows.Count == 0)
                panNoData1.Visible = true;

            var dt2 = SubsidyTiersData(2);
            rptSubsidyTiersGroup2.DataSource = dt2;
            rptSubsidyTiersGroup2.DataBind();

            if (dt2.Rows.Count == 0)
                panNoData2.Visible = true;
        }

        private DataTable SubsidyTiersData(int groupId)
        {
            if (dsTieredSubsidyBilling == null)
                dsTieredSubsidyBilling = DataCommand().Param("Action", "PopulateTieredSubsidyBilling").Param("Period", pp1.SelectedPeriod).FillDataSet("dbo.TieredSubsidyBilling_Select");

            var dt = FormatTierData(dsTieredSubsidyBilling.Tables[2], groupId);

            return dt;
        }

        private DataTable FormatTierData(DataTable dtRaw, int groupId)
        {
            var dt = new DataTable();
            dt.Columns.Add("AnnualFees", typeof(string));
            dt.Columns.Add("LNF", typeof(string));
            dt.Columns.Add("PI", typeof(string));
            dt.Columns.Add("UserCharge", typeof(string));
            dt.Columns.Add("RunningTotal", typeof(string));

            DataRow dr = null;
            DataRow drNext = null;
            DataRow drLast = null;
            string annualFee = string.Empty;
            double floor = 0;
            double ceiling = 0;
            double pi = 0;
            double lnf = 0;
            double userCharge = 0;
            double runningTotal = 0;
            DataRow[] rows = dtRaw.Select(string.Format("GroupID = {0}", groupId));

            if (rows.Length > 0)
            {
                for (int r = 0; r < rows.Length - 1; r++) //stop at 2nd to last index
                {
                    dr = rows[r];
                    drNext = rows[r + 1];

                    if (!double.TryParse(dr["FloorAmount"].ToString(), out floor))
                        floor = -1;

                    if (!double.TryParse(drNext["FloorAmount"].ToString(), out ceiling))
                        ceiling = -1;

                    annualFee = FormatAnnualFee(floor, ceiling);
                    SetPercentages(dr, out lnf, out pi);
                    if (floor != -1 && ceiling != -1 && lnf != -1 && pi != -1)
                    {
                        userCharge = (pi * (ceiling - floor));
                        runningTotal += userCharge;
                    }

                    dt.Rows.Add(annualFee, FormatTierPercentage(lnf), FormatTierPercentage(pi), FormatTierCharge(userCharge), FormatTierCharge(runningTotal));
                }

                drLast = rows[rows.Length - 1];
                if (!double.TryParse(drLast["FloorAmount"].ToString(), out floor))
                    floor = -1;

                annualFee = FormatAnnualFee(floor, double.MaxValue);
                SetPercentages(drLast, out lnf, out pi);
                dt.Rows.Add(annualFee, FormatTierPercentage(lnf), FormatTierPercentage(pi), "---", "---");
            }

            return dt;
        }

        private string FormatAnnualFee(double floor, double ceiling)
        {
            string result = string.Empty;

            if (floor != -1)
            {
                if (floor == 0)
                    result += "$0";
                else
                    result += floor.ToString("$#,##0");
            }
            else
            {
                result += "NaN";
            }

            if (ceiling != -1)
            {
                if (ceiling == 0)
                    result += " - $0";
                else
                {
                    if (ceiling < double.MaxValue)
                        result += " - " + ceiling.ToString("$#,##0");
                    else
                        result += " and above";
                }
            }
            else
            {
                result += " - NaN";
            }

            return result;
        }

        private string FormatTierPercentage(double amount)
        {
            if (amount != -1)
                return amount.ToString("0%");
            else
                return "NaN";
        }

        private string FormatTierCharge(double amount)
        {
            if (amount != -1)
                return amount.ToString("$#,##0");
            else
                return "NaN";
        }

        private void SetPercentages(DataRow dr, out double lnf, out double pi)
        {
            if (double.TryParse(dr["UserPaymentPercentage"].ToString(), out double d))
            {
                pi = d;
                lnf = 1 - pi;
            }
            else
            {
                lnf = -1;
                pi = -1;
            }
        }
    }
}