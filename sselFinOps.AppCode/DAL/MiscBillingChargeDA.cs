using LNF.CommonTools;
using LNF.Repository;
using System;
using System.Data;

namespace sselFinOps.AppCode.DAL
{
    public static class MiscBillingChargeDA
    {
        public static DataTable GetDataByPeriod(int year, int month)
        {
            DateTime period = new DateTime(year, month, 1);

            var dt = DA.Command()
                .Param("Action", "GetAllByPeriod")
                .Param("Period", period)
                .FillDataTable("dbo.MiscBillingCharge_Select");

            dt.Columns.Add("TotalCost", typeof(double), "Quantity * UnitCost");
            dt.Columns.Add("UserPayment", typeof(double), "(Quantity * UnitCost) - SubsidyDiscount");

            return dt;
        }

        public static DataTable GetDataByPeriodAndSUBType(DateTime startPeriod, DateTime endPeriod, string subType, int orgId)
        {
            return DA.Command()
                .Param("Action", "GetAllByPeriodAndSUBTypeAndOrgID")
                .Param("StartPeriod", startPeriod)
                .Param("EndPeriod", endPeriod)
                .Param("SUBType", subType)
                .Param("OrgID", orgId)
                .FillDataTable("dbo.MiscBillingCharge_Select");
        }

        public static int SaveNewEntry(int clientId, int accountId, string subType, DateTime period, string description, double quantity, double unitCost)
        {

            return DA.Command()
                .Param("ClientID", clientId)
                .Param("AccountID", accountId)
                .Param("SUBType", subType)
                .Param("ActDate", DateTime.Now)
                .Param("Period", period.FirstOfMonth())
                .Param("Description", description)
                .Param("Quantity", quantity)
                .Param("UnitCost", unitCost)
                .ExecuteNonQuery("dbo.MiscBillingCharge_Insert").Value;
        }

        public static DataTable GetByExpID(int expId)
        {
            return DA.Command()
                .Param("Action", "ByExpID")
                .Param("ExpID", expId)
                .FillDataTable("dbo.MiscBillingCharge_Select");
        }

        public static int UpdateEntry(int expId, DateTime period, string description, double quantity, double unitCost)
        {
            return DA.Command()
                .Param("ExpID", expId)
                .Param("Period", period.FirstOfMonth())
                .Param("Description", description)
                .Param("Quantity", quantity)
                .Param("UnitCost", unitCost)
                .ExecuteNonQuery("dbo.MiscBillingCharge_Update").Value;
        }

        public static int DeleteEntry(int expId)
        {
            return DA.Command()
                .Param("ExpID", expId)
                .ExecuteNonQuery("dbo.MiscBillingCharge_Delete").Value;
        }
    }
}
