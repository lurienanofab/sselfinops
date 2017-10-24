using LNF.CommonTools;
using LNF.Repository;
using System;
using System.Data;

namespace sselFinOps.AppCode.DAL
{
    public static class ClientDA
    {
        public static DataTable GetAllClient(DateTime sDate, DateTime eDate)
        {
            using (var dba = new SQLDBAccess("cnSselData"))
            {
                dba.AddParameter("@Action", "AllUniqueName");
                dba.AddParameter("@sDate", sDate);
                dba.AddParameter("@eDate", eDate);
                DataTable dt = dba.FillDataTable("Client_Select");

                //Must set primary key because the client code need to find data in this table
                //should this code below to business logic or data access?
                dt.PrimaryKey =   new[] {dt.Columns["ClientID"]};

                return dt;
            }
        }

        public static DataTable GetClientOrgListWithLabUserPrivilege(int numMonths)
        {
            using (var dba = new SQLDBAccess("cnSselData"))
            {
                dba.AddParameter("@Action", "ForExpCost");
                DataTable dtClientOrg = dba.FillDataTable("ClientOrg_Select");

                for (int i = 0; i < numMonths; i++)
                {
                    dtClientOrg.Columns.Add(string.Format("mn{0}Room", i), typeof(double));
                    dtClientOrg.Columns.Add(string.Format("mn{0}Tool", i), typeof(double));
                }

                foreach (DataRow dr in dtClientOrg.Rows)
                {
                    for (int i = 0; i < numMonths; i++)
                    {
                        dr.SetField(string.Format("mn{0}Room", i), 0.0);
                        dr.SetField(string.Format("mn{0}Tool", i), 0.0);
                    }
                }

                return dtClientOrg;
            }
        }

        public static DataTable GetAllClientAccountWithManagerName(DateTime sDate, DateTime eDate)
        {
            using (var dba = new SQLDBAccess("cnSselData"))
            {
                dba.AddParameter("@Action", "AllWithManagerName");
                dba.AddParameter("@sDate", sDate);
                dba.AddParameter("@eDate", eDate);
                DataTable dt = dba.FillDataTable("ClientAccount_Select");

                //Must set primary key because the client code need to find data in this table
                //should this code below to business logic or data access?
                dt.PrimaryKey = new[] {dt.Columns["ClientAccountID"]};

                return dt;
            }
        }

        public static DataTable GetInternalClients(DateTime sDate, DateTime eDate)
        {
            using (var dba = new SQLDBAccess("cnSselData"))
            {
                dba.AddParameter("@Action", "ByInternal");
                dba.AddParameter("@sDate", sDate);
                dba.AddParameter("@eDate", eDate);
                return dba.FillDataTable("Client_Select");
            }
        }

        public static DataTable GetStaff(DateTime sDate, DateTime eDate)
        {
            using (var dba = new SQLDBAccess("cnSselData"))
            {
                dba.AddParameter("@Action", "ByStaff");
                dba.AddParameter("@sDate", sDate);
                dba.AddParameter("@eDate", eDate);
                return dba.FillDataTable("Client_Select");
            }
        }

        public static DataTable GetRemoteUser(DateTime sDate, DateTime eDate)
        {
            using (var dba = new SQLDBAccess("cnSselData"))
            {
                dba.AddParameter("@Action", "ByRemoteUser");
                dba.AddParameter("@sDate", sDate);
                dba.AddParameter("@eDate", eDate);
                return dba.FillDataTable("Client_Select");
            }
        }
    }
}
