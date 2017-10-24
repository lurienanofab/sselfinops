using LNF.CommonTools;
using LNF.Repository;
using System;
using System.Data;

namespace sselFinOps.AppCode.DAL
{
    public static class AccountDA
    {
        public static DataTable GetAllInternalAccount(DateTime sDate, DateTime eDate)
        {
            using (var dba = new SQLDBAccess("cnSselData"))
            {
                dba.AddParameter("@Action", "AllInternalAccounts");
                dba.AddParameter("@sDate", sDate);
                dba.AddParameter("@eDate", eDate);
                DataTable dt = dba.FillDataTable("Account_Select");

                //Must set primary key because the client code need to find data in this table
                //should this code below to business logic or data access?
                dt.PrimaryKey = new[] {dt.Columns["AccountID"]};

                return dt;
            }
        }

        public static DataView GetAllAccount(int year, int month)
        {
            using (var dba = new SQLDBAccess("cnSselData"))
            {
                DateTime sDate = new DateTime(year, month, 1);
                dba.AddParameter("@Action", "AllActive");
                dba.AddParameter("@sDate", sDate);
                dba.AddParameter("@eDate", sDate.AddMonths(1));
                DataTable dt = dba.FillDataTable("Account_Select");
                DataView dv = dt.DefaultView;
                dv.Sort = "Name";
                return dv;
            }
        }

        public static int GetChargeType(int accountId)
        {
            using (var dba = new SQLDBAccess("cnSselData"))
            {
                dba.AddParameter("@Action", "GetChargeType");
                dba.AddParameter("@AccountID", accountId);
                int chargetypeId = dba.ExecuteScalar<int>("Account_Select");
                return chargetypeId;
            }
        }

        public static DataTable  GetAccountsByClientID(int clientId)
        {
            using (var dba = new SQLDBAccess("cnSselData"))
            {
                dba.AddParameter("@Action", "GetAccountsByClientID");
                dba.AddParameter("@ClientID", clientId);
                return dba.FillDataTable("Account_Select");
            }
        }

        public static DataTable  GetAccountsByClientIDAndDate(int clientId, DateTime sDate, DateTime eDate)
        {
            using (var dba = new SQLDBAccess("cnSselData"))
            {
                dba.AddParameter("@Action", "GetAccountsByClientIDAndDate");
                dba.AddParameter("@ClientID", clientId);
                dba.AddParameter("@sDate", sDate);
                dba.AddParameter("@eDate", eDate);
                return dba.FillDataTable("Account_Select");
            }
        }

        public static DataTable GetNonBillingAccount()
        {
            using (var dba = new SQLDBAccess("cnSselData"))
            {
                dba.AddParameter("@Action", "GetAllNonBillingAccounts");
                return dba.FillDataTable("Account_Select");
            }
        }
    }
}
