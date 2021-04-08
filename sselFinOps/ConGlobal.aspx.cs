using LNF.CommonTools;
using LNF.Data;
using sselFinOps.AppCode;
using System;
using System.Data;

namespace sselFinOps
{
    public partial class ConGlobal : ReportPage
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!Page.IsPostBack)
            {
                var dtAccount = DataCommand()
                    .Param("Action", "ActiveNow")
                    .FillDataTable("dbo.Account_Select");

                var dtAdmin = DataCommand()
                    .Param("Action", "")
                    .Param("Privs", (int)ClientPrivilege.Administrator)
                    .FillDataTable("dbo.Client_Select");

                DataTable current = GetCurrent();

                if (current.Rows.Count > 0) //must have one row
                {
                    DataRow dr = current.Rows[0];

                    if (dr["BusinessDay"] != DBNull.Value)
                        txtBusinessDay.Text = dr["BusinessDay"].ToString();

                    if (dr["AccessToOld"] != DBNull.Value)
                        txtAccessToOld.Text = dr["AccessToOld"].ToString();

                    if (dr["LabAccountID"] == DBNull.Value)
                        ddlGenLabAcct.ClearSelection();
                    else
                        ddlGenLabAcct.SelectedValue = dr["LabAccountID"].ToString();

                    if (dr["LabCreditAccountID"] == DBNull.Value)
                        ddlGenLabCredit.ClearSelection();
                    else
                        ddlGenLabCredit.SelectedValue = dr["LabCreditAccountID"].ToString();

                    if (dr["SubsidyCreditAccountID"] == DBNull.Value)
                        ddlSubsidyCreditAcct.ClearSelection();
                    else
                        ddlSubsidyCreditAcct.SelectedValue = dr["SubsidyCreditAccountID"].ToString();

                    if (dr["AdminID"] == DBNull.Value)
                        ddlAdmin.ClearSelection();
                    else
                        ddlAdmin.SelectedValue = dr["AdminID"].ToString();
                }

                ddlGenLabAcct.DataSource = dtAccount;
                ddlGenLabAcct.DataBind();

                ddlGenLabCredit.DataSource = dtAccount;
                ddlGenLabCredit.DataBind();

                ddlSubsidyCreditAcct.DataSource = dtAccount;
                ddlSubsidyCreditAcct.DataBind();

                // get admins
                ddlAdmin.DataSource = dtAdmin;
                ddlAdmin.DataBind();
            }
        }

        protected void BtnSave_Click(object sender, EventArgs e)
        {
            DataTable current = GetCurrent();

            // Global costs are never updated, a new row is always written

            bool modified = false;

            int labAccountId = Convert.ToInt32(ddlGenLabAcct.SelectedValue);
            int labCreditAccountId = Convert.ToInt32(ddlGenLabCredit.SelectedValue);
            int subsidyCreditAccountId = Convert.ToInt32(ddlSubsidyCreditAcct.SelectedValue);
            int adminId = Convert.ToInt32(ddlAdmin.SelectedValue);
            int businessDay = Utility.ConvertTo(txtBusinessDay.Text, 4);
            int accessToOld = Utility.ConvertTo(txtAccessToOld.Text, 365);

            if (current.Rows.Count > 0)
            {
                DataRow dr = current.Rows[0];
                ModifiedCheck(dr, "LabAccountID", labAccountId, ref modified);
                ModifiedCheck(dr, "LabCreditAccountID", labCreditAccountId, ref modified);
                ModifiedCheck(dr, "SubsidyCreditAccountID", subsidyCreditAccountId, ref modified);
                ModifiedCheck(dr, "AdminID", adminId, ref modified);
                ModifiedCheck(dr, "BusinessDay", businessDay, ref modified);
                ModifiedCheck(dr, "AccessToOld", accessToOld, ref modified);
            }
            else
            {
                //no existing row so we need to insert
                modified = true;
            }

            if (modified)
            {
                DataCommand()
                    .Param("LabAccountID", Convert.ToInt32(ddlGenLabAcct.SelectedValue))
                    .Param("LabCreditAccountID", Convert.ToInt32(ddlGenLabCredit.SelectedValue))
                    .Param("SubsidyCreditAccountID", Convert.ToInt32(ddlSubsidyCreditAcct.SelectedValue))
                    .Param("AdminID", Convert.ToInt32(ddlAdmin.SelectedValue))
                    .Param("BusinessDay", Utility.ConvertTo(txtBusinessDay.Text, 4))
                    .Param("AccessToOld", Utility.ConvertTo(txtAccessToOld.Text, 365))
                    .ExecuteNonQuery("dbo.GlobalCost_Insert");
            }

            BackButton_Click(sender, e);
        }

        private DataTable GetCurrent()
        {
            return DataCommand().FillDataTable("dbo.GlobalCost_Select");
        }

        private void ModifiedCheck<T>(DataRow dr, string column, T value, ref bool modified)
        {
            if (!value.Equals(Utility.ConvertTo(dr[column], default(T))))
                modified = true;
        }
    }
}