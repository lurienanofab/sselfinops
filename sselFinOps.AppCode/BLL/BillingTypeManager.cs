using LNF.CommonTools;
using LNF.Repository;
using LNF.Repository.Billing;
using System;
using System.Data;

namespace sselFinOps.AppCode.BLL
{
    public static class BillingTypeManager
    {
        public static DataTable GetBillingTypes()
        {
            using (var dba = new SQLDBAccess("cnSselData"))
                return dba.ApplyParameters(new {Action="All"}).FillDataTable("BillingType_Select");
        }

	    public static double GetTotalCostByBillingType(int billingTypeId, double hours, double entries, LabRoom room, double totalCalcCost)
        {
		    double result = 0;

            if (room == LabRoom.CleanRoom)
            {
                if (billingTypeId == BillingType.Int_Ga)
                    return (totalCalcCost / 1315) * 875;

                else if (billingTypeId == BillingType.Int_Si)
                    return totalCalcCost;

                else if (billingTypeId == BillingType.Int_Hour)
                    return 2.5 * entries + 15 * hours;

                else if (billingTypeId == BillingType.Int_Tools)
                    return 2.5 * entries;

                else if (billingTypeId == BillingType.ExtAc_Ga)
                    return (totalCalcCost / 1315) * 875;

                else if (billingTypeId == BillingType.ExtAc_Si)
                    return totalCalcCost;

                else if (billingTypeId == BillingType.ExtAc_Tools)
                    return 2.5 * entries;

                else if (billingTypeId == BillingType.ExtAc_Hour)
                    return 2.5 * entries + 15 * hours;

                else if (billingTypeId == BillingType.NonAc)
                    return hours * 77;

                else if (billingTypeId == BillingType.NonAc_Tools)
                    return 2.5 * hours;

                else if (billingTypeId == BillingType.NonAc_Hour)
                    return 2.5 * entries + 45 * hours;

                else if (billingTypeId == BillingType.Other)
                    return 0;
            }
            else if (room == LabRoom.ChemRoom)
            {
                if (billingTypeId == BillingType.Other)
                    return 0;
                else if (billingTypeId >= BillingType.NonAc)
                    return 190;
                else
                    return 95;
            }
            else if (room == LabRoom.TestingLab)
            {
                if (billingTypeId == BillingType.Other)
                    return 0;
                else if (billingTypeId >= BillingType.NonAc)
                    return 50;
                else
                    return 25;
            }
            else
                return 99999;

		    return result;
	    }

	    public static string GetBillingTypeName(int clientOrgId)
        {
            using (var dba = new SQLDBAccess("cnSselData"))
            {
                dba.AddParameter("@Action", "GetCurrentTypeName");
                dba.AddParameter("@ClientOrgID", clientOrgId);

                string billingtype = string.Empty;

                try
                {
                    billingtype = dba.ExecuteScalar<string>("ClientOrgBillingTypeTS_Select");
                    return billingtype;
                }
                catch (Exception ex)
                {
                    return string.Format("Error, there is no billing type with this user [{0}]", ex.Message);
                }
            }
        }
    }
}
