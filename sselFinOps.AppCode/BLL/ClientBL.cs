using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using sselFinOps.AppCode.DAL;

namespace sselFinOps.AppCode.BLL
{
    public static class ClientBL
    {
        public static DataTable GetAllClientByDate(int year, int month)
        {
            DateTime sDate = new DateTime(year, month, 1);
            return ClientDA.GetAllClient(sDate, sDate.AddMonths(1));
        }

        public static DataTable GetStaff(int year, int month)
        {
            DateTime sDate = new DateTime(year, month, 1);
            return ClientDA.GetStaff(sDate, sDate.AddMonths(1));
        }

        public static DataTable GetRemoteClientByDate(int year, int month)
        {
            DateTime sDate = new DateTime(year, month, 1);
            return ClientDA.GetRemoteUser(sDate, sDate.AddMonths(1));
        }
    }
}
