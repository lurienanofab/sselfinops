using LNF.Models.Billing;
using LNF.Models.Billing.Reports.ServiceUnitBilling;
using OnlineServices.Api;
using OnlineServices.Api.Billing;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace sselFinOps.AppCode
{
    public static class ReportFactory
    {
        public static async Task<RoomJU> GetReportRoomJU(DateTime sd, DateTime ed, JournalUnitTypes type, int id)
        {
            using (var billingClient = new BillingClient())
            {
                RoomJU report = await billingClient.GetRoomJU(sd, ed, type, id);
                return report;
            }
        }

        public static async Task<ToolJU> GetReportToolJU(DateTime sd, DateTime ed, JournalUnitTypes type, int id)
        {
            using (var billingClient = new BillingClient())
            {
                ToolJU report = await billingClient.GetToolJU(sd, ed, type, id);
                return report;
            }
        }

        public static async Task<RoomSUB> GetReportRoomSUB(DateTime sd, DateTime ed, int id)
        {
            using (var billingClient = new BillingClient())
            {
                RoomSUB report = await billingClient.GetRoomSUB(sd, ed, id);
                return report;
            }
        }

        public static async Task<ToolSUB> GetReportToolSUB(DateTime sd, DateTime ed, int id)
        {
            using (var billingClient = new BillingClient())
            {
                ToolSUB report = await billingClient.GetToolSUB(sd, ed, id);
                return report;
            }
        }

        public static async Task<StoreSUB> GetReportStoreSUB(DateTime sd, DateTime ed, bool twoCreditAccounts, int id)
        {
            using (var billingClient = new BillingClient())
            {
                StoreSUB report = await billingClient.GetStoreSUB(sd, ed, twoCreditAccounts, id);
                return report;
            }
        }

        public static List<JournalUnitReportItem> GetAllRoomJU(RoomJU juA, RoomJU juB, RoomJU juC, out double total)
        {
            var allItems = new List<JournalUnitReportItem>();

            allItems.AddRange(juA.Items);
            allItems.AddRange(juB.Items);
            allItems.AddRange(juC.Items);

            total = 0;
            total += juA.CreditEntry.MerchandiseAmount;
            total += juB.CreditEntry.MerchandiseAmount;
            total += juC.CreditEntry.MerchandiseAmount;

            return allItems;
        }

        public static List<JournalUnitReportItem> GetAllToolJU(ToolJU juA, ToolJU juB, ToolJU juC, out double total)
        {
            var allItems = new List<JournalUnitReportItem>();

            allItems.AddRange(juA.Items);
            allItems.AddRange(juB.Items);
            allItems.AddRange(juC.Items);

            total = 0;
            total += juA.CreditEntry.MerchandiseAmount;
            total += juB.CreditEntry.MerchandiseAmount;
            total += juC.CreditEntry.MerchandiseAmount;

            return allItems;
        }

        public static List<JournalUnitReportItem> GetAllJU(RoomJU roomJU, ToolJU toolJU, out double total)
        {
            var allItems = new List<JournalUnitReportItem>();

            allItems.AddRange(roomJU.Items);
            allItems.AddRange(toolJU.Items);

            total = 0;
            total += roomJU.CreditEntry.MerchandiseAmount;
            total += toolJU.CreditEntry.MerchandiseAmount;

            return allItems;
        }

        public static List<ServiceUnitBillingReportItem> GetAllSUB(RoomSUB roomSUB, ToolSUB toolSUB, StoreSUB storeSUB, out double total)
        {
            var allItems = new List<ServiceUnitBillingReportItem>();

            allItems.AddRange(roomSUB.CombinedItems);
            allItems.AddRange(toolSUB.CombinedItems);
            allItems.AddRange(storeSUB.CombinedItems);

            total = 0;

            foreach (var bu in roomSUB.Summaries)
            {
                total += bu.MerchandiseAmount;
            }

            foreach (var bu in toolSUB.Summaries)
            {
                total += bu.MerchandiseAmount;
            }

            foreach (var bu in storeSUB.Summaries)
            {
                total += bu.MerchandiseAmount;
            }

            return allItems;
        }
    }
}