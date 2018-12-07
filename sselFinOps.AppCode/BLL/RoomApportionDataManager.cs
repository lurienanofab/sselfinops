using LNF.Repository;
using System;
using System.Data;

namespace sselFinOps.AppCode.BLL
{
    public static class RoomApportionDataManager
    {
        public static DataTable GetNAPRoomApportionDataByPeriod(DateTime startPeriod, DateTime endPeriod, int roomId)
        {
            var dt = DA.Command()
                .Param("Action", "SelectByPeriod")
                .Param("StartPeriod", startPeriod)
                .Param("EndPeriod", endPeriod)
                .Param("RoomID", roomId)
                .FillDataTable("dbo.RoomApportionData_Select");

            dt.PrimaryKey = new[] { dt.Columns["RoomApportionDataID"] };

            return dt;
        }
    }
}
