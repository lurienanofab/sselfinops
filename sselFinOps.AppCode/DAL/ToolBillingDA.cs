using LNF.CommonTools;
using LNF.Repository;
using System;
using System.Data;

namespace sselFinOps.AppCode.DAL
{
    public static class ToolBillingDA
    {
        public static DataSet GetDataTablesForSUBReport(DateTime startPeriod, DateTime endPeriod)
        {
            using (var dba = new SQLDBAccess("cnSselData"))
            {
                dba.AddParameter("@Action", "ForSUBReport");
                dba.AddParameter("@StartPeriod", startPeriod);
                dba.AddParameter("@EndPeriod", endPeriod);
                return dba.FillDataSet("ToolBilling_Select");
            }
        }
    }
}
