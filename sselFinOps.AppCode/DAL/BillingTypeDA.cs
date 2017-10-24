using LNF.CommonTools;
using LNF.Repository;
using System.Data;

namespace sselFinOps.AppCode.DAL
{
    public static class BillingTypeDA
    {
        public static DataTable GetAllBillingTypes()
        {
            using (var dba = new SQLDBAccess("cnSselData"))
            {
                dba.AddParameter("@Action", "All");
                DataTable dt = dba.FillDataTable("BillingType_Select");
                dt.PrimaryKey = new[] { dt.Columns["BillingTypeID"] };
                return dt;
            }
        }
    }
}
