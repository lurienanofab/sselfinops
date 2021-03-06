﻿using LNF.Repository;
using System.Data;

namespace sselFinOps.AppCode.DAL
{
    public static class BillingTypeDA
    {
        public static DataTable GetAllBillingTypes()
        {
            var dt = DataCommand.Create().Param("Action", "All").FillDataTable("dbo.BillingType_Select");
            dt.PrimaryKey = new[] { dt.Columns["BillingTypeID"] };
            return dt;
        }
    }
}
