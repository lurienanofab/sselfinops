using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using LNF.Repository;
using LNF.CommonTools;

namespace sselFinOps.AppCode.DAL
{
    public static class MiscBillingChargeDA
    {
        public static DataTable GetDataByPeriod(int year, int month)
        {
            using (var dba = GetAdapter())
            {
                DateTime period = new DateTime(year, month, 1);
                dba.AddParameter("@Action", "GetAllByPeriod");
                dba.AddParameter("@Period", period);
                DataTable dt = dba.FillDataTable("MiscBillingCharge_Select");
                dt.Columns.Add("TotalCost", typeof(double), "Quantity * UnitCost");
                dt.Columns.Add("UserPayment", typeof(double), "(Quantity * UnitCost) - SubsidyDiscount");
                return dt;
            }
        }

        public static DataTable GetDataByPeriodAndSUBType(DateTime startPeriod, DateTime endPeriod, string subType, int orgId)
        {
            using (var dba = GetAdapter())
            {
                dba.AddParameter("@Action", "GetAllByPeriodAndSUBTypeAndOrgID");
                dba.AddParameter("@StartPeriod", startPeriod);
                dba.AddParameter("@EndPeriod", endPeriod);
                dba.AddParameter("@SUBType", subType);
                dba.AddParameter("@OrgID", orgId);
                return dba.FillDataTable("MiscBillingCharge_Select");
            }
        }

        public static int SaveNewEntry(int clientId, int accountId, string subType, DateTime period, string description, double quantity, double unitCost)
        {
            using (var dba = GetAdapter())
            {
                dba.AddParameter("@ClientID", clientId);
                dba.AddParameter("@AccountID", accountId);
                dba.AddParameter("@SUBType", subType);
                dba.AddParameter("@ActDate", DateTime.Now);
                dba.AddParameter("@Period", new DateTime(period.Year, period.Month, 1));
                dba.AddParameter("@Description", description);
                dba.AddParameter("@Quantity", quantity);
                dba.AddParameter("@UnitCost", unitCost);
                return dba.ExecuteNonQuery("MiscBillingCharge_Insert");
            }
        }

        public static DataTable GetByExpID(int expId)
        {
            using (var dba = GetAdapter())
            {
                dba.AddParameter("@Action", "ByExpID");
                dba.AddParameter("@ExpID", expId);
                var dt = dba.FillDataTable("MiscBillingCharge_Select");
                return dt;
            }
        }

        public static int UpdateEntry(int expId, DateTime period, string description, double quantity, double unitCost)
        {
            using (var dba = GetAdapter())
            {
                dba.AddParameter("@ExpID", expId);
                dba.AddParameter("@Period", new DateTime(period.Year, period.Month, 1));
                dba.AddParameter("@Description", description);
                dba.AddParameter("@Quantity", quantity);
                dba.AddParameter("@UnitCost", unitCost);
                int result = dba.ExecuteNonQuery("MiscBillingCharge_Update");
                return result;
            }
        }

        public static int DeleteEntry(int expId)
        {
            using (var dba = GetAdapter())
            {
                dba.AddParameter("@ExpID", expId);
                var result = dba.ExecuteNonQuery("MiscBillingCharge_Delete");
                return result;
            }
        }

        private static UnitOfWorkAdapter GetAdapter()
        {
            return new SQLDBAccess("cnSselData");
        }
    }
}
