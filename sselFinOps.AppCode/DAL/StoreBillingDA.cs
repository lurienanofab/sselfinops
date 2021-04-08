using LNF.Repository;
using System;
using System.Data;

namespace sselFinOps.AppCode.DAL
{
    public static class StoreBillingDA
    {
        public static DataSet GetDataTablesForSUBReport(DateTime startPeriod, DateTime endPeriod)
        {
            return DataCommand.Create()
                .Param("Action", "ForSUBReport")
                .Param("StartPeriod", startPeriod)
                .Param("EndPeriod", endPeriod)
                .FillDataSet("dbo.StoreBilling_Select");
        }

        public static DataSet GetDataTablesForSUBReportWithTwoCreditsAccount(DateTime startPeriod, DateTime endPeriod)
        {
            return DataCommand.Create()
                .Param("Action", "ForSUBReportWithTwoCreditAccounts")
                .Param("StartPeriod", startPeriod)
                .Param("EndPeriod", endPeriod)
                .FillDataSet("dbo.StoreBilling_Select");
        }
    }
}
