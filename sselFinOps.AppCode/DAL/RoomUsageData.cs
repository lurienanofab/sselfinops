using LNF.CommonTools;
using LNF.Repository;
using System;
using System.Data;

namespace sselFinOps.AppCode.DAL
{
    public static class RoomUsageData
    {
        public static DataTable GetUserListLessThanXMin(DateTime startPeriod, DateTime endPeriod, float x, string roomType)
        {
            using (var dba = new SQLDBAccess("cnSselData"))
            {
                if (roomType == "CleanRoom")
                    dba.AddParameter("@Action", "GetUserListLessThanXMinutesCleanRoom");
                else
                    dba.AddParameter("@Action", "GetUserListLessThanXMinutesChemRoom");
                
                dba.AddParameter("@StartPeriod", startPeriod);
                dba.AddParameter("@EndPeriod", endPeriod);
                dba.AddParameter("@XMinutes", x); //must use floating number
                return dba.FillDataTable("RoomData_Select");
            }
        }
    }
}
