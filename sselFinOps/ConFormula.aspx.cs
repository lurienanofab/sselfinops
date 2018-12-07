using LNF.Billing;
using LNF.Cache;
using LNF.CommonTools;
using LNF.Repository;
using sselFinOps.AppCode;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Web.UI.WebControls;

namespace sselFinOps
{
    public partial class ConFormula : ReportPage
    {
        private DataSet dsFormula;
        string tableNamePrefix;

        protected void Page_Load(object sender, EventArgs e)
        {
            lblError.Visible = false;

            txtFormula.Attributes.Add("spellcheck", "false");

            if (Page.IsPostBack)
            {
                dsFormula = CacheManager.Current.CacheData();
                if (dsFormula == null)
                    Response.Redirect("~");
                else if (dsFormula.DataSetName != "ConFormula")
                    Response.Redirect("~");
            }
            else
            {
                CacheManager.Current.RemoveCacheData(); //remove anything left in cache

                CacheManager.Current.ItemType(Request.QueryString["ItemType"]);
                CacheManager.Current.Exp(Request.QueryString["Exp"]);
                string exp = CacheManager.Current.Exp();
                if (!string.IsNullOrEmpty(exp))
                {
                    tableNamePrefix = exp;
                    btnBack.Text = "Return to Experimental Cost Config";
                }

                litHeader.Text = string.Format("Configure {0} costing fomulas", tableNamePrefix);
                dsFormula = new DataSet("ConFormula");
                DA.Command().Param("sDate", DateTime.Now).MapSchema().FillDataSet(dsFormula, $"dbo.{tableNamePrefix}CostFormula_Select", "Formula");

                CacheManager.Current.CacheData(dsFormula);
            }
        }

        protected void RblFormulaType_SelectedIndexChanged(object sender, EventArgs e)
        {
            dgSample.DataSource = null;
            dgSample.DataBind();

            // create instance of compile class
            Compile mCompile = new Compile();

            // show allowable parameters
            int selType = Convert.ToInt32(rblFormulaType.SelectedValue);
            StringBuilder sbInfo = new StringBuilder();
            sbInfo.Append("The last line of the function must be of the form - CalcCost = ...");
            sbInfo.Append("<br />");
            sbInfo.Append("Parameters available for store cost calculations:");
            sbInfo.Append("<br />");
            for (int i = 0; i < mCompile.aryVars[selType].Length; i++)
            {
                sbInfo.Append(mCompile.aryVars[selType][i]);
                sbInfo.Append(" - ");
                sbInfo.Append(mCompile.aryNotes[selType][i]);
                sbInfo.Append("<br />");
            }

            lblInfo.Text = sbInfo.ToString();
            lblInfo.Visible = true;
            btnValidate.Enabled = true;

            DataRow[] fdr = dsFormula.Tables["Formula"].Select(string.Format("FormulaType = '{0}'", rblFormulaType.SelectedItem.Text), "EffDate DESC");
            if (fdr.Length > 0)
                txtFormula.Text = fdr[fdr.Length - 1]["Formula"].ToString();
            else
                txtFormula.Text = "CalcCost = ";

            txtFormula.Visible = true;

            if (fdr.Length == 2)
                btnRevert.Enabled = true;
            else
                btnRevert.Enabled = false;
        }

        protected void BtnRevert_Click(object sender, EventArgs e)
        {
            DataRow[] fdr = dsFormula.Tables["Formula"].Select(string.Format("FormulaType = '{0}'", rblFormulaType.SelectedItem.Text), "EffDate ASC");
            if (fdr.Length > 1) // must be true, but checking is a good thing
                dsFormula.Tables["Formula"].Rows.Remove(fdr[1]);
            btnRevert.Enabled = false;

            txtFormula.Text = fdr[0]["Formula"].ToString();
        }

        protected void BtnValidate_Click(object sender, EventArgs e)
        {
            tableNamePrefix = CacheManager.Current.Exp();

            // create instance of compile class
            Compile mCompile = new Compile();

            int i;
            string strType = rblFormulaType.SelectedItem.Text;
            int selType = Convert.ToInt32(rblFormulaType.SelectedValue);

            // now try formula
            try
            {
                DateTime Period = new DateTime(DateTime.Now.Date.AddMonths(-1).Year, DateTime.Now.Date.AddMonths(-1).Month, 1);
                DataTable dtData = mCompile.CalcCost(strType, txtFormula.Text.Trim(), "", 0, Period, 0, 0, Compile.AggType.None, true, tableNamePrefix);

                if (dtData.Rows.Count > 0)
                {
                    // add calcCost as a array element to facilitate display
                    IList<string> temp;

                    temp = mCompile.aryVars[selType].ToList();
                    temp.Add("CalcCost");
                    mCompile.aryVars[selType] = temp.ToArray();

                    temp = mCompile.aryVarFormats[selType].ToList();
                    temp.Add("{0:$#,##0.00}");
                    mCompile.aryVarFormats[selType] = temp.ToArray();

                    BoundColumn bc;
                    for (i = 0; i < mCompile.aryVars[selType].Length; i++)
                    {
                        bc = new BoundColumn { DataField = mCompile.aryVars[selType][i] };
                        bc.ItemStyle.HorizontalAlign = HorizontalAlign.Right;
                        bc.HeaderText = mCompile.aryVars[selType][i];
                        bc.HeaderStyle.Font.Bold = true;
                        bc.HeaderStyle.HorizontalAlign = HorizontalAlign.Right;
                        if (mCompile.aryVarFormats[selType][i].Length > 0)
                            bc.DataFormatString = mCompile.aryVarFormats[selType][i];
                        dgSample.Columns.Add(bc);
                    }

                    dgSample.DataSource = dtData;
                    dgSample.DataBind();
                }
            }
            catch (Exception ex)
            {
                lblError.Text = ex.Message;
                lblError.Visible = true;
                return;
            }

            // if no errors, store new formula.
            // if a new formula has been stored, over-write it
            DataRow[] fdr = dsFormula.Tables["Formula"].Select(string.Format("FormulaType = '{0}'", rblFormulaType.SelectedItem.Text), "EffDate ASC");
            if (fdr.Length > 1)
                dsFormula.Tables["Formula"].Rows.Remove(fdr[1]);

            DataRow ndr = dsFormula.Tables["Formula"].NewRow();
            ndr["FormulaType"] = rblFormulaType.SelectedItem.Text;
            ndr["Formula"] = txtFormula.Text;
            ndr["EffDate"] = DateTime.Now;
            dsFormula.Tables["Formula"].Rows.Add(ndr);

            CacheManager.Current.CacheData(dsFormula);
            btnRevert.Enabled = true;
        }

        protected void BtnSave_Click(object sender, EventArgs e)
        {
            // only store formulas once they have been validated.
            // formulas are stored as strings - no compiler stuff going on here
            // update the rebates for each category
            CacheManager.Current.ItemType(Request.QueryString["ItemType"]);
            tableNamePrefix = CacheManager.Current.Exp();

            DA.Command().Update(dsFormula.Tables["Formula"], cfg =>
            {
                cfg.Insert.AddParameter("FormulaType", SqlDbType.NVarChar, 25);
                cfg.Insert.AddParameter("Formula", SqlDbType.NVarChar, 4000);
                cfg.Insert.SetCommandText($"dbo.{tableNamePrefix}CostFormula_Insert");
            });

            BackButton_Click(sender, e);
        }

        protected override void Back()
        {
            //CacheManager.Current.Updated(false);
            CacheManager.Current.RemoveCacheData(); //remove anything left in cache
            if (string.IsNullOrEmpty(CacheManager.Current.Exp()))
                Response.Redirect("~");
            else
                Response.Redirect("~/MscExp.aspx");
        }
    }
}