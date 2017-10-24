using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using sselFinOps.AppCode.DAL;

namespace sselFinOps.AppCode.BLL
{
    public static class AccountBL
    {
        public static DataTable GetAccountsByClientIDAndDate(int clientId, int year, int month)
        {
            DateTime sDate = new DateTime(year, month, 1);
            DateTime eDate = sDate.AddMonths(1);

            return AccountDA.GetAccountsByClientIDAndDate(clientId, sDate, eDate);
        }
    }
}
