using LNF.CommonTools;
using LNF.Repository;
using System.Data;

namespace sselFinOps.AppCode.DAL
{
    public static class AddressDA
    {
        public static DataTable GetAddressByAccountID(int accountId)
        {
            using (var dba = new SQLDBAccess("cnSselData"))
            {
                dba.AddParameter("@Action", "ByAccount");
                dba.AddParameter("@AccountID", accountId);
                return dba.FillDataTable("Address_Select");
            }
        }
    }
}
