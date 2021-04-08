using LNF.Repository;
using System;

namespace sselFinOps.AppCode.DAL
{
    public static class GlobalCostDA
    {
        public static GlobalCost GetGlobalCost()
        {
            using (var reader = DataCommand.Create().ExecuteReader("dbo.GlobalCost_Select"))
            {
                GlobalCost gc = new GlobalCost();

                if (reader.Read())
                {
                    gc.LabAccountID = Convert.ToInt32(reader["LabAccountID"]);
                    gc.LabCreditAccountID = Convert.ToInt32(reader["labCreditAccountID"]);
                    gc.Number = Convert.ToString(reader["Number"]);
                    gc.ShortCode = Convert.ToString(reader["ShortCode"]);
                    gc.SubsidyCreditAccountNumber = Convert.ToString(reader["SubsidyCreditAccountNumber"]);
                }
                else
                {
                    //Make sure we know something is wrong
                    gc.LabAccountID = -1;
                    gc.LabCreditAccountID = -1;
                    gc.Number = string.Empty;
                    gc.ShortCode = string.Empty;
                }

                reader.Close();

                return gc;
            }
        }
    }
}
