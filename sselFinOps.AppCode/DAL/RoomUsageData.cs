using LNF.Repository;
using System;
using System.Data;

namespace sselFinOps.AppCode.DAL
{
    public static class RoomUsageData
    {
        public static DataTable GetUserListLessThanXMin(DateTime startPeriod, DateTime endPeriod, float x, string roomType)
        {
            return DataCommand.Create()
                .Param("Action", roomType == "CleanRoom", "GetUserListLessThanXMinutesCleanRoom", "GetUserListLessThanXMinutesChemRoom")
                .Param("StartPeriod", startPeriod)
                .Param("EndPeriod", endPeriod)
                .Param("XMinutes", x) //must use floating number
                .FillDataTable("dbo.RoomData_Select");
        }
    }
}
