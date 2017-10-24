<%@ WebHandler Language="C#" Class="sselFinOps.Ajax.Invoice" %>

using System;
using System.Collections;
using System.Linq;
using System.Web;
using System.Web.SessionState;
using System.Data;
using LNF.Billing;
using LNF.Repository;
using LNF.Repository.Data;
using LNF.Repository.Billing;
using Newtonsoft.Json;

namespace sselFinOps.Ajax
{
    public class Invoice : IHttpHandler, IReadOnlySessionState
    {
        public bool IsReusable
        {
            get { return false; }
        }

        public void ProcessRequest(HttpContext context)
        {
            IEnumerable data = null;
            try
            {
                string command = context.Request["command"];
                switch (command)
                {
                    case "external-invoice-summary":
                        bool showRemote = bool.Parse(context.Request["showRemote"]);
                        DateTime period = DateTime.Parse(context.Request["period"]);
                        DateTime sd = period;
                        DateTime ed = period.AddMonths(1);
                        //data = GetExternalInvoiceSummary(sd, ed, showRemote);
                        data = GetExternalInvoiceSummaryData(showRemote, sd, ed);
                        context.Response.ContentType = "application/json";
                        context.Response.Write(GetJson(new { Success = true, Message = string.Empty, Data = data }));
                        break;
                    default:
                        throw new NotImplementedException();
                }
            }
            catch (Exception ex)
            {
                context.Response.Write(GetJson(new { Success = false, Message = ex.Message, Data = data }));
            }
        }

        private string GetJson(object value)
        {
            return JsonConvert.SerializeObject(value);
        }

        private IEnumerable GetExternalInvoiceSummaryData(bool showRemote, DateTime sd, DateTime ed)
        {
            DataSet dsInvoiceReport = ExternalInvoiceUtility.GetInvoiceReportFromSession(sd, ed);
            if (dsInvoiceReport != null)
            {
                DataView dvOrgs = ExternalInvoiceUtility.GetOrgs(dsInvoiceReport, sd);
                DataView dvSummary = ExternalInvoiceUtility.GetUsageSummary(dvOrgs, showRemote, sd, ed);

                var list = dvSummary.Cast<DataRowView>().Select(drv => new { ChargeType = drv["ChargeType"].ToString(), Total = Convert.ToDouble(drv["Total"]).ToString("C") });

                return list;
            }
            return null;
        }

        private InvoiceSummaryItem[] GetExternalInvoiceSummary(DateTime sd, DateTime ed, bool showRemote)
        {
            int internalChargeTypeId = 5;
            int remoteBilingTypeID = BillingType.Remote;

            ChargeType[] externalChargeTypes = DA.Current.Query<ChargeType>().Where(x => x.ChargeTypeID != internalChargeTypeId).ToArray();

            //get all usage during the date range
            ToolBilling[] toolUsage = DA.Current.Query<ToolBilling>().Where(x => x.Period >= sd && x.Period < ed && x.ChargeTypeID != internalChargeTypeId).ToArray();
            RoomBilling[] roomUsage = DA.Current.Query<RoomBilling>().Where(x => x.Period >= sd && x.Period < ed && x.ChargeTypeID != internalChargeTypeId).ToArray();
            StoreBilling[] storeUsage = DA.Current.Query<StoreBilling>().Where(x => x.Period >= sd && x.Period < ed && x.ChargeTypeID != internalChargeTypeId).ToArray();
            MiscBillingCharge[] miscUsage = DA.Current.Query<MiscBillingCharge>().Where(x => x.Period >= sd && x.Period < ed && x.Account.Org.OrgType.ChargeType.ChargeTypeID != internalChargeTypeId).ToArray();

            var result = externalChargeTypes.Select(x =>
            {
                var item = new InvoiceSummaryItem();

                item.ChargeTypeName = x.ChargeTypeName;

                decimal total = 0;

                total += toolUsage.Where(u => u.ChargeTypeID == x.ChargeTypeID && (u.BillingTypeID != remoteBilingTypeID || showRemote)).Sum(s => s.GetLineCost());
                total += roomUsage.Where(u => u.ChargeTypeID == x.ChargeTypeID && (u.BillingTypeID != remoteBilingTypeID || showRemote)).Sum(s => s.GetLineCost());
                total += storeUsage.Where(u => u.ChargeTypeID == x.ChargeTypeID).Sum(s => s.GetLineCost());
                total += miscUsage.Where(u => u.Account.Org.OrgType.ChargeType.ChargeTypeID == x.ChargeTypeID).Sum(s => s.GetLineCost());

                item.Total = total.ToString("C");

                return item;
            }).ToArray();

            return result;
        }
    }

    public class InvoiceSummaryItem
    {
        public string ChargeTypeName { get; set; }
        public string Total { get; set; }
    }
}