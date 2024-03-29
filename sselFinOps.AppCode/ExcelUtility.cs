﻿using LNF.Billing;
using LNF.CommonTools;
using LNF.Repository;
using sselFinOps.AppCode.BLL;
using sselFinOps.AppCode.DAL;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.OleDb;
using System.IO;
using System.Linq;

namespace sselFinOps.AppCode
{
    public static class ExcelUtility
    {
        public static IExcelManager NewExcelManager()
        {
            var typeName = Utility.GetRequiredAppSetting("ExcelManager");
            var result = (IExcelManager)Activator.CreateInstance(Type.GetType(typeName));
            return result;
        }

        public static string GetWorkPath(int clientId)
        {
            return GetSpreadsheetsPath(string.Format("Work\\{0}", clientId));
        }

        public static string GetTemplatePath(string name)
        {
            return GetSpreadsheetsPath(string.Format("Templates\\{0}", name));
        }

        public static string SpreadsheetsDirectory
        {
            get { return ConfigurationManager.AppSettings["SpreadsheetsDirectory"]; }
        }

        private static string GetSpreadsheetsPath(string path)
        {
            if (string.IsNullOrEmpty(path))
                return SpreadsheetsDirectory;
            else
                return Path.Combine(SpreadsheetsDirectory, path);
        }

        public static string GenerateInvoiceExcelReport(ExternalInvoice inv, int currentUserClientId, string subFolder, bool deleteWorkDir, ref string alert)
        {
            //Output Table - contains all types of usage
            var usage = inv.Usage;

            // point to template files
            string fileName = Utility.GetRequiredAppSetting("Invoice_Template");
            string templatePath = GetTemplatePath(fileName);
            string workPathDir = GetWorkPath(currentUserClientId);

            if (!string.IsNullOrEmpty(subFolder))
                workPathDir = Path.Combine(workPathDir, subFolder);

            // Delete old work files
            // need to check and see if any old files are left behind
            // remember, WorkDir always includes the ClientID
            if (!Directory.Exists(GetWorkPath(currentUserClientId)))
                Directory.CreateDirectory(GetWorkPath(currentUserClientId));

            if (deleteWorkDir)
                ClearWorkDirectory(GetWorkPath(currentUserClientId));

            // this is (possibly) a sub folder of WorkDir
            // if subFolder is empty then subPath is the same as ExcelUtility.WorkPath and we already checked to see if that exists
            if (!string.IsNullOrEmpty(subFolder))
            {
                if (!Directory.Exists(workPathDir))
                    Directory.CreateDirectory(workPathDir);
            }

            // Write to excel
            int attempts = 0;
            string orgName = inv.Header.OrgName;
            string acctName = inv.Header.AccountName;
            string workFileName = EnsureFileNameIsValid(orgName + " (" + acctName + ") " + inv.StartDate.ToString("yyyy-MM") + Path.GetExtension(fileName));
            string workFilePath = Path.Combine(workPathDir, workFileName);
            string errmsg = string.Empty;

            while (!File.Exists(workFilePath) && attempts < 10)
            {
                try
                {
                    File.Copy(templatePath, workFilePath, true);
                }
                catch (Exception ex)
                {
                    //I only care about the last one
                    errmsg = ex.Message;
                }
                attempts += 1;
            }

            if (File.Exists(workFilePath))
            {
                using (var mgr = NewExcelManager())
                {
                    mgr.OpenWorkbook(workFilePath);
                    mgr.SetActiveWorksheet("Invoice");

                    //show invoice date
                    mgr.SetCellTextValue("F4", DateTime.Now.ToString("MMMM dd, yyyy"));

                    DataTable dtAddress = AddressDA.GetAddressByAccountID(inv.Header.AccountID);
                    foreach (DataRow drAddr in dtAddress.Rows)
                    {
                        int startRow = (drAddr.Field<string>("AddrType") == "Billing") ? 7 : 13;
                        mgr.SetCellTextValue($"C{startRow}", inv.Header.OrgName);
                        mgr.SetCellTextValue($"C{startRow + 1}", drAddr.Field<string>("InternalAddress"));
                        mgr.SetCellTextValue($"C{startRow + 2}", drAddr.Field<string>("StrAddress1"));
                        mgr.SetCellTextValue($"C{startRow + 3}", drAddr.Field<string>("StrAddress2"));
                        mgr.SetCellTextValue($"C{startRow + 4}", drAddr.Field<string>("City") + ", " + drAddr.Field<string>("State") + " " + drAddr.Field<string>("Zip"));
                    }

                    //invoice number that needs to be entered
                    mgr.SetCellTextValue("E12", inv.Header.InvoiceNumber);
                    mgr.SetCellTextValue("I14", inv.Header.DeptRef);

                    //now print charges
                    int r = 21;
                    double totalCharge = usage.Sum(x => x.Cost);

                    foreach (var item in usage)
                    {
                        if (item.Cost != 0)
                        {
                            mgr.SetCellTextValue($"C{r}", inv.StartDate.ToString("MM/yyyy"));
                            mgr.SetCellTextValue($"D{r}", item.Description.Replace("'", "''"));
                            mgr.SetCellNumberValue($"H{r}", item.Quantity);
                            mgr.SetCellNumberValue($"I{r}", item.Cost);
                            r += 1;
                        }
                    }

                    mgr.SaveAs(workFilePath);
                }
            }
            else
            {
                alert = "Unable to create Excel file, copying template failed: " + errmsg;
                workFilePath = string.Empty;
            }

            return workFilePath;
        }

        //public static string GenerateInvoiceExcelReport(DateTime startPeriod, DateTime endPeriod, int clientId, bool showRemote, ExternalInvoiceHeader orgItem, string subFolder, bool deleteWorkDir, ref string alert)
        //{
        //    ExternalInvoiceManager mgr = new ExternalInvoiceManager(orgItem.AccountID, startPeriod, endPeriod, showRemote);
        //    string result = GenerateInvoiceExcelReport(mgr, clientId, orgItem, subFolder, deleteWorkDir, ref alert);
        //    return result;
        //}

        public static string MakeSpreadSheet(int accountId, string invoiceNumber, string deptRef, string orgName, int currentUserClientId, DateTime startPeriod, DateTime endPeriod)
        {
            var dtAddress = DataCommand.Create()
                .Param("Action", "ByAccount")
                .Param("AccountID", accountId)
                .FillDataTable("dbo.Address_Select");

            // get client data
            // using All is hackish. It was ByOrg, but this caused a problem with remote users
            // the other option is to select for each client, but that is probably even less efficient
            var dtClient = DataCommand.Create()
                .Param("Action", "All")
                .Param("sDate", startPeriod)
                .Param("eDate", endPeriod)
                .FillDataTable("dbo.Client_Select");

            dtClient.PrimaryKey = new[] { dtClient.Columns["ClientID"] };

            // NOTE: all reports here are for external users
            DataRow drUsage;
            DataTable dtUsage = new DataTable();
            dtUsage.Columns.Add("Descrip", typeof(string));
            dtUsage.Columns.Add("Quantity", typeof(double));
            dtUsage.Columns.Add("Cost", typeof(double));

            DataRow cdr;
            DataTable dtAggCost;
            string[] costType = { "Room", "StoreInv", "Tool", "Misc" };
            Compile mCompile = new Compile();
            DataTable dtClientWithCharges;
            double capCost;
            object temp;
            double totalCharges;

            //2009-01 this for loop will loop through different Cost types, so when added code for each specific type, remember to distinguish each CostType
            for (int i = 0; i < costType.Length; i++)
            {
                dtAggCost = mCompile.CalcCost(costType[i], string.Empty, "AccountID", accountId, startPeriod, 0, 0, Compile.AggType.CliAcct);
                //dtAggCost is the main table that contains chargeable items
                //0 ClientID
                //1 AccountID
                //2 RoomID
                //3 TotalCalcCost
                //4 TotalEntries
                //5 TotalHours

                //Only Room costtype will execute the code below
                if (costType[i] == "Room")
                {
                    //******** The code below handles the cost of NAP rooms, because at this point, all NAP rooms access data have total cost of zero****
                    //Get all active NAP Rooms with their costs, all chargetypes are returned
                    DataTable dtNAPRoomForAllChargeType = RoomManager.GetAllNAPRoomsWithCosts(startPeriod);

                    //filter out the chargetype so that we only have Internal costs with each NAP room
                    DataRow[] dtdrsNAPRoomForExternal = dtNAPRoomForAllChargeType.Select(string.Format("ChargeTypeID = {0}", AccountDA.GetChargeType(accountId)));

                    //Loop through each room and find out this specified month's apportionment data.
                    foreach (DataRow dr1 in dtdrsNAPRoomForExternal)
                    {
                        DataTable dtApportionData = RoomApportionDataManager.GetNAPRoomApportionDataByPeriod(startPeriod, endPeriod, dr1.Field<int>("RoomID"));

                        foreach (DataRow dr2 in dtApportionData.Rows)
                        {
                            DataRow[] drs = dtAggCost.Select(String.Format("ClientID = {0} AND AccountID = {1} AND RoomID = {2}", dr2["ClientID"], dr2["AccountID"], dr2["RoomID"]));

                            if (drs.Length == 1)
                            {
                                //2008-06-19 Sandrine requested all monthly room should have charge of the same across all organizations
                                //so if a guy works for two diferent companies, he should be charged full amount for both companeis on all monthly rooms.
                                //the only exception is when the apportionment percentage is 0 for this organization.  When it's 0 percent, we simply
                                //cannot charge this organizaiton at all
                                if (dr2.Field<double>("Percentage") > 0)
                                    drs[0]["TotalCalcCost"] = dr1["RoomCost"];
                            }
                        }
                    }

                    //2009-01 remember not to charge clean/chem room usage for less than x amount of minutes
                    int cleanRoomMinMinutes = int.Parse(ConfigurationManager.AppSettings["CleanRoomMinTimeMinute"]);
                    int chemRoomMinMinutes = int.Parse(ConfigurationManager.AppSettings["ChemRoomMinTimeMinute"]);

                    //we simply set totalCalcCost to 0.0 at this point, then those 0.0 charges items will not be published to excel report
                    foreach (DataRow drAggCost in dtAggCost.Rows)
                    {
                        if (drAggCost.Field<LabRoom>("RoomID") == LabRoom.CleanRoom)
                        {
                            if (drAggCost.Field<double>("TotalHours") < cleanRoomMinMinutes / 60)
                                drAggCost.SetField("TotalCalcCost", 0.0);
                        }
                        else if (drAggCost.Field<LabRoom>("RoomID") == LabRoom.ChemRoom)
                        {
                            if (drAggCost.Field<double>("TotalHours") < chemRoomMinMinutes / 60)
                                drAggCost.SetField("TotalCalcCost", 0.0);
                        }
                    }
                }

                if (costType[i] != "Misc")
                {
                    dtClientWithCharges = mCompile.GetTable(1);
                    capCost = mCompile.CapCost;

                    foreach (DataRow drCWC in dtClientWithCharges.Rows)
                    {
                        temp = dtAggCost.Compute("SUM(TotalCalcCost)", string.Format("ClientID = {0}", drCWC["ClientID"]));
                        if (temp == null || temp == DBNull.Value)
                            totalCharges = 0.0;
                        else
                            totalCharges = Convert.ToDouble(temp);

                        //BUG FIX: I have to exclude StoreInv charge here since the CapCost for it is always 0
                        if (totalCharges > capCost && costType[i] != "StoreInv")
                        {
                            DataRow[] fdr = dtAggCost.Select(string.Format("ClientID = {0}", drCWC["ClientID"]));
                            for (int j = 0; j < fdr.Length; j++)
                                fdr[j].SetField("TotalCalcCost", fdr[j].Field<double>("TotalCalcCost") * capCost / totalCharges);
                        }
                    }
                }

                foreach (DataRow drAggCost in dtAggCost.Rows)
                {
                    cdr = dtClient.Rows.Find(drAggCost.Field<int>("ClientID"));

                    drUsage = dtUsage.NewRow();
                    if (costType[i] == "Misc")
                    {
                        drUsage["Descrip"] = drAggCost["Description"];
                        drUsage["Quantity"] = drAggCost["Quantity"];
                        drUsage["Cost"] = drAggCost["UnitCost"];
                    }
                    else
                    {
                        drUsage["Descrip"] = string.Format("{0} usage for {1}. {2}", costType[i].Substring(0, 5), cdr["FName"].ToString().Substring(0, 1), cdr["LName"]);
                        drUsage["Quantity"] = 1;
                        drUsage["Cost"] = drAggCost["TotalCalcCost"];
                    }
                    dtUsage.Rows.Add(drUsage);
                }
            }

            // Write to excel
            using (var mgr = NewExcelManager())
            {
                string fileName = Utility.GetRequiredAppSetting("Invoice_Template");
                mgr.OpenWorkbook(GetTemplatePath(fileName));
                mgr.SetActiveWorksheet("Invoice");

                // show invoice date
                mgr.SetCellTextValue("F4", DateTime.Now.ToShortDateString());

                int startRow;
                int useRow = 0;
                foreach (DataRow drAddr in dtAddress.Rows)
                {
                    if (drAddr["AddrType"].ToString() == "Billing")
                        startRow = 7;
                    else
                        startRow = 13;

                    FillField(mgr, "C", startRow, orgName, ref useRow);
                    FillField(mgr, "C", 0, drAddr.Field<string>("InternalAddress"), ref useRow);
                    FillField(mgr, "C", 0, drAddr.Field<string>("StrAddress1"), ref useRow);
                    FillField(mgr, "C", 0, drAddr.Field<string>("StrAddress2"), ref useRow);
                    FillField(mgr, "C", 0, drAddr.Field<string>("City") + ", " + drAddr.Field<string>("State") + " " + drAddr.Field<string>("Zip"), ref useRow);
                }

                // invoice number that needs to be entered
                mgr.SetCellTextValue("E12", invoiceNumber);
                mgr.SetCellTextValue("I14", deptRef);

                int rowRef = 21;
                string rowCell;

                // now print charges
                foreach (DataRow dr in dtUsage.Rows)
                {
                    if (dr.Field<double>("Cost") != 0)
                    {
                        rowCell = "C" + rowRef.ToString();
                        mgr.SetCellTextValue(rowCell, startPeriod.ToString("MM/yyyy"));

                        rowCell = "D" + rowRef.ToString();
                        mgr.SetCellTextValue(rowCell, dr["Descrip"]);

                        rowCell = "H" + rowRef.ToString();
                        mgr.SetCellNumberValue(rowCell, dr["Quantity"]);

                        rowCell = "I" + rowRef.ToString();
                        mgr.SetCellNumberValue(rowCell, dr["Cost"]);

                        rowRef += 1;
                    }
                }

                string workFilePath = Path.Combine(GetWorkPath(currentUserClientId), orgName + Path.GetExtension(fileName));
                mgr.SaveAs(workFilePath);

                return workFilePath;
            }
        }

        private static void FillField(IExcelManager mgr, string col, int row, string data, ref int useRow)
        {
            if (row != 0)
                useRow = row;

            if (data.Length > 0)
            {
                mgr.SetCellTextValue(col + useRow.ToString(), data);
                useRow += 1;
            }
        }

        private static void ClearWorkDirectory(string workPath)
        {
            if (!Directory.Exists(workPath))
                Directory.CreateDirectory(workPath);

            foreach (string f in Directory.GetFiles(workPath))
            {
                File.Delete(f);
                foreach (string d in Directory.GetDirectories(workPath))
                    ClearWorkDirectory(d);
            }
        }

        private static string EnsureFileNameIsValid(string fileName)
        {
            var invalid = new List<char>();
            invalid.AddRange(Path.GetInvalidFileNameChars());
            invalid.AddRange(Path.GetInvalidPathChars());

            foreach (char c in invalid)
                fileName = fileName.Replace(c.ToString(), "~");

            return fileName;
        }
    }
}
