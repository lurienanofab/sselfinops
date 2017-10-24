using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using LNF.CommonTools;

namespace sselFinOps.AppCode.DAL
{
    public static class GlobalCostDA
    {
        public static GlobalCost GetGlobalCost()
        {
            using (var dba = new SQLDBAccess("cnSselData"))
            using (IDataReader dr = dba.ExecuteReader("GlobalCost_Select"))
            {
                GlobalCost gc = new GlobalCost();

                if (dr.Read())
                {
                    gc.LabAccountID = Convert.ToInt32(dr["LabAccountID"]);
                    gc.LabCreditAccountID = Convert.ToInt32(dr["labCreditAccountID"]);
                    gc.Number = dr["Number"].ToString();
                    gc.ShortCode = dr["ShortCode"].ToString();
                    gc.SubsidyCreditAccountNumber = dr["SubsidyCreditAccountNumber"].ToString();
                }
                else
                {
                    //Make sure we know something is wrong
                    gc.LabAccountID = -1;
                    gc.LabCreditAccountID = -1;
                    gc.Number = string.Empty;
                    gc.ShortCode = string.Empty;
                }

                dr.Close();

                return gc;
            }
        }
    }
}
