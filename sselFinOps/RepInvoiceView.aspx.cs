﻿using LNF.Billing;
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
                int accountId = GetAccountID();
                DateTime startPeriod = GetStartPeriod();
                DateTime endPeriod = GetEndPeriod();

                if (accountId == 0)
                {
                    ShowError("Invalid parameter: AccountID");
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

                var mgr = new ExternalInvoiceManager(accountId, startPeriod, endPeriod, ShowRemote());
                var inv = mgr.GetInvoices(accountId).First();

                switch (reportType)
                {
                    case "Excel":
                        CreateExcelInvoice(inv);
                        break;
                    case "Html":
                        DisplayHtmlInvoice(inv);
                        break;
                    default:
                        litErrorMessage.Text = "<div class=\"error-message\">Invalid parameter: ReportType</div>";
                        break;
                }
            }
        }

        private int GetAccountID()
        {
            int result;

            if (int.TryParse(Request.QueryString["AccountID"], out result))
                return result;

            return 0;
        }

        private DateTime GetStartPeriod()
        {
            DateTime result;

            if (DateTime.TryParse(Request.QueryString["StartPeriod"], out result))
                return result;

            return default(DateTime);
        }

        private DateTime GetEndPeriod()
        {
            DateTime result;

            if (DateTime.TryParse(Request.QueryString["EndPeriod"], out result))
                return result;

            return default(DateTime);
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

        private void CreateExcelInvoice(ExternalInvoice inv)
        {
            string filePath;
            if (inv.StartDate >= RepInvoice.July2009)
            {
                string alert = string.Empty;
                filePath = ExcelUtility.GenerateInvoiceExcelReport(inv, CacheManager.Current.ClientID, string.Empty, true, ref alert);
                ShowError(alert);
            }
            else
                filePath = ExcelUtility.MakeSpreadSheet(inv.Header.AccountID, inv.Header.InvoiceNumber, inv.Header.DeptRef, inv.Header.OrgName, CacheManager.Current.ClientID, inv.StartDate, inv.EndDate);

            // display excel spreadsheet
            if (!string.IsNullOrEmpty(filePath))
            {
                //only include the file name
                Response.Redirect(RepInvoice.GetVirtualPath(Path.GetFileName(filePath), null));
            }
        }

        public void DisplayHtmlInvoice(ExternalInvoice inv)
        {
            litAccountName.Text = string.Format("{0} ({1})", inv.Header.AccountName, inv.Header.OrgName);

            if (inv.StartDate.AddMonths(1) == inv.EndDate)
                litAccountName.Text += string.Format(" - {0:MMM yyyy}", inv.StartDate);
            else
                litAccountName.Text += string.Format(" - {0:MMM yyyy} to {1:MMM yyyy}", inv.StartDate, inv.EndDate.AddMonths(-1));

            litInvoiceTotal.Text = string.Format("Total: {0:C}", inv.Usage.Sum(x => x.LineTotal));

            rptInvoice.DataSource = inv.Usage;
            rptInvoice.DataBind();
        }

        protected void btnCreateExcelInvoice_Click(object sender, EventArgs e)
        {
            int accountId = GetAccountID();
            var mgr = new ExternalInvoiceManager(accountId, GetStartPeriod(), GetEndPeriod(), ShowRemote());
            var inv = mgr.GetInvoices(accountId).First();
            CreateExcelInvoice(inv);
        }
    }
}