using LNF.Billing.Reports.ServiceUnitBilling;
using LNF.CommonTools;
using sselFinOps.AppCode.DAL;
using System;
using System.Data;
using System.IO;
using System.Web;

namespace sselFinOps.AppCode.SUB
{
    public class StoreReport : ReportBase
    {
        public StoreReport(DateTime startPeriod, DateTime endPeriod) : base(startPeriod, endPeriod) { }

        protected override void FillDataTable(DataTable dt)
        {
            BillingUnit summaryUnit1 = summaryUnits[0];
            BillingUnit summaryUnit2 = summaryUnits[1];

            Compile mCompile = new Compile();
            //Get Cleints who order items in store in the Period with Credit and Debit and TotalCost calculated
            DataTable dtStoreDB = mCompile.CalcCost("StoreJE", string.Empty, string.Empty, 0, EndPeriod.AddMonths(-1), 0, 0, Compile.AggType.CliAcct);

            //Return dtStoreDB
            //DataTable dtStore = new DataTable();
            //BuildDataTable(dtStore);

            DataTable dtClient = ClientDA.GetAllClient(StartPeriod, EndPeriod);
            DataTable dtAccount = AccountDA.GetAllInternalAccount(StartPeriod, EndPeriod);
            DataTable dtClientAccount = ClientDA.GetAllClientAccountWithManagerName(StartPeriod, EndPeriod); //used to find out manager's name

            //Get the general lab account ID and lab credit account ID
            GlobalCost gc = GlobalCostDA.GetGlobalCost();

            //For performance issue, we have to calculate something first, since it's used on all rows
            string depRefNum = string.Empty;
            double fTotal = 0;
            string lastCreditAccount = "default";
            string creditAccount = string.Empty;
            string creditAccountShortCode = string.Empty; //we also have to show those credit accounts' shortcodes
            string lastCreditAccountShortCode = string.Empty; //we need this, just like we need 'LastCreditAccount' to track the changes
            AccountNumber creditAccountNum;

            DataView dv = dtStoreDB.DefaultView;
            dv.Sort = "CreditAccountID";

            //This for loop will loop through each transaction record and create SUB record on every transactional record
            foreach (DataRowView sdr in dv)
            {
                //do not show an item if the charge and xcharge accounts are the 'same' - can only happen for 941975
                if (!(Convert.ToInt32(sdr["DebitAccountID"]) == gc.LabAccountID && Convert.ToInt32(sdr["CreditAccountID"]) == gc.LabCreditAccountID))
                {
                    DataRow ndr = dt.NewRow();

                    DataRow drAccount = dtAccount.Rows.Find(Convert.ToInt32(sdr["DebitAccountID"]));
                    string debitAccount = string.Empty;
                    string shortCode = string.Empty;
                    if (drAccount == null)
                    {
                        debitAccount = string.Format("unknown_{0}", sdr["DebitAccountID"]);
                        shortCode = string.Format("unknown_{0}", sdr["DebitAccountID"]);
                    }
                    else
                    {
                        debitAccount = drAccount["Number"].ToString();
                        shortCode = drAccount["ShortCode"].ToString();
                    }

                    //get manager's name
                    DataRow[] drClientAccount = dtClientAccount.Select(string.Format("AccountID = {0}", sdr["DebitAccountID"]));
                    if (drClientAccount.Length > 0)
                        depRefNum = drClientAccount[0]["ManagerName"].ToString();
                    else
                        depRefNum = "No Manager Found";

                    ndr["CardType"] = 1;
                    ndr["ShortCode"] = shortCode;
                    AccountNumber debitAccountNum = AccountNumber.Parse(debitAccount);
                    ndr["Account"] = debitAccountNum.Account;
                    ndr["FundCode"] = debitAccountNum.FundCode;
                    ndr["DeptID"] = debitAccountNum.DeptID;
                    ndr["ProgramCode"] = debitAccountNum.ProgramCode;
                    ndr["Class"] = debitAccountNum.Class;
                    ndr["ProjectGrant"] = debitAccountNum.ProjectGrant;

                    ndr["InvoiceDate"] = StartPeriod.ToString("yyyy/MM/dd");
                    ndr["InvoiceID"] = $"{ReportSettings.CompanyName} Store Charge";
                    ndr["Uniqname"] = dtClient.Rows.Find(Convert.ToInt32(sdr["ClientID"]))["UserName"];
                    ndr["DepartmentalReferenceNumber"] = depRefNum;
                    ndr["ItemDescription"] = dtClient.Rows.Find(Convert.ToInt32(sdr["ClientID"]))["DisplayName"].ToString().Substring(0, 30);
                    ndr["QuantityVouchered"] = "1.0000";
                    double chargeAmount = Math.Round(Convert.ToDouble(sdr["TotalCalcCost"]), 5);
                    ndr["UnitOfMeasure"] = chargeAmount;
                    ndr["MerchandiseAmount"] = Math.Round(chargeAmount, 2);
                    creditAccount = dtAccount.Rows.Find(Convert.ToInt32(sdr["CreditAccountID"]))["Number"].ToString();
                    ndr["CreditAccount"] = creditAccount;

                    //2008-10-09 Depend on credit account, we have different vendor ID
                    creditAccountNum = AccountNumber.Parse(creditAccount);
                    if (creditAccountNum.ProjectGrant == "U023440")
                        ndr["VendorID"] = "0000456136";
                    else
                        ndr["VendorID"] = "0000456133";

                    //Used to calculate the total credit amount
                    fTotal += chargeAmount;

                    //2008-10-08 We have to find out the shortcode for the credit account as well, requested by Sandrine
                    creditAccountShortCode = dtAccount.Rows.Find(Convert.ToInt32(sdr["CreditAccountID"]))["ShortCode"].ToString();

                    if (creditAccount != lastCreditAccount && lastCreditAccount != "default")
                    {
                        //Summary row
                        fTotal -= chargeAmount; //we have to deduct the charge amount again because its no longer belong to this group

                        AccountNumber lastCreditAccountNum = AccountNumber.Parse(lastCreditAccount);
                        summaryUnit2.CardType = 1;
                        summaryUnit2.ShortCode = lastCreditAccountShortCode;
                        summaryUnit2.Account = lastCreditAccountNum.Account;
                        summaryUnit2.FundCode = lastCreditAccountNum.FundCode;
                        summaryUnit2.DeptID = lastCreditAccountNum.DeptID;
                        summaryUnit2.ProgramCode = lastCreditAccountNum.ProgramCode;
                        summaryUnit2.ClassName = lastCreditAccountNum.Class;
                        summaryUnit2.ProjectGrant = lastCreditAccountNum.ProjectGrant;
                        summaryUnit2.InvoiceDate = EndPeriod.AddMonths(-1).ToString("yyyy/MM/dd");
                        summaryUnit2.Uniqname = "CreditAccount";
                        summaryUnit2.DepartmentalReferenceNumber = depRefNum;
                        summaryUnit2.ItemDescription = "CreditAccount";
                        summaryUnit2.MerchandiseAmount = Math.Round(-fTotal, 2);
                        summaryUnit2.CreditAccount = creditAccount;

                        fTotal = chargeAmount; //add the chargeamount back, because we have new group
                    }

                    lastCreditAccount = creditAccount;
                    lastCreditAccountShortCode = creditAccountShortCode;
                    dt.Rows.Add(ndr);
                }
            }

            //Summary row
            creditAccountNum = AccountNumber.Parse(creditAccount);
            summaryUnit1.CardType = 1;
            summaryUnit1.ShortCode = creditAccountShortCode;
            summaryUnit1.Account = creditAccountNum.Account;
            summaryUnit1.FundCode = creditAccountNum.FundCode;
            summaryUnit1.DeptID = creditAccountNum.DeptID;
            summaryUnit1.ProgramCode = creditAccountNum.ProgramCode;
            summaryUnit1.ClassName = creditAccountNum.Class;
            summaryUnit1.ProjectGrant = creditAccountNum.ProjectGrant;
            summaryUnit1.InvoiceDate = EndPeriod.AddMonths(-1).ToString("yyyy/MM/dd");
            summaryUnit1.Uniqname = "CreditAccount";
            summaryUnit1.DepartmentalReferenceNumber = depRefNum;
            summaryUnit1.ItemDescription = "CreditAccount";
            summaryUnit1.MerchandiseAmount = Math.Round(-fTotal, 2);
            summaryUnit1.CreditAccount = creditAccount;

            //Clean things up manually might help performance in general
            dtStoreDB.Clear();
            dtClient.Clear();
            dtAccount.Clear();
        }

        public override string GenerateExcelFile(DataTable dt)
        {
            BillingUnit summaryUnit1 = summaryUnits[0];
            BillingUnit summaryUnit2 = summaryUnits[1];

            //Complete code rewrite is needed in here
            //The problem stems form Material JE has two summary rows
            //The SummaryUnit and SummaryUnit2 are shifted due to Credit account # sorting.

            DataView dv = dt.DefaultView;
            dv.Sort = "CreditAccount ASC, ItemDescription ASC, ProjectGrant ASC";
            string lastCreditAccount = "default";

            //Contruct the excel object
            string fileName = Utility.GetRequiredAppSetting("SUB_Template");
            string templatePath = HttpContext.Current.Server.MapPath($".\\SpreadSheets\\Templates\\{fileName}");
            string workPathDir = HttpContext.Current.Server.MapPath(".\\SpreadSheets\\Work");

            using (var mgr = ExcelUtility.NewExcelManager())
            {
                mgr.OpenWorkbook(templatePath);
                mgr.SetActiveWorksheet("Sheet1");

                int iRow = 1;
                int iRowNumber2 = 0; //this keep the last row of the first portion of Store SUB, needed this to formulate correct formula for total sum cell

                foreach (DataRowView drv in dv)
                {
                    string creditAccount = drv["CreditAccount"].ToString();
                    if (creditAccount != lastCreditAccount && lastCreditAccount != "default" && summaryUnit2 != null)
                    {
                        mgr.SetCellTextValue(iRow, 0, summaryUnit1.CardType);
                        mgr.SetCellTextValue(iRow, 1, summaryUnit1.ShortCode);
                        mgr.SetCellTextValue(iRow, 2, summaryUnit1.Account);
                        mgr.SetCellTextValue(iRow, 3, summaryUnit1.FundCode);
                        mgr.SetCellTextValue(iRow, 4, summaryUnit1.DeptID);
                        mgr.SetCellTextValue(iRow, 5, summaryUnit1.ProgramCode);
                        mgr.SetCellTextValue(iRow, 6, summaryUnit1.ClassName);
                        mgr.SetCellTextValue(iRow, 7, summaryUnit1.ProjectGrant);
                        mgr.SetCellTextValue(iRow, 9, summaryUnit1.InvoiceDate);
                        mgr.SetCellTextValue(iRow, 11, summaryUnit1.Uniqname);
                        mgr.SetCellTextValue(iRow, 18, summaryUnit1.ItemDescription);
                        mgr.SetCellTextValue(iRow, 24, summaryUnit1.QuantityVouchered);
                        mgr.SetCellFormula(iRow, 27, string.Format("=-SUM(AB2:AB{0})", iRow));

                        iRow += 1;
                        iRowNumber2 = iRow + 1;
                    }

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
                    mgr.SetCellTextValue(iRow, 11, uniqName);
                    mgr.SetCellTextValue(iRow, 15, drv["DepartmentalReferenceNumber"]);
                    mgr.SetCellTextValue(iRow, 18, drv["ItemDescription"]);
                    mgr.SetCellTextValue(iRow, 24, drv["QuantityVouchered"]);
                    mgr.SetCellTextValue(iRow, 26, Convert.ToDouble(drv["UnitOfMeasure"]));
                    mgr.SetCellTextValue(iRow, 27, Convert.ToDouble(drv["MerchandiseAmount"]));

                    iRow += 1;
                    lastCreditAccount = creditAccount;
                }

                //Add the last row - which is the summary unit
                mgr.SetCellTextValue(iRow, 0, summaryUnit2.CardType);
                mgr.SetCellTextValue(iRow, 1, summaryUnit2.ShortCode);
                mgr.SetCellTextValue(iRow, 2, summaryUnit2.Account);
                mgr.SetCellTextValue(iRow, 3, summaryUnit2.FundCode);
                mgr.SetCellTextValue(iRow, 4, summaryUnit2.DeptID);
                mgr.SetCellTextValue(iRow, 5, summaryUnit2.ProgramCode);
                mgr.SetCellTextValue(iRow, 6, summaryUnit2.ClassName);
                mgr.SetCellTextValue(iRow, 7, summaryUnit2.ProjectGrant);
                mgr.SetCellTextValue(iRow, 9, summaryUnit2.InvoiceDate);
                mgr.SetCellTextValue(iRow, 11, summaryUnit2.Uniqname);
                mgr.SetCellTextValue(iRow, 18, summaryUnit2.ItemDescription);
                mgr.SetCellTextValue(iRow, 24, summaryUnit2.QuantityVouchered);
                mgr.SetCellFormula(iRow, 27, string.Format("=-SUM(AB{0}:AB{1})", iRowNumber2, iRow));

                mgr.SetColumnCollapsed("I", true);
                mgr.SetColumnCollapsed("J", true);
                mgr.SetColumnWidth(10, 1);

                string workFilePath = workPathDir + "\\" + "StoreSUB" + "_";
                if (EndPeriod == StartPeriod.AddMonths(1))
                    workFilePath += StartPeriod.ToString("yyyy-MM") + Path.GetExtension(fileName);
                else
                    workFilePath += StartPeriod.ToString("yyyy-MM") + "_" + EndPeriod.ToString("yyyy-MM") + Path.GetExtension(fileName);

                mgr.SaveAs(workFilePath);

                return workFilePath;
            }
        }
    }
}