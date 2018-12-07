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
            return DA.Command()
                .Param("Action", "All")
                .FillDataTable("dbo.BillingType_Select");
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
            try
            {
                return DA.Command()
                    .Param("Action", "GetCurrentTypeName")
                    .Param("ClientOrgID", clientOrgId)
                    .ExecuteScalar<string>("dbo.ClientOrgBillingTypeTS_Select");
            }
            catch (Exception ex)
            {
                return $"Error, there is no billing type with this user [{ex.Message}]";
            }
        }
    }
}
