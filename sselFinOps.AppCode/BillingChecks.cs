using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using LNF.Repository;
using LNF.CommonTools;

namespace sselFinOps.AppCode
{
    public class BillingChecks
    {
        public DateTime Period {get;private set;}

        public BillingChecks(DateTime period)
        {
            Period = period;
        }

        public DataView DaysWithData()
        {
            using (var dba = DA.Current.GetAdapter())
            {
                dba.AddParameter("@Period", Period);
                DataTable dt = dba.FillDataTable("Billing.dbo.Report_DaysWithData");
                DataView dv = new DataView(dt, string.Empty, string.Empty, DataViewRowState.CurrentRows);
                return dv;
            }
        }

        public DataView SubsidyComparison()
        {
            using (var dba = DA.Current.GetAdapter())
            {
                dba.AddParameter("@Period", Period);
                DataTable dt = dba.FillDataTable("Billing.dbo.Report_SubsidyComparison");
                DataView dv = new DataView(dt, string.Empty, string.Empty, DataViewRowState.CurrentRows);
                return dv;
            }
        }
    }
}
