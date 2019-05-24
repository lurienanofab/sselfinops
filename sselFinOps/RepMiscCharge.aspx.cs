using LNF;
using LNF.Cache;
using LNF.CommonTools;
using LNF.Data;
using LNF.Models.Billing;
using LNF.Models.Billing.Process;
using sselFinOps.AppCode;
using sselFinOps.AppCode.BLL;
using sselFinOps.AppCode.DAL;
using System;
using System.Data;
using System.Linq;
using System.Web.UI.WebControls;

namespace sselFinOps
{
    public partial class RepMiscCharge : ReportPage
    {
        private int userSelectedValue = -1;

        protected void Page_Load(object sender, EventArgs e)
        {
            lblUserValidation.Visible = false;
            lblAccountValidation.Visible = false;

            if (!Page.IsPostBack)
            {
                var command = Request.QueryString["Command"];

                switch (command)
                {
                    case "RecalcSubsidy":
                        if (string.IsNullOrEmpty(Request.QueryString["ClientID"]))
                            throw new Exception("Missing required QueryString parameter: ClientID");

                        if (!int.TryParse(Request.QueryString["ClientID"], out var clientId))
                            throw new Exception("Invalid QueryString value: ClientID");

                        if (string.IsNullOrEmpty(Request.QueryString["Period"]))
                            throw new Exception("Missing required QueryString parameter: Period");

                        if (!DateTime.TryParse(Request.QueryString["Period"], out var period))
                            throw new Exception("Invalid QueryString value: Period");

                        RecalculateSubsidy(period, clientId);
                        break;
                }

                LoadClients();

                int updateAccounts = 0;

                if (Session["MiscCharge_Period"] != null)
                {
                    ++updateAccounts;
                    PeriodPicker1.SelectedPeriod = Convert.ToDateTime(Session["MiscCharge_Period"]);
                    txtActDate.Text = PeriodPicker1.SelectedPeriod.ToString("M/d/yyyy");
                    Session.Remove("MiscCharge_Period");
                }

                if (Session["MiscCharge_ClientID"] != null)
                {
                    ++updateAccounts;
                    ddlClient.SelectedValue = Convert.ToInt32(Session["MiscCharge_ClientID"]).ToString();
                    Session.Remove("MiscCharge_ClientID");
                }

                if (updateAccounts == 2)
                {
                    LoadAccounts();
                }

                if (Session["MiscCharge_AccountID"] != null)
                {
                    var item = ddlAccount.Items.FindByValue(Convert.ToInt32(Session["MiscCharge_AccountID"]).ToString());
                    if (item != null) ddlAccount.SelectedValue = item.Value;
                    Session.Remove("MiscCharge_AccountID");
                }

                if (Session["MiscCharge_UsageType"] != null)
                {
                    ddlSUBType.SelectedValue = Convert.ToString(Session["MiscCharge_UsageType"]);
                    Session.Remove("MiscCharge_UsageType");
                }

                if (Session["MiscCharge_Quantity"] != null)
                {
                    txtQuantity.Text = Convert.ToDouble(Session["MiscCharge_Quantity"]).ToString("0.00");
                    Session.Remove("MiscCharge_Quantity");
                }

                if (Session["MiscCharge_UnitCost"] != null)
                {
                    txtCost.Text = Convert.ToDouble(Session["MiscCharge_UnitCost"]).ToString("0.00");
                    Session.Remove("MiscCharge_UnitCost");
                }

                if (Session["MiscCharge_Description"] != null)
                {
                    txtDesc.Text = Convert.ToString(Session["MiscCharge_Description"]);
                    Session.Remove("MiscCharge_Description");
                }

                txtActDate.Text = PeriodPicker1.SelectedPeriod.ToString("M/d/yyyy");

                LoadGrid();
            }
            else
            {
                userSelectedValue = string.IsNullOrEmpty(ddlClient.SelectedValue) ? -1 : Convert.ToInt32(ddlClient.SelectedValue);
            }
        }

        private void LoadClients()
        {
            var dt = ClientBL.GetAllClientByDate(PeriodPicker1.SelectedYear, PeriodPicker1.SelectedMonth);

            ddlClient.DataSource = dt;
            ddlClient.DataBind();

            var item = ddlClient.Items.FindByValue(userSelectedValue.ToString());

            if (item != null)
                item.Selected = true;
            else
            { 
                userSelectedValue = -1;
                ddlClient.ClearSelection();
            }
        }

        private void LoadAccounts()
        {
            userSelectedValue = string.IsNullOrEmpty(ddlClient.SelectedValue) ? -1 : Convert.ToInt32(ddlClient.SelectedValue);

            if (userSelectedValue != -1)
            {
                var client = CacheManager.Current.GetClient(userSelectedValue);
                var accts = AccountManager.GetActiveAccounts(client.ClientID, PeriodPicker1.SelectedPeriod, PeriodPicker1.SelectedPeriod.AddMonths(1)).ToList();
                var util = new ClientPreferenceUtility(Provider);
                var orderedAccounts = util.OrderAccountsByUserPreference(client, accts);
                if (orderedAccounts != null)
                {
                    ddlAccount.DataSource = orderedAccounts.Select(x => new { AccountName = x.FullAccountName, AccountID = x.AccountID.ToString() });
                    ddlAccount.DataBind();
                }
            }
            else
            {
                ddlAccount.Items.Clear();
            }
        }

        private void LoadGrid()
        {
            hidPeriod.Value = PeriodPicker1.SelectedPeriod.ToString("yyyy-MM-dd");
            gvMiscCharge.DataSource = ServiceProvider.Current.Billing.Misc.GetMiscBillingCharges(PeriodPicker1.SelectedPeriod);
            gvMiscCharge.DataBind();
        }

        protected void BtnSave_Click(object sender, EventArgs e)
        {
            bool isValid = true;

            lblUserValidation.Visible = false;
            lblUserValidation.Text = string.Empty;

            lblAccountValidation.Visible = false;
            lblAccountValidation.Text = string.Empty;

            lblQuantityValidation.Visible = false;
            lblQuantityValidation.Text = string.Empty;

            lblCostValidation.Visible = false;
            lblCostValidation.Text = string.Empty;

            lblDescriptionValidation.Visible = false;
            lblDescriptionValidation.Text = string.Empty;

            if (string.IsNullOrEmpty(ddlClient.SelectedValue))
            {
                isValid = false;
                lblUserValidation.Visible = true;
                lblUserValidation.Text = "* Required";
            }

            if (string.IsNullOrEmpty(ddlAccount.SelectedValue))
            {
                isValid = false;
                lblAccountValidation.Visible = true;
                lblAccountValidation.Text = "* Required";
            }

            if (string.IsNullOrEmpty(txtQuantity.Text))
            {
                isValid = false;
                lblQuantityValidation.Visible = true;
                lblQuantityValidation.Text = "* Required";
            }

            if (string.IsNullOrEmpty(txtCost.Text))
            {
                isValid = false;
                lblCostValidation.Visible = true;
                lblCostValidation.Text = "* Required";
            }

            if (string.IsNullOrEmpty(txtDesc.Text))
            {
                isValid = false;
                lblDescriptionValidation.Visible = true;
                lblDescriptionValidation.Text = "* Required";
            }

            if (!double.TryParse(txtQuantity.Text.Trim(), out double qty))
            {
                isValid = false;
                lblQuantityValidation.Visible = true;
                lblQuantityValidation.Text = "* Must be numeric";
            }

            if (!double.TryParse(txtCost.Text.Trim(), out double cost))
            {
                isValid = false;
                lblCostValidation.Visible = true;
                lblCostValidation.Text = "* Must be numeric";
            }

            if (!DateTime.TryParse(txtActDate.Text, out DateTime actDate))
            {
                isValid = false;
                litDebug.Text = "'Apply period' is required.";
            }

            if (!isValid) return;

            DateTime period = actDate.FirstOfMonth();
            int clientId = Convert.ToInt32(ddlClient.SelectedValue);
            int accountId = Convert.ToInt32(ddlAccount.SelectedValue);

            // Save Misc Charge
            var expId = ServiceProvider.Current.Billing.Misc.CreateMiscBillingCharge(new MiscBillingChargeCreateArgs
            {
                ClientID = clientId,
                AccountID = accountId,
                SUBType = ddlSUBType.SelectedItem.Text,
                Period = period,
                ActDate = actDate,
                Description = txtDesc.Text,
                Quantity = qty,
                UnitCost = Convert.ToDecimal(cost)
            });

            if (expId == 0)
                SetDebugError(period, clientId, "Failed to create new MiscBillingCharge.");
            else
                RecalculateSubsidy(period, clientId);

            LoadGrid();
        }

        protected void PeriodPicker1_SelectedPeriodChanged(object sender, EventArgs e)
        {
            txtActDate.Text = PeriodPicker1.SelectedPeriod.ToString("M/d/yyyy");
            LoadClients();
            LoadAccounts();
            LoadGrid();
        }

        protected void DdlClient_DataBound(object sender, EventArgs e)
        {
            ddlClient.Items.Insert(0, new ListItem("", ""));
            if (userSelectedValue != -1)
                ddlClient.SelectedIndex = ddlClient.Items.IndexOf(ddlClient.Items.FindByValue(userSelectedValue.ToString()));
        }

        protected string GetRecalcSubsidyUrl(object item)
        {
            var i = (MiscBillingChargeItem)item;

            if (DateTime.TryParse(txtActDate.Text, out DateTime actDate))
                return GetRecalcSubsidyUrl(actDate, i.ClientID);
            else
                return GetRecalcSubsidyUrl(PeriodPicker1.SelectedPeriod, i.ClientID);
        }

        private string GetRecalcSubsidyUrl(DateTime period, int clientId)
        {
            return $"~/RepMiscCharge.aspx?Command=RecalcSubsidy&Period={period:yyyy-MM-dd}&ClientID={clientId}";
        }

        private void RecalculateSubsidy(DateTime period, int clientId)
        {
            //[2015-11-12 jg] only subsidy step4 is needed now
            try
            {
                ServiceProvider.Current.Billing.Process.BillingProcessStep4(new BillingProcessStep4Command
                {
                    Command = "subsidy",
                    Period = period,
                    ClientID = clientId
                });
            }
            catch (Exception ex)
            {
                SetDebugError(period, clientId, ex.ToString());
            }

            ////must call BillingDataProcessStep2 because data from here is used in Step4 (only ByAccount tables are needed)
            //BillingDataProcessStep2.PopulateRoomBillingByAccount(period, clientId);
            //BillingDataProcessStep2.PopulateToolBillingByAccount(period, clientId);

            ////now call Step4
            //BillingDataProcessStep4Subsidy.PopulateSubsidyBilling(period, clientId);

            ////must call BillingDataProcessStep2 again to fill the subsidy columns based on Step4 results
            //BillingDataProcessStep2.PopulateRoomBillingByRoomOrg(period, clientId);
            //BillingDataProcessStep2.PopulateRoomBillingByAccount(period, clientId);
            //BillingDataProcessStep2.PopulateToolBillingByToolOrg(period, clientId);
            //BillingDataProcessStep2.PopulateToolBillingByAccount(period, clientId);
            //BillingDataProcessStep3.PopulateRoomBillingByOrg(period, clientId);
            //BillingDataProcessStep3.PopulateToolBillingByOrg(period, clientId);
        }

        private void SetDebugError(string errmsg)
        {
            if (string.IsNullOrEmpty(errmsg))
                litDebug.Text = string.Empty;
            else
                litDebug.Text = $"<pre class=\"debug\">{errmsg}</pre>";
        }

        private void SetDebugError(DateTime period, int clientId, string errmsg)
        {
            SetDebugError($"period: {period:yyyy-MM-dd HH:mm:ss}, clientId: {clientId}{Environment.NewLine}--------------------{Environment.NewLine}{errmsg}");
        }

        protected void DdlClient_SelectedIndexChanged(object sender, EventArgs e)
        {
            LoadAccounts();
        }

        protected void GvMiscCharge_RowDeleting(object sender, GridViewDeleteEventArgs e)
        {
            int expId = Convert.ToInt32(e.Keys[0]);

            var mbc = ServiceProvider.Current.Billing.Misc.GetMiscBillingCharge(expId);

            int deleted = ServiceProvider.Current.Billing.Misc.DeleteMiscBillingCharge(expId);

            if (deleted == 0)
                SetDebugError($"Cannot find record with ExpID = {expId}");
            else
                RecalculateSubsidy(mbc.Period, mbc.ClientID);

            LoadGrid();
        }

        protected void GvMiscCharge_RowEditing(object sender, GridViewEditEventArgs e)
        {
            gvMiscCharge.EditIndex = e.NewEditIndex;
            LoadGrid();
        }

        protected void GvMiscCharge_RowCancelingEdit(object sender, GridViewCancelEditEventArgs e)
        {
            gvMiscCharge.EditIndex = -1;
            LoadGrid();
        }

        protected void GvMiscCharge_RowUpdating(object sender, GridViewUpdateEventArgs e)
        {
            int expId = Convert.ToInt32(e.Keys[0]);

            var mbc = ServiceProvider.Current.Billing.Misc.GetMiscBillingCharge(expId);

            var txtActDate = (TextBox)gvMiscCharge.Rows[e.RowIndex].FindControl("txtActDate");

            var actDate = Convert.ToDateTime(txtActDate.Text);
            var period = actDate.FirstOfMonth();

            var args = new MiscBillingChargeUpdateArgs
            {
                ExpID = expId,
                Period = period,
                Description = Convert.ToString(e.NewValues["Description"]),
                Quantity = Convert.ToDouble(e.NewValues["Quantity"]),
                UnitCost = Convert.ToDecimal(e.NewValues["UnitCost"])
            };

            var updated = ServiceProvider.Current.Billing.Misc.UpdateMiscBilling(args);

            if (updated == 0)
                SetDebugError($"Cannot find record with ExpID = {expId}");
            else
            {
                RecalculateSubsidy(period, mbc.ClientID);

                if (period != mbc.Period)
                    RecalculateSubsidy(mbc.Period, mbc.ClientID);
            }

            gvMiscCharge.EditIndex = -1;

            LoadGrid();
        }

        protected void BtnRecalcSubsidy_Command(object sender, CommandEventArgs e)
        {
            if (e.CommandName == "reclac")
            {
                var period = DateTime.Parse(hidPeriod.Value);
                var clientId = Convert.ToInt32(e.CommandArgument);
                RecalculateSubsidy(period, clientId);
                LoadGrid();
            }
        }
    }
}