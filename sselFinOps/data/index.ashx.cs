using LNF.Models.Billing;
using LNF.Models.Billing.Reports.ServiceUnitBilling;
using sselFinOps.AppCode;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web;
using System.Xml.Linq;

namespace sselFinOps.Data
{
    /// <summary>
    /// Summary description for index
    /// </summary>
    public class Index : IHttpHandler
    {
        public bool IsReusable => false;

        public void ProcessRequest(HttpContext context)
        {
            string report = context.Request.QueryString["report"];
            string charge = context.Request.QueryString["charge"];
            string sdate = context.Request.QueryString["sdate"];
            string edate = context.Request.QueryString["edate"];

            DateTime sd = DateTime.ParseExact(sdate, "yyyyMM", CultureInfo.InvariantCulture);
            DateTime ed = DateTime.ParseExact(edate, "yyyyMM", CultureInfo.InvariantCulture);
            int clientId = GetClientID(context);

            XDocument xdoc;

            switch (report)
            {
                case "sub":
                    xdoc = GetSubReport(charge, sd, ed, clientId);
                    break;
                case "ju-a":
                case "ju-b":
                case "ju-c":
                case "ju-all":
                    xdoc = GetJuReport(GetJuType(report), charge, sd, ed, clientId);
                    break;
                default:
                    throw new InvalidOperationException($"Invalid report: {report}");
            }

            context.Response.ContentType = "text/xml";
            context.Response.Write(xdoc.ToString());
        }

        private JournalUnitTypes GetJuType(string report)
        {
            switch (report)
            {
                case "ju-a":
                    return JournalUnitTypes.A;
                case "ju-b":
                    return JournalUnitTypes.B;
                case "ju-c":
                    return JournalUnitTypes.C;
                case "ju-all":
                    return JournalUnitTypes.All;
                default:
                    throw new ArgumentException($"Invalid report: {report}", report);
            }
        }

        private XDocument GetSubReport(string charge, DateTime sd, DateTime ed, int clientId)
        {
            RoomSUB roomSUB;
            ToolSUB toolSUB;
            StoreSUB storeSUB;
            double total;
            IEnumerable<ServiceUnitBillingReportItem> allItems;

            switch (charge)
            {
                case "all":
                    roomSUB = ReportFactory.GetReportRoomSUB(sd, ed, clientId);
                    toolSUB = ReportFactory.GetReportToolSUB(sd, ed, clientId);
                    storeSUB = ReportFactory.GetReportStoreSUB(sd, ed, false, clientId);
                    allItems = ReportFactory.GetAllSUB(roomSUB, toolSUB, storeSUB, out total);
                    break;
                case "room":
                    roomSUB = ReportFactory.GetReportRoomSUB(sd, ed, clientId);
                    allItems = ReportFactory.GetSUB(roomSUB, out total);
                    break;
                case "tool":
                    toolSUB = ReportFactory.GetReportToolSUB(sd, ed, clientId);
                    allItems = ReportFactory.GetSUB(toolSUB, out total);
                    break;
                case "store":
                    storeSUB = ReportFactory.GetReportStoreSUB(sd, ed, false, clientId);
                    allItems = ReportFactory.GetSUB(storeSUB, out total);
                    break;
                default:
                    throw new ArgumentException($"Invalid charge: {charge}", "charge");
            }

            Func<ServiceUnitBillingReportItem, int> invoiceDateOrder = item =>
            {
                if (string.IsNullOrEmpty(item.InvoiceDate))
                    return 0;
                else
                {
                    var d = DateTime.ParseExact(item.InvoiceDate, "yyyy/MM/dd", CultureInfo.InvariantCulture);
                    var ts = (int)(d.Subtract(new DateTime(1970, 1, 1))).TotalSeconds;
                    return ts;
                }
            };

            var items = allItems
                .Where(x => !string.IsNullOrEmpty(x.CardType))
                .OrderBy(x => x.ReportType)
                .ThenBy(x => x.ChargeType)
                .ThenBy(invoiceDateOrder);

            var result = new XDocument(new XElement("table",
                items.Select(x => new XElement("row",
                    new XElement("Period", x.Period),
                    new XElement("ReportType", x.ReportType),
                    new XElement("ChargeType", x.ChargeType),
                    new XElement("CardType", x.CardType),
                    new XElement("ShortCode", x.ShortCode),
                    new XElement("Account", x.Account),
                    new XElement("FundCode", x.FundCode),
                    new XElement("DeptID", x.DeptID),
                    new XElement("ProgramCode", x.ProgramCode),
                    new XElement("Class", x.Class),
                    new XElement("ProjectGrant", x.ProjectGrant),
                    new XElement("VendorID", x.VendorID),
                    new XElement("InvoiceDate", x.InvoiceDate),
                    new XElement("InvoiceID", x.InvoiceID),
                    new XElement("Uniqname", x.Uniqname),
                    new XElement("LocationCode", x.LocationCode),
                    new XElement("DeliverTo", x.DeliverTo),
                    new XElement("VendorOrderNum", x.VendorOrderNum),
                    new XElement("DepartmentalReferenceNumber", x.DepartmentalReferenceNumber),
                    new XElement("TripOrEventNumber", x.TripOrEventNumber),
                    new XElement("ItemDescription", x.ItemDescription),
                    new XElement("VendorItemID", x.VendorItemID),
                    new XElement("ManufacturerName", x.ManufacturerName),
                    new XElement("ModelNum", x.ModelNum),
                    new XElement("SerialNum", x.SerialNum),
                    new XElement("UMTagNum", x.UMTagNum),
                    new XElement("QuantityVouchered", x.QuantityVouchered),
                    new XElement("UnitOfMeasure", x.UnitOfMeasure),
                    new XElement("UnitPrice", x.UnitPrice),
                    new XElement("MerchandiseAmount", x.MerchandiseAmount),
                    new XElement("VoucherComment", x.VoucherComment)
                ))
            ));

            return result;
        }

        private XDocument GetJuReport(JournalUnitTypes juType, string charge, DateTime sd, DateTime ed, int clientId)
        {
            RoomJU roomJU;
            RoomJU roomJUA;
            RoomJU roomJUB;
            RoomJU roomJUC;
            ToolJU toolJU;
            ToolJU toolJUA;
            ToolJU toolJUB;
            ToolJU toolJUC;
            double temp, total;
            List<JournalUnitReportItem> allItems;

            switch (charge)
            {
                case "all":
                    switch (juType)
                    {
                        case JournalUnitTypes.A:
                        case JournalUnitTypes.B:
                        case JournalUnitTypes.C:
                            roomJU = ReportFactory.GetReportRoomJU(sd, ed, juType, clientId);
                            toolJU = ReportFactory.GetReportToolJU(sd, ed, juType, clientId);
                            allItems = ReportFactory.GetAllJU(roomJU, toolJU, out total);
                            break;
                        case JournalUnitTypes.All:
                            roomJUA = ReportFactory.GetReportRoomJU(sd, ed, JournalUnitTypes.A, clientId);
                            toolJUA = ReportFactory.GetReportToolJU(sd, ed, JournalUnitTypes.A, clientId);
                            roomJUB = ReportFactory.GetReportRoomJU(sd, ed, JournalUnitTypes.B, clientId);
                            toolJUB = ReportFactory.GetReportToolJU(sd, ed, JournalUnitTypes.B, clientId);
                            roomJUC = ReportFactory.GetReportRoomJU(sd, ed, JournalUnitTypes.C, clientId);
                            toolJUC = ReportFactory.GetReportToolJU(sd, ed, JournalUnitTypes.C, clientId);

                            total = 0;

                            allItems = new List<JournalUnitReportItem>();

                            allItems.AddRange(ReportFactory.GetAllJU(roomJUA, toolJUA, out temp));
                            total += temp;

                            allItems.AddRange(ReportFactory.GetAllJU(roomJUB, toolJUB, out temp));
                            total += temp;

                            allItems.AddRange(ReportFactory.GetAllJU(roomJUC, toolJUC, out temp));
                            total += temp;
                            break;
                        default:
                            throw new ArgumentException(string.Format("Invalid JournalUnitTypes value: {0}", juType));
                    }
                    break;
                case "room":
                    switch (juType)
                    {
                        case JournalUnitTypes.A:
                        case JournalUnitTypes.B:
                        case JournalUnitTypes.C:
                            roomJU = ReportFactory.GetReportRoomJU(sd, ed, juType, clientId);
                            allItems = ReportFactory.GetAllJU(roomJU, null, out total);
                            break;
                        case JournalUnitTypes.All:
                            roomJUA = ReportFactory.GetReportRoomJU(sd, ed, JournalUnitTypes.A, clientId);
                            roomJUB = ReportFactory.GetReportRoomJU(sd, ed, JournalUnitTypes.B, clientId);
                            roomJUC = ReportFactory.GetReportRoomJU(sd, ed, JournalUnitTypes.C, clientId);

                            total = 0;

                            allItems = new List<JournalUnitReportItem>();

                            allItems.AddRange(ReportFactory.GetAllJU(roomJUA, null, out temp));
                            total += temp;

                            allItems.AddRange(ReportFactory.GetAllJU(roomJUB, null, out temp));
                            total += temp;

                            allItems.AddRange(ReportFactory.GetAllJU(roomJUC, null, out temp));
                            total += temp;
                            break;
                        default:
                            throw new ArgumentException(string.Format("Invalid JournalUnitTypes value: {0}", juType));
                    }
                    break;
                case "tool":
                    switch (juType)
                    {
                        case JournalUnitTypes.A:
                        case JournalUnitTypes.B:
                        case JournalUnitTypes.C:
                            toolJU = ReportFactory.GetReportToolJU(sd, ed, juType, clientId);
                            allItems = ReportFactory.GetAllJU(null, toolJU, out total);
                            break;
                        case JournalUnitTypes.All:
                            toolJUA = ReportFactory.GetReportToolJU(sd, ed, JournalUnitTypes.A, clientId);
                            toolJUB = ReportFactory.GetReportToolJU(sd, ed, JournalUnitTypes.B, clientId);
                            toolJUC = ReportFactory.GetReportToolJU(sd, ed, JournalUnitTypes.C, clientId);

                            total = 0;

                            allItems = new List<JournalUnitReportItem>();

                            allItems.AddRange(ReportFactory.GetAllJU(null, toolJUA, out temp));
                            total += temp;

                            allItems.AddRange(ReportFactory.GetAllJU(null, toolJUB, out temp));
                            total += temp;

                            allItems.AddRange(ReportFactory.GetAllJU(null, toolJUC, out temp));
                            total += temp;
                            break;
                        default:
                            throw new ArgumentException(string.Format("Invalid JournalUnitTypes value: {0}", juType));
                    }
                    break;
                default:
                    throw new ArgumentException($"Invalid charge: {charge}", "charge");
            }

            var items = allItems
                .Where(i => i.ItemDescription != "zzdoscar")
                .OrderBy(x => x.ReportType)
                .ThenBy(x => x.ChargeType);

            var result = new XDocument(new XElement("table",
                items.Select(x => new XElement("row",
                    new XElement("Period", x.Period),
                    new XElement("ReportType", x.ReportType),
                    new XElement("ChargeType", x.ChargeType),
                    new XElement("Account", x.Account),
                    new XElement("Fund", x.FundCode),
                    new XElement("Department", x.DeptID),
                    new XElement("Program", x.ProgramCode),
                    new XElement("Class", x.Class),
                    new XElement("PojectGrant", x.ProjectGrant),
                    new XElement("MonetaryAmount", GetMonetaryAmount(x)),
                    new XElement("JournalLineReference", x.DepartmentalReferenceNumber),
                    new XElement("LineDescription", x.ItemDescription),
                    new XElement("StatAmount", string.Empty)
                ))
            ));

            return result;
        }

        private int GetClientID(HttpContext context)
        {
            if (!string.IsNullOrEmpty(context.Request.QueryString["clientId"]))
                return int.Parse(context.Request.QueryString["clientId"]);

            return 0;
        }

        private double GetMonetaryAmount(JournalUnitReportItem item)
        {
            return string.IsNullOrEmpty(item.MerchandiseAmount) ? 0 : Convert.ToDouble(item.MerchandiseAmount);
        }
    }
}