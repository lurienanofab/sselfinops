using LNF.Repository;
using System;
using System.Data;

namespace sselFinOps.AppCode.DAL
{
    public static class RoomBillingDA
    {
        public static DataSet GetDataTablesForSUBReport(DateTime startPeriod, DateTime endPeriod)
        {
            return DataCommand.Create()
                .Param("Action", "ForSUBReport")
                .Param("StartPeriod", startPeriod)
                .Param("EndPeriod", endPeriod)
                .FillDataSet("dbo.RoomApportionmentInDaysMonthly_Select");
        }
    }
}
