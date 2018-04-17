using LNF.CommonTools;
using LNF.Models.Data;
using LNF.Repository;
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
                DataTable dtAccount = null;
                DataTable dtAdmin = new DataTable();
                using (SQLDBAccess dba = new SQLDBAccess("cnSselData"))
                {
                    // get account
                    dtAccount = dba.ApplyParameters(new { Action = "ActiveNow" }).FillDataTable("Account_Select");
                    dtAdmin = dba.ApplyParameters(new { Action = "All", Privs = (int)ClientPrivilege.Administrator }).FillDataTable("Client_Select");
                }
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

        protected void btnSave_Click(object sender, EventArgs e)
        {
            DataTable current = GetCurrent();

            using (SQLDBAccess dba = new SQLDBAccess("cnSselData"))
            {
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
                    dba.SelectCommand
                        .AddParameter("@LabAccountID", Convert.ToInt32(ddlGenLabAcct.SelectedValue))
                        .AddParameter("@LabCreditAccountID", Convert.ToInt32(ddlGenLabCredit.SelectedValue))
                        .AddParameter("@SubsidyCreditAccountID", Convert.ToInt32(ddlSubsidyCreditAcct.SelectedValue))
                        .AddParameter("@AdminID", Convert.ToInt32(ddlAdmin.SelectedValue))
                        .AddParameter("@BusinessDay", Utility.ConvertTo(txtBusinessDay.Text, 4))
                        .AddParameter("@AccessToOld", Utility.ConvertTo(txtAccessToOld.Text, 365))
                        .ExecuteNonQuery("GlobalCost_Insert");
                }

                BackButton_Click(sender, e);
            }
        }

        private DataTable GetCurrent()
        {
            using (SQLDBAccess dba = new SQLDBAccess("cnSselData"))
                return dba.FillDataTable("GlobalCost_Select");
        }

        private void ModifiedCheck<T>(DataRow dr, string column, T value, ref bool modified)
        {
            if (!value.Equals(Utility.ConvertTo(dr[column], default(T))))
                modified = true;
        }
    }
}