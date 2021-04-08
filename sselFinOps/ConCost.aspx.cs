using LNF.Data;
using sselFinOps.AppCode;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.OleDb;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.UI.WebControls;

namespace sselFinOps
{
    public partial class ConCost : ReportPage
    {
        private IEnumerable<int> _ChargeTypeIds;
        private DataSet dsCost;
        private string tableNamePrefix;
        private Color modifiedColor = Color.FromArgb(255, 150, 150);

        public override ClientPrivilege AuthTypes
        {
            get { return ClientPrivilege.Executive | ClientPrivilege.Administrator; }
        }

        public string ItemType
        {
            get
            {
                string val = "Tool"; //default

                if (!string.IsNullOrEmpty(Request.QueryString["ItemType"]))
                    val = Request.QueryString["ItemType"];

                return val;
            }
        }

        public IEnumerable<int> ChargeTypeIDs
        {
            get
            {
                if (_ChargeTypeIds == null)
                    _ChargeTypeIds = dsCost.Tables["ChargeType"].AsEnumerable().Select(x => x.Field<int>("ChargeTypeID"));
                return _ChargeTypeIds;
            }
        }

        protected void Page_Init(object sender, EventArgs e)
        {
            //All this must be done in Page_Init because we must have data and all controls must be created before PostBack data is loaded.
            //By the time we get to Page_Load the PostBack data (i.e. user entered values) have already been loaded. If the controls didn't
            //exist at that time they won't have the user entered values. When the controls are created (in Sub AddCostRow) they are only
            //assigned a value if IsPostBack = False and .NET takes care of the assignment when IsPostBack = True.

            //Fill dsCost with data
            RetrieveAllCostData();

            //Create the table header
            BuildCostTableHeader("AuxCost");
            BuildCostTableHeader("Cost");

            //Add all the rows (create dynamic controls)
            BuildCostTables();
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            hidModifiedColor.Value = "#" + modifiedColor.R.ToString("x2") + modifiedColor.G.ToString("x2") + modifiedColor.B.ToString("x2");
        }

        private void RetrieveAllCostData()
        {
            dsCost = new DataSet("ConCost");

            bool experimental = !string.IsNullOrEmpty(Request.QueryString["Exp"]);
            if (experimental)
            {
                Session["Exp"] = Request.QueryString["Exp"];
                tableNamePrefix = Convert.ToString(Session["Exp"]);
                btnBack1.Text = "Return to Experimental Cost Config";
                btnBack2.Text = "Return to Experimental Cost Config";
            }

            switch (ItemType)
            {
                case "Tool":
                    litHeader.Text = string.Format("Configure {0} costs for: ", tableNamePrefix)
                        + string.Format("Tool | <a href=\"{0}Room{1}\">Room</a> | <a href=\"{0}Store{1}\">Store</a>", Request.Url.GetLeftPart(UriPartial.Path) + "?ItemType=", (experimental ? "&Exp=Exp" : string.Empty));
                    break;
                case "Room":
                    litHeader.Text = string.Format("Configure {0} costs for: ", tableNamePrefix)
                        + string.Format("<a href=\"{0}Tool{1}\">Tool</a> | Room | <a href=\"{0}Store{1}\">Store</a>", Request.Url.GetLeftPart(UriPartial.Path) + "?ItemType=", (experimental ? "&Exp=Exp" : string.Empty));
                    break;
                case "Store":
                    litHeader.Text = string.Format("Configure {0} costs for: ", tableNamePrefix)
                        + string.Format("<a href=\"{0}Tool{1}\">Tool</a> | <a href=\"{0}Room{1}\">Room</a> | Store", Request.Url.GetLeftPart(UriPartial.Path) + "?ItemType=", (experimental ? "&Exp=Exp" : string.Empty));
                    break;
            }

            litAuxCostHdr.Text = $"{ItemType} Auxilliary Costs";

            if (ItemType == "Store")
                litCostHdr.Text = "Rebates";
            else
                litCostHdr.Text = $"{ItemType} Costs";

            // gets list of appropriate items
            DataCommand()
                .Param("ItemType", ItemType)
                .Param("IsActive", 1)
                .FillDataSet(dsCost, "dbo.Item_Select", ItemType);

            // charge type is needed
            DataCommand()
                .Param("Action", "All")
                .FillDataSet(dsCost, "dbo.ChargeType_Select", "ChargeType");

            // get multipliers
            DataCommand()
                .Param("TableNameOrDescript", $"{ItemType}[a-z]%Cost")
                .Param("ChargeDate", DateTime.Now)
                .FillDataSet(dsCost, $"dbo.{tableNamePrefix}Cost_Select", "AuxCost");

            dsCost.Tables["AuxCost"].PrimaryKey = new[] { dsCost.Tables["AuxCost"].Columns["CostID"] };
            dsCost.Tables["AuxCost"].PrimaryKey[0].AutoIncrement = true;
            dsCost.Tables["AuxCost"].PrimaryKey[0].AutoIncrementSeed = -1;
            dsCost.Tables["AuxCost"].PrimaryKey[0].AutoIncrementStep = -1;

            DataCommand()
                .Param("TableNameOrDescript", $"{ItemType}Cost")
                .Param("ChargeDate", DateTime.Now)
                .FillDataSet(dsCost, $"dbo.{tableNamePrefix}Cost_Select", "Cost");

            dsCost.Tables["Cost"].PrimaryKey = new[] { dsCost.Tables["Cost"].Columns["CostID"] };
            dsCost.Tables["Cost"].PrimaryKey[0].AutoIncrement = true;
            dsCost.Tables["Cost"].PrimaryKey[0].AutoIncrementSeed = -1;
            dsCost.Tables["Cost"].PrimaryKey[0].AutoIncrementStep = -1;
        }

        private void BuildCostTableHeader(string costType)
        {
            DataTable dtChargeTypes = dsCost.Tables["ChargeType"];
            TableRow tr;
            TableHeaderCell th;
            Table tbl;
            int colspan;
            string nameColumnHeaderText;
            string nameColumnWidth;
            string perPeriodColumnHeaderText;

            switch (costType)
            {
                case "Cost":
                    tbl = tblCost;
                    colspan = 3;
                    nameColumnHeaderText = NameColumnHeaderText();
                    nameColumnWidth = "300px";
                    perPeriodColumnHeaderText = "Per Period Charge";
                    break;
                case "AuxCost":
                    tbl = tblAuxCost;
                    colspan = 2;
                    nameColumnHeaderText = "Cost Item";
                    nameColumnWidth = "180px";
                    perPeriodColumnHeaderText = "Per Period Multiplier";
                    break;
                default:
                    throw new Exception("Invalid costType, must be Cost or AuxCost.");
            }

            // first header row - ChargeType names
            tr = new TableRow();
            th = new TableHeaderCell
            {
                VerticalAlign = VerticalAlign.Bottom,
                CssClass = "right-border-thick",
                Text = "&nbsp;"
            };
            th.Style.Add("width", nameColumnWidth);
            tr.Cells.Add(th);

            foreach (DataRow dr in dtChargeTypes.Rows)
            {
                tr.Cells.Add(new TableHeaderCell
                {
                    VerticalAlign = VerticalAlign.Bottom,
                    CssClass = "right-border-thick",
                    ColumnSpan = colspan,
                    Text = dr.Field<string>("ChargeType")
                });
            }

            if (costType == "Cost")
            {
                tr.Cells.Add(new TableHeaderCell
                {
                    CssClass = "right-border-thick",
                    VerticalAlign = VerticalAlign.Bottom,
                    Text = "&nbsp;"
                });
            }

            tbl.Rows.Add(tr);

            // second header row - three columns per charge type
            tr = new TableRow();
            th = new TableHeaderCell
            {
                CssClass = "right-border-thick",
                VerticalAlign = VerticalAlign.Bottom,
                Text = nameColumnHeaderText
            };
            th.Style.Add("text-align", "left");
            tr.Cells.Add(th);

            foreach (DataRow dr in dtChargeTypes.Rows)
            {
                if (costType == "Cost")
                {
                    tr.Cells.Add(new TableHeaderCell
                    {
                        VerticalAlign = VerticalAlign.Bottom,
                        Text = "Acct Per"
                    });
                }

                tr.Cells.Add(new TableHeaderCell
                {
                    VerticalAlign = VerticalAlign.Bottom,
                    Text = "Per Use Charge"
                });

                tr.Cells.Add(new TableHeaderCell
                {
                    VerticalAlign = VerticalAlign.Bottom,
                    CssClass = "right-border-thick",
                    Text = perPeriodColumnHeaderText
                });
            }

            if (costType == "Cost")
            {
                tr.Cells.Add(new TableHeaderCell
                {
                    CssClass = "right-border-thick",
                    VerticalAlign = VerticalAlign.Bottom,
                    Text = "Modified"
                });
            }

            tbl.Rows.Add(tr);
        }

        private void BuildCostTables()
        {
            int r;

            DataView dvAuxCostTableData = AuxCostData();

            if (dvAuxCostTableData != null)
            {
                r = 2; //rows start at 2 (0 and 1 are for the header)
                foreach (DataRowView drv in dvAuxCostTableData)
                {
                    AddAuxCostTableRow(drv.Row, r);
                    r += 1;
                }
            }

            bool groupItems = false;

            DataView dvCostTableData = CostData(ref groupItems);

            int groupId = -1;
            int categoryId = -1;
            if (dvCostTableData != null)
            {
                r = 2; //rows start at 2 (0 and 1 are for the header)
                foreach (DataRowView drv in dvCostTableData)
                {
                    if (groupItems)
                    {
                        if (Convert.ToInt32(drv["GroupID"]) != groupId)
                        {
                            groupId = Convert.ToInt32(drv["GroupID"]);
                            categoryId = -1;
                            AddGroupRow(drv.Row, r);
                            r += 1;
                        }

                        if (Convert.ToInt32(drv["CategoryID"]) != categoryId)
                        {
                            categoryId = Convert.ToInt32(drv["CategoryID"]);
                            AddCategoryRow(drv.Row, r);
                            r += 1;
                        }
                    }

                    AddCostItemRow(drv.Row, r, groupItems);
                    r += 1;
                }
            }

            //========= OBSOLETE =============
            string strItemTypeID = string.Empty;
            string strItemTypeName = string.Empty;

            switch (ItemType)
            {
                case "Tool":
                    strItemTypeID = "ResourceID";
                    strItemTypeName = "ResourceName";
                    break;
                case "Room":
                    strItemTypeID = "RoomID";
                    strItemTypeName = "Room";
                    break;
                case "Store":
                    strItemTypeID = "CategoryID";
                    strItemTypeName = "Category";
                    break;
            }

            // build AuxCost table and cost table - only gets called at page load
            // this table is the array'ized version of cost - makes binding easier
            // however, no reason to keep the table - use it and discard it
            DataTable dtItemAuxCost = new DataTable();
            dtItemAuxCost.Columns.Add("AuxCostItem", typeof(string));

            foreach (DataRow drChargeType in dsCost.Tables["ChargeType"].Rows)
            {
                dtItemAuxCost.Columns.Add(string.Format("AddVal{0}", drChargeType.Field<int>("ChargeTypeID")), typeof(string));
                dtItemAuxCost.Columns.Add(string.Format("MulVal{0}", drChargeType.Field<int>("ChargeTypeID")), typeof(string));
            }

            DataTable dtAuxCost = DataCommand().Param("CostType", ItemType).FillDataTable("dbo.AuxCost_Select");

            // force rows into the AuxCost table - one per each type of AuxCost item
            // cannot pull from DB since rows may not yet exist
            // force rows into the AuxCost table - one per each type of AuxCost item
            // cannot pull from DB since rows may not yet exist
            foreach (DataRow dr in dtAuxCost.Rows)
                AddNewAuxCostRow(dtItemAuxCost, dsCost.Tables["ChargeType"].Rows.Count, dr.Field<string>("AuxCostParm"), dr.Field<bool>("AllowPerUse"), dr.Field<bool>("AllowPerPeriod"));

            DataRow drNew;
            DataRow[] fdr;

            // fill with values from DB
            foreach (DataRow dr in dsCost.Tables["AuxCost"].Rows)
            {
                fdr = dtItemAuxCost.Select(string.Format("AuxCostItem = '{0}'", dr["TableNameOrDescript"]));
                if (fdr.Length > 0)
                {
                    fdr[0][string.Format("AddVal{0}", dr["ChargeTypeID"])] = dr["AddVal"];
                    fdr[0][string.Format("MulVal{0}", dr["ChargeTypeID"])] = dr["MulVal"];
                }
            }

            // now, costs
            DataTable dtCost = new DataTable();
            dtCost.Columns.Add(strItemTypeID, typeof(int));
            dtCost.Columns.Add(ItemType, typeof(string));
            foreach (DataRow drChargeType in dsCost.Tables["ChargeType"].Rows)
            {
                dtCost.Columns.Add(string.Format("AcctPer{0}", drChargeType["ChargeTypeID"]), typeof(string));
                dtCost.Columns.Add(string.Format("AddVal{0}", drChargeType["ChargeTypeID"]), typeof(double));
                dtCost.Columns.Add(string.Format("MulVal{0}", drChargeType["ChargeTypeID"]), typeof(double));
            }
            dtCost.Columns.Add("ShowHourly", typeof(bool));

            // add a row for each item
            foreach (DataRow dr in dsCost.Tables[ItemType].Rows)
            {
                drNew = dtCost.NewRow();
                drNew[strItemTypeID] = dr[strItemTypeID];
                drNew[ItemType] = dr[strItemTypeName];
                for (int i = 0; i < dsCost.Tables["ChargeType"].Rows.Count; i++)
                {
                    drNew[i * 3 + 2] = "None";
                    drNew[i * 3 + 3] = 0.0;
                    drNew[i * 3 + 4] = 0.0;
                }
                switch (ItemType)
                {
                    case "Room":
                        drNew["ShowHourly"] = dr.Field<bool>("PassbackRoom");
                        break;
                    case "Store":
                        drNew["ShowHourly"] = false;
                        break;
                    case "Tool":
                        drNew["ShowHourly"] = true;
                        break;
                }
                dtCost.Rows.Add(drNew);
            }

            // now, fill in values for the costs
            foreach (DataRow dr in dsCost.Tables["Cost"].Rows)
            {
                fdr = dtCost.Select(string.Format("{0} = {1}", strItemTypeID, dr["RecordID"]));
                if (fdr.Length > 0)
                {
                    fdr[0][string.Format("AcctPer{0}", dr["ChargeTypeID"])] = dr["AcctPer"];
                    fdr[0][string.Format("AddVal{0}", dr["ChargeTypeID"])] = dr["AddVal"];
                    fdr[0][string.Format("MulVal{0}", dr["ChargeTypeID"])] = dr["MulVal"];
                }
            }

            //ShowCostGrid()

            //dgAuxCost.Visible = False
            //dgAuxCost.DataSource = dtItemAuxCost
            //dgAuxCost.DataBind()

            //dgCost.Visible = False
            //dgCost.DataKeyField = strItemTypeID
            //dgCost.DataSource = dtCost
            //dgCost.DataBind()
        }

        private DataView CostData(ref bool group)
        {
            DataTable dt = BaseCostTable();

            DataTable dtItem = dsCost.Tables[ItemType];
            DataTable dtCost = dsCost.Tables["Cost"];

            string itemIdColumn = string.Empty;
            string itemNameColumn = string.Empty;
            string groupIdColumn = string.Empty;
            string groupNameColumn = string.Empty;
            string categoryIdColumn = string.Empty;
            string categoryNameColumn = string.Empty;
            group = false;

            switch (ItemType)
            {
                case "Tool":
                    itemIdColumn = "ResourceID";
                    itemNameColumn = "ResourceName";
                    groupIdColumn = "LabID";
                    groupNameColumn = "LabName";
                    categoryIdColumn = "ProcessTechID";
                    categoryNameColumn = "ProcessTechName";
                    group = true;
                    break;
                case "Room":
                    itemIdColumn = "RoomID";
                    itemNameColumn = "Room";
                    break;
                case "Store":
                    itemIdColumn = "CategoryID";
                    itemNameColumn = "Category";
                    break;
            }

            foreach (DataRow dr in dtItem.Rows)
            {
                DataRow nr = dt.NewRow();
                int recordId = dr.Field<int>(itemIdColumn);
                if (recordId > 0)
                {
                    string recordName = dr.Field<string>(itemNameColumn);
                    nr["RecordID"] = recordId;
                    nr["RecordName"] = recordName;

                    if (group)
                    {
                        nr["GroupID"] = dr.Field<int>(groupIdColumn);
                        nr["GroupName"] = dr.Field<string>(groupNameColumn);
                        nr["CategoryID"] = dr.Field<int>(categoryIdColumn);
                        nr["CategoryName"] = dr.Field<string>(categoryNameColumn);
                    }

                    foreach (int chargeTypeId in ChargeTypeIDs)
                        FillNewCostRow(nr, dtCost, recordId, chargeTypeId);

                    switch (ItemType)
                    {
                        case "Tool":
                            nr["ShowHourly"] = true;
                            break;
                        case "Room":
                            nr["ShowHourly"] = dr["PassbackRoom"].Equals(1);
                            break;
                        case "Store":
                            nr["ShowHourly"] = false;
                            break;
                    }

                    dt.Rows.Add(nr);
                }
            }

            return new DataView(dt);
        }

        private DataView AuxCostData()
        {
            //Create the table
            DataTable dt = new DataTable();
            dt.Columns.Add("AuxCostItem", typeof(string));

            foreach (int id in ChargeTypeIDs)
            {
                dt.Columns.Add(string.Format("AddVal_{0}", id), typeof(string));
                dt.Columns.Add(string.Format("MulVal_{0}", id), typeof(string));
            }

            //Fill the table
            DataTable dtAuxCost = dsCost.Tables["AuxCost"];
            foreach (DataRow dr in dtAuxCost.Rows)
            {
                int chargeTypeId = dr.Field<int>("ChargeTypeID");
                string auxCostItem = dr.Field<string>("TableNameOrDescript");
                object addval = dr["AddVal"];
                object mulval = dr["MulVal"];
                if (!string.IsNullOrEmpty(auxCostItem))
                {
                    DataRow drCurrent;
                    DataRow[] rows = dt.Select(string.Format("AuxCostItem = '{0}'", auxCostItem));
                    if (rows.Length > 0)
                        drCurrent = rows[0];
                    else
                    {
                        drCurrent = dt.NewRow();
                        drCurrent["AuxCostItem"] = auxCostItem;
                        dt.Rows.Add(drCurrent);
                    }
                    drCurrent[string.Format("AddVal_{0}", chargeTypeId)] = addval;
                    drCurrent[string.Format("MulVal_{0}", chargeTypeId)] = mulval;
                }
            }

            return new DataView(dt, string.Empty, "AuxCostItem ASC", DataViewRowState.CurrentRows);
        }

        private DataTable BaseCostTable()
        {
            DataTable dt = new DataTable();
            dt.Columns.Add("RecordID", typeof(int));
            dt.Columns.Add("RecordName", typeof(string));
            dt.Columns.Add("GroupID", typeof(int));
            dt.Columns.Add("GroupName", typeof(string));
            dt.Columns.Add("CategoryID", typeof(int));
            dt.Columns.Add("CategoryName", typeof(string));
            foreach (int id in ChargeTypeIDs)
            {
                dt.Columns.Add(string.Format("AcctPer_{0}", id), typeof(string));
                dt.Columns.Add(string.Format("AddVal_{0}", id), typeof(double));
                dt.Columns.Add(string.Format("MulVal_{0}", id), typeof(double));
            }
            dt.Columns.Add("ShowHourly", typeof(bool));
            return dt;
        }

        private void FillNewCostRow(DataRow nr, DataTable dtCosts, int recordId, int chargeTypeId)
        {
            //At this point dtCosts only has the rows needed for the current ItemType
            DataRow[] rows = dtCosts.Select(string.Format("RecordID = {0} AND ChargeTypeID = {1}", recordId, chargeTypeId));
            if (rows.Length > 0)
            {
                nr[string.Format("AcctPer_{0}", chargeTypeId)] = rows[0]["AcctPer"];
                nr[string.Format("AddVal_{0}", chargeTypeId)] = rows[0]["AddVal"];
                nr[string.Format("MulVal_{0}", chargeTypeId)] = rows[0]["MulVal"];
            }
        }

        private string NameColumnHeaderText()
        {
            switch (ItemType)
            {
                case "Tool":
                    return "Tool Name";
                case "Room":
                    return "Room Name";
                case "Store":
                    return "Category Name";
                default:
                    return "Undefined";
            }
        }

        private void AddNewAuxCostRow(DataTable dt, int colCount, string strAuxCostItem, bool bAddValReal, bool bMulValReal)
        {
            DataRow drNew = dt.NewRow();
            drNew["AuxCostItem"] = strAuxCostItem;
            for (int i = 0; i < colCount; i++)
            {
                if (bAddValReal) drNew[i * 2 + 1] = 0.0;
                if (bMulValReal) drNew[i * 2 + 2] = 0.0;
            }
            dt.Rows.Add(drNew);
        }

        private void AddGroupRow(DataRow dr, int index)
        {
            TableRow tr = new TableRow { CssClass = "cost-group" };
            TableCell td = new TableCell { CssClass = "name-column right-border-thick" };

            td.Controls.Add(new HiddenField
            {
                ID = $"hidRecordID_{index}",
                Value = dr.Field<int>("GroupID").ToString()
            });

            td.Controls.Add(new HiddenField
            {
                ID = $"hidRowType_{index}",
                Value = "group"
            });

            td.Controls.Add(new Literal
            {
                ID = $"litRecordName_{index}",
                Text = dr.Field<string>("GroupName")
            });

            tr.Cells.Add(td);

            foreach (int chargeTypeId in ChargeTypeIDs)
            {
                tr.Cells.Add(new TableCell
                {
                    ColumnSpan = 3,
                    CssClass = "right-border-thick"
                });
            }

            tr.Cells.Add(new TableCell
            {
                CssClass = "right-border-thick",
                Text = "&nbsp;"
            });

            tblCost.Rows.Add(tr);
        }

        private void AddCostItemRow(DataRow dr, int index, bool group)
        {
            TableRow tr = new TableRow();
            if (group) tr.CssClass = "cost-item";

            DropDownList ddl;
            TextBox txt;

            tr.CssClass += (index % 2 == 0) ? " item" : " alt-item";

            var td = new TableCell { CssClass = "name-column right-border-thick" };

            td.Controls.Add(new HiddenField()
            {
                ID = $"hidRecordID_{index}",
                Value = dr.Field<int>("RecordID").ToString()
            });

            td.Controls.Add(new HiddenField
            {
                ID = $"hidRowType_{index}",
                Value = "item"
            });

            td.Controls.Add(new Literal
            {
                ID = $"litRecordName_{index}",
                Text = dr.Field<string>("RecordName")
            });

            tr.Cells.Add(td);

            //Only assign values when IsPostBack = False. During post back values will be assigned by .NET magic (using user entered values if any)
            foreach (int chargeTypeId in ChargeTypeIDs)
            {
                string idSuffix = "_" + chargeTypeId.ToString() + "_" + index.ToString();
                string colSuffix = "_" + chargeTypeId.ToString();

                td = new TableCell { CssClass = "right-border-none" };
                td.Style.Add("text-align", "center");
                ddl = new DropDownList { ID = $"ddlAcctPer{idSuffix}" };
                ddl.Attributes.Add("onchange", $"markModified(this, {index});");
                ddl.DataTextField = "AcctPerText";
                ddl.DataValueField = "AcctPerValue";
                ddl.DataSource = AcctPers(dr.Field<bool>("ShowHourly"));
                ddl.DataBind();
                if (!Page.IsPostBack)
                    ddl.SelectedValue = dr.Field<string>($"AcctPer{colSuffix}");
                td.Controls.Add(ddl);
                tr.Cells.Add(td);

                td = new TableCell { CssClass = "right-border-none" };
                td.Style.Add("text-align", "center");
                txt = new TextBox { ID = $"txtAddVal{idSuffix}" };
                txt.Attributes.Add("onchange", $"markModified(this, {index});");
                txt.CssClass = "numeric-text";
                if (!Page.IsPostBack)
                    txt.Text = GetDoubleValue(dr, $"AddVal{colSuffix}").ToString();
                td.Controls.Add(txt);
                tr.Cells.Add(td);

                td = new TableCell { CssClass = "right-border-thick" };
                td.Style.Add("text-align", "center");
                txt = new TextBox { ID = $"txtMulVal{idSuffix}" };
                txt.Attributes.Add("onchange", $"markModified(this, {index});");
                txt.CssClass = "numeric-text";
                if (!Page.IsPostBack)
                    txt.Text = GetDoubleValue(dr, $"MulVal{colSuffix}").ToString();
                td.Controls.Add(txt);
                tr.Cells.Add(td);
            }

            td = new TableCell()
            {
                ColumnSpan = 3,
                CssClass = "right-border-thick"
            };

            td.Style.Add("text-align", "center");

            td.Controls.Add(new CheckBox
            {
                ID = $"chkModified_{index}",
                CssClass = $"modified-checkbox index-{index}",
                Enabled = false,
                Checked = false
            });

            tr.Cells.Add(td);

            tblCost.Rows.Add(tr);
        }

        private double GetDoubleValue(DataRow dr, string column)
        {
            object obj = dr[column];

            if (obj != null && obj != DBNull.Value)
            {
                if (double.TryParse(obj.ToString(), out double result))
                    return result;
                else
                    return 0;
            }

            return 0;
        }

        private void AddCategoryRow(DataRow dr, int index)
        {
            TableRow tr = new TableRow { CssClass = "cost-category" };
            TableCell td = new TableCell { CssClass = "name-column right-border-thick" };

            td.Controls.Add(new HiddenField
            {
                ID = $"hidRecordID_{index}",
                Value = dr.Field<int>("CategoryID").ToString()
            });

            td.Controls.Add(new HiddenField
            {
                ID = $"hidRowType_{index}",
                Value = "category"
            });

            td.Controls.Add(new Literal
            {
                ID = $"litRecordName_{index}",
                Text = dr.Field<string>("CategoryName")
            });

            tr.Cells.Add(td);

            foreach (int chargeTypeId in ChargeTypeIDs)
            {
                td = new TableCell()
                {
                    ColumnSpan = 3,
                    CssClass = "right-border-thick"
                };

                tr.Cells.Add(td);
            }

            td = new TableCell()
            {
                CssClass = "right-border-thick",
                Text = "&nbsp;"
            };

            tr.Cells.Add(td);

            tblCost.Rows.Add(tr);
        }

        private void AddAuxCostTableRow(DataRow dr, int index)
        {
            TableRow tr = new TableRow();
            TableCell td;
            Literal lit;
            HiddenField hid;
            TextBox txt;

            tr.CssClass = (index % 2 == 0) ? "item" : "alt-item";

            td = new TableCell()
            {
                CssClass = "right-border-thick"
            };

            hid = new HiddenField()
            {
                ID = "hidAuxCostItem_" + index.ToString(),
                Value = dr.Field<string>("AuxCostItem")
            };

            td.Controls.Add(hid);

            lit = new Literal()
            {
                ID = "litAuxCostItem_" + index.ToString(),
                Text = dr.Field<string>("AuxCostItem")
            };

            td.Controls.Add(lit);
            tr.Cells.Add(td);

            //Only assign values when IsPostBack = False. During PostBack values will be assigned by .NET magic (including user entered values if any)
            foreach (int chargeTypeId in ChargeTypeIDs)
            {
                string idSuffix = "_" + chargeTypeId.ToString() + "_" + index.ToString();
                string colSuffix = "_" + chargeTypeId.ToString();

                td = new TableCell()
                {
                    CssClass = "right-border-none"
                };

                td.Style.Add("text-align", "center");
                object addval = dr["AddVal" + colSuffix];

                if (addval != DBNull.Value)
                {
                    txt = new TextBox()
                    {
                        ID = "txtAuxAddVal" + idSuffix,
                        CssClass = "numeric-text"
                    };

                    if (!Page.IsPostBack)
                        txt.Text = Convert.ToDouble(addval).ToString();

                    td.Controls.Add(txt);
                }
                tr.Cells.Add(td);

                td = new TableCell()
                {
                    CssClass = "right-border-thick"
                };

                td.Style.Add("text-align", "center");
                object mulval = dr["MulVal" + colSuffix];

                if (mulval != DBNull.Value)
                {
                    txt = new TextBox()
                    {
                        ID = "txtAuxMulVal" + idSuffix,
                        CssClass = "numeric-text"
                    };

                    if (!Page.IsPostBack)
                        txt.Text = Convert.ToDouble(mulval).ToString();

                    td.Controls.Add(txt);
                }
                tr.Cells.Add(td);
            }

            tblAuxCost.Rows.Add(tr);
        }

        private DataTable AcctPers(bool showHourly)
        {
            DataTable result = new DataTable();
            result.Columns.Add("AcctPerValue", typeof(string));
            result.Columns.Add("AcctPerText", typeof(string));

            switch (ItemType)
            {
                case "Room":
                    result.Rows.Add("None", "None");
                    if (showHourly) result.Rows.Add("Hourly", "Hourly");
                    result.Rows.Add("Daily", "Daily");
                    result.Rows.Add("Monthly", "Monthly");
                    result.Rows.Add("Per Entry", "Per Entry");
                    break;
                case "Store":
                    result.Rows.Add("None", "None");
                    result.Rows.Add("Per Month", "Per Month");
                    break;
                case "Tool":
                    result.Rows.Add("None", "None");
                    if (showHourly) result.Rows.Add("Hourly", "Hourly");
                    result.Rows.Add("Daily", "Daily");
                    result.Rows.Add("Monthly", "Monthly");
                    result.Rows.Add("Per Use", "Per Use");
                    break;
            }

            return result;
        }

        protected void BtnUploadCostWorksheet_Click(object sender, EventArgs e)
        {
            if (FileUpload1.HasFile)
            {
                HttpPostedFile postedFile = FileUpload1.PostedFile;
                FileInfo fi = new FileInfo(postedFile.FileName);
                string fileName = $"{CurrentUser.ClientID}_{DateTime.Now:yyyyMMddHHmmss}_{postedFile.FileName}";
                string filePath = Server.MapPath(Path.Combine(ConfigurationManager.AppSettings["SpreadsheetsDirectory"], fileName));
                string connstr = string.Empty;
                switch (fi.Extension)
                {
                    case ".xls":
                    case ".xlsx":
                        connstr = string.Format("Provider=Microsoft.ACE.OLEDB.12.0;Data Source={0};Extended Properties=\"Excel 12.0;HDR=Yes;IMEX=1\"", filePath);
                        break;
                    default:
                        litUploadMessage.Text = "&nbsp;<span class=\"error\">Invalid file type, only Excel files (*.xls, *.xlsx) are allowed.</span>";
                        return;
                }

                postedFile.SaveAs(filePath);

                if (File.Exists(filePath))
                {
                    DataTable dt;
                    using (OleDbConnection conn = new OleDbConnection(connstr))
                    using (OleDbCommand cmd = new OleDbCommand("SELECT * FROM [Sheet1$]", conn))
                    using (OleDbDataAdapter adap = new OleDbDataAdapter(cmd))
                    {
                        dt = new DataTable();
                        adap.Fill(dt);
                        cmd.Connection.Close();
                    }

                    File.Delete(filePath);

                    if (!dt.Columns.Contains("RecordID"))
                    {
                        litUploadMessage.Text = "&nbsp;<span class=\"error\">Invalid cost worksheet. Column not found: RecordID.</span>";
                        return;
                    }

                    if (!dt.Columns.Contains("ChargeTypeID"))
                    {
                        litUploadMessage.Text = "&nbsp;<span class=\"error\">Invalid cost worksheet. Column not found: ChargeTypeID.</span>";
                        return;
                    }

                    if (!dt.Columns.Contains("AcctPer"))
                    {
                        litUploadMessage.Text = "&nbsp;<span class=\"error\">Invalid cost worksheet. Column not found: AcctPer.</span>";
                        return;
                    }

                    if (!dt.Columns.Contains("AddVal"))
                    {
                        litUploadMessage.Text = "&nbsp;<span class=\"error\">Invalid cost worksheet. Column not found: AddVal.</span>";
                        return;
                    }

                    if (!dt.Columns.Contains("MulVal"))
                    {
                        litUploadMessage.Text = "&nbsp;<span class=\"error\">Invalid cost worksheet. Column not found: MulVal.</span>";
                        return;
                    }

                    foreach (DataRow dr in dt.Rows)
                    {
                        int recordId = Convert.ToInt32(dr["RecordID"]);
                        int chargeTypeId = Convert.ToInt32(dr["ChargeTypeID"]);
                        string acctPer = dr["AcctPer"].ToString();
                        double addVal = Convert.ToDouble(dr["AddVal"]);
                        double mulVal = Convert.ToDouble(dr["MulVal"]);
                        TableRow tr = GetTableRowByRecordID(recordId, out int index);
                        if (tr != null)
                            SetValues(tr, index, chargeTypeId, acctPer, addVal, mulVal);
                    }

                    litUploadMessage.Text = "&nbsp;<span class=\"message\">Upload OK.</span>";
                }
                else
                    litUploadMessage.Text = "&nbsp;<span class=\"error\">Unable to save file.</span>";
            }
            else
                litUploadMessage.Text = "&nbsp;<span class=\"error\">No file was detected.</span>";
        }

        private TableRow GetTableRowByRecordID(int recordId, out int index)
        {
            TableRow tr = null;
            int i = 0;
            index = -1;
            HiddenField hid;
            foreach (TableRow row in tblCost.Rows)
            {
                hid = (HiddenField)row.FindControl("hidRecordID_" + i.ToString());
                if (hid != null)
                {
                    if (hid.Value == recordId.ToString())
                    {
                        tr = row;
                        index = i;
                        break;
                    }
                }
                i += 1;
            }
            return tr;
        }

        private void SetValues(TableRow tr, int index, int chargeTypeId, string acctPer, double addVal, double mulVal)
        {
            DropDownList ddl;
            TextBox txt;
            CheckBox chk;
            string idSuffix = "_" + chargeTypeId.ToString() + "_" + index.ToString();

            ddl = (DropDownList)tr.FindControl("ddlAcctPer" + idSuffix);
            if (ddl != null && ddl.SelectedValue != acctPer)
            {
                ddl.SelectedValue = acctPer;
                ddl.BackColor = modifiedColor;
            }

            txt = (TextBox)tr.FindControl("txtAddVal" + idSuffix);
            if (txt != null && txt.Text != addVal.ToString())
            {
                txt.Text = addVal.ToString();
                txt.BackColor = modifiedColor;
            }

            txt = (TextBox)tr.FindControl("txtMulVal" + idSuffix);
            if (txt != null && txt.Text != mulVal.ToString())
            {
                txt.Text = mulVal.ToString();
                txt.BackColor = modifiedColor;
            }

            chk = (CheckBox)tr.FindControl("chkModified_" + index.ToString());
            if (chk != null)
                chk.Checked = true;
        }

        protected void BtnSave_Click(object sender, EventArgs e)
        {
            Save();
        }

        private void Save()
        {
            SetSaveMessage(string.Empty);


            DateTime d = DateTime.Now;
            DataTable dtModifiedCost = GetModifiedCostValues(d, out bool valid);

            if (!valid)
            {
                SetSaveMessage("<span class=\"error\">All values must be numeric.</span>");
                return;
            }

            DataTable dtModifiedAuxCost = GetModifiedAuxCostValues(d, out valid);

            if (!valid)
            {
                SetSaveMessage("<span class=\"error\">All values must be numeric.</span>");
                return;
            }

            SaveModifiedCostValues(dtModifiedCost);
            SaveModifiedCostValues(dtModifiedAuxCost);
            int totalRecordsCount = dtModifiedCost.Rows.Count + dtModifiedAuxCost.Rows.Count;
            string message = "Complete: " + totalRecordsCount.ToString() + " record" + (totalRecordsCount != 1 ? "s" : string.Empty) + " modified.";
            SetSaveMessage(string.Format("<span class=\"message\">{0}</span>", message));
        }

        private void SetSaveMessage(string text)
        {
            litSaveMessage1.Text = text;
            litSaveMessage2.Text = text;
        }

        private int SaveModifiedCostValues(DataTable dt)
        {
            if (dt.Rows.Count > 0)
            {
                tableNamePrefix = Convert.ToString(Session["Exp"]);

                return DataCommand().Update(dt, cfg =>
                {
                    cfg.Insert.AddParameter("ChargeTypeID", SqlDbType.Int);
                    cfg.Insert.AddParameter("TableNameOrDescript", SqlDbType.NVarChar, 50);
                    cfg.Insert.AddParameter("RecordID", SqlDbType.Int);
                    cfg.Insert.AddParameter("AcctPer", SqlDbType.NVarChar, 25);
                    cfg.Insert.AddParameter("AddVal", SqlDbType.Float);
                    cfg.Insert.AddParameter("MulVal", SqlDbType.Float);
                    cfg.Insert.AddParameter("EffDate", SqlDbType.DateTime);
                    cfg.Insert.SetCommandText($"dbo.{tableNamePrefix}Cost_Insert");
                });
            }

            return 0;
        }

        private DataTable GetModifiedCostValues(DateTime d, out bool valid)
        {
            DataTable dt = BaseModifiedTable();

            int r = 2; //rows start at 2 (0 and 1 are for the header)
            int recordId;
            object origAcctPer;
            object origAddVal;
            object origMulVal;
            object acctPer;
            object addVal;
            object mulVal;

            valid = true;

            while (r < tblCost.Rows.Count)
            {
                recordId = GetRecordID(r, out string rowType);
                if (rowType == "item")
                {
                    foreach (int chargeTypeId in ChargeTypeIDs)
                    {
                        origAcctPer = GetOrigCostVal(recordId, chargeTypeId, "AcctPer");
                        origAddVal = GetOrigCostVal(recordId, chargeTypeId, "AddVal");
                        origMulVal = GetOrigCostVal(recordId, chargeTypeId, "MulVal");
                        acctPer = GetAcctPer(chargeTypeId, r);
                        addVal = GetCostVal(chargeTypeId, r, tblCost, "txtAddVal");
                        mulVal = GetCostVal(chargeTypeId, r, tblCost, "txtMulVal");

                        if (addVal != DBNull.Value)
                            valid = valid && double.TryParse(addVal.ToString(), out double dbl);

                        if (mulVal != DBNull.Value)
                            valid = valid && double.TryParse(mulVal.ToString(), out double dbl);

                        if (!valid) return dt;

                        if (origAcctPer.ToString() != acctPer.ToString() || origAddVal.ToString() != addVal.ToString() || origMulVal.ToString() != mulVal.ToString())
                        {
                            DataRow nr = dt.NewRow();
                            nr.SetField("ChargeTypeID", chargeTypeId);
                            nr.SetField("TableNameOrDescript", ItemType + "Cost");
                            nr.SetField("EffDate", d);
                            nr.SetField("RecordID", recordId);
                            nr.SetField("OrigAcctPer", origAcctPer);
                            nr.SetField("AcctPer", acctPer);
                            nr.SetField("OrigAddVal", origAddVal);
                            nr.SetField("AddVal", addVal);
                            nr.SetField("OrigMulVal", origMulVal);
                            nr.SetField("MulVal", mulVal);
                            dt.Rows.Add(nr);
                        }
                    }
                }

                CheckBox chk = (CheckBox)tblCost.Rows[r].FindControl("chkModified_" + r.ToString());
                if (chk != null)
                    chk.Checked = false;

                r += 1;
            }

            return dt;
        }

        private DataTable GetModifiedAuxCostValues(DateTime d, out bool valid)
        {
            DataTable dt = BaseModifiedTable();

            int r = 2; //rows start at 2 (0 and 1 are for the header);
            string auxCostItem;
            object origAddVal;
            object origMulVal;
            object addVal;
            object mulVal;
            valid = true;
            while (r < tblAuxCost.Rows.Count)
            {
                auxCostItem = GetAuxCostItem(r);

                foreach (int chargeTypeId in ChargeTypeIDs)
                {
                    origAddVal = GetOrigAuxCostVal(auxCostItem, chargeTypeId, "AddVal");
                    origMulVal = GetOrigAuxCostVal(auxCostItem, chargeTypeId, "MulVal");
                    addVal = GetCostVal(chargeTypeId, r, tblAuxCost, "txtAuxAddVal");
                    mulVal = GetCostVal(chargeTypeId, r, tblAuxCost, "txtAuxMulVal");

                    double dbl;
                    if (addVal != DBNull.Value)
                        valid = valid && double.TryParse(addVal.ToString(), out dbl);
                    if (mulVal != DBNull.Value)
                        valid = valid && double.TryParse(mulVal.ToString(), out dbl);

                    if (!valid) return dt;

                    if (origAddVal.ToString() != addVal.ToString() || origMulVal.ToString() != mulVal.ToString())
                    {
                        DataRow nr = dt.NewRow();
                        nr.SetField("ChargeTypeID", chargeTypeId);
                        nr.SetField("TableNameOrDescript", auxCostItem);
                        nr.SetField("EffDate", d);
                        nr.SetField("RecordID", DBNull.Value);
                        nr.SetField("OrigAcctPer", DBNull.Value);
                        nr.SetField("AcctPer", DBNull.Value);
                        nr.SetField("OrigAddVal", origAddVal);
                        nr.SetField("AddVal", addVal);
                        nr.SetField("OrigMulVal", origMulVal);
                        nr.SetField("MulVal", mulVal);
                        dt.Rows.Add(nr);
                    }
                }

                r += 1;
            }

            return dt;
        }

        private DataTable BaseModifiedTable()
        {
            DataTable dt = new DataTable();
            dt.Columns.Add("ChargeTypeID", typeof(int));
            dt.Columns.Add("TableNameOrDescript", typeof(string));
            dt.Columns.Add("EffDate", typeof(DateTime));
            dt.Columns.Add("RecordID", typeof(int));
            dt.Columns.Add("OrigAcctPer", typeof(string));
            dt.Columns.Add("AcctPer", typeof(string));
            dt.Columns.Add("OrigAddVal", typeof(double));
            dt.Columns.Add("AddVal", typeof(double));
            dt.Columns.Add("OrigMulVal", typeof(double));
            dt.Columns.Add("MulVal", typeof(double));
            return dt;
        }

        private int GetRecordID(int row, out string rowType)
        {
            TableRow tr = tblCost.Rows[row];

            int result = 0;
            HiddenField hidRecordID = (HiddenField)tr.FindControl("hidRecordID_" + row.ToString());
            if (hidRecordID != null)
                result = Convert.ToInt32(hidRecordID.Value);

            rowType = "Undefined";
            HiddenField hidRowType = (HiddenField)tr.FindControl("hidRowType_" + row.ToString());
            if (hidRowType != null)
                rowType = hidRowType.Value;

            return result;
        }

        private object GetOrigCostVal(int recordId, int chargeTypeId, string column)
        {
            object result = DBNull.Value;
            DataTable dtCost = dsCost.Tables["Cost"];
            DataRow[] rows = dtCost.Select(string.Format("RecordID = {0} AND ChargeTypeID = {1}", recordId, chargeTypeId));
            if (rows.Length > 0)
                result = rows[0][column];
            return result;
        }

        private string GetAuxCostItem(int row)
        {
            TableRow tr = tblAuxCost.Rows[row];
            string result = string.Empty;
            HiddenField hid = (HiddenField)tr.FindControl("hidAuxCostItem_" + row.ToString());
            if (hid != null) result = hid.Value;
            return result;
        }

        private object GetOrigAuxCostVal(string auxCostItem, int chargeTypeId, string column)
        {
            object result = DBNull.Value;
            DataTable dtAuxCost = dsCost.Tables["AuxCost"];
            DataRow[] rows = dtAuxCost.Select(string.Format("TableNameOrDescript = '{0}' AND ChargeTypeID = {1}", auxCostItem, chargeTypeId));
            if (rows.Length > 0)
                result = rows[0][column];
            return result;
        }

        private object GetCostVal(int chargeTypeId, int row, Table tbl, string ctrlPrefix)
        {
            object result = DBNull.Value;
            string idSuffix = "_" + chargeTypeId.ToString() + "_" + row.ToString();
            TableRow tr = tbl.Rows[row];
            TextBox txt = (TextBox)tr.FindControl(ctrlPrefix + idSuffix);
            if (txt != null)
                result = txt.Text;
            return result;
        }

        private object GetAcctPer(int chargeTypeId, int row)
        {
            object result = DBNull.Value;
            string idSuffix = "_" + chargeTypeId.ToString() + "_" + row.ToString();
            TableRow tr = tblCost.Rows[row];
            DropDownList ddl = (DropDownList)tr.FindControl("ddlAcctPer" + idSuffix);
            if (ddl != null)
                result = ddl.SelectedValue;
            return result;
        }
    }
}