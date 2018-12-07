using LNF.Billing;
using LNF.Cache;
using LNF.Models.Data;
using sselFinOps.AppCode;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.UI.WebControls;

namespace sselFinOps
{
    public partial class RepInvoice : ReportPage
    {
        public static readonly DateTime July2009 = new DateTime(2009, 7, 1);

        private int alertCount = 0;
        private ExternalInvoiceManager _mgr;

        public override ClientPrivilege AuthTypes
        {
            get { return ClientPrivilege.Executive | ClientPrivilege.Administrator; }
        }

        public DateTime StartPeriod
        {
            get { return pp1.SelectedPeriod; }
            set { pp1.SelectedPeriod = value; }
        }

        public DateTime EndPeriod
        {
            get { return StartPeriod.AddMonths(1); }
        }

        public bool ShowRemote
        {
            get { return chkShowRemote.Checked; }
            set { chkShowRemote.Checked = value; }
        }

        public bool CanDownloadAll()
        {
            return StartPeriod >= July2009;
        }

        protected string GetAjaxUrl()
        {
            return VirtualPathUtility.ToAbsolute("~/Ajax/Invoice.ashx");
        }

        private void Page_Load(object sender, EventArgs e)
        {
            if (!Page.IsPostBack)
            {
                DateTime period = CacheManager.Current.StartPeriod();

                if (period != default(DateTime))
                    pp1.SelectedPeriod = period;

                MakeOrgGrid(true);

                if (!string.IsNullOrEmpty(Request.QueryString["DownloadAll"]))
                {
                    string downloadFile = Request.QueryString["DownloadAll"];
                    litDownloadAllWorkingMessage.Text = string.Format("<div style=\"margin-top: 10px; font-weight: bold;\"><a href=\"{0}\">Zip file created. Click here to download.</a>", VirtualPathUtility.ToAbsolute(GetVirtualPath(downloadFile, "zip")));
                }
            }
        }

        protected void Pp1_SelectedPeriodChanged(object sender, EventArgs e)
        {
            // StartPeriod is always pp1.SelectedPeriod
            // EndPeriod is always one month after StartPeriod

            // store the dates in the sessio
            CacheManager.Current.StartPeriod(StartPeriod);
            CacheManager.Current.EndPeriod(EndPeriod);

            MakeOrgGrid(true);
        }

        private void MakeOrgGrid(bool refresh)
        {
            panWarning.Visible = false;
            panSummary.Visible = true;
            panDetail.Visible = true;

            _mgr = new ExternalInvoiceManager(StartPeriod, EndPeriod, ShowRemote, BillingTypeManager);

            var invoices = _mgr.GetInvoices();

            if (invoices.Count() == 0)
            {
                panWarning.Visible = true;
                panSummary.Visible = false;
                panDetail.Visible = false;
            }
            else
            {
                rptUsageSummary.DataSource = _mgr.GetSummary();
                rptUsageSummary.DataBind();

                rptInvoice.DataSource = invoices;
                rptInvoice.DataBind();

                btnDownloadAll.Visible = CanDownloadAll();
            }
        }

        public IEnumerable<ExternalInvoiceSummaryItem> GetSummary()
        {
            _mgr = new ExternalInvoiceManager(StartPeriod, EndPeriod, ShowRemote, BillingTypeManager);
            return _mgr.GetSummary();
        }

        protected string FormatDecimal(decimal value)
        {
            if (value == 0)
                return string.Empty;
            else
                return value.ToString("C");
        }

        protected string GetExpireCssClass(DateTime? poEndDate)
        {
            if (!poEndDate.HasValue)
                return string.Empty;

            var daysTillExpire = poEndDate.Value.Subtract(DateTime.Now.Date).TotalDays;

            if (daysTillExpire <= 15)
            {
                return "expire-fifteen";
            }
            else if (daysTillExpire <= 45)
            {
                return "expire-fortyfive";
            }

            return string.Empty;
        }

        protected string GetViewUrl(int accountId, string reportType)
        {
            return string.Format(VirtualPathUtility.ToAbsolute("~/RepInvoiceView.aspx?AccountID={0}&StartPeriod={1:yyyy-MM-dd}&EndPeriod={2:yyyy-MM-dd}&ReportType={3}&ShowRemote={4}"), accountId, StartPeriod, EndPeriod, reportType, ShowRemote);
        }

        // nothing uses this, can it be deleted?
        protected void DgOrg_ItemDataBound(object sender, DataGridItemEventArgs e)
        {
            if (e.Item.ItemType == ListItemType.AlternatingItem || e.Item.ItemType == ListItemType.Item)
            {
                var h = (ExternalInvoiceHeader)e.Item.DataItem;

                Label lblOrg = (Label)e.Item.FindControl("lblOrg");
                Label lblAccount = (Label)e.Item.FindControl("lblAccount");

                lblOrg.Text = h.OrgName;
                lblAccount.Text = h.AccountName;

                if (h.PoEndDate.HasValue)
                {
                    Label lblExpire = (Label)e.Item.FindControl("lblExpire");
                    lblExpire.Text = string.Format("{0:d-MMM-yyyy}", h.PoEndDate.Value);

                    double daysTillExpire = h.PoEndDate.Value.Subtract(DateTime.Now.Date).TotalDays;

                    if (daysTillExpire <= 15)
                    {
                        lblExpire.Font.Bold = true;
                        lblExpire.ForeColor = System.Drawing.Color.Red;
                    }
                    else if (daysTillExpire <= 45)
                        lblExpire.ForeColor = System.Drawing.Color.Orange;

                    if (h.PoRemainingFunds > 0)
                    {
                        Label lblFunds = (Label)e.Item.FindControl("lblFunds");
                        lblFunds.Text = string.Format("{0:C}", h.PoRemainingFunds);
                    }
                }
            }
        }

        public static string GetVirtualPath(string name, string type)
        {
            string result = "~/Spreadsheets.ashx?name=" + HttpUtility.UrlEncode(name);
            if (!string.IsNullOrEmpty(type))
                result += "&type=" + HttpUtility.UrlEncode(type);
            return result;
        }

        protected void OrgInvoice_Command(object sender, CommandEventArgs e)
        {
            int accountId = Convert.ToInt32(e.CommandArgument);
            CreateExcelInvoice(accountId);
        }

        private void CreateExcelInvoice(int accountId)
        {
            var inv = _mgr.GetInvoices(accountId).First();

            string filePath;
            if (StartPeriod >= July2009)
            {
                string alert = string.Empty;
                filePath = ExcelUtility.GenerateInvoiceExcelReport(inv, CacheManager.Current.CurrentUser.ClientID, string.Empty, true, ref alert);
                ShowAlert(alert);
            }
            else
                filePath = ExcelUtility.MakeSpreadSheet(inv.Header.AccountID, inv.Header.InvoiceNumber, inv.Header.DeptRef, inv.Header.OrgName, CacheManager.Current.CurrentUser.ClientID, StartPeriod, EndPeriod);

            // display excel spreadsheet
            if (!string.IsNullOrEmpty(filePath))
            {
                //only include the file name
                Response.Redirect(GetVirtualPath(Path.GetFileName(filePath), null));
            }
        }

        private void ShowAlert(string message)
        {
            if (!string.IsNullOrEmpty(message))
            {
                Page.ClientScript.RegisterStartupScript(GetType(), $"alert-{alertCount}", $"alert('{message}');", true);
                alertCount += 1;
            }
        }

        protected void ChkShowRemote_CheckedChanged(object sender, EventArgs e)
        {
            MakeOrgGrid(false);
        }

        protected void BtnDownloadAll_Click(object sender, EventArgs e)
        {
            MakeOrgGrid(false);

            var items = (IEnumerable<ExternalInvoice>)rptInvoice.DataSource;
            bool del = true;

            if (items.Count() > 0)
            {
                var invoices = _mgr.GetInvoices();

                foreach (ExternalInvoice i in items)
                {
                    int accountId = i.Header.AccountID;

                    // get datarows for Org and Account
                    var inv = invoices.First(x => x.Header.AccountID == accountId);
                    string alert = string.Empty;
                    string fileName = ExcelUtility.GenerateInvoiceExcelReport(i, CacheManager.Current.CurrentUser.ClientID, "Zip", del, ref alert);
                    del = false;
                }

                Response.Redirect("~/RepInvoice.aspx?DownloadAll=" + "ExternalInvoices_" + StartPeriod.ToString("yyyy-MM"));
            }
        }
    }
}