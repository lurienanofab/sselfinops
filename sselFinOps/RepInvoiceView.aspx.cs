using LNF.Billing;
using LNF.Cache;
using sselFinOps.AppCode;
using System;
using System.IO;
using System.Linq;

namespace sselFinOps
{
    public partial class RepInvoiceView : ReportPage
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!Page.IsPostBack)
            {
                int orgAcctId = GetOrgAcctID();
                DateTime startPeriod = GetStartPeriod();
                DateTime endPeriod = GetEndPeriod();

                if (orgAcctId == 0)
                {
                    ShowError("Invalid parameter: OrgAcctID");
                    return;
                }

                if (startPeriod == default(DateTime))
                {
                    ShowError("Invalid parameter: StartPeriod");
                    return;
                }

                if (endPeriod == default(DateTime))
                {
                    ShowError("Invalid parameter: EndPeriod");
                    return;
                }

                if (string.IsNullOrEmpty(Request.QueryString["ReportType"]))
                {
                    ShowError("Invalid parameter: ReportType");
                    return;
                }

                string reportType = Request.QueryString["ReportType"];

                if (!new[] { "Excel", "Html" }.Contains(reportType))
                {
                    ShowError("Invalid parameter: ReportType");
                    return;
                }

                // made it this far so we passed all the validation tests

                //GetInvoiceData(startPeriod, endPeriod);
                var mgr = new ExternalInvoiceManager(startPeriod, endPeriod, ShowRemote());

                switch (reportType)
                {
                    case "Excel":
                        CreateExcelInvoice(mgr, orgAcctId);
                        break;
                    case "Html":
                        DisplayHtmlInvoice(mgr, orgAcctId);
                        break;
                    default:
                        litErrorMessage.Text = "<div class=\"error-message\">Invalid parameter: ReportType</div>";
                        break;
                }
            }
        }

        private int GetOrgAcctID()
        {
            int result;

            if (int.TryParse(Request.QueryString["OrgAcctID"], out result))
                return result;

            return 0;
        }

        private DateTime GetStartPeriod()
        {
            DateTime result;

            if (DateTime.TryParse(Request.QueryString["StartPeriod"], out result))
                return result;
            else
                return CacheManager.Current.StartPeriod();
        }

        private DateTime GetEndPeriod()
        {
            DateTime result;

            if (DateTime.TryParse(Request.QueryString["EndPeriod"], out result))
                return result;
            else
                return CacheManager.Current.EndPeriod();
        }

        private bool ShowRemote()
        {
            if (!string.IsNullOrEmpty(Request.QueryString["ShowRemote"]))
            {
                bool result;
                if (bool.TryParse(Request.QueryString["ShowRemote"], out result))
                    return result;
            }

            return false;
        }

        private void ShowError(string msg)
        {
            if (!string.IsNullOrEmpty(msg))
                litErrorMessage.Text = string.Format("<div class=\"error-message\">{0}</div>", msg);
        }

        private void CreateExcelInvoice(ExternalInvoiceManager mgr, int orgAcctId)
        {
            var inv = mgr.GetInvoices(orgAcctId).First();

            string filePath;
            if (mgr.StartDate >= RepInvoice.July2009)
            {
                string alert = string.Empty;
                filePath = ExcelUtility.GenerateInvoiceExcelReport(inv, CacheManager.Current.ClientID, string.Empty, true, ref alert);
                ShowError(alert);
            }
            else
                filePath = ExcelUtility.MakeSpreadSheet(inv.Header.AccountID, inv.Header.InvoiceNumber, inv.Header.DeptRef, inv.Header.OrgName, CacheManager.Current.ClientID, mgr.StartDate, mgr.EndDate);

            // display excel spreadsheet
            if (!string.IsNullOrEmpty(filePath))
            {
                //only include the file name
                Response.Redirect(RepInvoice.GetVirtualPath(Path.GetFileName(filePath), null));
            }
        }

        public void DisplayHtmlInvoice(ExternalInvoiceManager mgr, int orgAcctId)
        {
            var inv = mgr.GetInvoices(orgAcctId).First();

            litAccountName.Text = string.Format("{0} ({1})", inv.Header.AccountName, inv.Header.OrgName);

            if (mgr.StartDate.AddMonths(1) == mgr.EndDate)
                litAccountName.Text += string.Format(" - {0:MMM yyyy}", mgr.StartDate);
            else
                litAccountName.Text += string.Format(" - {0:MMM yyyy} to {1:MMM yyyy}", mgr.StartDate, mgr.EndDate.AddMonths(-1));

            litInvoiceTotal.Text = string.Format("Total: {0:C}", inv.Usage.Sum(x => x.LineTotal));

            rptInvoice.DataSource = inv.Usage;
            rptInvoice.DataBind();
        }

        protected void btnCreateExcelInvoice_Click(object sender, EventArgs e)
        {
            int orgAcctId = GetOrgAcctID();
            var mgr = new ExternalInvoiceManager(GetStartPeriod(), GetEndPeriod(), ShowRemote());
            CreateExcelInvoice(mgr, orgAcctId);
        }
    }
}