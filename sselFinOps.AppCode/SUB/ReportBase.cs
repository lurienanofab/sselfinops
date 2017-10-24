using GemBox.ExcelLite;
using LNF.Models.Billing.Reports.ServiceUnitBilling;
using System;
using System.Data;
using System.IO;
using System.Linq;
using System.Web;

namespace sselFinOps.AppCode.SUB
{
    public abstract class ReportBase
    {
        private DateTime _StartPeriod;
        private DateTime _EndPeriod;
        protected BillingUnit[] summaryUnits;

        public string CompanyName { get { return "LNF"; } }
        public string FinancialManagerUserName { get { return "doscar"; } }
        public DateTime July2010 { get { return new DateTime(2010, 7, 1); } }
        public DateTime StartPeriod { get { return _StartPeriod; } }
        public DateTime EndPeriod { get { return _EndPeriod; } }

        public ReportBase(DateTime startPeriod, DateTime endPeriod)
        {
            _StartPeriod = startPeriod;
            _EndPeriod = endPeriod;
        }

        //The table built represents the SUB excel file, so it has all the columns the SUB excel file has
        protected void BuildDataTable(DataTable dt)
        {
            dt.Columns.Add("Period", typeof(DateTime));
            dt.Columns.Add("CardType", typeof(string));
            dt.Columns.Add("ShortCode", typeof(string));
            dt.Columns.Add("Account", typeof(string));
            dt.Columns.Add("FundCode", typeof(string));
            dt.Columns.Add("DeptID", typeof(string));
            dt.Columns.Add("ProgramCode", typeof(string));
            dt.Columns.Add("Class", typeof(string));
            dt.Columns.Add("ProjectGrant", typeof(string));
            dt.Columns.Add("VendorID", typeof(string));
            dt.Columns.Add("InvoiceDate", typeof(string));
            dt.Columns.Add("InvoiceID", typeof(string));
            dt.Columns.Add("Uniqname", typeof(string));
            dt.Columns.Add("LocationCode", typeof(string));
            dt.Columns.Add("DeliverTo", typeof(string));
            dt.Columns.Add("VendorOrderNum", typeof(string));
            dt.Columns.Add("DepartmentalReferenceNumber", typeof(string));
            dt.Columns.Add("Trip/EventNumber", typeof(string));
            dt.Columns.Add("ItemID", typeof(string));
            dt.Columns.Add("ItemDescription", typeof(string));
            dt.Columns.Add("VendorItemID", typeof(string));
            dt.Columns.Add("ManufacturerName", typeof(string));
            dt.Columns.Add("ModelNum", typeof(string));
            dt.Columns.Add("SerialNum", typeof(string));
            dt.Columns.Add("UMTagNum", typeof(string));
            dt.Columns.Add("QuantityVouchered", typeof(string));
            dt.Columns.Add("UnitOfMeasure", typeof(string));
            dt.Columns.Add("UnitPrice", typeof(string));
            dt.Columns.Add("MerchandiseAmount", typeof(string));
            dt.Columns.Add("VoucherComment", typeof(string));
            dt.Columns.Add("SubsidyDiscount", typeof(string));
            dt.Columns.Add("BilledCharge", typeof(string));
            dt.Columns.Add("UsageCharge", typeof(string));

            //Not belong in the excel format, but added for other purpose
            dt.Columns.Add("CreditAccount", typeof(string));
            dt.Columns.Add("AccountID", typeof(string));
        }

        //This class will generate a real excel file and return the file path of this newly generated excel file
        protected string GenerateExcel(DataTable dt, string JEType)
        {
            BillingUnit summaryUnit = summaryUnits.First();

            //Contruct the excel object
            string templatePath = HttpContext.Current.Server.MapPath("~\\SpreadSheets\\Templates\\SUB_Template.xlt");
            string workPathDir = HttpContext.Current.Server.MapPath("SpreadSheets\\Work");

            DirectoryInfo di = new DirectoryInfo(workPathDir);
            try
            {
                //Determine whether the directory exists.
                if (!di.Exists) di.Create();
            }
            catch (Exception ex)
            {
                return "Error - " + ex.Message;
            }

            ExcelLite.SetLicense("EL6N-Z669-AZZG-3LS7");
            ExcelFile spreadSheet = new ExcelFile();
            spreadSheet.LoadXls(templatePath);
            ExcelWorksheet ws = spreadSheet.Worksheets["Sheet1"];

            //We start at first row, because for ExcelLite control, the header row is not included
            int iRow = 1;
            DataView dv = dt.DefaultView;
            dv.Sort = "CreditAccount ASC, ItemDescription ASC, ProjectGrant ASC";
            foreach (DataRowView drv in dv)
            {
                ws.Cells[iRow, 0].Value = drv["CardType"];
                ws.Cells[iRow, 1].Value = drv["ShortCode"];
                ws.Cells[iRow, 2].Value = drv["Account"];
                ws.Cells[iRow, 3].Value = drv["FundCode"];
                ws.Cells[iRow, 4].Value = drv["DeptID"];
                ws.Cells[iRow, 5].Value = drv["ProgramCode"];
                ws.Cells[iRow, 6].Value = drv["Class"];
                ws.Cells[iRow, 7].Value = drv["ProjectGrant"];
                ws.Cells[iRow, 8].Value = drv["VendorID"];
                ws.Cells[iRow, 9].Value = drv["InvoiceDate"];
                ws.Cells[iRow, 10].Value = drv["InvoiceID"];
                string uniqName = drv["Uniqname"].ToString();
                if (uniqName.Length > 8)
                    uniqName = uniqName.Substring(0, 8);
                ws.Cells[iRow, 11].Value = uniqName;  //2009-01-20 Only 8 character is allowed
                ws.Cells[iRow, 15].Value = drv["DepartmentalReferenceNumber"];

                ws.Cells[iRow, 17].Value = drv["ItemDescription"];
                ws.Cells[iRow, 23].Value = drv["QuantityVouchered"];
                ws.Cells[iRow, 25].Value = Convert.ToDouble(drv["UnitOfMeasure"]);
                ws.Cells[iRow, 26].Value = Convert.ToDouble(drv["MerchandiseAmount"]);

                iRow += 1;
            }

            //Add the last row - which is the summary unit
            ws.Cells[iRow, 0].Value = summaryUnit.CardType;
            ws.Cells[iRow, 1].Value = summaryUnit.ShortCode;
            ws.Cells[iRow, 2].Value = summaryUnit.Account;
            ws.Cells[iRow, 3].Value = summaryUnit.FundCode;
            ws.Cells[iRow, 4].Value = summaryUnit.DeptID;
            ws.Cells[iRow, 5].Value = summaryUnit.ProgramCode;
            ws.Cells[iRow, 6].Value = summaryUnit.ClassName;
            ws.Cells[iRow, 7].Value = summaryUnit.ProjectGrant;
            ws.Cells[iRow, 9].Value = summaryUnit.InvoiceDate;
            ws.Cells[iRow, 11].Value = summaryUnit.Uniqname;

            ws.Cells[iRow, 17].Value = summaryUnit.ItemDescription;
            ws.Cells[iRow, 23].Value = "1.0000";
            ws.Cells[iRow, 25].Value = summaryUnit.MerchandiseAmount.ToString() + "000";
            ws.Cells[iRow, 26].Formula = string.Format("=-SUM(AB2:AB{0})", iRow);

            spreadSheet.Worksheets.ActiveWorksheet = spreadSheet.Worksheets[0];

            string workFilePath = workPathDir + "\\" + JEType;
            if (EndPeriod == StartPeriod.AddMonths(1))
                workFilePath += "_" + StartPeriod.ToString("yyyy-MM") + ".xls";
            else
                workFilePath += "_" + StartPeriod.ToString("yyyy-MM") + "_" + EndPeriod.ToString("yyyy-MM") + ".xls";

            spreadSheet.SaveXls(workFilePath);
            spreadSheet = null;
            GC.Collect();

            return workFilePath;
        }

        public int zGetSUBNumber(DateTime period, string SUBType)
        {
            int yearoff = period.Year - July2010.Year;
            int monthoff = period.Month - July2010.Month;

            int increment = (yearoff * 12 + monthoff) * 3;

            //263 is the starting number for room sub in July 2010
            if (SUBType == "Tool")
                return 263 + increment + 1;
            else if (SUBType == "Store")
                return 263 + increment + 2;
            else
                return 263 + increment;
        }

        protected string GetLineDesc(DataRow dr, DataTable dtClient, DataTable dtBillingType = null)
        {
            int clientId = dr.Field<int>("ClientID");
            DataRow drClient = dtClient.Rows.Find(clientId);
            string lname = drClient["LName"].ToString();
            string fname = drClient["FName"].ToString();
            int bt = dr.Field<int>("BillingType");
            DataRow drbt = null;
            string billingType = string.Empty;
            if (dtBillingType != null)
            {
                drbt = dtBillingType.Rows.Find(bt);
                billingType = "-" + (drbt == null ? string.Format("unknown_{0}", bt) : drbt["BillingTypeName"].ToString());
            }
            string lineDesc = FinancialManagerUserName + "-" + lname + "," + fname + billingType;
            return (lineDesc.Length > 30) ? lineDesc.Substring(0, 30) : lineDesc;
        }

        protected string GetItemDesc(DataRow dr, DataTable dtClient, DataTable dtBillingType)
        {
            int clientId = dr.Field<int>("ClientID");

            string itemDesc = dtClient.Rows.Find(clientId)["DisplayName"].ToString().Substring(0, 20) + "-";
            DataRow drbt = dtBillingType.Rows.Find(dr.Field<int>("BillingType"));
            if (drbt != null)
                itemDesc += drbt["BillingTypeName"].ToString().Substring(0, 9);
            else
                itemDesc += string.Format("unknown_{0}", dr["BillingType"]);

            return itemDesc;
        }

        public DataTable GenerateDataTable(params BillingUnit[] summaryUnits)
        {
            this.summaryUnits = summaryUnits;
            DataTable dt = new DataTable();
            BuildDataTable(dt);
            FillDataTable(dt);
            return dt;
        }

        protected abstract void FillDataTable(DataTable dt);

        public abstract string GenerateExcelFile(DataTable dt);
    }

    public struct AccountNumber
    {
        private AccountNumber(string account, string fundCode, string deptId, string programCode, string className, string projectGrant)
        {
            _Account = account;
            _FundCode = fundCode;
            _DeptID = deptId;
            _ProgramCode = programCode;
            _Class = className;
            _ProjectGrant = projectGrant;
        }

        private string _Account;
        private string _FundCode;
        private string _DeptID;
        private string _ProgramCode;
        private string _Class;
        private string _ProjectGrant;

        public static AccountNumber Parse(string number)
        {
            if (number.Length != 34)
                throw new ArgumentException("Length must be 34", "number");

            return new AccountNumber(
                account: number.Substring(0, 6),
                fundCode: number.Substring(6, 5),
                deptId: number.Substring(11, 6),
                programCode: number.Substring(17, 5),
                className: number.Substring(22, 5),
                projectGrant: number.Substring(27, 7)
            );
        }

        public string Account { get { return _Account; } }
        public string FundCode { get { return _FundCode; } }
        public string DeptID { get { return _DeptID; } }
        public string ProgramCode { get { return _ProgramCode; } }
        public string Class { get { return _Class; } }
        public string ProjectGrant { get { return _ProjectGrant; } }
    }
}