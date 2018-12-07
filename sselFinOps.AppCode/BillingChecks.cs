using LNF.Repository;
using System;
using System.Data;

namespace sselFinOps.AppCode
{
    public class BillingChecks
    {
        public DateTime Period { get; private set; }

        public BillingChecks(DateTime period)
        {
            Period = period;
        }

        public DataView DaysWithData()
        {
            var dt = DA.Command()
                .Param("Period", Period)
                .FillDataTable("Billing.dbo.Report_DaysWithData");

            var dv = new DataView(dt, string.Empty, string.Empty, DataViewRowState.CurrentRows);

            return dv;
        }

        public DataView SubsidyComparison()
        {
            var dt = DA.Command()
                .Param("Period", Period)
                .FillDataTable("Billing.dbo.Report_SubsidyComparison");

            var dv = new DataView(dt, string.Empty, string.Empty, DataViewRowState.CurrentRows);

            return dv;
        }
    }
}
