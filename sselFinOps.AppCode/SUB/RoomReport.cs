using LNF.Billing;
using LNF.Billing.Reports.ServiceUnitBilling;
using LNF.CommonTools;
using sselFinOps.AppCode.DAL;
using System;
using System.Configuration;
using System.Data;
using System.Linq;

namespace sselFinOps.AppCode.SUB
{
    public class RoomReport : ReportBase
    {
        public RoomReport(DateTime startPeriod, DateTime endPeriod) : base(startPeriod, endPeriod) { }

        protected override void FillDataTable(DataTable dt)
        {
            BillingUnit summaryUnit = summaryUnits.First();

            Compile mCompile = new Compile();
            DataTable dtRoomDB = mCompile.CalcCost("Room", string.Empty, "ChargeTypeID", 5, EndPeriod.AddMonths(-1), 0, 0, Compile.AggType.CliAcctType);
            DataTable dtClientWithCharges = mCompile.GetTable(1);
            double roomCapCost = mCompile.CapCost;

            //*****************************************************************************
            //2008-01-22 The code below is an EXTRA step for calculating the cost of room charge
            // Right now the strategy is not to change Compile.CalcCost at all and if I want to 
            // add new features that would affect CalcCost, I would rather do it after CalcCost is called.
            // But future new design is required else the system will get too complicated.

            //2208-05-15 the reason why we are doing this extra step is to show NAP rooms (as of now, it's DC Test lab and Chem room)
            //with correct monthly fee on the JE

            //dtNAPRoomForAllChargeType's columns
            //CostID
            //ChargeTypeID
            //TableNameOrDescript
            //RoomID
            //AcctPer
            //AddVal
            //RoomCost
            //effDate

            //Get all active NAP Rooms with their costs, all chargetypes are returned
            //This is a temporary table, it's used to derive the really useful table below
            DataTable dtNAPRoomForAllChargeType = BLL.RoomManager.GetAllNAPRoomsWithCosts(EndPeriod);

            //filter out the chargetype so that we only have Internal costs with each NAP room
            DataRow[] drsNAPRoomForInternal = dtNAPRoomForAllChargeType.Select("ChargeTypeID = 5");

            //Loop through each room and find out this specified month's apportionment data.
            foreach (DataRow dr1 in drsNAPRoomForInternal)
            {
                DataTable dtApportionData = BLL.RoomApportionDataManager.GetNAPRoomApportionDataByPeriod(StartPeriod, EndPeriod, dr1.Field<int>("RoomID"));

                foreach (DataRow dr2 in dtApportionData.Rows)
                {
                    DataRow[] drs = dtRoomDB.Select(string.Format("ClientID = {0} AND AccountID = {1} AND RoomID = {2}", dr2["ClientID"], dr2["AccountID"], dr2["RoomID"]));

                    if (drs.Length == 1)
                        drs[0].SetField("TotalCalcCost", (dr2.Field<double>("Percentage") * dr1.Field<double>("RoomCost")) / 100);
                }
            }

            dtRoomDB.Columns.Add("DebitAccount", typeof(string));
            dtRoomDB.Columns.Add("CreditAccount", typeof(string));
            dtRoomDB.Columns.Add("LineDesc", typeof(string));
            dtRoomDB.Columns.Add("TotalAllAccountCost", typeof(double));

            //dtRoom - ClientID, AccountID, RoomID, TotalCalCost, TotalEntries, TotalHours
            // cap costs - capping is per clientorg, thus apportion cappeing across charges
            // note that this assumes that there is only one org for internal academic!!!
            object temp;
            double totalRoomCharges;
            foreach (DataRow drCWC in dtClientWithCharges.Rows)
            {
                temp = dtRoomDB.Compute("SUM(TotalCalcCost)", string.Format("ClientID = {0}", drCWC["ClientID"]));
                if (temp == null || temp == DBNull.Value)
                    totalRoomCharges = 0;
                else
                    totalRoomCharges = Convert.ToDouble(temp);

                if (totalRoomCharges > roomCapCost)
                {
                    DataRow[] fdr = dtRoomDB.Select(string.Format("ClientID = {0}", drCWC["ClientID"]));
                    for (int i = 0; i < fdr.Length; i++)
                        fdr[i].SetField("TotalCalcCost", fdr[i].Field<double>("TotalCalcCost") * roomCapCost / totalRoomCharges);
                }
            }

            DataTable dtClient = ClientDA.GetAllClient(StartPeriod, EndPeriod);
            DataTable dtAccount = AccountDA.GetAllInternalAccount(StartPeriod, EndPeriod);
            DataTable dtClientAccount = ClientDA.GetAllClientAccountWithManagerName(StartPeriod, EndPeriod); //used to find out manager's name

            //Get the general lab account ID and lab credit account ID
            GlobalCost gc = GlobalCostDA.GetGlobalCost();

            //2008-05-15 very complicated code - trying to figure out the percentage distribution for monthly users, since the "TotalCalcCost" has
            //been calculated based on percentage in the CalcCost function, so we need to figure out the percentage here again by findind out the total 
            //and divide the individual record's "TotalCalcCost'
            foreach (DataRow drCWC in dtClientWithCharges.Rows)
            {
                DataRow[] fdr = dtRoomDB.Select(string.Format("ClientID = {0} AND RoomID = {1}", drCWC["ClientID"], (int)BLL.LabRoom.CleanRoom));
                if (fdr.Length > 1)
                {
                    //this user has multiple account for the clean room usage, so we have to find out the total of all accounts on this clean room
                    double tempTotal = Convert.ToDouble(dtRoomDB.Compute("SUM(TotalCalcCost)", string.Format("ClientID = {0} AND RoomID = {1}", drCWC["ClientID"], (int)BLL.LabRoom.CleanRoom)));

                    DataRow[] fdrRoom = dtRoomDB.Select(string.Format("ClientID = {0} AND RoomID = {1}", drCWC["ClientID"], (int)BLL.LabRoom.CleanRoom));
                    for (int i = 0; i < fdrRoom.Length; i++)
                        fdrRoom[i].SetField("TotalAllAccountCost", tempTotal); //assign the total to each record
                }
            }

            //2008-08-28 Get Billing Type
            DataTable dtBillingType = BillingTypeDA.GetAllBillingTypes();
            int billingTypeId = 99;
            foreach (DataRow dr in dtRoomDB.Rows)
            {
                dr["DebitAccount"] = dtAccount.Rows.Find(dr.Field<int>("AccountID"))["Number"];
                dr["CreditAccount"] = dtAccount.Rows.Find(gc.LabCreditAccountID)["Number"];
                //2007-06-19 doscar is not an administrator, but her name must be on JE
                dr["LineDesc"] = GetLineDesc(dr, dtClient, dtBillingType);

                //2008-05-15 the code below handles the clean room monthly users - it's special code that we have to get rid of when all
                //billingtype are all gone
                billingTypeId = dr.Field<int>("BillingType");

                if (dr.Field<BLL.LabRoom>("RoomID") == BLL.LabRoom.CleanRoom) //6 is clean room
                {
                    if (BillingTypes.IsMonthlyUserBillingType(billingTypeId))
                    {
                        if (dr["TotalAllAccountCost"] == DBNull.Value)
                        {
                            //if TotalAllAccountCost is nothing, it means this user has only one account
                            //2008-10-27 but it might also that the user has only one internal account, and he apportion all hours to his external accouts
                            //so we must also check 'TotalHours' to make sure the user has more than 0 hours 
                            if (dr.Field<double>("TotalHours") != 0)
                                dr.SetField("TotalCalcCost", BLL.BillingTypeManager.GetTotalCostByBillingType(billingTypeId, 0, 0, BLL.LabRoom.CleanRoom, 1315));
                        }
                        else
                        {
                            double total = dr.Field<double>("TotalAllAccountCost");
                            dr.SetField("TotalCalcCost", (dr.Field<double>("TotalCalcCost") / total) * BLL.BillingTypeManager.GetTotalCostByBillingType(billingTypeId, 0, 0, BLL.LabRoom.CleanRoom, 1315));
                        }
                    }
                }
            }

            //****** apply filters ******
            //Get the list below so that we can exclude users who spent less than X minutes in lab(Clean or Chem) in this month
            DataTable dtlistClean = RoomUsageData.GetUserListLessThanXMin(StartPeriod, EndPeriod, int.Parse(ConfigurationManager.AppSettings["CleanRoomMinTimeMinute"]), "CleanRoom");
            DataTable dtlistChem = RoomUsageData.GetUserListLessThanXMin(StartPeriod, EndPeriod, int.Parse(ConfigurationManager.AppSettings["ChemRoomMinTimeMinute"]), "ChemRoom");

            //For performance issue, we have to calculate something first, since it's used on all rows
            string depRefNum = string.Empty;
            double chargeAmount = 0;
            double fTotal = 0;
            string creditAccount = dtAccount.Rows.Find(gc.LabCreditAccountID)["Number"].ToString();
            string creditAccountShortCode = dtAccount.Rows.Find(gc.LabCreditAccountID)["ShortCode"].ToString();

            //Do not show an item if the charge and xcharge accounts are the 'same' - can only happen for 941975
            //Do not show items that are associated with specific accounts - need to allow users to add manually here in future
            foreach (DataRow sdr in dtRoomDB.Rows)
            {
                if (sdr.Field<double>("TotalCalcCost") > 0)
                {
                    var excludedAccounts = new[] { gc.LabAccountID, 143, 179, 188 };

                    if (!excludedAccounts.Contains(sdr.Field<int>("AccountID")) && sdr.Field<int>("BillingType") != BillingTypes.Other)
                    {
                        //2006-12-21 get rid of people who stayed in the lab less than 30 minutes in a month
                        string expression = string.Format("ClientID = {0}", sdr["ClientID"]);
                        DataRow[] foundRows;
                        bool flag = false;
                        if (sdr.Field<BLL.LabRoom>("RoomID") == BLL.LabRoom.CleanRoom) //6 = clean room
                            foundRows = dtlistClean.Select(expression);
                        else if (sdr.Field<BLL.LabRoom>("RoomID") == BLL.LabRoom.ChemRoom) //25 = chem room
                            foundRows = dtlistChem.Select(expression);
                        else //DCLab
                            foundRows = null;

                        if (foundRows == null)
                            flag = true; //add to the SUB
                        else
                        {
                            if (foundRows.Length == 0)
                                flag = true;
                        }

                        if (flag) //if no foundrow, we can add this client to JE
                        {
                            DataRow ndr = dt.NewRow();

                            DataRow drAccount = dtAccount.Rows.Find(sdr.Field<int>("AccountID"));
                            string debitAccount = drAccount["Number"].ToString();
                            string shortCode = drAccount["ShortCode"].ToString();

                            //get manager's name
                            DataRow[] drClientAccount = dtClientAccount.Select(string.Format("AccountID = {0}", sdr["AccountID"]));
                            if (drClientAccount.Length > 0)
                                depRefNum = drClientAccount[0]["ManagerName"].ToString();
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
                            ndr["InvoiceID"] = "LNF Room Charge";
                            ndr["Uniqname"] = dtClient.Rows.Find(sdr.Field<int>("ClientID"))["UserName"];
                            ndr["DepartmentalReferenceNumber"] = depRefNum;
                            ndr["ItemDescription"] = GetItemDesc(sdr, dtClient, dtBillingType);
                            ndr["QuantityVouchered"] = "1.0000";
                            chargeAmount = Math.Round(sdr.Field<double>("TotalCalcCost"), 5);
                            ndr["UnitOfMeasure"] = chargeAmount;
                            ndr["MerchandiseAmount"] = Math.Round(chargeAmount, 2);
                            ndr["CreditAccount"] = creditAccount;
                            //Used to calculate the total credit amount
                            fTotal += chargeAmount;

                            dt.Rows.Add(ndr);
                        }
                    }
                }
            }

            //Summary row
            summaryUnit.CardType = 1;
            summaryUnit.ShortCode = creditAccountShortCode;
            summaryUnit.Account = creditAccount.Substring(0, 6);
            summaryUnit.FundCode = creditAccount.Substring(6, 5);
            summaryUnit.DeptID = creditAccount.Substring(11, 6);
            summaryUnit.ProgramCode = creditAccount.Substring(17, 5);
            summaryUnit.ClassName = creditAccount.Substring(22, 5);
            summaryUnit.ProjectGrant = creditAccount.Substring(27, 7);
            summaryUnit.InvoiceDate = EndPeriod.AddMonths(-1).ToString("yyyy/MM/dd");
            summaryUnit.Uniqname = FinancialManagerUserName;
            summaryUnit.DepartmentalReferenceNumber = depRefNum;
            summaryUnit.ItemDescription = FinancialManagerUserName;
            summaryUnit.MerchandiseAmount = Math.Round(-fTotal, 2);
            summaryUnit.CreditAccount = creditAccount;

            //Clean things up manually might help performance in general
            dtRoomDB.Clear();
            dtClient.Clear();
            dtAccount.Clear();
        }

        public override string GenerateExcelFile(DataTable dt)
        {
            return GenerateExcel(dt, "LabtimeSUB");
        }
    }
}