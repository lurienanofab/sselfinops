using LNF.Repository;
using System;
using System.Data;

namespace sselFinOps.AppCode.DAL
{
    public static class AccountDA
    {
        public static DataTable GetAllInternalAccount(DateTime sDate, DateTime eDate)
        {
            var dt = DA.Command()
                .Param("Action", "AllInternalAccounts")
                .Param("sDate", sDate)
                .Param("eDate", eDate)
                .FillDataTable("dbo.Account_Select");

            //Must set primary key because the client code need to find data in this table
            //should this code below to business logic or data access?
            dt.PrimaryKey = new[] { dt.Columns["AccountID"] };

            return dt;
        }

        public static DataView GetAllAccount(int year, int month)
        {
            DateTime sDate = new DateTime(year, month, 1);

            var dt = DA.Command()
                .Param("Action", "AllActive")
                .Param("sDate", sDate)
                .Param("eDate", sDate.AddMonths(1))
                .FillDataTable("dbo.Account_Select");

            dt.DefaultView.Sort = "Name";

            return dt.DefaultView;
        }

        public static int GetChargeType(int accountId)
        {
            return DA.Command()
                .Param("Action", "GetChargeType")
                .Param("AccountID", accountId)
                .ExecuteScalar<int>("dbo.Account_Select");
        }

        public static DataTable GetAccountsByClientID(int clientId)
        {
            return DA.Command()
                .Param("Action", "GetAccountsByClientID")
                .Param("ClientID", clientId)
                .FillDataTable("dbo.Account_Select");
        }

        public static DataTable GetAccountsByClientIDAndDate(int clientId, DateTime sDate, DateTime eDate)
        {
            return DA.Command()
                .Param("Action", "GetAccountsByClientIDAndDate")
                .Param("ClientID", clientId)
                .Param("sDate", sDate)
                .Param("eDate", eDate)
                .FillDataTable("dbo.Account_Select");
        }

        public static DataTable GetNonBillingAccount()
        {
            return DA.Command().Param("Action", "GetAllNonBillingAccounts").FillDataTable("dbo.Account_Select");
        }
    }
}
