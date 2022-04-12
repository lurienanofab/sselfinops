<%@ Page Title="FinOps Main Page" Language="C#" MasterPageFile="~/FinOpsMaster.Master" AutoEventWireup="true" CodeBehind="Index.aspx.cs" Inherits="sselFinOps.Index" %>

<%@ Import Namespace="LNF.Data" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <style>
        a.command-button {
            background-color: rgb(221,221,221);
            border: #663399 ridge;
            text-align: center;
            font-size: small;
            font-family: 'Arial';
            display: block;
            line-height: 26px;
        }

            a.command-button:link,
            a.command-button:visited,
            a.command-button:active {
                text-decoration: none;
                color: black;
            }

        .container {
            display: flex;
            margin-bottom: 10px;
            margin-left: 5px;
        }

            .container > .col {
                margin-right: 10px;
                padding: 10px;
            }

            .container .CommandButton {
                display: block;
                margin-top: 15px;
            }

        .user-info {
            margin: 10px 0 20px 0;
        }
    </style>
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    <div class="section">

        <div style="padding: 10px;">
            <span class="PageHeader">LNF Financial Operations</span>
            <div style="padding: 10px 0 10px 0; font-weight: bold;">
                Current user:
                <asp:Label runat="server" ID="lblName"></asp:Label>
            </div>
        </div>

        <div class="container">
            <div runat="server" id="divRep" visible='<%#HasPriv(ClientPrivilege.Administrator | ClientPrivilege.Executive)%>' class="col">
                <div class="ButtonGroupHeader">
                    <asp:Label runat="server" ID="lblRep">Financial Reports</asp:Label>
                </div>
                <asp:Button runat="server" ID="btnRepMiscCharge" CssClass="CommandButton" Visible='<%#HasPriv(ClientPrivilege.Administrator)%>' Text="Misc. Charges" OnCommand="Button_Command" CommandName="navigate" CommandArgument="~/RepMiscCharge.aspx" />
                <asp:Button runat="server" ID="btnRepSUB" CssClass="CommandButton" Visible='<%#HasPriv(ClientPrivilege.Administrator | ClientPrivilege.Executive)%>' Text="SUB Reports" ToolTip="All SUB Reports" OnCommand="Button_Command" CommandName="navigate" CommandArgument="~/RepSUB.aspx" />
                <asp:Button runat="server" ID="btnRepOther" CssClass="CommandButton" Visible="false" Text="External Misc. Charges" ToolTip="Used to enter other charges incurred by non-UM organizations" OnCommand="Button_Command" CommandName="navigate" CommandArgument="~/RepOther.aspx" />
                <asp:Button runat="server" ID="btnRepInvoice" CssClass="CommandButton" Visible='<%#HasPriv(ClientPrivilege.Executive | ClientPrivilege.Administrator)%>' Text="External Org Invoice" ToolTip="Creates a UM Invoice spreadsheet for external (non-UMich) organizations" OnCommand="Button_Command" CommandName="navigate" CommandArgument="~/RepInvoice.aspx" />
                <asp:Button runat="server" ID="btnRepToolBilling" CssClass="CommandButton" Visible='<%#HasPriv(ClientPrivilege.Administrator)%>' Text="Tool Billing Report" ToolTip="Display tool billing data by reservation" OnCommand="Button_Command" CommandName="navigate" CommandArgument="~/RepToolBilling.aspx" />
                <asp:Button runat="server" ID="btnRepLabTimeJE" CssClass="CommandButton" Visible="false" Text="Labtime JE" ToolTip="Creates a UM Journal Entry spreadsheet for internal user time-in-lab usage" OnCommand="Button_Command" CommandName="navigate" CommandArgument="~/RepLabTimeJE.aspx" />
                <asp:Button runat="server" ID="btnRepToolUseJE" CssClass="CommandButton" Visible="false" Text="Tool Use JE" ToolTip="Creates a UM Journal Entry spreadsheet for internal user tool usage" OnCommand="Button_Command" CommandName="navigate" CommandArgument="~/RepToolUseJE.aspx" />
                <asp:Button runat="server" ID="btnRepMaterialsJE" CssClass="CommandButton" Visible="false" Text="Materials JE" ToolTip="Creates a UM Journal Entry spreadsheet for items purchased form the on-line store" OnCommand="Button_Command" CommandName="navigate" CommandArgument="~/RepMaterialsJE.aspx" />
                <asp:Button runat="server" ID="btnRepManagerUsageReport" CssClass="CommandButton" Visible='<%#HasPriv(ClientPrivilege.Administrator | ClientPrivilege.Executive)%>' Text="Manager Usage Report" ToolTip="Display usage by user and account for a manager." OnCommand="Button_Command" CommandName="navigate" CommandArgument="~/RepManagerUsageReport.aspx" />
                <asp:Button runat="server" ID="btnRepManagerUsageEmails" CssClass="CommandButton" Visible='<%#HasPriv(ClientPrivilege.Administrator | ClientPrivilege.Executive)%>' Text="Manager Usage Emails" ToolTip="Send the Manager Usage emails." OnCommand="Button_Command" CommandName="navigate" CommandArgument="~/RepManagerUsageEmails.aspx" />
                <asp:Button runat="server" ID="btnRepFinancialManager" CssClass="CommandButton" Visible='<%#HasPriv(ClientPrivilege.Administrator)%>' Text="Financial Manager Report" ToolTip="Display tool billing data by reservation" OnCommand="Button_Command" CommandName="navigate" CommandArgument="~/RepFinancialManager.aspx" />
                <asp:Button runat="server" ID="btnRepSUBStore" CssClass="CommandButton" Visible="false" Text="Old SUB (2007 June)" ToolTip="All SUB Reports Before July 2009" OnCommand="Button_Command" CommandName="navigate" CommandArgument="~/RepSUBStore.aspx" />
                <asp:Button runat="server" ID="btnRepDurationReport" CssClass="CommandButton" Visible='<%#HasPriv(ClientPrivilege.Administrator)%>' Text="Durations Report" ToolTip="Displays detailed billing information about reservation durations." OnCommand="Button_Command" CommandName="navigate" CommandArgument="~/RepDurationsReport.aspx" />
            </div>
            <div runat="server" id="divCon" visible='<%#HasPriv(ClientPrivilege.Administrator | ClientPrivilege.Executive)%>' class="col">
                <div class="ButtonGroupHeader">
                    <asp:Label runat="server" ID="lblCon">Configuration</asp:Label>
                </div>
                <asp:Button runat="server" ID="btnConHolidays" CssClass="CommandButton" Visible='<%#HasPriv(ClientPrivilege.Administrator)%>' Text="Define Holidays" ToolTip="Specify official University and SSEL holidays" OnCommand="Button_Command" CommandName="navigate" CommandArgument="~/ConHolidays.aspx" />
                <asp:Button runat="server" ID="btnConGlobal" CssClass="CommandButton" Visible='<%#HasPriv(ClientPrivilege.Administrator)%>' Text="Configure Global Parameters" ToolTip="Specify monthly/yearly cost caps, etc" OnCommand="Button_Command" CommandName="navigate" CommandArgument="~/ConGlobal.aspx" />
                <asp:Button runat="server" ID="btnAuxCostConfiguration" CssClass="CommandButton" Visible='<%#HasPriv(ClientPrivilege.Administrator)%>' Text="Define Aux Cost Parameters" ToolTip="Create auxilliary costing parameters, such as caps, penalties, etc." OnCommand="Button_Command" CommandName="navigate" CommandArgument="~/AuxCostConfiguration.aspx" />
                <asp:Button runat="server" ID="btnConRoomCost" CssClass="CommandButton" Visible='<%#HasPriv(ClientPrivilege.Administrator | ClientPrivilege.Executive)%>' Text="Configure Room Charges" ToolTip="Used to define the cost model and values for each of the access controlled room within the SSEL" OnCommand="Button_Command" CommandName="navigate" CommandArgument="~/ConCost.aspx?ItemType=Room" />
                <asp:Button runat="server" ID="btnConStoreCost" CssClass="CommandButton" Visible='<%#HasPriv(ClientPrivilege.Administrator | ClientPrivilege.Executive)%>' Text="Configure Store Charges" ToolTip="Used to define the cost parameters for the SSEL store" OnCommand="Button_Command" CommandName="navigate" CommandArgument="~/ConCost.aspx?ItemType=Store" />
                <asp:Button runat="server" ID="btnConToolCost" CssClass="CommandButton" Visible='<%#HasPriv(ClientPrivilege.Administrator | ClientPrivilege.Executive)%>' Text="Configure Tool Charges" ToolTip="Used to define the cost model and values for each of the reservable tools within the SSEL" OnCommand="Button_Command" CommandName="navigate" CommandArgument="~/ConCost.aspx?ItemType=Tool" />
                <asp:Button runat="server" ID="btnConFormula" CssClass="CommandButton" Visible='<%#HasPriv(ClientPrivilege.Administrator)%>' Text="Configure Costing Formulas" ToolTip="Used to define the formulas for calculating costs" OnCommand="Button_Command" CommandName="navigate" CommandArgument="~/ConFormula.aspx" />
                <asp:Button runat="server" ID="btnConRemoteProcessing" CssClass="CommandButton" Visible='<%#HasPriv(ClientPrivilege.Administrator)%>' Text="Configure Remote Processing" ToolTip="Remote Processing users and accouts mapping" OnCommand="Button_Command" CommandName="navigate" CommandArgument="~/ConRemoteProcessing.aspx" />
                <asp:Button runat="server" ID="btnConOrgRecharge" CssClass="CommandButton" Visible='<%#HasPriv(ClientPrivilege.Administrator)%>' Text="Configure Recharge Accounts" ToolTip="Link an organization to a particular recharge account" OnCommand="Button_Command" CommandName="navigate" CommandArgument="~/ConOrgRecharge.aspx" />
            </div>
            <div runat="server" visible='<%#HasPriv(ClientPrivilege.Administrator | ClientPrivilege.Executive)%>' class="col">
                <div runat="server" id="divDat" visible='<%#HasPriv(ClientPrivilege.Administrator)%>' style="margin-bottom: 29px;">
                    <div class="ButtonGroupHeader">
                        <asp:Label runat="server" ID="lblDat">Summaries</asp:Label>
                    </div>
                    <asp:Button runat="server" ID="btnDatHistorical" CssClass="CommandButton" Visible='<%#HasPriv(ClientPrivilege.Administrator)%>' Text="Historical Reports" ToolTip="Generates a high level report by organization showing total lab time, tool usage and store purchases" OnCommand="Button_Command" CommandName="navigate" CommandArgument="~/DatHistorical.aspx" />
                    <asp:Button runat="server" ID="btnDatSubsidyTiers" CssClass="CommandButton" Visible='<%#HasPriv(ClientPrivilege.Administrator)%>' Text="Subsidy Tiers Report" ToolTip="View the tiers used to compute subsidy amounts in a given period." OnCommand="Button_Command" CommandName="navigate" CommandArgument="~/DatSubsidyTiers.aspx" />
                    <asp:Button runat="server" ID="btnDatCurrentCosts" CssClass="CommandButton" Visible="false" Text="Current Costs Report" ToolTip="Generate current Costs PDF files" OnCommand="Button_Command" CommandName="navigate" CommandArgument="~/DatCurrentCosts.aspx" />
                </div>
                <div runat="server" id="divMisc" visible='<%#HasPriv(ClientPrivilege.Administrator | ClientPrivilege.Executive)%>'>
                    <div class="ButtonGroupHeader">
                        <asp:Label runat="server" ID="lblMsc" CssClass="ButtonGroupHeader">Other</asp:Label>
                    </div>
                    <asp:Button runat="server" ID="btnMscInternalSpecial" CssClass="CommandButton" Visible='<%#HasPriv(ClientPrivilege.Administrator | ClientPrivilege.Executive)%>' Text="Per Usage Reports" ToolTip="Calculate the total amount to bill for special cases" OnCommand="Button_Command" CommandName="navigate" CommandArgument="~/MscInternalSpecial.aspx" />
                    <asp:Button runat="server" ID="btnMscExp" CssClass="CommandButton" Visible='<%#HasPriv(ClientPrivilege.Administrator)%>' Text="Experimental Cost Config" ToolTip="For generating income results based on experimental cost values" OnCommand="Button_Command" CommandName="navigate" CommandArgument="~/MscExp.aspx" />
                    <asp:Button runat="server" ID="btnMscExp2" CssClass="CommandButton" Visible='<%#HasPriv(ClientPrivilege.Administrator)%>' Text="Experimental Cost Config 2" OnCommand="Button_Command" CommandName="navigate" CommandArgument="~/MscExp2.aspx" />
                    <asp:Button runat="server" ID="btnMscBillingChecks" CssClass="CommandButton" Visible="false" Text="Billing Checks" ToolTip="Used to check if billing data is accurate" OnCommand="Button_Command" CommandName="navigate" CommandArgument="~/MscBillingChecks.aspx" />
                    <asp:Button runat="server" ID="btnMscBillingImportUtility" CssClass="CommandButton" Visible='<%#HasPriv(ClientPrivilege.Administrator)%>' Text="Billing Import Utility" OnCommand="Button_Command" CommandName="navigate" CommandArgument="~/MscBillingImportUtility.aspx" />
                </div>
            </div>
        </div>

        <div class="container">
            <div runat="server" visible='<%#HasPriv(0)%>' class="col">
                <div class="ButtonGroupHeader">
                    <asp:Label runat="server">Application Control</asp:Label>
                </div>
                <asp:Button ID="btnLogout" runat="server" Width="209" CssClass="CommandButton" Text="Exit Application" OnClick="BtnLogout_Click" />
            </div>
        </div>
    </div>
</asp:Content>

<asp:Content ID="Content3" ContentPlaceHolderID="scripts" runat="server">
</asp:Content>
