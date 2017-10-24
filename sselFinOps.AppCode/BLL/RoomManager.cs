using LNF.CommonTools;
using LNF.Repository;
using System;
using System.Data;

namespace sselFinOps.AppCode.BLL
{
    public enum LabRoom
    {
        TestingLab = 2,
        Organics = 4,
        CleanRoom = 6,
        ChemRoom = 25
    }

    /// <summary>
    /// The class handles the data manipulate on table 'Room'
    /// </summary>
    public static class RoomManager
    {
        public static DataTable GetAllNAPRoomsWithCosts(DateTime period)
        {
            using (var dba = new SQLDBAccess("cnSselData"))
            {
                dba.AddParameter("@Action", "NAPRoomsWithCost");
                dba.AddParameter("@Period", period);
                return dba.FillDataTable("Room_Select");
            }
        }
    }
}
