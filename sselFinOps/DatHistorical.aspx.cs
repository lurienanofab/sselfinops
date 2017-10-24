using LNF.Cache;
using LNF.CommonTools;
using LNF.Repository;
using sselFinOps.AppCode;
using System;
using System.Data;
using System.Web.UI.WebControls;

namespace sselFinOps
{
    public enum FormatType
    {
        None = 0,
        Date = 1,
        Currency = 2
    }

    public partial class DatHistorical : ReportPage
    {
        private bool bDisplayUpdate = false;
        private DataSet dsReport;
        private int roomCtr = 0;

        protected void Page_Load(object sender, EventArgs e)
        {
            lblWarning.Text = string.Empty;
            lblWarning.Visible = false;

            if (Page.IsPostBack)
            {
                dsReport = (DataSet)CacheManager.Current.CacheData();
                if (dsReport == null)
                    Response.Redirect("~");
                else if (dsReport.DataSetName != "DatHistorical")
                    Response.Redirect("~");
            }
            else
            {
                CacheManager.Current.RemoveCacheData(); //remove anything left in cache

                using (var dba = new SQLDBAccess("cnSselData"))
                {
                    dsReport = new DataSet("DatHistorical");

                    dba.ApplyParameters(new { Action = "All", sDate = DateTime.Parse("1/1/2000") }).FillDataSet(dsReport, "Client_Select", "Client");
                    dsReport.Tables["Client"].PrimaryKey = new[] { dsReport.Tables["Client"].Columns["ClientID"] };

                    using (var reader = dba.ApplyParameters(new { Action = "All" }).ExecuteReader("Org_Select"))
                    {
                        ddlOrg.DataSource = reader;
                        ddlOrg.DataValueField = "OrgID";
                        ddlOrg.DataTextField = "OrgName";
                        ddlOrg.DataBind();
                        ddlOrg.Items.Insert(0, new ListItem("-- Select --", "0"));
                        ddlOrg.ClearSelection();
                        reader.Close();
                    }
                }

                CacheManager.Current.CacheData(dsReport);

                // populate site dropdown - preselect using site linked in from
            }
        }

        protected void ddlOrg_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (ddlOrg.SelectedIndex != 0)
            {
                ddlAccount.Items.Clear();

                if (dsReport.Tables.Contains("Account"))
                    dsReport.Tables.Remove(dsReport.Tables["Account"]);

                using (var dba = new SQLDBAccess("cnSselData"))
                {
                    // get account and clientAccount info
                    dba.ApplyParameters(new { Action = "AllByOrg", OrgID = int.Parse(ddlOrg.SelectedValue) }).FillDataSet(dsReport, "Account_Select", "Account");
                }

                CacheManager.Current.CacheData(dsReport);

                LoadAccounts();
            }
        }

        protected void rblAcctDisplay_SelectedIndexChanged(object sender, EventArgs e)
        {
            LoadAccounts();
        }

        private void LoadAccounts()
        {
            if (dsReport.Tables["Account"].Rows.Count > 0)
            {
                bDisplayUpdate = true;
                int accountId = 0;
                if (ddlAccount.Items.Count > 0)
                    accountId = int.Parse(ddlAccount.SelectedValue);

                ddlAccount.Items.Clear();
                dsReport.Tables["Account"].DefaultView.Sort = rblAcctDisplay.SelectedValue;
                ddlAccount.DataSource = dsReport.Tables["Account"].DefaultView;
                ddlAccount.DataValueField = "AccountID";
                ddlAccount.DataTextField = rblAcctDisplay.SelectedValue;
                ddlAccount.DataBind();

                if (ddlAccount.Items.Count > 1)
                    ddlAccount.Items.Insert(0, new ListItem("-- Select --", "0"));

                if (accountId == 0)
                    ddlAccount.ClearSelection();
                else
                    ddlAccount.SelectedValue = accountId.ToString();

                MakeSummary();
            }
        }

        protected void rblTimeFrame_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (ddlAccount.Items.Count > 0)
                MakeSummary();
        }

        protected void ddlAccount_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (!bDisplayUpdate)
                MakeSummary();
        }

        private void MakeSummary()
        {
            if (ddlAccount.SelectedValue == "0")
                return;

            // ending date is the end of the previous month
            DateTime endDate = new DateTime(DateTime.Now.Date.Year, DateTime.Now.Date.Month, 1);
            DateTime sDate = new DateTime(2004, 1, 1);
            if (rblTimeFrame.SelectedValue != "0")
                sDate = endDate.AddMonths(int.Parse(rblTimeFrame.SelectedValue));

            // now, aggregate all of this into a single table to facilitate display
            DataTable dtDisplay = new DataTable();
            dtDisplay.Columns.Add("Period", typeof(DateTime));
            dtDisplay.Columns.Add("ClientID", typeof(int));
            dtDisplay.Columns.Add("DisplayName", typeof(string));
            dtDisplay.Columns.Add("Room", typeof(double));
            dtDisplay.Columns.Add("Tool", typeof(double));
            dtDisplay.Columns.Add("StoreInv", typeof(double));
            dtDisplay.Columns.Add("Misc", typeof(double));

            bool bRowAdded;
            DataRow ndr;
            DataRow[] fdr;
            DataTable dtAggCost;
            string[] costType = { "Room", "StoreInv", "Tool", "Misc" };
            Compile mCompile = new Compile();

            while (sDate < endDate)
            {
                // will be a header row
                ndr = dtDisplay.NewRow();
                ndr["Period"] = sDate;
                ndr["DisplayName"] = string.Empty;
                dtDisplay.Rows.Add(ndr);

                bRowAdded = false;
                for (int i = 0; i < costType.Length; i++)
                {
                    dtAggCost = mCompile.CalcCost(costType[i], string.Empty, "AccountID", int.Parse(ddlAccount.SelectedValue), sDate, 0, 0, Compile.AggType.CliAcct);
                    foreach (DataRow drAggCost in dtAggCost.Rows)
                    {
                        bRowAdded = true;
                        fdr = dtDisplay.Select(string.Format("ClientID = {0} AND Period ='{1}'", drAggCost["ClientID"], sDate));
                        if (fdr.Length == 0)
                        {
                            ndr = dtDisplay.NewRow();
                            ndr["Period"] = sDate;
                            ndr["ClientID"] = drAggCost["ClientID"];
                            ndr["DisplayName"] = dsReport.Tables["Client"].Rows.Find(drAggCost["ClientID"])["DisplayName"];
                            dtDisplay.Rows.Add(ndr);
                        }
                        else
                        {
                            ndr = fdr[0];
                        }

                        ndr[costType[i]] = drAggCost["TotalCalcCost"];
                    }
                }

                if (!bRowAdded)
                {
                    ndr = dtDisplay.NewRow();
                    ndr["Period"] = sDate;
                    ndr["DisplayName"] = "No usage this month";
                    dtDisplay.Rows.Add(ndr);
                }

                sDate = sDate.AddMonths(1);
            }

            dtDisplay.DefaultView.Sort = "Period, DisplayName";
            dgReport.DataSource = dtDisplay.DefaultView;
            dgReport.DataBind();

            Table1.Visible = true;
        }

        protected void dgReport_ItemDataBound(object sender, DataGridItemEventArgs e)
        {
            int rowCntr = 0;
            DataRowView drv = (DataRowView)e.Item.DataItem;
            switch (e.Item.ItemType)
            {
                case ListItemType.AlternatingItem:
                case ListItemType.Item:
                    //Check the Row Type
                    //See if we have a Subheader!
                    roomCtr = e.Item.Cells.Count;
                    if (string.IsNullOrEmpty(drv["DisplayName"].ToString()))
                    {
                        rowCntr = 0;
                        //Set the cell to a ColSpan of 3 and Remove the right cells
                        e.Item.Cells[0].ColumnSpan = roomCtr;
                        while (e.Item.Cells.Count > 1)
                            e.Item.Cells.RemoveAt(e.Item.Cells.Count - 1);
                        //Align the cell to the left, make its text bold, and set its background color
                        e.Item.Cells[0].CssClass = "period";
                    }
                    else
                    {
                        e.Item.Cells[0].Text = string.Empty;
                        if (drv["DisplayName"].ToString() == "No usage this month")
                        {
                            //Set the cell to a ColSpan of 3 and Remove the right cells
                            e.Item.Cells[1].ColumnSpan = roomCtr - 1;
                            while (e.Item.Cells.Count > 2)
                                e.Item.Cells.RemoveAt(e.Item.Cells.Count - 1);
                        }
                        else
                        {
                            rowCntr += 1;
                            if (rowCntr % 2 == 0)
                                e.Item.BackColor = System.Drawing.Color.OldLace;
                        }
                    }
                    break;
            }
        }
    }
}