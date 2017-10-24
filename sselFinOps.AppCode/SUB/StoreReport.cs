using GemBox.ExcelLite;
using LNF.CommonTools;
using LNF.Models.Billing.Reports.ServiceUnitBilling;
using sselFinOps.AppCode.DAL;
using System;
using System.Data;
using System.Web;

namespace sselFinOps.AppCode.SUB
{
    public class StoreReport : ReportBase
{
    public StoreReport(DateTime startPeriod, DateTime endPeriod):base(startPeriod, endPeriod){}

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
        double chargeAmount = 0;
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
                ndr["InvoiceID"] = "LNF Store Charge";
                ndr["Uniqname"] = dtClient.Rows.Find(Convert.ToInt32(sdr["ClientID"]))["UserName"];
                ndr["DepartmentalReferenceNumber"] = depRefNum;
                ndr["ItemDescription"] = dtClient.Rows.Find(Convert.ToInt32(sdr["ClientID"]))["DisplayName"].ToString().Substring(0, 30);
                ndr["QuantityVouchered"] = "1.0000";
                chargeAmount = Math.Round(Convert.ToDouble(sdr["TotalCalcCost"]), 5);
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
        string creditAccount = string.Empty;
        string lastCreditAccount = "default";
        
        //Contruct the excel object
        string templatePath = HttpContext.Current.Server.MapPath(".\\SpreadSheets\\Templates\\SUB_Template.xlt");
        string workPathDir = HttpContext.Current.Server.MapPath(".\\SpreadSheets\\Work");
        ExcelLite.SetLicense("EL6N-Z669-AZZG-3LS7");
        ExcelFile spreadSheet = new ExcelFile();
        spreadSheet.LoadXls(templatePath);
        ExcelWorksheet ws = spreadSheet.Worksheets[0];

        int iRow = 1;
        int iRowNumber2 = 0; //this keep the last row of the first portion of Store SUB, needed this to formulate correct formula for total sum cell

        foreach (DataRowView drv in dv)
        {
            creditAccount = drv["CreditAccount"].ToString();
            if (creditAccount != lastCreditAccount && lastCreditAccount != "default" && summaryUnit2 != null)
            {
                ws.Cells[iRow, 0].Value = summaryUnit1.CardType;
                ws.Cells[iRow, 1].Value = summaryUnit1.ShortCode;
                ws.Cells[iRow, 2].Value = summaryUnit1.Account;
                ws.Cells[iRow, 3].Value = summaryUnit1.FundCode;
                ws.Cells[iRow, 4].Value = summaryUnit1.DeptID;
                ws.Cells[iRow, 5].Value = summaryUnit1.ProgramCode;
                ws.Cells[iRow, 6].Value = summaryUnit1.ClassName;
                ws.Cells[iRow, 7].Value = summaryUnit1.ProjectGrant;
                ws.Cells[iRow, 9].Value = summaryUnit1.InvoiceDate;
                ws.Cells[iRow, 11].Value = summaryUnit1.Uniqname;
                ws.Cells[iRow, 18].Value = summaryUnit1.ItemDescription;
                ws.Cells[iRow, 24].Value = summaryUnit1.QuantityVouchered;
                ws.Cells[iRow, 27].Formula = string.Format("=-SUM(AB2:AB{0})", iRow);

                iRow += 1;
                iRowNumber2 = iRow + 1;
            }

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
            ws.Cells[iRow, 11].Value = uniqName;
            ws.Cells[iRow, 15].Value = drv["DepartmentalReferenceNumber"];
            ws.Cells[iRow, 18].Value = drv["ItemDescription"];
            ws.Cells[iRow, 24].Value = drv["QuantityVouchered"];
            ws.Cells[iRow, 26].Value = Convert.ToDouble(drv["UnitOfMeasure"]);
            ws.Cells[iRow, 27].Value = Convert.ToDouble(drv["MerchandiseAmount"]);

            iRow += 1;
            lastCreditAccount = creditAccount;
        }

        //Add the last row - which is the summary unit
        ws.Cells[iRow, 0].Value = summaryUnit2.CardType;
        ws.Cells[iRow, 1].Value = summaryUnit2.ShortCode;
        ws.Cells[iRow, 2].Value = summaryUnit2.Account;
        ws.Cells[iRow, 3].Value = summaryUnit2.FundCode;
        ws.Cells[iRow, 4].Value = summaryUnit2.DeptID;
        ws.Cells[iRow, 5].Value = summaryUnit2.ProgramCode;
        ws.Cells[iRow, 6].Value = summaryUnit2.ClassName;
        ws.Cells[iRow, 7].Value = summaryUnit2.ProjectGrant;
        ws.Cells[iRow, 9].Value = summaryUnit2.InvoiceDate;
        ws.Cells[iRow, 11].Value = summaryUnit2.Uniqname;
        ws.Cells[iRow, 18].Value = summaryUnit2.ItemDescription;
        ws.Cells[iRow, 24].Value = summaryUnit2.QuantityVouchered;
        ws.Cells[iRow, 27].Formula = string.Format("=-SUM(AB{0}:AB{1})", iRowNumber2, iRow);

        ws.Columns["I"].Collapsed = true;
        ws.Columns["J"].Collapsed = true;
        ws.Columns[10].Width = 1;

        string workFilePath = workPathDir + "\\" + "StoreSUB" + "_";
        if (EndPeriod == StartPeriod.AddMonths(1))
            workFilePath += StartPeriod.ToString("yyyy-MM") + ".xls";
        else
            workFilePath += StartPeriod.ToString("yyyy-MM") + "_" + EndPeriod.ToString("yyyy-MM") + ".xls";

        spreadSheet.SaveXls(workFilePath);
        spreadSheet = null;
        GC.Collect();

        return workFilePath;
    }
}
}