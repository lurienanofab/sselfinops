using GemBox.ExcelLite;
using LNF.Cache;
using LNF.Models.Billing;
using LNF.Models.Billing.Reports.ServiceUnitBilling;
using LNF.Models.Data;
using sselFinOps.AppCode;
using sselFinOps.AppCode.SUB;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace sselFinOps
{
    public partial class RepSUB : ReportPage
    {
        public static readonly DateTime July2009 = new DateTime(2009, 7, 1);
        public static readonly DateTime July2010 = new DateTime(2010, 7, 1);

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
                int clientId = 0;
                if (int.TryParse(Request.QueryString["ClientID"], out clientId))
                    return clientId;
            }

            return 0;
        }

        private void SetTotalText(double amount)
        {
            //lblTotal.Text = amount.ToString("0.00");
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

        protected void btnReportLabtime_Click(object sender, EventArgs e)
        {
            RegisterAsyncTask(new PageAsyncTask(async () =>
            {
                if (chkHTML.Checked)
                    await ProcessHtmlRoomSUB();
                else
                    await ProcessExcelRoomSUB();
            }));
        }

        private async Task ProcessHtmlRoomSUB()
        {
            RoomSUB report = await ReportFactory.GetReportRoomSUB(StartPeriod, EndPeriod, GetClientID());
            LoadGridSUB(report.Items, report.Summaries);
            SetLinkText("sub", "room");
        }

        private async Task ProcessExcelRoomSUB()
        {
            string filePath;
            if (StartPeriod >= July2009)
            {
                RoomSUB report = await ReportFactory.GetReportRoomSUB(StartPeriod, EndPeriod, GetClientID());
                filePath = GenerateExcelSUB(report.Items, report.Summaries, "Labtime");
            }
            else
            {
                BillingUnit summaryUnit = new BillingUnit();
                RoomReport mgr = new RoomReport(StartPeriod, EndPeriod, BillingTypeManager);
                DataTable dtRoom = mgr.GenerateDataTable(summaryUnit);
                filePath = mgr.GenerateExcelFile(dtRoom);
            }

            OutputExcel(filePath);
        }

        protected void btnRoomJU_Command(object sender, CommandEventArgs e)
        {
            var juType = (JournalUnitTypes)Enum.Parse(typeof(JournalUnitTypes), e.CommandArgument.ToString());

            RegisterAsyncTask(new PageAsyncTask(async () =>
            {
                RoomJU report = await ReportFactory.GetReportRoomJU(StartPeriod, EndPeriod, juType, GetClientID());

                if (chkHTML.Checked)
                    ProcessHtmlRoomJU(report);
                else
                    ProcessExcelRoomJU(report);
            }));
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

        protected void btnAllRoomJU_Click(object sender, EventArgs e)
        {
            RegisterAsyncTask(new PageAsyncTask(ProcessHtmlAllRoomJU));
        }

        private async Task ProcessHtmlAllRoomJU()
        {
            var juA = await ReportFactory.GetReportRoomJU(StartPeriod, EndPeriod, JournalUnitTypes.A, GetClientID());
            var juB = await ReportFactory.GetReportRoomJU(StartPeriod, EndPeriod, JournalUnitTypes.B, GetClientID());
            var juC = await ReportFactory.GetReportRoomJU(StartPeriod, EndPeriod, JournalUnitTypes.C, GetClientID());

            double total;
            List<JournalUnitReportItem> allItems = ReportFactory.GetAllRoomJU(juA, juB, juC, out total);

            gvJU.Columns[1].Visible = false;
            gvJU.Columns[2].Visible = true;
            gvJU.Columns[3].Visible = EndPeriod != StartPeriod.AddMonths(1);
            gvJU.DataSource = allItems;
            gvJU.DataBind();
            SetTotalText(total);
            SetLinkText("ju-all", "room");
        }

        protected void btnReportTool_Click(object sender, EventArgs e)
        {
            RegisterAsyncTask(new PageAsyncTask(async () =>
            {
                if (chkHTML.Checked)
                    await ProcessHtmlToolSUB();
                else
                    await ProcessExcelToolSUB();
            }));
        }

        private async Task ProcessHtmlToolSUB()
        {
            ToolSUB report = await ReportFactory.GetReportToolSUB(StartPeriod, EndPeriod, GetClientID());
            LoadGridSUB(report.Items, report.Summaries);
            SetLinkText("sub", "tool");
        }

        private async Task ProcessExcelToolSUB()
        {
            string filePath;

            if (StartPeriod >= July2009)
            {
                ToolSUB report = await ReportFactory.GetReportToolSUB(StartPeriod, EndPeriod, GetClientID());
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

        protected void btnToolJU_Command(object sender, CommandEventArgs e)
        {
            JournalUnitTypes juType = (JournalUnitTypes)Enum.Parse(typeof(JournalUnitTypes), e.CommandArgument.ToString());

            RegisterAsyncTask(new PageAsyncTask(async () =>
            {
                ToolJU report = await ReportFactory.GetReportToolJU(StartPeriod, EndPeriod, juType, GetClientID());

                if (chkHTML.Checked)
                    ProcessHtmlToolJU(report);
                else
                    ProcessExcelToolJU(report);
            }));
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

        protected void btnAllToolJU_Click(object sender, EventArgs e)
        {
            RegisterAsyncTask(new PageAsyncTask(ProcessAllToolJUHTML));
        }

        private async Task ProcessAllToolJUHTML()
        {
            var juA = await ReportFactory.GetReportToolJU(StartPeriod, EndPeriod, JournalUnitTypes.A, GetClientID());
            var juB = await ReportFactory.GetReportToolJU(StartPeriod, EndPeriod, JournalUnitTypes.B, GetClientID());
            var juC = await ReportFactory.GetReportToolJU(StartPeriod, EndPeriod, JournalUnitTypes.C, GetClientID());

            double total;
            List<JournalUnitReportItem> allItems = ReportFactory.GetAllToolJU(juA, juB, juC, out total);

            gvJU.Columns[1].Visible = false;
            gvJU.Columns[2].Visible = true;
            gvJU.Columns[3].Visible = EndPeriod != StartPeriod.AddMonths(1);
            gvJU.DataSource = allItems;
            gvJU.DataBind();
            SetTotalText(total);
            SetLinkText("ju-all", "tool");
        }

        protected void btnReportStore_Click(object sender, EventArgs e)
        {
            RegisterAsyncTask(new PageAsyncTask(async () =>
            {
                if (chkHTML.Checked)
                    await ProcessHtmlStoreSUB(false);
                else
                    await ProcessExcelStoreSUB(false);
            }));
        }

        protected void btnReportStoreTwoAccounts_Click(object sender, EventArgs e)
        {
            RegisterAsyncTask(new PageAsyncTask(async () =>
            {
                if (chkHTML.Checked)
                    await ProcessHtmlStoreSUB(true);
                else
                    await ProcessExcelStoreSUB(true);
            }));
        }

        private async Task ProcessHtmlStoreSUB(bool twoCreditAccounts)
        {
            StoreSUB report = await ReportFactory.GetReportStoreSUB(StartPeriod, EndPeriod, twoCreditAccounts, GetClientID());
            LoadGridSUB(report.Items, report.Summaries);
            SetLinkText("sub", "store", twoCreditAccounts);
        }

        private async Task ProcessExcelStoreSUB(bool twoCreditAccounts)
        {
            string filePath = string.Empty;
            if (twoCreditAccounts)
            {
                if (StartPeriod >= July2009)
                {
                    StoreSUB report = await ReportFactory.GetReportStoreSUB(StartPeriod, EndPeriod, twoCreditAccounts, GetClientID());
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
                if (StartPeriod >= July2009)
                {
                    StoreSUB report = await ReportFactory.GetReportStoreSUB(StartPeriod, EndPeriod, twoCreditAccounts, GetClientID());
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

        protected void btnAllSUB_Click(object sender, EventArgs e)
        {
            RegisterAsyncTask(new PageAsyncTask(ProcessHtmlAllSUB));
        }

        private async Task ProcessHtmlAllSUB()
        {
            RoomSUB roomSUB = await ReportFactory.GetReportRoomSUB(StartPeriod, EndPeriod, GetClientID());
            ToolSUB toolSUB = await ReportFactory.GetReportToolSUB(StartPeriod, EndPeriod, GetClientID());
            StoreSUB storeSUB = await ReportFactory.GetReportStoreSUB(StartPeriod, EndPeriod, false, GetClientID());

            double total;
            IEnumerable<ServiceUnitBillingReportItem> allItems = ReportFactory.GetAllSUB(roomSUB, toolSUB, storeSUB, out total);

            gvSUB.Columns[1].Visible = true;
            gvSUB.Columns[2].Visible = EndPeriod != StartPeriod.AddMonths(1);
            gvSUB.DataSource = allItems;
            gvSUB.DataBind();
            SetTotalText(total);
            SetLinkText("sub", "all");
        }

        protected void btnAllJU_Command(object sender, CommandEventArgs e)
        {
            JournalUnitTypes juType = (JournalUnitTypes)Enum.Parse(typeof(JournalUnitTypes), e.CommandArgument.ToString(), true);

            RegisterAsyncTask(new PageAsyncTask(async () =>
            {
                await ProcessHtmlAllJU(juType);
            }));
        }

        private async Task ProcessHtmlAllJU(JournalUnitTypes juType)
        {
            double total;
            List<JournalUnitReportItem> allItems;

            switch (juType)
            {
                case JournalUnitTypes.A:
                case JournalUnitTypes.B:
                case JournalUnitTypes.C:
                    RoomJU roomJU = await ReportFactory.GetReportRoomJU(StartPeriod, EndPeriod, juType, GetClientID());
                    ToolJU toolJU = await ReportFactory.GetReportToolJU(StartPeriod, EndPeriod, juType, GetClientID());
                    allItems = ReportFactory.GetAllJU(roomJU, toolJU, out total);
                    break;
                case JournalUnitTypes.All:
                    RoomJU roomJUA = await ReportFactory.GetReportRoomJU(StartPeriod, EndPeriod, JournalUnitTypes.A, GetClientID());
                    ToolJU toolJUA = await ReportFactory.GetReportToolJU(StartPeriod, EndPeriod, JournalUnitTypes.A, GetClientID());
                    RoomJU roomJUB = await ReportFactory.GetReportRoomJU(StartPeriod, EndPeriod, JournalUnitTypes.B, GetClientID());
                    ToolJU toolJUB = await ReportFactory.GetReportToolJU(StartPeriod, EndPeriod, JournalUnitTypes.B, GetClientID());
                    RoomJU roomJUC = await ReportFactory.GetReportRoomJU(StartPeriod, EndPeriod, JournalUnitTypes.C, GetClientID());
                    ToolJU toolJUC = await ReportFactory.GetReportToolJU(StartPeriod, EndPeriod, JournalUnitTypes.C, GetClientID());

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
            string templatePath = ExcelUtility.GetTemplatePath("SUB_Template.xlt");
            string workPathDir = ExcelUtility.GetWorkPath(CacheManager.Current.CurrentUser.ClientID);
            ExcelLite.SetLicense("EL6N-Z669-AZZG-3LS7");
            ExcelFile SpreadSheet = new ExcelFile();
            SpreadSheet.LoadXls(templatePath);
            ExcelWorksheet ws = SpreadSheet.Worksheets[0];

            int iRow = 1; //zero based, iRow = 0 is Row #1 on spreadsheet (the header)

            //dv.Sort = "ItemDescription ASC, ProjectGrant ASC";
            foreach (ServiceUnitBillingReportItem item in items[0])
            {
                if (!string.IsNullOrEmpty(item.CardType))
                {
                    ws.Cells[iRow, 0].Value = item.CardType;
                    ws.Cells[iRow, 1].Value = item.ShortCode;
                    ws.Cells[iRow, 2].Value = item.Account;
                    ws.Cells[iRow, 3].Value = item.FundCode;
                    ws.Cells[iRow, 4].Value = item.DeptID;
                    ws.Cells[iRow, 5].Value = item.ProgramCode;
                    ws.Cells[iRow, 6].Value = item.Class;
                    ws.Cells[iRow, 7].Value = item.ProjectGrant;
                    ws.Cells[iRow, 8].Value = item.VendorID;
                    ws.Cells[iRow, 9].Value = item.InvoiceDate;
                    ws.Cells[iRow, 10].Value = item.InvoiceID;

                    string uniqName = item.Uniqname.ToString();
                    if (uniqName.Length > 8)
                        uniqName = uniqName.Substring(0, 8);

                    ws.Cells[iRow, 11].Value = uniqName;
                    ws.Cells[iRow, 15].Value = item.DepartmentalReferenceNumber;

                    //R = 17
                    //[2014-09-08 jg] All column S and higher are now shifted one to the left so 18 -> 17, 19 -> 18, etc
                    //ws.Cell[iRow, 18].Value = item.ItemDescription;
                    //ws.Cell[iRow, 24].Value = item.QuantityVouchered;
                    //ws.Cell[iRow, 26].Value = string.IsNullOrEmpty(item.UnitOfMeasure) ? 0 : Convert.ToDouble(item.UnitOfMeasure);
                    //ws.Cell[iRow, 27].Value = string.IsNullOrEmpty(item.MerchandiseAmount) ? 0 : Convert.ToDouble(item.MerchandiseAmount);

                    ws.Cells[iRow, 17].Value = item.ItemDescription;
                    ws.Cells[iRow, 23].Value = item.QuantityVouchered;
                    ws.Cells[iRow, 25].Value = string.IsNullOrEmpty(item.UnitOfMeasure) ? 0 : Convert.ToDouble(item.UnitOfMeasure);
                    ws.Cells[iRow, 26].Value = string.IsNullOrEmpty(item.MerchandiseAmount) ? 0 : Convert.ToDouble(item.MerchandiseAmount);

                    iRow += 1;
                }
            }

            ws.Cells[iRow, 0].Value = SummaryUnit[0].CardType;
            ws.Cells[iRow, 1].Value = SummaryUnit[0].ShortCode;
            ws.Cells[iRow, 2].Value = SummaryUnit[0].Account;
            ws.Cells[iRow, 3].Value = SummaryUnit[0].FundCode;
            ws.Cells[iRow, 4].Value = SummaryUnit[0].DeptID;
            ws.Cells[iRow, 5].Value = SummaryUnit[0].ProgramCode;
            ws.Cells[iRow, 6].Value = SummaryUnit[0].ClassName;
            ws.Cells[iRow, 7].Value = SummaryUnit[0].ProjectGrant;
            ws.Cells[iRow, 9].Value = SummaryUnit[0].InvoiceDate;
            ws.Cells[iRow, 11].Value = SummaryUnit[0].Uniqname;

            //ws.Cells[iRow, 18].Value = SummaryUnit[0].ItemDescription;
            //ws.Cells[iRow, 24].Value = SummaryUnit[0].QuantityVouchered;
            //ws.Cells[iRow, 26].Value = Math.Round(SummaryUnit[0].MerchandiseAmount, 5);
            //ws.Cells[iRow, 27].Formula = string.Format("=-SUM(AB2:AB{0})", iRow);

            ws.Cells[iRow, 17].Value = SummaryUnit[0].ItemDescription;
            ws.Cells[iRow, 23].Value = SummaryUnit[0].QuantityVouchered;
            ws.Cells[iRow, 25].Value = Math.Round(SummaryUnit[0].MerchandiseAmount, 5);
            ws.Cells[iRow, 26].Formula = string.Format("=-SUM(AB2:AB{0})", iRow);

            if (items.Length > 1 && SummaryUnit.Length > 1)
            {
                iRow += 1;
                int startingRowNumber = iRow + 1; //we use excel formula to calculate the sum, so we have to keep this to get the starting range

                //dv.Sort = "ItemDescription ASC, ProjectGrant ASC";
                foreach (ServiceUnitBillingReportItem item in items[1])
                {
                    if (!string.IsNullOrEmpty(item.CardType))
                    {
                        ws.Cells[iRow, 0].Value = item.CardType;
                        ws.Cells[iRow, 1].Value = item.ShortCode;
                        ws.Cells[iRow, 2].Value = item.Account;
                        ws.Cells[iRow, 3].Value = item.FundCode;
                        ws.Cells[iRow, 4].Value = item.DeptID;
                        ws.Cells[iRow, 5].Value = item.ProgramCode;
                        ws.Cells[iRow, 6].Value = item.Class;
                        ws.Cells[iRow, 7].Value = item.ProjectGrant;
                        ws.Cells[iRow, 8].Value = item.VendorID;
                        ws.Cells[iRow, 9].Value = item.InvoiceDate;
                        ws.Cells[iRow, 10].Value = item.InvoiceID;

                        string uniqName = item.Uniqname;
                        if (uniqName.Length > 8)
                            uniqName = uniqName.Substring(0, 8);

                        ws.Cells[iRow, 11].Value = uniqName;
                        ws.Cells[iRow, 15].Value = item.DepartmentalReferenceNumber;

                        //ws.Cells[iRow, 18].Value = item.ItemDescription;
                        //ws.Cells[iRow, 24].Value = item.QuantityVouchered;
                        //ws.Cells[iRow, 26].Value = string.IsNullOrEmpty(item.UnitOfMeasure) ? 0 : Convert.ToDouble(item.UnitOfMeasure);
                        //ws.Cells[iRow, 27].Value = string.IsNullOrEmpty(item.MerchandiseAmount) ? 0 : Convert.ToDouble(item.MerchandiseAmount);

                        ws.Cells[iRow, 17].Value = item.ItemDescription;
                        ws.Cells[iRow, 23].Value = item.QuantityVouchered;
                        ws.Cells[iRow, 25].Value = string.IsNullOrEmpty(item.UnitOfMeasure) ? 0 : Convert.ToDouble(item.UnitOfMeasure);
                        ws.Cells[iRow, 26].Value = string.IsNullOrEmpty(item.MerchandiseAmount) ? 0 : Convert.ToDouble(item.MerchandiseAmount);

                        iRow += 1;
                    }
                }

                ws.Cells[iRow, 0].Value = SummaryUnit[1].CardType;
                ws.Cells[iRow, 1].Value = SummaryUnit[1].ShortCode;
                ws.Cells[iRow, 2].Value = SummaryUnit[1].Account;
                ws.Cells[iRow, 3].Value = SummaryUnit[1].FundCode;
                ws.Cells[iRow, 4].Value = SummaryUnit[1].DeptID;
                ws.Cells[iRow, 5].Value = SummaryUnit[1].ProgramCode;
                ws.Cells[iRow, 6].Value = SummaryUnit[1].ClassName;
                ws.Cells[iRow, 7].Value = SummaryUnit[1].ProjectGrant;
                ws.Cells[iRow, 9].Value = SummaryUnit[1].InvoiceDate;
                ws.Cells[iRow, 11].Value = SummaryUnit[1].Uniqname;

                //ws.Cells[iRow, 18].Value = SummaryUnit[1].ItemDescription;
                //ws.Cells[iRow, 24].Value = SummaryUnit[1].QuantityVouchered;
                //ws.Cells[iRow, 26].Value = Math.Round(SummaryUnit[0].MerchandiseAmount, 5);
                //ws.Cells[iRow, 27].Formula = string.Format("=-SUM(AB{0}:AB{1})", startingRowNumber, iRow);

                ws.Cells[iRow, 17].Value = SummaryUnit[1].ItemDescription;
                ws.Cells[iRow, 23].Value = SummaryUnit[1].QuantityVouchered;
                ws.Cells[iRow, 25].Value = Math.Round(SummaryUnit[0].MerchandiseAmount, 5);
                ws.Cells[iRow, 26].Formula = string.Format("=-SUM(AB{0}:AB{1})", startingRowNumber, iRow);

                ws.Columns["I"].Collapsed = true;
                ws.Columns["J"].Collapsed = true;
                ws.Columns[10].Width = 1;
            }

            SpreadSheet.Worksheets.ActiveWorksheet = SpreadSheet.Worksheets[0];

            if (!Directory.Exists(workPathDir))
                Directory.CreateDirectory(workPathDir);

            string workFilePath = Path.Combine(workPathDir, ChargeType + "SUB_" + StartPeriod.ToString("yyyy-MM"));

            if (EndPeriod == StartPeriod.AddMonths(1))
                workFilePath += ".xls";
            else
                workFilePath += "_" + EndPeriod.AddMonths(-1).ToString("yyyy-MM") + ".xls";

            SpreadSheet.SaveXls(workFilePath);
            SpreadSheet = null;
            GC.Collect();

            return workFilePath;
        }

        private string GenerateExcelJU(JournalUnitReportItem[] items, CreditEntry creditEntry, BillingCategory billingCategory, JournalUnitTypes juType)
        {
            //Contruct the excel object
            string templatePath = ExcelUtility.GetTemplatePath("JU_Template.xls");

            ExcelLite.SetLicense("EL6N-Z669-AZZG-3LS7");
            ExcelFile SpreadSheet = new ExcelFile();
            SpreadSheet.LoadXls(templatePath);
            ExcelWorksheet ws = SpreadSheet.Worksheets[0];

            DateTime Period = EndPeriod.AddMonths(-1);
            string billingCategoryName = Enum.GetName(typeof(BillingCategory), billingCategory);

            //We start at first row, because for ExcelLite control, the header row is not included
            ws.Cells[2, 0].Value = "H";
            ws.Cells[2, 1].Value = "NEXT";
            ws.Cells[2, 2].Value = DateTime.Now.ToString("MM/dd/yyyy");
            ws.Cells[2, 3].Value = "JU";
            ws.Cells[2, 4].Value = Period.ToString("yyyy/MM");
            ws.Cells[2, 5].Value = string.Format("JU {0} {1:MM/yy} LNF {2}{3}", GetJournalUnitNumber(Period, billingCategory, juType), Period, billingCategoryName, juType);
            ws.Cells[2, 6].Value = "Rice";

            int iRow = 3;
            int iCol = 7;

            //DataView dv = dt.DefaultView;
            //dv.Sort = "ItemDescription ASC, ProjectGrant ASC";
            foreach (JournalUnitReportItem item in items.Where(i => i.ItemDescription != "zzdoscar"))
            {
                ws.Cells[iRow, 0].Value = "L";
                ws.Cells[iRow, iCol].Value = item.Account;
                ws.Cells[iRow, iCol + 1].Value = item.FundCode;
                ws.Cells[iRow, iCol + 2].Value = item.DeptID;
                ws.Cells[iRow, iCol + 3].Value = item.ProgramCode;
                ws.Cells[iRow, iCol + 4].Value = item.Class;
                ws.Cells[iRow, iCol + 5].Value = item.ProjectGrant;
                ws.Cells[iRow, iCol + 6].Value = string.IsNullOrEmpty(item.MerchandiseAmount) ? 0 : Convert.ToDouble(item.MerchandiseAmount);
                ws.Cells[iRow, iCol + 7].Value = item.DepartmentalReferenceNumber;
                ws.Cells[iRow, iCol + 8].Value = item.ItemDescription;
                iRow += 1;
            }

            //Add the last row - which is the summary unit
            ws.Cells[iRow, 0].Value = "L";
            ws.Cells[iRow, iCol].Value = creditEntry.Account;
            ws.Cells[iRow, iCol + 1].Value = creditEntry.FundCode;
            ws.Cells[iRow, iCol + 2].Value = creditEntry.DeptID;
            ws.Cells[iRow, iCol + 3].Value = creditEntry.ProgramCode;
            ws.Cells[iRow, iCol + 4].Value = creditEntry.ClassName;
            ws.Cells[iRow, iCol + 5].Value = creditEntry.ProjectGrant;
            ws.Cells[iRow, iCol + 6].Value = creditEntry.MerchandiseAmount;
            //ws.Cells[iRow, iCol + 6].Formula = "=-SUM(AB2:AB" + iRow.ToString() + ")";
            ws.Cells[iRow, iCol + 7].Value = creditEntry.DepartmentalReferenceNumber;
            ws.Cells[iRow, iCol + 8].Value = creditEntry.ItemDescription;

            SpreadSheet.Worksheets.ActiveWorksheet = SpreadSheet.Worksheets[0];

            string workPathDir = ExcelUtility.GetWorkPath(CacheManager.Current.CurrentUser.ClientID);

            if (!Directory.Exists(workPathDir))
                Directory.CreateDirectory(workPathDir);

            string workFilePath = Path.Combine(workPathDir, "JU" + Enum.GetName(typeof(JournalUnitTypes), juType) + "_" + billingCategoryName + "_" + Period.ToString("yyyy-MM") + ".xls");
            SpreadSheet.SaveXls(workFilePath);
            SpreadSheet = null;

            GC.Collect();

            return workFilePath;
        }

        private int GetJournalUnitNumber(DateTime period, BillingCategory billingCategory, JournalUnitTypes juType)
        {
            int yearOff = period.Year - July2010.Year;
            int monthOff = period.Month - July2010.Month;

            int increment = (yearOff * 12 + monthOff) * 6;

            //263 is the starting number for room sub in July 2010
            if (billingCategory == BillingCategory.Room && juType == JournalUnitTypes.A)
                return 100 + increment;
            else if (billingCategory == BillingCategory.Room && juType == JournalUnitTypes.B)
                return 100 + increment + 1;
            else if (billingCategory == BillingCategory.Room && juType == JournalUnitTypes.C)
                return 100 + increment + 2;
            else if (billingCategory == BillingCategory.Tool && juType == JournalUnitTypes.A)
                return 100 + increment + 3;
            else if (billingCategory == BillingCategory.Tool && juType == JournalUnitTypes.B)
                return 100 + increment + 4;
            else if (billingCategory == BillingCategory.Tool && juType == JournalUnitTypes.C)
                return 100 + increment + 5;
            else
                throw new ArgumentException("Invalid arguments passed to GetJournalUnitNumber in RepSUB.aspx");
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

        protected void chkHTML_CheckChanged(object sender, EventArgs e)
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
    }
}