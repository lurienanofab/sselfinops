using LNF.Billing;
using LNF.Billing.Reports;
using LNF.Billing.Reports.ServiceUnitBilling;
using System;
using System.Collections.Generic;

namespace sselFinOps.AppCode
{
    public class ReportFactory
    {
        public IReportRepository ReportRepository { get; }

        public ReportFactory(IReportRepository repo)
        {
            ReportRepository = repo;
        }

        public RoomJU GetReportRoomJU(DateTime sd, DateTime ed, JournalUnitTypes type, int id)
        {
            return ReportRepository.GetRoomJU(sd, ed, type.ToString(), id);
        }

        public ToolJU GetReportToolJU(DateTime sd, DateTime ed, JournalUnitTypes type, int id)
        {
            return ReportRepository.GetToolJU(sd, ed, type.ToString(), id);
        }

        public RoomSUB GetReportRoomSUB(DateTime sd, DateTime ed, int id)
        {
            return ReportRepository.GetRoomSUB(sd, ed, id);
        }

        public ToolSUB GetReportToolSUB(DateTime sd, DateTime ed, int id)
        {
            return ReportRepository.GetToolSUB(sd, ed, id);
        }

        public StoreSUB GetReportStoreSUB(DateTime sd, DateTime ed, bool twoCreditAccounts, int id)
        {
            string option = null;

            if (twoCreditAccounts)
                option = "two-credit-accounts";

            return ReportRepository.GetStoreSUB(sd, ed, id, option);
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
            total = 0;
            var allItems = new List<JournalUnitReportItem>();

            if (roomJU != null)
            {
                allItems.AddRange(roomJU.Items);
                total += roomJU.CreditEntry.MerchandiseAmount;
            }

            if (toolJU != null)
            {
                allItems.AddRange(toolJU.Items);
                total += toolJU.CreditEntry.MerchandiseAmount;
            }

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

        public static List<ServiceUnitBillingReportItem> GetSUB(ServiceUnitBillingReport sub, out double total)
        {
            var allItems = new List<ServiceUnitBillingReportItem>();

            foreach (var group in sub.Items)
                allItems.AddRange(group);

            total = 0;
            foreach (var bu in sub.Summaries)
                total += bu.MerchandiseAmount;

            return allItems;
        }
    }
}