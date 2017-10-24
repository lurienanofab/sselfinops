using LNF.Billing;
using LNF.Cache;
using LNF.Models.Data;
using LNF.Repository;
using LNF.Repository.Scheduler;
using sselFinOps.AppCode;
using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web.UI.WebControls;

namespace sselFinOps
{
    public partial class ConCostMaterial : ReportPage
    {
        private DataSet dsCost;
        //private int numCellWidth = 70;
        //private int ddlCellWidth = 100;
        private string tableNamePrefix = string.Empty;
        public const string MATERIAL = "Material";

        public const string HOURLY = "HOURLY";
        public const string DAILY = "DAILY";
        public const string MONTHLY = "MONTHLY";
        public const string PERUSE = "PERUSE";

        public const string UMICH = "UMICH";
        public const string OTHER_ACADEMIC = "OTHER_ACAD";
        public const string NON_ACADEMIC = "NON_ACAD";

        public override ClientPrivilege AuthTypes
        {
            get
            {
                return ClientPrivilege.Executive | ClientPrivilege.Administrator;
            }
        }

        public string ItemType
        {
            get { return MATERIAL; }
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            SqlConnection cnSselData = new SqlConnection(ConfigurationManager.ConnectionStrings["cnSselData"].ConnectionString);
            GenerateTitleLinks(cnSselData);

            //RetrieveAllCostData(cnSselData)

            GenerateMaterialGrid();
        }

        private void GenerateMaterialGrid()
        {
            //Dim allPIPs As IEnumerable(Of ProcessInfoLineParam) = DA.Search(Of ProcessInfoLineParam)(Function(x) x.IsPremiumMaterial = True).OrderBy(Function(x) x.Resource.ResourceID)
            var allPIPs = DA.Current.Query<ProcessInfoLineParam>().OrderBy(x => x.Resource.ResourceID).ToArray();
            int currentResourceId = -1;

            foreach (var item in allPIPs)
            {
                var tRow = new TableRow();
                tRow.Attributes["class"] = "cost-item";
                if (currentResourceId != item.Resource.ResourceID)
                {
                    if (item.Resource.ResourceID > 0)
                    {
                        //tRow.Attributes("class") = tRow.Attributes("class") + " tr-top"
                        AddResourceNameRow(item.Resource, tblmaterial);
                    }
                    currentResourceId = item.Resource.ResourceID;
                }

                AddMaterial(item, tRow, item.Resource.ResourceID);

                AddChargeRow(tRow, UMICH);
                AddChargeRow(tRow, OTHER_ACADEMIC);
                AddChargeRow(tRow, NON_ACADEMIC);

                tblmaterial.Rows.Add(tRow);
            }
        }

        private void AddMaterial(ProcessInfoLineParam pilp, TableRow tRow, int resourceId)
        {
            var tCell = new TableCell();
            var lblPILP = new Label();

            lblPILP.Text = pilp.ParameterName + ")" + pilp.ProcessInfoLineParamID.ToString();
            lblPILP.Attributes["class"] = "lbl-pilp";
            lblPILP.Attributes["resourceID"] = resourceId.ToString();
            lblPILP.Attributes["pilpID"] = pilp.ProcessInfoLineParamID.ToString();

            tCell.Attributes["class"] = "name-column right-border-thick";
            tCell.Controls.Add(lblPILP);
            tRow.Cells.Add(tCell);
        }

        private void AddResourceNameRow(Resource res, Table tbl)
        {
            var rhRow = new TableRow();
            var lblPILP = new Label();
            lblPILP.Text = "<b>" + res.ResourceName + "</b>";
            var tCell = new TableCell();
            tCell.Controls.Add(lblPILP);
            tCell.ColumnSpan = 10;
            tCell.Attributes["class"] = "name-column right-border-thick";
            rhRow.Cells.Add(tCell);
            rhRow.Attributes["class"] = "cost-category tr-top";
            tbl.Rows.Add(rhRow);
        }

        private void AddChargeRow(TableRow tRow, string orgType)
        {
            tRow.Cells.Add(GetDropDownCell(orgType));

            var tc2 = new TableCell();
            var txtPerUsage = new TextBox();
            txtPerUsage.Text = "0";
            txtPerUsage.Attributes["class"] = "txtUSAGE_" + orgType;
            tc2.Controls.Add(txtPerUsage);
            tRow.Cells.Add(tc2);

            var tc3 = new TableCell();
            var txtPerPeriod = new TextBox();
            txtPerPeriod.Text = "0";
            txtPerPeriod.Attributes["class"] = "txtPERIOD_" + orgType;
            tc3.Attributes["class"] = "right-border-thick";
            tc3.Controls.Add(txtPerPeriod);

            tRow.Cells.Add(tc3);
        }

        private TableCell GetDropDownCell(string orgType)
        {
            var tCell = new TableCell();
            var ddl = new DropDownList();
            ddl.Items.Add(new ListItem("Hourly", HOURLY));
            ddl.Items.Add(new ListItem("Daily", DAILY));
            ddl.Items.Add(new ListItem("Monthly", MONTHLY));
            ddl.Items.Add(new ListItem("Per Use", PERUSE));
            ddl.Attributes["class"] = "ddlAcct_" + orgType;
            tCell.Controls.Add(ddl);
            return tCell;
        }

        private void GenerateTitleLinks(SqlConnection cnSselData)
        {
            dsCost = new DataSet("ConCost");

            bool experimental = !string.IsNullOrEmpty(Request.QueryString["Exp"]);

            if (experimental)
            {
                CacheManager.Current.Exp(Request.QueryString["Exp"]);
                tableNamePrefix = CacheManager.Current.Exp();
                btnBack1.Text = "Return to Experimental Cost Config";
                btnBack2.Text = "Return to Experimental Cost Config";
            }

            string path = Request.Url.GetLeftPart(UriPartial.Path);
            string[] allTitles = { "Tool", "Room", "Store", MATERIAL };

            string titlesURL = "Configure " + tableNamePrefix + " costs for: ";
            foreach (string item in allTitles)
            {
                if (item == ItemType)
                    titlesURL += item + "|";
                else
                {
                    string exp = experimental ? "&Exp=Exp" : string.Empty;
                    titlesURL += "<a href=" + path + "?ItemType=" + item + exp + ">" + item + "</a>" + "|";
                }
            }

            litHeader.Text = titlesURL;
        }

        protected void btnSave_Click(object sender, EventArgs e)
        {
            //Save()
        }
    }
}