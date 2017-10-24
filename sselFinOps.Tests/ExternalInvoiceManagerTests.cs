using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using LNF.Billing;
using System.Linq;
using LNF;

namespace sselFinOps.Tests
{
    [TestClass]
    public class ExternalInvoiceManagerTests
    {
        [TestMethod]
        public void GetOrgsTest()
        {
            using (Providers.DataAccess.StartUnitOfWork())
            {
                DateTime sp = DateTime.Parse("2017-09-01");
                DateTime ep = DateTime.Parse("2017-10-01");
                bool showRemote = false;

                var _mgr = new ExternalInvoiceManager(sp, ep, showRemote);

                var summary = _mgr.GetSummary();

                int otherAcadChargeTypeId = 15;

                var summaryTotal = summary.First(x => x.ChargeTypeID == otherAcadChargeTypeId).TotalCharges;

                var invoices = _mgr.GetInvoices();
                var invoiceTotal = invoices.Sum(x => x.Usage.GetChargeTypeTotal(otherAcadChargeTypeId));

                Console.WriteLine("summary total: ${0:#,##0.00}", summaryTotal);
                Console.WriteLine("invoice total: ${0:#,##0.00}", invoiceTotal);
                
                // compare external academic summary total vs invoice totals
                Assert.AreEqual(summaryTotal, invoiceTotal);
            }
        }
    }
}
