using LNF.CommonTools;
using LNF.Repository;
using System;
using System.Data;

namespace sselFinOps.AppCode.BLL
{
    public static class RoomApportionDataManager
    {
        public static DataTable GetNAPRoomApportionDataByPeriod(DateTime startPeriod, DateTime endPeriod, int roomId)
        {
            using (var dba = new SQLDBAccess("cnSselData"))
            {
                dba.AddParameter("@Action", "SelectByPeriod");
                dba.AddParameter("@StartPeriod", startPeriod);
                dba.AddParameter("@EndPeriod", endPeriod);
                dba.AddParameter("@RoomID", roomId);
                DataTable dt = dba.FillDataTable("RoomApportionData_Select");

                dt.PrimaryKey = new[] { dt.Columns["RoomApportionDataID"] };

                return dt;
            }
        }
    }
}
