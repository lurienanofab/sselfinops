using LNF.Billing.Reports.ServiceUnitBilling;
using LNF.CommonTools;
using System;
using System.Data;
using System.IO;
using System.Linq;
using System.Web;

namespace sselFinOps.AppCode.SUB
{
    public abstract class ReportBase
    {
        protected BillingUnit[] summaryUnits;

        public DateTime July2010 => new DateTime(2010, 7, 1);
        public DateTime StartPeriod { get; }
        public DateTime EndPeriod { get; }

        public ReportBase(DateTime startPeriod, DateTime endPeriod)
        {
            StartPeriod = startPeriod;
            EndPeriod = endPeriod;
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
            string fileName = Utility.GetRequiredAppSetting("SUB_Template");
            string templatePath = HttpContext.Current.Server.MapPath($"~\\SpreadSheets\\Templates\\{fileName}");
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

            using (var mgr = ExcelUtility.NewExcelManager())
            {
                mgr.OpenWorkbook(templatePath);
                mgr.SetActiveWorksheet("Sheet1");

                //We start at first row, because for ExcelLite control, the header row is not included
                int iRow = 1;
                DataView dv = dt.DefaultView;
                dv.Sort = "CreditAccount ASC, ItemDescription ASC, ProjectGrant ASC";
                foreach (DataRowView drv in dv)
                {
                    mgr.SetCellTextValue(iRow, 0, drv["CardType"]);
                    mgr.SetCellTextValue(iRow, 1, drv["ShortCode"]);
                    mgr.SetCellTextValue(iRow, 2, drv["Account"]);
                    mgr.SetCellTextValue(iRow, 3, drv["FundCode"]);
                    mgr.SetCellTextValue(iRow, 4, drv["DeptID"]);
                    mgr.SetCellTextValue(iRow, 5, drv["ProgramCode"]);
                    mgr.SetCellTextValue(iRow, 6, drv["Class"]);
                    mgr.SetCellTextValue(iRow, 7, drv["ProjectGrant"]);
                    mgr.SetCellTextValue(iRow, 8, drv["VendorID"]);
                    mgr.SetCellTextValue(iRow, 9, drv["InvoiceDate"]);
                    mgr.SetCellTextValue(iRow, 10, drv["InvoiceID"]);
                    string uniqName = drv["Uniqname"].ToString();
                    if (uniqName.Length > 8)
                        uniqName = uniqName.Substring(0, 8);
                    mgr.SetCellTextValue(iRow, 11, uniqName);  //2009-01-20 Only 8 character is allowed
                    mgr.SetCellTextValue(iRow, 15, drv["DepartmentalReferenceNumber"]);

                    mgr.SetCellTextValue(iRow, 17, drv["ItemDescription"]);
                    mgr.SetCellNumberValue(iRow, 23, drv["QuantityVouchered"]);
                    mgr.SetCellNumberValue(iRow, 25, Convert.ToDouble(drv["UnitOfMeasure"]));
                    mgr.SetCellNumberValue(iRow, 26, Convert.ToDouble(drv["MerchandiseAmount"]));

                    iRow += 1;
                }

                //Add the last row - which is the summary unit
                mgr.SetCellTextValue(iRow, 0, summaryUnit.CardType);
                mgr.SetCellTextValue(iRow, 1, summaryUnit.ShortCode);
                mgr.SetCellTextValue(iRow, 2, summaryUnit.Account);
                mgr.SetCellTextValue(iRow, 3, summaryUnit.FundCode);
                mgr.SetCellTextValue(iRow, 4, summaryUnit.DeptID);
                mgr.SetCellTextValue(iRow, 5, summaryUnit.ProgramCode);
                mgr.SetCellTextValue(iRow, 6, summaryUnit.ClassName);
                mgr.SetCellTextValue(iRow, 7, summaryUnit.ProjectGrant);
                mgr.SetCellTextValue(iRow, 9, summaryUnit.InvoiceDate);
                mgr.SetCellTextValue(iRow, 11, summaryUnit.Uniqname);

                mgr.SetCellTextValue(iRow, 17, summaryUnit.ItemDescription);
                mgr.SetCellNumberValue(iRow, 23, "1.0000");
                mgr.SetCellNumberValue(iRow, 25, summaryUnit.MerchandiseAmount.ToString() + "000");
                mgr.SetCellFormula(iRow, 26, string.Format("=-SUM(AB2:AB{0})", iRow));

                string workFilePath = workPathDir + "\\" + JEType;
                if (EndPeriod == StartPeriod.AddMonths(1))
                    workFilePath += "_" + StartPeriod.ToString("yyyy-MM") + Path.GetExtension(fileName);
                else
                    workFilePath += "_" + StartPeriod.ToString("yyyy-MM") + "_" + EndPeriod.ToString("yyyy-MM") + Path.GetExtension(fileName);

                mgr.SaveAs(workFilePath);

                return workFilePath;
            }
        }

        protected string GetLineDesc(DataRow dr, DataTable dtClient, DataTable dtBillingType = null)
        {
            int clientId = dr.Field<int>("ClientID");
            DataRow drClient = dtClient.Rows.Find(clientId);
            string lname = drClient["LName"].ToString();
            string fname = drClient["FName"].ToString();
            int bt = dr.Field<int>("BillingType");
            string billingType = string.Empty;
            if (dtBillingType != null)
            {
                DataRow drbt = dtBillingType.Rows.Find(bt);
                billingType = "-" + (drbt == null ? string.Format("unknown_{0}", bt) : drbt["BillingTypeName"].ToString());
            }
            string lineDesc = ReportSettings.FinancialManagerUserName + "-" + lname + "," + fname + billingType;
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
        public string Account { get; }
        public string FundCode { get; }
        public string DeptID { get; }
        public string ProgramCode { get; }
        public string Class { get; }
        public string ProjectGrant { get; }

        private AccountNumber(string account, string fundCode, string deptId, string programCode, string className, string projectGrant)
        {
            Account = account;
            FundCode = fundCode;
            DeptID = deptId;
            ProgramCode = programCode;
            Class = className;
            ProjectGrant = projectGrant;
        }

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
    }
}