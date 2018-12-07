using LNF.Repository;
using System.Data;

namespace sselFinOps.AppCode.DAL
{
    public static class AddressDA
    {
        public static DataTable GetAddressByAccountID(int accountId)
        {
            return DA.Command()
                .Param("Action", "ByAccount")
                .Param("AccountID", accountId)
                .FillDataTable("dbo.Address_Select");
        }
    }
}
