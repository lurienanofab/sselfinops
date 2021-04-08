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
            return DataCommand.Create()
                .Param("Action", "NAPRoomsWithCost")
                .Param("Period", period)
                .FillDataTable("dbo.Room_Select");
        }
    }
}
