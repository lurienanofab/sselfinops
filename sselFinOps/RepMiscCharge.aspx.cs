using LNF.Cache;
using LNF.Data;
using OnlineServices.Api.Billing;
using sselFinOps.AppCode;
using sselFinOps.AppCode.DAL;
using System;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Web.UI;
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
                txtActDate.Text = PeriodPicker1.SelectedPeriod.ToString("M/d/yyyy");
                LoadGrid();
            }
            else
                userSelectedValue = string.IsNullOrEmpty(ClientDropDown.SelectedValue) ? -1 : Convert.ToInt32(ClientDropDown.SelectedValue);
        }

        private void LoadGrid()
        {
            MiscChargeGridView.DataSource = MiscBillingChargeDA.GetDataByPeriod(PeriodPicker1.SelectedYear, PeriodPicker1.SelectedMonth);
            MiscChargeGridView.DataBind();
        }

        private void UpdateAccountDDL()
        {
            userSelectedValue = string.IsNullOrEmpty(ClientDropDown.SelectedValue) ? -1 : Convert.ToInt32(ClientDropDown.SelectedValue);
            if (userSelectedValue != -1)
            {
                var client = CacheManager.Current.GetClient(userSelectedValue);
                var accts = DataRepository.FindActiveAccountsInDateRange(client.ClientID, PeriodPicker1.SelectedPeriod, PeriodPicker1.SelectedPeriod.AddMonths(1)).ToList();
                var orderedAccounts = ClientPreferenceUtility.OrderAccountsByUserPreference(client.ClientID, accts);
                if (orderedAccounts != null)
                {
                    ddlAccount.DataSource = orderedAccounts.Select(x => new {AccountName = x.GetFullAccountName(), AccountID = x.AccountID.ToString()});
                    ddlAccount.DataBind();
                }
            }
        }

        protected void SaveButton_Click(object sender, EventArgs e)
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

            if (string.IsNullOrEmpty(ClientDropDown.SelectedValue))
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
            }

            if (!isValid) return;

            int clientId = Convert.ToInt32(ClientDropDown.SelectedValue);
            int accountId = Convert.ToInt32(ddlAccount.SelectedValue);

            MiscBillingChargeDA.SaveNewEntry(clientId, accountId, ddlSUBType.SelectedItem.Text, actDate, txtDesc.Text, qty, cost);

            //re-calculate the subsidy
            DateTime period = new DateTime(actDate.Year, actDate.Month, 1);

            RegisterAsyncTask(new PageAsyncTask(async () =>
            {
                await RecalculateSubsidy(period, clientId);
                LoadGrid();
            }));
        }

        protected void PeriodPicker1_SelectedPeriodChanged(object sender, EventArgs e)
        {
            txtActDate.Text = PeriodPicker1.SelectedPeriod.ToString("M/d/yyyy");
            UpdateAccountDDL();
            LoadGrid();
        }

        protected void ClientDropDown_DataBound(object sender, EventArgs e)
        {
            ClientDropDown.Items.Insert(0, new ListItem("", ""));
            if (userSelectedValue != -1)
                ClientDropDown.SelectedIndex = ClientDropDown.Items.IndexOf(ClientDropDown.Items.FindByValue(userSelectedValue.ToString()));
        }

        protected void RecalcSubsidyButton_Command(object sender, CommandEventArgs e)
        {
            if (DateTime.TryParse(txtActDate.Text, out DateTime actDate))
            { 
                int clientId = Convert.ToInt32(e.CommandArgument);
                DateTime period = new DateTime(actDate.Year, actDate.Month, 1);

                RegisterAsyncTask(new PageAsyncTask(async () =>
                {
                    await RecalculateSubsidy(period, clientId);
                    LoadGrid();
                }));
            }
        }

        private async Task RecalculateSubsidy(DateTime period, int clientId)
        {
            //[2015-11-12 jg] only subsidy step4 is needed now
            using (var billingClient = new BillingClient())
                await billingClient.BillingProcessStep4("subsidy", period, clientId);

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

        protected void ClientDropDown_SelectedIndexChanged(object sender, EventArgs e)
        {
            UpdateAccountDDL();
        }

        protected void MiscChargeGridView_RowDeleting(object sender, GridViewDeleteEventArgs e)
        {
            int expId = Convert.ToInt32(e.Keys[0]);

            var dt = MiscBillingChargeDA.GetByExpID(expId);

            if (dt.Rows.Count > 0)
            {
                var dr = dt.Rows[0];

                DateTime period = dr.Field<DateTime>("Period");
                int clientId = dr.Field<int>("ClientID");

                MiscBillingChargeDA.DeleteEntry(expId);

                RegisterAsyncTask(new PageAsyncTask(async () =>
                {
                    await RecalculateSubsidy(period, clientId);
                    LoadGrid();
                }));
            }
            else
                throw new Exception(string.Format("Cannot find record with ExpID = {0}", expId));
        }

        protected void MiscChargeGridView_RowEditing(object sender, GridViewEditEventArgs e)
        {
            MiscChargeGridView.EditIndex = e.NewEditIndex;
            LoadGrid();
        }

        protected void MiscChargeGridView_RowCancelingEdit(object sender, GridViewCancelEditEventArgs e)
        {
            MiscChargeGridView.EditIndex = -1;
            LoadGrid();
        }

        protected void MiscChargeGridView_RowUpdating(object sender, GridViewUpdateEventArgs e)
        {
            int expId = Convert.ToInt32(e.Keys[0]);

            DataTable dt = MiscBillingChargeDA.GetByExpID(expId);

            if (dt.Rows.Count > 0)
            {
                var txtActDate = (TextBox)MiscChargeGridView.Rows[e.RowIndex].FindControl("txtActDate");

                DateTime period = Convert.ToDateTime(txtActDate.Text);
                string description = Convert.ToString(e.NewValues["Description"]);
                double quantity = Convert.ToDouble(e.NewValues["Quantity"]);
                double unitCost = Convert.ToDouble(e.NewValues["UnitCost"]);

                int clientId = dt.Rows[0].Field<int>("ClientID");

                MiscBillingChargeDA.UpdateEntry(expId, period, description, quantity, unitCost);

                RegisterAsyncTask(new PageAsyncTask(async () =>
                {
                    await RecalculateSubsidy(period, clientId);
                    MiscChargeGridView.EditIndex = -1;
                    LoadGrid();
                }));
            }
            else
                throw new Exception(string.Format("Cannot find record with ExpID = {0}", expId));
        }
    }
}