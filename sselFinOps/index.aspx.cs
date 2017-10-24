using LNF.Cache;
using LNF.Models.Data;
using sselFinOps.AppCode;
using System;
using System.Collections.Generic;
using System.Web.UI.WebControls;

namespace sselFinOps
{
    public partial class index : LNF.Web.Content.LNFPage
    {
        private Dictionary<Button, AuthInfo> appPages;

        public override ClientPrivilege AuthTypes
        {
            get { return 0; }
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            // store the relationship between the buttons and pages
            // there should be one line per button, equals number of pages
            appPages = new Dictionary<Button, AuthInfo>();
            appPages.Add(btnRepOther, AuthInfo.Create("~/RepOther.aspx", true, 0));
            appPages.Add(btnRepInvoice, AuthInfo.Create("~/RepInvoice.aspx", true, ClientPrivilege.Executive | ClientPrivilege.Administrator));
            appPages.Add(btnRepMaterialsJE, AuthInfo.Create("~/RepMaterialsJE.aspx", true, 0));
            appPages.Add(btnRepLabTimeJE, AuthInfo.Create("~/RepLabTimeJE.aspx", true, 0));
            appPages.Add(btnRepToolUseJE, AuthInfo.Create("~/RepToolUseJE.aspx", true, 0));
            appPages.Add(btnConHolidays, AuthInfo.Create("~/ConHolidays.aspx", true, ClientPrivilege.Administrator));
            appPages.Add(btnConGlobal, AuthInfo.Create("~/ConGlobal.aspx", true, ClientPrivilege.Administrator));
            appPages.Add(btnConAuxCost, AuthInfo.Create("~/ConAuxCost.aspx", true, ClientPrivilege.Administrator));
            appPages.Add(btnConRoomCost, AuthInfo.Create("~/ConCost.aspx", true, ClientPrivilege.Executive | ClientPrivilege.Administrator));
            appPages.Add(btnConToolCost, AuthInfo.Create("~/ConCost.aspx", true, ClientPrivilege.Executive | ClientPrivilege.Administrator));
            appPages.Add(btnConStoreCost, AuthInfo.Create("~/ConCost.aspx", true, ClientPrivilege.Executive | ClientPrivilege.Administrator));
            appPages.Add(btnConFormula, AuthInfo.Create("~/ConFormula.aspx", true, ClientPrivilege.Administrator));
            appPages.Add(btnConOrgRecharge, AuthInfo.Create("~/ConOrgRecharge.aspx", true, ClientPrivilege.Administrator));
            appPages.Add(btnDatHistorical, AuthInfo.Create("~/DatHistorical.aspx", true, ClientPrivilege.Administrator));
            appPages.Add(btnDatSubsidyTiers, AuthInfo.Create("~/DatSubsidyTiers.aspx", true, ClientPrivilege.Administrator));
            appPages.Add(btnDatCurrentCosts, AuthInfo.Create("~/DatCurrentCosts.aspx", true, 0));
            appPages.Add(btnMscExp, AuthInfo.Create("~/MscExp.aspx", true, ClientPrivilege.Administrator)); // experimental cost configureation
            appPages.Add(btnMscExp2, AuthInfo.Create("~/MscExp2.aspx", true, ClientPrivilege.Administrator)); // 2007-04-16 created another experimental page and use it only on Test server
            appPages.Add(btnMscInternalSpecial, AuthInfo.Create("~/MscInternalSpecial.aspx", true, ClientPrivilege.Executive | ClientPrivilege.Administrator));
            appPages.Add(btnRepSUBStore, AuthInfo.Create("~/RepSUBStore.aspx", true, 0));
            appPages.Add(btnRepSUB, AuthInfo.Create("~/RepSUB.aspx", true, ClientPrivilege.Executive | ClientPrivilege.Administrator));
            appPages.Add(btnRepMiscCharge, AuthInfo.Create("~/RepMiscCharge.aspx", true, ClientPrivilege.Administrator));
            appPages.Add(btnConRemoteProcessing, AuthInfo.Create("~/ConRemoteProcessing.aspx", true, ClientPrivilege.Administrator));
            appPages.Add(btnMscBillingChecks, AuthInfo.Create("~/MscBillingChecks.aspx", true, ClientPrivilege.Administrator));
            appPages.Add(btnMscToolBillingReport, AuthInfo.Create("~/RepToolBilling.aspx", true, ClientPrivilege.Administrator));

            if (!Page.IsPostBack)
            {
                if (!Page.IsPostBack)
                {
                    if (Request.QueryString["AbandonSession"] == "1")
                    {
                        Session.Abandon();
                        Response.Redirect("~");
                    }
                }

                // check to see if session is valid
                if (Request.QueryString.Count > 0) //probably coming from sselonline
                {
                    int clientId;
                    if (int.TryParse(Request.QueryString["ClientID"].Trim(), out clientId))
                    {
                        if (CacheManager.Current.CurrentUser.ClientID != clientId)
                        {
                            CacheManager.Current.AbandonSession();
                            Response.Redirect("~");
                        }
                    }
                }

                foreach (var kvp in appPages)
                {
                    Button btn = kvp.Key;
                    string reportType = btn.ID.Substring(3, 3);
                    Label lbl = (Label)FindControlRecursive("lbl" + reportType);
                    lbl.Visible = lbl.Visible | DisplayButton(btn, kvp.Value);
                }

                lblName.Text = CacheManager.Current.CurrentUser.DisplayName;
            }
        }

        // handles all button clicks
        protected void FinOpsButton_Click(object sender, EventArgs e)
        {
            Button button = (Button)sender;
            string redirectPage = appPages[button].Location + button.CommandArgument;
            Response.Redirect(redirectPage);
        }

        private bool DisplayButton(Button btn, AuthInfo authInfo)
        {
            btn.Visible = authInfo.ShowButton && CacheManager.Current.CurrentUser.HasPriv(authInfo.AuthTypes);
            return btn.Visible;
        }

        protected void btnLogout_Click(object sender, EventArgs e)
        {
            CacheManager.Current.RemoveCacheData(); //remove anything left in cache
            CacheManager.Current.AbandonSession();
            Response.Redirect("/sselonline/Blank.aspx");
        }
    }
}