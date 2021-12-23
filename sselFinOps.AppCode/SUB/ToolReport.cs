using LNF.Billing;
using LNF.Billing.Reports.ServiceUnitBilling;
using LNF.CommonTools;
using sselFinOps.AppCode.DAL;
using System;
using System.Data;
using System.Linq;

namespace sselFinOps.AppCode.SUB
{
    public class ToolReport : ReportBase
    {
        public ToolReport(DateTime startPeriod, DateTime endPeriod) : base(startPeriod, endPeriod) { }

        protected override void FillDataTable(DataTable dt)
        {
            BillingUnit summaryUnit = summaryUnits.First();

            Compile mCompile = new Compile();
            DataTable dtToolDB = mCompile.CalcCost("Tool", string.Empty, "ChargeTypeID", 5, EndPeriod.AddMonths(-1), 0, 0, Compile.AggType.CliAcct);
            DataTable dtClientWithCharges = mCompile.GetTable(1);
            double toolCapCost = mCompile.CapCost;

            // cap costs - capping is per ClientOrg, thus apportion capping across charges
            // note that this assumes that there is only one org for internal academic!!!
            object temp;
            double totalToolCharges;
            foreach (DataRow drCWC in dtClientWithCharges.Rows)
            {
                temp = dtToolDB.Compute("SUM(TotalCalcCost)", string.Format("ClientID = {0}", drCWC["ClientID"]));
                if (temp == null || temp == DBNull.Value)
                    totalToolCharges = 0;
                else
                    totalToolCharges = Convert.ToDouble(temp);

                if (totalToolCharges > toolCapCost)
                {
                    DataRow[] fdr = dtToolDB.Select(string.Format("ClientID = {0}", drCWC["ClientID"]));
                    for (int i = 0; i < fdr.Length; i++)
                        fdr[i].SetField("TotalCalcCost", fdr[i].Field<double>("TotalCalcCost") * toolCapCost / totalToolCharges);
                }
            }

            DataTable dtClient = ClientDA.GetAllClient(StartPeriod, EndPeriod);
            DataTable dtAccount = AccountDA.GetAllInternalAccount(StartPeriod, EndPeriod);
            DataTable dtClientAccount = ClientDA.GetAllClientAccountWithManagerName(StartPeriod, EndPeriod); //used to find out manager's name
            DataTable dtBillingType = BillingTypeDA.GetAllBillingTypes();

            //Get the general lab account ID and lab credit account ID
            GlobalCost gc = GlobalCostDA.GetGlobalCost();

            //For performance issue, we have to calculate something first, since it's used on all rows
            string depRefNum = string.Empty;
            double fTotal = 0;
            string creditAccount = dtAccount.Rows.Find(gc.LabCreditAccountID)["Number"].ToString();
            string creditAccountShortCode = dtAccount.Rows.Find(gc.LabCreditAccountID)["ShortCode"].ToString();

            //Do not show an item if the charge and xcharge accounts are the 'same' - can only happen for 941975
            //Do not show items that are associated with specific accounts - need to allow users to add manually here in future
            foreach (DataRow sdr in dtToolDB.Rows)
            {
                if (sdr.Field<double>("TotalCalcCost") > 0)
                {
                    var excludedAccounts = new[] { gc.LabAccountID, 143, 179, 188 };

                    if (!excludedAccounts.Contains(sdr.Field<int>("AccountID")) && sdr.Field<int>("BillingType") != BillingTypes.Other)
                    {
                        DataRow ndr = dt.NewRow();

                        DataRow drAccount = dtAccount.Rows.Find(sdr.Field<int>("AccountID"));
                        string debitAccount = drAccount["Number"].ToString();
                        string shortCode = drAccount["ShortCode"].ToString();

                        //get manager's name
                        DataRow[] drClientAccount = dtClientAccount.Select(string.Format("AccountID = {0}", sdr["AccountID"]));
                        if (drClientAccount.Length > 0)
                            depRefNum = drClientAccount.First()["ManagerName"].ToString();
                        else
                            depRefNum = "No Manager Found";

                        AccountNumber debitAccountNum = AccountNumber.Parse(debitAccount);
                        ndr["CardType"] = 1;
                        ndr["ShortCode"] = shortCode;
                        ndr["Account"] = debitAccountNum.Account;
                        ndr["FundCode"] = debitAccountNum.FundCode;
                        ndr["DeptID"] = debitAccountNum.DeptID;
                        ndr["ProgramCode"] = debitAccountNum.ProgramCode;
                        ndr["Class"] = debitAccountNum.Class;
                        ndr["ProjectGrant"] = debitAccountNum.ProjectGrant;
                        ndr["VendorID"] = "0000456136";
                        ndr["InvoiceDate"] = EndPeriod.AddMonths(-1).ToString("yyyy/MM/dd");
                        ndr["InvoiceID"] = $"{ReportSettings.CompanyName} Tool Charge";
                        ndr["Uniqname"] = dtClient.Rows.Find(sdr.Field<int>("ClientID"))["UserName"];
                        ndr["DepartmentalReferenceNumber"] = depRefNum;
                        ndr["ItemDescription"] = GetItemDesc(sdr, dtClient, dtBillingType);
                        ndr["QuantityVouchered"] = "1.0000";
                        double chargeAmount = Math.Round(sdr.Field<double>("TotalCalcCost"), 5);
                        ndr["UnitOfMeasure"] = chargeAmount.ToString();
                        ndr["MerchandiseAmount"] = Math.Round(chargeAmount, 2).ToString();
                        ndr["CreditAccount"] = creditAccount;
                        //Used to calculate the total credit amount
                        fTotal += chargeAmount;

                        dt.Rows.Add(ndr);
                    }
                }
            }

            //Summary row
            AccountNumber creditAccountNum = AccountNumber.Parse(creditAccount);
            summaryUnit.CardType = 1;
            summaryUnit.ShortCode = creditAccountShortCode;
            summaryUnit.Account = creditAccountNum.Account;
            summaryUnit.FundCode = creditAccountNum.FundCode;
            summaryUnit.DeptID = creditAccountNum.DeptID;
            summaryUnit.ProgramCode = creditAccountNum.ProgramCode;
            summaryUnit.ClassName = creditAccountNum.Class;
            summaryUnit.ProjectGrant = creditAccountNum.ProjectGrant;
            summaryUnit.InvoiceDate = EndPeriod.AddMonths(-1).ToString("yyyy/MM/dd");
            summaryUnit.Uniqname = ReportSettings.FinancialManagerUserName;
            summaryUnit.DepartmentalReferenceNumber = depRefNum;
            summaryUnit.ItemDescription = ReportSettings.FinancialManagerUserName;
            summaryUnit.MerchandiseAmount = Math.Round(-fTotal, 2);
            summaryUnit.CreditAccount = creditAccount;

            //Clean things up manually might help performance in general
            dtToolDB.Clear();
            dtClient.Clear();
            dtAccount.Clear();
        }

        public override string GenerateExcelFile(DataTable dt)
        {
            return GenerateExcel(dt, "ToolSUB");
        }
    }
}