using LNF.Billing;
using LNF.Billing.Reports.ServiceUnitBilling;
using LNF.CommonTools;
using LNF.Data;
using sselFinOps.AppCode;
using sselFinOps.AppCode.SUB;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.UI.WebControls;

namespace sselFinOps
{
    public partial class RepSUB : ReportPage
    {
        public bool ShowTotal { get; set; }

        public override ClientPrivilege AuthTypes
        {
            get { return ClientPrivilege.Executive | ClientPrivilege.Administrator; }
        }

        public DateTime StartPeriod
        {
            get { return ppStart.SelectedPeriod; }
        }

        public DateTime EndPeriod
        {
            get { return ppEnd.SelectedPeriod.AddMonths(1); }
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (Page.IsPostBack)
            {
                gvSUB.DataSource = null;
                gvSUB.DataBind();
                gvJU.DataSource = null;
                gvJU.DataBind();
            }
        }

        private int GetClientID()
        {
            if (!string.IsNullOrEmpty(Request.QueryString["ClientID"]))
            {
                if (int.TryParse(Request.QueryString["ClientID"], out int clientId))
                    return clientId;
            }

            return 0;
        }

        private void SetTotalText(double amount)
        {
            if (ShowTotal)
                lblTotal.Text = amount.ToString("0.00");
        }

        private void LoadGridSUB(ServiceUnitBillingReportItem[][] items, BillingUnit[] summaries)
        {
            List<ServiceUnitBillingReportItem> allItems = new List<ServiceUnitBillingReportItem>();

            foreach (var group in items)
                allItems.AddRange(group);

            double totalMerchAmount = 0;
            foreach (var bu in summaries)
                totalMerchAmount += bu.MerchandiseAmount;

            gvSUB.Columns[1].Visible = false;
            gvSUB.Columns[2].Visible = EndPeriod != StartPeriod.AddMonths(1);
            gvSUB.DataSource = allItems;
            gvSUB.DataBind();
            SetTotalText(totalMerchAmount);
        }

        private void LoadGridJU(JournalUnitReportItem[] items, CreditEntry creditEntry)
        {
            gvJU.Columns[1].Visible = false;
            gvJU.Columns[2].Visible = false;
            gvJU.Columns[3].Visible = EndPeriod != StartPeriod.AddMonths(1);
            gvJU.DataSource = items;
            gvJU.DataBind();
            SetTotalText(creditEntry.MerchandiseAmount);
        }

        protected void BtnReportLabtime_Click(object sender, EventArgs e)
        {
            if (chkHTML.Checked)
                ProcessHtmlRoomSUB();
            else
                ProcessExcelRoomSUB();
        }

        private void ProcessHtmlRoomSUB()
        {
            RoomSUB report = GetReportFactory().GetReportRoomSUB(StartPeriod, EndPeriod, GetClientID());
            LoadGridSUB(report.Items, report.Summaries);
            SetLinkText("sub", "room");
        }

        private void ProcessExcelRoomSUB()
        {
            string filePath;
            if (StartPeriod >= ReportSettings.July2009)
            {
                RoomSUB report = GetReportFactory().GetReportRoomSUB(StartPeriod, EndPeriod, GetClientID());
                filePath = GenerateExcelSUB(report.Items, report.Summaries, "Labtime");
            }
            else
            {
                BillingUnit summaryUnit = new BillingUnit();
                RoomReport mgr = new RoomReport(StartPeriod, EndPeriod);
                DataTable dtRoom = mgr.GenerateDataTable(summaryUnit);
                filePath = mgr.GenerateExcelFile(dtRoom);
            }

            OutputExcel(filePath);
        }

        protected void BtnRoomJU_Command(object sender, CommandEventArgs e)
        {
            var juType = (JournalUnitTypes)Enum.Parse(typeof(JournalUnitTypes), e.CommandArgument.ToString());

            RoomJU report = GetReportFactory().GetReportRoomJU(StartPeriod, EndPeriod, juType, GetClientID());

            if (chkHTML.Checked)
                ProcessHtmlRoomJU(report);
            else
                ProcessExcelRoomJU(report);
        }

        private void ProcessHtmlRoomJU(RoomJU report)
        {
            LoadGridJU(report.Items, report.CreditEntry);
            SetLinkText("ju-" + Enum.GetName(typeof(JournalUnitTypes), report.JournalUnitType).ToLower(), "room");
        }

        private void ProcessExcelRoomJU(RoomJU report)
        {
            string filePath = GenerateExcelJU(report.Items, report.CreditEntry, report.BillingCategory, report.JournalUnitType);
            OutputExcel(filePath);
        }

        protected void BtnAllRoomJU_Click(object sender, EventArgs e)
        {
            ProcessHtmlAllRoomJU();
        }

        private void ProcessHtmlAllRoomJU()
        {
            var factory = GetReportFactory();

            var juA = factory.GetReportRoomJU(StartPeriod, EndPeriod, JournalUnitTypes.A, GetClientID());
            var juB = factory.GetReportRoomJU(StartPeriod, EndPeriod, JournalUnitTypes.B, GetClientID());
            var juC = factory.GetReportRoomJU(StartPeriod, EndPeriod, JournalUnitTypes.C, GetClientID());

            List<JournalUnitReportItem> allItems = ReportFactory.GetAllRoomJU(juA, juB, juC, out double total);

            gvJU.Columns[1].Visible = false;
            gvJU.Columns[2].Visible = true;
            gvJU.Columns[3].Visible = EndPeriod != StartPeriod.AddMonths(1);
            gvJU.DataSource = allItems;
            gvJU.DataBind();
            SetTotalText(total);
            SetLinkText("ju-all", "room");
        }

        protected void BtnReportTool_Click(object sender, EventArgs e)
        {
            if (chkHTML.Checked)
                ProcessHtmlToolSUB();
            else
                ProcessExcelToolSUB();
        }

        private void ProcessHtmlToolSUB()
        {
            ToolSUB report = GetReportFactory().GetReportToolSUB(StartPeriod, EndPeriod, GetClientID());
            LoadGridSUB(report.Items, report.Summaries);
            SetLinkText("sub", "tool");
        }

        private void ProcessExcelToolSUB()
        {
            string filePath;

            if (StartPeriod >= ReportSettings.July2009)
            {
                ToolSUB report = GetReportFactory().GetReportToolSUB(StartPeriod, EndPeriod, GetClientID());
                filePath = GenerateExcelSUB(report.Items, report.Summaries, "Tool");
            }
            else
            {
                BillingUnit SummaryUnit = new BillingUnit();
                ToolReport rpt = new ToolReport(StartPeriod, EndPeriod);
                DataTable dtTool = rpt.GenerateDataTable(SummaryUnit);
                filePath = rpt.GenerateExcelFile(dtTool);
            }

            OutputExcel(filePath);
        }

        protected void BtnToolJU_Command(object sender, CommandEventArgs e)
        {
            JournalUnitTypes juType = (JournalUnitTypes)Enum.Parse(typeof(JournalUnitTypes), e.CommandArgument.ToString());

            ToolJU report = GetReportFactory().GetReportToolJU(StartPeriod, EndPeriod, juType, GetClientID());

            if (chkHTML.Checked)
                ProcessHtmlToolJU(report);
            else
                ProcessExcelToolJU(report);
        }

        private void ProcessHtmlToolJU(ToolJU report)
        {
            LoadGridJU(report.Items, report.CreditEntry);
            SetLinkText("ju-" + Enum.GetName(typeof(JournalUnitTypes), report.JournalUnitType).ToLower(), "tool");
        }

        private void ProcessExcelToolJU(ToolJU report)
        {
            string filePath = GenerateExcelJU(report.Items, report.CreditEntry, report.BillingCategory, report.JournalUnitType);
            OutputExcel(filePath);
        }

        protected void BtnAllToolJU_Click(object sender, EventArgs e)
        {
            ProcessAllToolJUHTML();
        }

        private void ProcessAllToolJUHTML()
        {
            var factory = GetReportFactory();

            var juA = factory.GetReportToolJU(StartPeriod, EndPeriod, JournalUnitTypes.A, GetClientID());
            var juB = factory.GetReportToolJU(StartPeriod, EndPeriod, JournalUnitTypes.B, GetClientID());
            var juC = factory.GetReportToolJU(StartPeriod, EndPeriod, JournalUnitTypes.C, GetClientID());

            List<JournalUnitReportItem> allItems = ReportFactory.GetAllToolJU(juA, juB, juC, out double total);

            gvJU.Columns[1].Visible = false;
            gvJU.Columns[2].Visible = true;
            gvJU.Columns[3].Visible = EndPeriod != StartPeriod.AddMonths(1);
            gvJU.DataSource = allItems;
            gvJU.DataBind();
            SetTotalText(total);
            SetLinkText("ju-all", "tool");
        }

        protected void BtnReportStore_Click(object sender, EventArgs e)
        {
            if (chkHTML.Checked)
                ProcessHtmlStoreSUB(false);
            else
                ProcessExcelStoreSUB(false);
        }

        protected void BtnReportStoreTwoAccounts_Click(object sender, EventArgs e)
        {
            if (chkHTML.Checked)
                ProcessHtmlStoreSUB(true);
            else
                ProcessExcelStoreSUB(true);
        }

        private void ProcessHtmlStoreSUB(bool twoCreditAccounts)
        {
            StoreSUB report = GetReportFactory().GetReportStoreSUB(StartPeriod, EndPeriod, twoCreditAccounts, GetClientID());
            LoadGridSUB(report.Items, report.Summaries);
            SetLinkText("sub", "store", twoCreditAccounts);
        }

        private void ProcessExcelStoreSUB(bool twoCreditAccounts)
        {
            string filePath;
            if (twoCreditAccounts)
            {
                if (StartPeriod >= ReportSettings.July2009)
                {
                    StoreSUB report = GetReportFactory().GetReportStoreSUB(StartPeriod, EndPeriod, twoCreditAccounts, GetClientID());
                    filePath = GenerateExcelSUB(report.Items, report.Summaries, "Store");
                }
                else
                {
                    BillingUnit SummaryUnit1 = new BillingUnit();
                    BillingUnit SummaryUnit2 = new BillingUnit();
                    StoreReport rpt = new StoreReport(StartPeriod, EndPeriod);
                    DataTable dtStore = rpt.GenerateDataTable(SummaryUnit1, SummaryUnit2);
                    filePath = rpt.GenerateExcelFile(dtStore);
                }
            }
            else
            {
                if (StartPeriod >= ReportSettings.July2009)
                {
                    StoreSUB report = GetReportFactory().GetReportStoreSUB(StartPeriod, EndPeriod, twoCreditAccounts, GetClientID());
                    filePath = GenerateExcelSUB(report.Items, report.Summaries, "Store");
                }
                else
                {
                    BillingUnit SummaryUnit1 = new BillingUnit();
                    BillingUnit SummaryUnit2 = new BillingUnit();
                    StoreReport rpt = new StoreReport(StartPeriod, EndPeriod);
                    DataTable dtStore = rpt.GenerateDataTable(SummaryUnit1, SummaryUnit2);
                    filePath = rpt.GenerateExcelFile(dtStore);
                }
            }

            OutputExcel(filePath);
        }

        protected void BtnAllSUB_Click(object sender, EventArgs e)
        {
            ProcessHtmlAllSUB();
        }

        private void ProcessHtmlAllSUB()
        {
            var factory = GetReportFactory();
            RoomSUB roomSUB = factory.GetReportRoomSUB(StartPeriod, EndPeriod, GetClientID());
            ToolSUB toolSUB = factory.GetReportToolSUB(StartPeriod, EndPeriod, GetClientID());
            StoreSUB storeSUB = factory.GetReportStoreSUB(StartPeriod, EndPeriod, false, GetClientID());

            IEnumerable<ServiceUnitBillingReportItem> allItems = ReportFactory.GetAllSUB(roomSUB, toolSUB, storeSUB, out double total);

            gvSUB.Columns[1].Visible = true;
            gvSUB.Columns[2].Visible = EndPeriod != StartPeriod.AddMonths(1);
            gvSUB.DataSource = allItems;
            gvSUB.DataBind();
            SetTotalText(total);
            SetLinkText("sub", "all");
        }

        protected void BtnAllJU_Command(object sender, CommandEventArgs e)
        {
            JournalUnitTypes juType = (JournalUnitTypes)Enum.Parse(typeof(JournalUnitTypes), e.CommandArgument.ToString(), true);
            ProcessHtmlAllJU(juType);
        }

        private void ProcessHtmlAllJU(JournalUnitTypes juType)
        {
            double total;
            List<JournalUnitReportItem> allItems;

            var factory = GetReportFactory();

            switch (juType)
            {
                case JournalUnitTypes.A:
                case JournalUnitTypes.B:
                case JournalUnitTypes.C:
                    RoomJU roomJU = factory.GetReportRoomJU(StartPeriod, EndPeriod, juType, GetClientID());
                    ToolJU toolJU = factory.GetReportToolJU(StartPeriod, EndPeriod, juType, GetClientID());
                    allItems = ReportFactory.GetAllJU(roomJU, toolJU, out total);
                    break;
                case JournalUnitTypes.All:
                    RoomJU roomJUA = factory.GetReportRoomJU(StartPeriod, EndPeriod, JournalUnitTypes.A, GetClientID());
                    ToolJU toolJUA = factory.GetReportToolJU(StartPeriod, EndPeriod, JournalUnitTypes.A, GetClientID());
                    RoomJU roomJUB = factory.GetReportRoomJU(StartPeriod, EndPeriod, JournalUnitTypes.B, GetClientID());
                    ToolJU toolJUB = factory.GetReportToolJU(StartPeriod, EndPeriod, JournalUnitTypes.B, GetClientID());
                    RoomJU roomJUC = factory.GetReportRoomJU(StartPeriod, EndPeriod, JournalUnitTypes.C, GetClientID());
                    ToolJU toolJUC = factory.GetReportToolJU(StartPeriod, EndPeriod, JournalUnitTypes.C, GetClientID());

                    double temp;
                    total = 0;

                    allItems = new List<JournalUnitReportItem>();

                    allItems.AddRange(ReportFactory.GetAllJU(roomJUA, toolJUA, out temp));
                    total += temp;

                    allItems.AddRange(ReportFactory.GetAllJU(roomJUB, toolJUB, out temp));
                    total += temp;

                    allItems.AddRange(ReportFactory.GetAllJU(roomJUC, toolJUC, out temp));
                    total += temp;
                    break;
                default:
                    throw new ArgumentException(string.Format("Invalid JournalUnitTypes value: {0}", juType));
            }

            gvJU.Columns[1].Visible = true;
            gvJU.Columns[2].Visible = juType == JournalUnitTypes.All;
            gvJU.Columns[3].Visible = EndPeriod != StartPeriod.AddMonths(1);
            gvJU.DataSource = allItems;
            gvJU.DataBind();
            SetTotalText(total);

            SetLinkText("ju-" + Enum.GetName(typeof(JournalUnitTypes), juType).ToLower(), "all");
        }

        //This got moved from SUBBase.vb: I can't seem to import the Billing.Reports namespace in vb App_Code files becuase it's in a cs file, but I can here.
        private string GenerateExcelSUB(ServiceUnitBillingReportItem[][] items, BillingUnit[] SummaryUnit, string ChargeType)
        {
            //Contruct the excel object

            string fileName = Utility.GetRequiredAppSetting("SUB_Template");
            string templatePath = ExcelUtility.GetTemplatePath(fileName);
            string workPathDir = ExcelUtility.GetWorkPath(CurrentUser.ClientID);

            using (var mgr = ExcelUtility.NewExcelManager())
            {
                mgr.OpenWorkbook(templatePath);
                mgr.SetActiveWorksheet("Sheet1");

                int iRow = 1; //zero based, iRow = 0 is Row #1 on spreadsheet (the header)

                //dv.Sort = "ItemDescription ASC, ProjectGrant ASC";
                foreach (ServiceUnitBillingReportItem item in items[0])
                {
                    if (!string.IsNullOrEmpty(item.CardType))
                    {
                        mgr.SetCellTextValue(iRow, 0, item.CardType);
                        mgr.SetCellTextValue(iRow, 1, item.ShortCode);
                        mgr.SetCellTextValue(iRow, 2, item.Account);
                        mgr.SetCellTextValue(iRow, 3, item.FundCode);
                        mgr.SetCellTextValue(iRow, 4, item.DeptID);
                        mgr.SetCellTextValue(iRow, 5, item.ProgramCode);
                        mgr.SetCellTextValue(iRow, 6, item.Class);
                        mgr.SetCellTextValue(iRow, 7, item.ProjectGrant);
                        mgr.SetCellTextValue(iRow, 8, item.VendorID);
                        mgr.SetCellTextValue(iRow, 9, item.InvoiceDate);
                        mgr.SetCellTextValue(iRow, 10, item.InvoiceID);

                        string uniqName = item.Uniqname.ToString();
                        if (uniqName.Length > 8)
                            uniqName = uniqName.Substring(0, 8);

                        mgr.SetCellTextValue(iRow, 11, uniqName);
                        mgr.SetCellTextValue(iRow, 15, item.DepartmentalReferenceNumber);

                        //R = 17
                        //[2014-09-08 jg] All column S and higher are now shifted one to the left so 18 -> 17, 19 -> 18, etc
                        //ws.Cell[iRow, 18].Value = item.ItemDescription;
                        //ws.Cell[iRow, 24].Value = item.QuantityVouchered;
                        //ws.Cell[iRow, 26].Value = string.IsNullOrEmpty(item.UnitOfMeasure) ? 0 : Convert.ToDouble(item.UnitOfMeasure);
                        //ws.Cell[iRow, 27].Value = string.IsNullOrEmpty(item.MerchandiseAmount) ? 0 : Convert.ToDouble(item.MerchandiseAmount);

                        mgr.SetCellTextValue(iRow, 17, item.ItemDescription);
                        mgr.SetCellNumberValue(iRow, 23, item.QuantityVouchered);
                        mgr.SetCellNumberValue(iRow, 25, string.IsNullOrEmpty(item.UnitOfMeasure) ? 0 : Convert.ToDouble(item.UnitOfMeasure));
                        mgr.SetCellNumberValue(iRow, 26, string.IsNullOrEmpty(item.MerchandiseAmount) ? 0 : Convert.ToDouble(item.MerchandiseAmount));

                        iRow += 1;
                    }
                }

                mgr.SetCellTextValue(iRow, 0, SummaryUnit[0].CardType);
                mgr.SetCellTextValue(iRow, 1, SummaryUnit[0].ShortCode);
                mgr.SetCellTextValue(iRow, 2, SummaryUnit[0].Account);
                mgr.SetCellTextValue(iRow, 3, SummaryUnit[0].FundCode);
                mgr.SetCellTextValue(iRow, 4, SummaryUnit[0].DeptID);
                mgr.SetCellTextValue(iRow, 5, SummaryUnit[0].ProgramCode);
                mgr.SetCellTextValue(iRow, 6, SummaryUnit[0].ClassName);
                mgr.SetCellTextValue(iRow, 7, SummaryUnit[0].ProjectGrant);
                mgr.SetCellTextValue(iRow, 9, SummaryUnit[0].InvoiceDate);
                mgr.SetCellTextValue(iRow, 11, SummaryUnit[0].Uniqname);

                //ws.Cells[iRow, 18].Value = SummaryUnit[0].ItemDescription;
                //ws.Cells[iRow, 24].Value = SummaryUnit[0].QuantityVouchered;
                //ws.Cells[iRow, 26].Value = Math.Round(SummaryUnit[0].MerchandiseAmount, 5);
                //ws.Cells[iRow, 27].Formula = string.Format("=-SUM(AB2:AB{0})", iRow);

                mgr.SetCellTextValue(iRow, 17, SummaryUnit[0].ItemDescription);
                mgr.SetCellNumberValue(iRow, 23, SummaryUnit[0].QuantityVouchered);
                mgr.SetCellNumberValue(iRow, 25, Math.Round(SummaryUnit[0].MerchandiseAmount, 5));
                mgr.SetCellFormula(iRow, 26, string.Format("=-SUM(AB2:AB{0})", iRow));

                if (items.Length > 1 && SummaryUnit.Length > 1)
                {
                    iRow += 1;
                    int startingRowNumber = iRow + 1; //we use excel formula to calculate the sum, so we have to keep this to get the starting range

                    //dv.Sort = "ItemDescription ASC, ProjectGrant ASC";
                    foreach (ServiceUnitBillingReportItem item in items[1])
                    {
                        if (!string.IsNullOrEmpty(item.CardType))
                        {
                            mgr.SetCellTextValue(iRow, 0, item.CardType);
                            mgr.SetCellTextValue(iRow, 1, item.ShortCode);
                            mgr.SetCellTextValue(iRow, 2, item.Account);
                            mgr.SetCellTextValue(iRow, 3, item.FundCode);
                            mgr.SetCellTextValue(iRow, 4, item.DeptID);
                            mgr.SetCellTextValue(iRow, 5, item.ProgramCode);
                            mgr.SetCellTextValue(iRow, 6, item.Class);
                            mgr.SetCellTextValue(iRow, 7, item.ProjectGrant);
                            mgr.SetCellTextValue(iRow, 8, item.VendorID);
                            mgr.SetCellTextValue(iRow, 9, item.InvoiceDate);
                            mgr.SetCellTextValue(iRow, 10, item.InvoiceID);

                            string uniqName = item.Uniqname;
                            if (uniqName.Length > 8)
                                uniqName = uniqName.Substring(0, 8);

                            mgr.SetCellTextValue(iRow, 11, uniqName);
                            mgr.SetCellTextValue(iRow, 15, item.DepartmentalReferenceNumber);

                            //ws.Cells[iRow, 18].Value = item.ItemDescription;
                            //ws.Cells[iRow, 24].Value = item.QuantityVouchered;
                            //ws.Cells[iRow, 26].Value = string.IsNullOrEmpty(item.UnitOfMeasure) ? 0 : Convert.ToDouble(item.UnitOfMeasure);
                            //ws.Cells[iRow, 27].Value = string.IsNullOrEmpty(item.MerchandiseAmount) ? 0 : Convert.ToDouble(item.MerchandiseAmount);

                            mgr.SetCellTextValue(iRow, 17, item.ItemDescription);
                            mgr.SetCellNumberValue(iRow, 23, item.QuantityVouchered);
                            mgr.SetCellNumberValue(iRow, 25, string.IsNullOrEmpty(item.UnitOfMeasure) ? 0 : Convert.ToDouble(item.UnitOfMeasure));
                            mgr.SetCellNumberValue(iRow, 26, string.IsNullOrEmpty(item.MerchandiseAmount) ? 0 : Convert.ToDouble(item.MerchandiseAmount));

                            iRow += 1;
                        }
                    }

                    mgr.SetCellTextValue(iRow, 0, SummaryUnit[1].CardType);
                    mgr.SetCellTextValue(iRow, 1, SummaryUnit[1].ShortCode);
                    mgr.SetCellTextValue(iRow, 2, SummaryUnit[1].Account);
                    mgr.SetCellTextValue(iRow, 3, SummaryUnit[1].FundCode);
                    mgr.SetCellTextValue(iRow, 4, SummaryUnit[1].DeptID);
                    mgr.SetCellTextValue(iRow, 5, SummaryUnit[1].ProgramCode);
                    mgr.SetCellTextValue(iRow, 6, SummaryUnit[1].ClassName);
                    mgr.SetCellTextValue(iRow, 7, SummaryUnit[1].ProjectGrant);
                    mgr.SetCellTextValue(iRow, 9, SummaryUnit[1].InvoiceDate);
                    mgr.SetCellTextValue(iRow, 11, SummaryUnit[1].Uniqname);

                    //ws.Cells[iRow, 18].Value = SummaryUnit[1].ItemDescription;
                    //ws.Cells[iRow, 24].Value = SummaryUnit[1].QuantityVouchered;
                    //ws.Cells[iRow, 26].Value = Math.Round(SummaryUnit[0].MerchandiseAmount, 5);
                    //ws.Cells[iRow, 27].Formula = string.Format("=-SUM(AB{0}:AB{1})", startingRowNumber, iRow);

                    mgr.SetCellTextValue(iRow, 17, SummaryUnit[1].ItemDescription);
                    mgr.SetCellNumberValue(iRow, 23, SummaryUnit[1].QuantityVouchered);
                    mgr.SetCellNumberValue(iRow, 25, Math.Round(SummaryUnit[0].MerchandiseAmount, 5));
                    mgr.SetCellFormula(iRow, 26, string.Format("=-SUM(AB{0}:AB{1})", startingRowNumber, iRow));

                    mgr.SetColumnCollapsed("I", true);
                    mgr.SetColumnCollapsed("J", true);
                    mgr.SetColumnWidth(10, 1);
                }

                if (!Directory.Exists(workPathDir))
                    Directory.CreateDirectory(workPathDir);

                // the sub number
                string workFilePath = Path.Combine(workPathDir, ChargeType + "SUB_" + StartPeriod.ToString("yyyy-MM"));

                if (EndPeriod == StartPeriod.AddMonths(1))
                    workFilePath += Path.GetExtension(fileName);
                else
                    workFilePath += "_" + EndPeriod.AddMonths(-1).ToString("yyyy-MM") + Path.GetExtension(fileName);

                mgr.SaveAs(workFilePath);

                return workFilePath;
            }
        }

        private string GenerateExcelJU(JournalUnitReportItem[] items, CreditEntry creditEntry, BillingCategory billingCategory, JournalUnitTypes juType)
        {
            //Contruct the excel object
            string fileName = Utility.GetRequiredAppSetting("JU_Template");
            string templatePath = ExcelUtility.GetTemplatePath(fileName);

            using (var mgr = ExcelUtility.NewExcelManager())
            {
                mgr.OpenWorkbook(templatePath);
                mgr.SetActiveWorksheet("Sheet1");

                DateTime period = EndPeriod.AddMonths(-1);
                string billingCategoryName = Utility.EnumToString(billingCategory);

                //We start at first row, because for ExcelLite control, the header row is not included

                // [2021-09-27 jg]
                //      source was previoulsy JU, now it is ENG (set in web.config)
                //      journalDescription was previously "JU {0} {1:MM/yy} {2} {3}{4}" now set in web.config
                string source = Utility.GetRequiredAppSetting("JournalSource"); //ENG
                int journalUnitNumber = ReportSettings.GetJournalUnitNumber(period, billingCategory, juType);
                // JournalDescription example: sponprogtechsupport@umich.edu; JU {0} {1:MM/yy} {2} {3}{4}
                string journalDescription = string.Format(
                    Utility.GetRequiredAppSetting("JournalDescription"),
                    /*0*/ journalUnitNumber,
                    /*1*/ period,
                    /*2*/ ReportSettings.CompanyName,
                    /*3*/ billingCategoryName,
                    /*4*/ Utility.EnumToString(juType)
                );

                //column A
                mgr.SetCellTextValue(2, 0, "H");
                //column B
                mgr.SetCellTextValue(2, 1, "NEXT");
                //column C
                mgr.SetCellTextValue(2, 2, DateTime.Now.ToString("MM/dd/yyyy"));
                //column D
                mgr.SetCellTextValue(2, 3, source);
                //column E
                mgr.SetCellTextValue(2, 4, period.ToString("yyyy/MM"));
                //column F
                mgr.SetCellTextValue(2, 5, journalDescription);
                //column G
                mgr.SetCellTextValue(2, 6, ReportSettings.FinancialManagerUserName);

                int iRow = 3;
                int iCol = 7;

                //DataView dv = dt.DefaultView;
                //dv.Sort = "ItemDescription ASC, ProjectGrant ASC";
                foreach (JournalUnitReportItem item in items.Where(i => i.ItemDescription != $"zz{ReportSettings.FinancialManagerUserName}"))
                {
                    //column A
                    mgr.SetCellTextValue(iRow, 0, "L");
                    //column H
                    mgr.SetCellTextValue(iRow, iCol, item.Account);
                    //column I
                    mgr.SetCellTextValue(iRow, iCol + 1, item.FundCode);
                    //column J
                    mgr.SetCellTextValue(iRow, iCol + 2, item.DeptID);
                    //column K
                    mgr.SetCellTextValue(iRow, iCol + 3, item.ProgramCode);
                    //column L
                    mgr.SetCellTextValue(iRow, iCol + 4, item.Class);
                    //column M
                    mgr.SetCellTextValue(iRow, iCol + 5, item.ProjectGrant);
                    //column N
                    mgr.SetCellNumberValue(iRow, iCol + 6, string.IsNullOrEmpty(item.MerchandiseAmount) ? 0 : Convert.ToDouble(item.MerchandiseAmount));
                    //column O
                    mgr.SetCellTextValue(iRow, iCol + 7, item.DepartmentalReferenceNumber);
                    //column P
                    mgr.SetCellTextValue(iRow, iCol + 8, item.ItemDescription);
                    iRow += 1;
                }

                //Add the last row - which is the summary unit
                mgr.SetCellTextValue(iRow, 0, "L");
                mgr.SetCellTextValue(iRow, iCol, creditEntry.Account);
                mgr.SetCellTextValue(iRow, iCol + 1, creditEntry.FundCode);
                mgr.SetCellTextValue(iRow, iCol + 2, creditEntry.DeptID);
                mgr.SetCellTextValue(iRow, iCol + 3, creditEntry.ProgramCode);
                mgr.SetCellTextValue(iRow, iCol + 4, creditEntry.ClassName);
                mgr.SetCellTextValue(iRow, iCol + 5, creditEntry.ProjectGrant);
                mgr.SetCellTextValue(iRow, iCol + 6, creditEntry.MerchandiseAmount);
                mgr.SetCellTextValue(iRow, iCol + 7, creditEntry.DepartmentalReferenceNumber); //should be davejd
                mgr.SetCellTextValue(iRow, iCol + 8, creditEntry.ItemDescription); // should be MM/YY LNF UsageCase Subsidy;SUB#

                string workPathDir = ExcelUtility.GetWorkPath(CurrentUser.ClientID);

                if (!Directory.Exists(workPathDir))
                    Directory.CreateDirectory(workPathDir);

                string workFilePath = Path.Combine(workPathDir, "JU" + Enum.GetName(typeof(JournalUnitTypes), juType) + "_" + billingCategoryName + "_" + period.ToString("yyyy-MM") + Path.GetExtension(fileName));
                mgr.SaveAs(workFilePath);

                return workFilePath;
            }
        }

        private void OutputExcel(string filePath)
        {
            if (string.IsNullOrEmpty(filePath))
            {
                litError.Text = @"<div class=""error"">Error encountered - no spreadsheet to display</div>";
                return;
            }

            if (!File.Exists(filePath))
            {
                litError.Text = @"<div class=""error"">Error encountered - no spreadsheet to display</div>";
                return;
            }

            //Display excel spreadsheet
            Response.Redirect("~/Spreadsheets.ashx?name=" + Path.GetFileName(filePath), false);

            //Response.Clear();
            //Response.Buffer = true;
            //Response.AddHeader("Content-Disposition", "attachment; filename=" + Path.GetFileName(filePath));
            //Response.ContentType = "application/vnd.ms-excel";
            //Response.Charset = string.Empty;
            //EnableViewState = false;

            //Response.WriteFile(filePath);

            //Response.End();
        }

        protected void ChkHTML_CheckChanged(object sender, EventArgs e)
        {
            btnAllSUB.Enabled = chkHTML.Checked;
            btnAllRoomJU.Enabled = chkHTML.Checked;
            btnAllToolJU.Enabled = chkHTML.Checked;
            btnAllJUA.Enabled = chkHTML.Checked;
            btnAllJUB.Enabled = chkHTML.Checked;
            btnAllJUC.Enabled = chkHTML.Checked;
            btnAllJU.Enabled = chkHTML.Checked;
        }

        private string GetDataURL(string report, string charge, bool twoCreditAccounts)
        {
            string result = VirtualPathUtility.ToAbsolute("~/data/");
            result += string.Format("?report={0}&charge={1}&sdate={2}&edate={3}", report, charge, StartPeriod.ToString("yyyyMM"), EndPeriod.ToString("yyyyMM"));
            if (charge == "store")
                result += "&accts=" + (twoCreditAccounts ? "1" : "0");
            return result;
        }

        private void SetLinkText(string report, string charge)
        {
            SetLinkText(report, charge, false);
        }

        private void SetLinkText(string report, string charge, bool twoCreditAccounts)
        {
            string url = GetDataURL(report, charge, twoCreditAccounts);
            string link = string.Format(@"<a href=""{0}"" target=""_blank"" target=""_blank"">xml</a>", url);
            litLink.Text = link;
        }

        private ReportFactory GetReportFactory() => new ReportFactory(Provider.Billing.Report);
    }
}