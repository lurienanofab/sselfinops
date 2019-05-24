using LNF.Cache;
using LNF.Repository;
using LNF.Web;
using sselFinOps.AppCode;
using System;
using System.Data;
using System.Text.RegularExpressions;
using System.Web.UI.WebControls;

namespace sselFinOps
{
    public partial class AuxCostConfiguration : ReportPage
    {
        private DataSet dsAuxCost;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (Page.IsPostBack)
            {
                dsAuxCost = ContextBase.CacheData();
                if (dsAuxCost == null)
                    Response.Redirect("~");
                else if (dsAuxCost.DataSetName != "ConAuxCost")
                    Response.Redirect("~");
            }
            else
            {
                ContextBase.RemoveCacheData(); //remove anything left in cache

                dsAuxCost = new DataSet("ConAuxCost");
                DA.Command().Param("CostType", "All").MapSchema().FillDataSet(dsAuxCost, "dbo.AuxCost_Select", "AuxCost");

                ContextBase.CacheData(dsAuxCost);

                BindGrids();
            }
        }

        private void BindGrids()
        {
            DataView dvRoom = dsAuxCost.Tables["AuxCost"].DefaultView;
            dvRoom.Sort = "AuxCostParm";
            dvRoom.RowFilter = "AuxCostParm like 'Room*'";
            dgRoomAuxCost.DataSource = dvRoom;
            dgRoomAuxCost.DataBind();

            DataView dvTool = dsAuxCost.Tables["AuxCost"].DefaultView;
            dvTool.Sort = "AuxCostParm";
            dvTool.RowFilter = "AuxCostParm like 'Tool*'";
            dgToolAuxCost.DataSource = dvTool;
            dgToolAuxCost.DataBind();

            DataView dvStore = dsAuxCost.Tables["AuxCost"].DefaultView;
            dvStore.Sort = "AuxCostParm";
            dvStore.RowFilter = "AuxCostParm like 'Store*'";
            dgStoreAuxCost.DataSource = dvStore;
            dgStoreAuxCost.DataBind();
        }

        private bool ValidateEntry(string check, string type)
        {
            string pattern = type + "[A-Za-z][A-Za-z]*";
            Regex r = new Regex(pattern);
            Match m = r.Match(check);
            if (m.Success)
            {
                DataRow[] fdr = dsAuxCost.Tables["AuxCost"].Select(string.Format("AuxCostParm = '{0}'", check));
                if (fdr.Length == 0)
                    return true;
                else
                    return false;
            }
            else
                return false;
        }

        private void AddNewParm(DataGridItem dgi, string type)
        {
            DataRow dr = dsAuxCost.Tables["AuxCost"].NewRow();
            dr["AuxCostParm"] = ((TextBox)dgi.FindControl("txt" + type + "AuxCost")).Text.Trim();
            dr["AllowPerUse"] = ((CheckBox)dgi.FindControl("chk" + type + "PerUse")).Checked;
            dr["AllowPerPeriod"] = ((CheckBox)dgi.FindControl("chk" + type + "PerPeriod")).Checked;
            dr["Description"] = ((TextBox)dgi.FindControl("txt" + type + "Description")).Text.Trim();
            dsAuxCost.Tables["AuxCost"].Rows.Add(dr);

            ContextBase.CacheData(dsAuxCost);

            BindGrids();
        }

        protected void DgRoomAuxCost_ItemCommand(object source, DataGridCommandEventArgs e)
        {
            // must be the add button - there is nothing else
            if (ValidateEntry(((TextBox)e.Item.FindControl("txtRoomAuxCost")).Text.Trim(), "Room"))
                AddNewParm(e.Item, "Room");
            else
                ServerJScript.JSAlert(this, "AuxCost parameter invalid - it must begin with Room and must be unique");
        }

        protected void DgToolAuxCost_ItemCommand(object source, DataGridCommandEventArgs e)
        {
            // must be the add button - there is nothing else
            if (ValidateEntry(((TextBox)e.Item.FindControl("txtToolAuxCost")).Text.Trim(), "Tool"))
                AddNewParm(e.Item, "Tool");
            else
                ServerJScript.JSAlert(this, "AuxCost parameter invalid - it must begin with Tool and must be unique");
        }

        protected void DgStoreAuxCost_ItemCommand(object source, DataGridCommandEventArgs e)
        {
            // must be the add button - there is nothing else
            if (ValidateEntry(((TextBox)e.Item.FindControl("txtStoreAuxCost")).Text.Trim(), "Store"))
                AddNewParm(e.Item, "Store");
            else
                ServerJScript.JSAlert(this, "AuxCost parameter invalid - it must begin with Store and must be unique");
        }

        protected void DgRoomAuxCost_ItemDataBound(object sender, DataGridItemEventArgs e)
        {
            DataRowView drv = (DataRowView)e.Item.DataItem;
            if (e.Item.ItemType == ListItemType.AlternatingItem || e.Item.ItemType == ListItemType.Item)
                BindRow(e.Item, drv, "Room");
        }

        protected void DgToolAuxCost_ItemDataBound(object sender, DataGridItemEventArgs e)
        {
            DataRowView drv = (DataRowView)e.Item.DataItem;
            if (e.Item.ItemType == ListItemType.AlternatingItem || e.Item.ItemType == ListItemType.Item)
                BindRow(e.Item, drv, "Tool");
        }

        protected void DgStoreAuxCost_ItemDataBound(object sender, DataGridItemEventArgs e)
        {
            DataRowView drv = (DataRowView)e.Item.DataItem;
            if (e.Item.ItemType == ListItemType.AlternatingItem || e.Item.ItemType == ListItemType.Item)
                BindRow(e.Item, drv, "Store");
        }

        private void BindRow(DataGridItem dgi, DataRowView drv, string type)
        {
            ((Label)dgi.FindControl("lbl" + type + "AuxCost")).Text = drv["AuxCostParm"].ToString();
            ((Label)dgi.FindControl("lbl" + type + "PerUse")).Text = Convert.ToBoolean(drv["AllowPerUse"]) ? "Yes" : "No";
            ((Label)dgi.FindControl("lbl" + type + "PerPeriod")).Text = Convert.ToBoolean(drv["AllowPerPeriod"]) ? "Yes" : "No";
            ((Label)dgi.FindControl("lbl" + type + "Description")).Text = drv["Description"].ToString();
        }

        protected void BtnSave_Click(object sender, EventArgs e)
        {
            DA.Command().Update(dsAuxCost.Tables["AuxCost"], cfg =>
            {
                cfg.Insert.AddParameter("AuxCostParam", SqlDbType.NVarChar, 25);
                cfg.Insert.AddParameter("AllowPerUse", SqlDbType.Bit);
                cfg.Insert.AddParameter("AllowPerPeriod", SqlDbType.Bit);
                cfg.Insert.AddParameter("Desc", SqlDbType.NVarChar, 150);
                cfg.Insert.SetCommandText("dbo.AuxCost_Insert");
            });

            Back();
        }
    }
}