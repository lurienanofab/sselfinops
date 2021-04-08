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
    </style>
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    <div class="section">
        <table class="button-table">
            <tr>
                <td colspan="3">
                    <span class="PageHeader">LNF Financial Operations</span>
                    <div style="padding: 10px 0 10px 0; font-weight: bold;">
                        Current user:
                        <asp:Label runat="server" ID="lblName"></asp:Label>
                    </div>
                </td>
            </tr>
            <tr>
                <td style="text-align: center;">
                    <asp:Label ID="lblRep" runat="server" Height="24" Width="200" CssClass="ButtonGroupHeader" Visible='<%#HasPriv(ClientPrivilege.Administrator | ClientPrivilege.Executive)%>'>Financial Reports</asp:Label>
                </td>
                <td style="text-align: center;">
                    <asp:Label ID="lblCon" runat="server" Height="24" Width="200" CssClass="ButtonGroupHeader" Visible='<%#HasPriv(ClientPrivilege.Administrator | ClientPrivilege.Executive)%>'>Configuration</asp:Label>
                </td>
                <td style="text-align: center;">
                    <asp:Label ID="lblDat" runat="server" Height="24" Width="200" CssClass="ButtonGroupHeader" Visible='<%#HasPriv(ClientPrivilege.Administrator)%>'>Summaries</asp:Label>
                </td>
            </tr>
            <tr>
                <td>
                    <asp:Button ID="btnRepMiscCharge" runat="server" CssClass="CommandButton" Visible='<%#HasPriv(ClientPrivilege.Administrator)%>' Text="Misc. Charges" OnCommand="Button_Command" CommandName="navigate" CommandArgument="~/RepMiscCharge.aspx" />
                </td>
                <td>
                    <asp:Button ID="btnConHolidays" runat="server" CssClass="CommandButton" Visible='<%#HasPriv(ClientPrivilege.Administrator)%>' Text="Define Holidays" ToolTip="Specify official University and SSEL holidays" OnCommand="Button_Command" CommandName="navigate" CommandArgument="~/HolidaysConfiguration.aspx" />
                </td>
                <td>
                    <asp:Button ID="btnDatHistorical" runat="server" CssClass="CommandButton" Visible='<%#HasPriv(ClientPrivilege.Administrator)%>' Text="Historical Reports" ToolTip="Generates a high level report by organization showing total lab time, tool usage and store purchases" OnCommand="Button_Command" CommandName="navigate" CommandArgument="~/DatHistorical.aspx" />
                </td>
            </tr>
            <tr>
                <td>
                    <asp:Button ID="btnRepSUB" runat="server" CssClass="CommandButton" Visible='<%#HasPriv(ClientPrivilege.Administrator | ClientPrivilege.Executive)%>' Text="SUB Reports" ToolTip="All SUB Reports" OnCommand="Button_Command" CommandName="navigate" CommandArgument="~/RepSUB.aspx" />
                    <asp:Button ID="btnRepOther" runat="server" CssClass="CommandButton" Visible="false" Text="External Misc. Charges" ToolTip="Used to enter other charges incurred by non-UM organizations" OnCommand="Button_Command" CommandName="navigate" CommandArgument="~/RepOther.aspx" />
                </td>
                <td>
                    <asp:Button ID="btnConGlobal" runat="server" CssClass="CommandButton" Visible='<%#HasPriv(ClientPrivilege.Administrator)%>' Text="Configure Global Parameters" ToolTip="Specify monthly/yearly cost caps, etc" OnCommand="Button_Command" CommandName="navigate" CommandArgument="~/ConGlobal.aspx" />
                </td>
                <td>
                    <asp:Button runat="server" ID="btnDatSubsidyTiers" CssClass="CommandButton" Visible='<%#HasPriv(ClientPrivilege.Administrator)%>' Text="Subsidy Tiers Report" ToolTip="View the tiers used to compute subsidy amounts in a given period." OnCommand="Button_Command" CommandName="navigate" CommandArgument="~/DatSubsidyTiers.aspx" />
                </td>
            </tr>
            <tr>
                <td>
                    <asp:Button runat="server" ID="btnRepInvoice" CssClass="CommandButton" Visible='<%#HasPriv(ClientPrivilege.Executive | ClientPrivilege.Administrator)%>' Text="External Org Invoice" ToolTip="Creates a UM Invoice spreadsheet for external (non-UMich) organizations" OnCommand="Button_Command" CommandName="navigate" CommandArgument="~/RepInvoice.aspx" />
                </td>
                <td>
                    <asp:Button runat="server" ID="btnAuxCostConfiguration" CssClass="CommandButton" Visible='<%#HasPriv(ClientPrivilege.Administrator)%>' Text="Define Aux Cost Parameters" ToolTip="Create auxilliary costing parameters, such as caps, penalties, etc." OnCommand="Button_Command" CommandName="navigate" CommandArgument="~/AuxCostConfiguration.aspx" />
                </td>
                <td>
                    <asp:Button runat="server" ID="btnDatCurrentCosts" CssClass="CommandButton" Visible="false" Text="Current Costs Report" ToolTip="Generate current Costs PDF files" OnCommand="Button_Command" CommandName="navigate" CommandArgument="~/DatCurrentCosts.aspx" />
                </td>
            </tr>
            <tr>
                <td>
                    <asp:Button runat="server" ID="btnReportToolBilling" CssClass="CommandButton" Visible='<%#HasPriv(ClientPrivilege.Administrator)%>' Text="Tool Billing Report" ToolTip="Display tool billing data by reservation" OnCommand="Button_Command" CommandName="navigate" CommandArgument="~/ReportToolBilling.aspx" />
                    <asp:Button runat="server" ID="btnRepLabTimeJE" CssClass="CommandButton" Visible="false" Text="Labtime JE" ToolTip="Creates a UM Journal Entry spreadsheet for internal user time-in-lab usage" OnCommand="Button_Command" CommandName="navigate" CommandArgument="~/RepLabTimeJE.aspx" />
                </td>
                <td>
                    <asp:Button runat="server" ID="btnConRoomCost" CssClass="CommandButton" Visible='<%#HasPriv(ClientPrivilege.Administrator | ClientPrivilege.Executive)%>' Text="Configure Room Charges" ToolTip="Used to define the cost model and values for each of the access controlled room within the SSEL" OnCommand="Button_Command" CommandName="navigate" CommandArgument="~/ConCost.aspx?ItemType=Room" />
                </td>
                <td style="text-align: center;">
                    <asp:Label runat="server" ID="lblMsc" CssClass="ButtonGroupHeader" Width="200" Height="24" Visible='<%#HasPriv(ClientPrivilege.Administrator | ClientPrivilege.Executive)%>'>Other</asp:Label>
                </td>
            </tr>
            <tr>
                <td>
                    <asp:Button runat="server" ID="btnRepToolUseJE" CssClass="CommandButton" Visible="false" Text="Tool Use JE" ToolTip="Creates a UM Journal Entry spreadsheet for internal user tool usage" OnCommand="Button_Command" CommandName="navigate" CommandArgument="~/RepToolUseJE.aspx" />
                    <asp:Button runat="server" ID="btnManagerUsageSummary" CssClass="CommandButton" Visible='<%#HasPriv(ClientPrivilege.Administrator | ClientPrivilege.Executive)%>' Text="Manager Usage Summary" ToolTip="Display usage by user and account for a manager." OnCommand="Button_Command" CommandName="navigate" CommandArgument="~/ManagerUsageSummary.aspx" />
                </td>
                <td>
                    <asp:Button runat="server" ID="btnConStoreCost" CssClass="CommandButton" Visible='<%#HasPriv(ClientPrivilege.Administrator | ClientPrivilege.Executive)%>' Text="Configure Store Charges" ToolTip="Used to define the cost parameters for the SSEL store" OnCommand="Button_Command" CommandName="navigate" CommandArgument="~/ConCost.aspx?ItemType=Store" />
                </td>
                <td>
                    <asp:Button runat="server" ID="btnMscInternalSpecial" CssClass="CommandButton" Visible='<%#HasPriv(ClientPrivilege.Administrator | ClientPrivilege.Executive)%>' Text="Per Usage Reports" ToolTip="Calculate the total amount to bill for special cases" OnCommand="Button_Command" CommandName="navigate" CommandArgument="~/MscInternalSpecial.aspx" />
                </td>
            </tr>
            <tr>
                <td>
                    <asp:Button runat="server" ID="btnReportFinancialManager" CssClass="CommandButton" Visible='<%#HasPriv(ClientPrivilege.Administrator)%>' Text="Financial Manager Report" ToolTip="Display tool billing data by reservation" OnCommand="Button_Command" CommandName="navigate" CommandArgument="~/ReportFinancialManager.aspx" />
                    <asp:Button runat="server" ID="btnRepSUBStore" CssClass="CommandButton" Visible="false" Text="Old SUB (2007 June)" ToolTip="All SUB Reports Before July 2009" OnCommand="Button_Command" CommandName="navigate" CommandArgument="~/RepSUBStore.aspx" />
                </td>
                <td>
                    <asp:Button runat="server" ID="btnConToolCost" CssClass="CommandButton" Visible='<%#HasPriv(ClientPrivilege.Administrator | ClientPrivilege.Executive)%>' Text="Configure Tool Charges" ToolTip="Used to define the cost model and values for each of the reservable tools within the SSEL" OnCommand="Button_Command" CommandName="navigate" CommandArgument="~/ConCost.aspx?ItemType=Tool" />
                </td>
                <td>
                    <asp:Button runat="server" ID="btnMscExp" CssClass="CommandButton" Visible='<%#HasPriv(ClientPrivilege.Administrator)%>' Text="Experimental Cost Config" ToolTip="For generating income results based on experimental cost values" OnCommand="Button_Command" CommandName="navigate" CommandArgument="~/MscExp.aspx" />
                </td>
            </tr>
            <tr>
                <td>
                    <asp:Button runat="server" ID="btnRepMaterialsJE" CssClass="CommandButton" Visible="false" Text="Materials JE" ToolTip="Creates a UM Journal Entry spreadsheet for items purchased form the on-line store" OnCommand="Button_Command" CommandName="navigate" CommandArgument="~/RepMaterialsJE.aspx" />
                </td>
                <td>
                    <asp:Button runat="server" ID="btnConFormula" CssClass="CommandButton" Visible='<%#HasPriv(ClientPrivilege.Administrator)%>' Text="Configure Costing Formulas" ToolTip="Used to define the formulas for calculating costs" OnCommand="Button_Command" CommandName="navigate" CommandArgument="~/ConFormula.aspx" />
                </td>
                <td>
                    <asp:Button runat="server" ID="btnMscExp2" CssClass="CommandButton" Visible='<%#HasPriv(ClientPrivilege.Administrator)%>' Text="Experimental Cost Config 2" OnCommand="Button_Command" CommandName="navigate" CommandArgument="~/MscExp2.aspx" />
                </td>
            </tr>
            <tr>
                <td>&nbsp;</td>
                <td>
                    <asp:Button runat="server" ID="btnConRemoteProcessing" CssClass="CommandButton" Visible='<%#HasPriv(ClientPrivilege.Administrator)%>' Text="Configure Remote Processing" ToolTip="Remote Processing users and accouts mapping" OnCommand="Button_Command" CommandName="navigate" CommandArgument="~/ConRemoteProcessing.aspx" />
                </td>
                <td>
                    <asp:Button runat="server" ID="btnMscBillingChecks" CssClass="CommandButton" Visible='<%#HasPriv(ClientPrivilege.Administrator)%>' Text="Billing Checks" ToolTip="Used to check if billing data is accurate" OnCommand="Button_Command" CommandName="navigate" CommandArgument="~/MscBillingChecks.aspx" />
                </td>
            </tr>
            <tr>
                <td>&nbsp;</td>
                <td>
                    <asp:Button runat="server" ID="btnConOrgRecharge" CssClass="CommandButton" Visible='<%#HasPriv(ClientPrivilege.Administrator)%>' Text="Configure Recharge Accounts" ToolTip="Link an organization to a particular recharge account" OnCommand="Button_Command" CommandName="navigate" CommandArgument="~/ConOrgRecharge.aspx" />
                </td>
                <td>
                    <asp:Button runat="server" ID="btnDurationReport" CssClass="CommandButton" Visible='<%#HasPriv(ClientPrivilege.Administrator)%>' Text="Durations Report" ToolTip="Displays detailed billing information about reservation durations." OnCommand="Button_Command" CommandName="navigate" CommandArgument="~/DurationsReport.aspx" />
                </td>
            </tr>
            <tr>
                <td>&nbsp;</td>
                <td>&nbsp;</td>
                <td>&nbsp;</td>
            </tr>
            <tr>
                <td style="text-align: center;">
                    <asp:Label ID="Label2" runat="server" Height="24" Width="200" CssClass="ButtonGroupHeader">Application Control</asp:Label>
                </td>
                <td>&nbsp;</td>
                <td>&nbsp;</td>
            </tr>
            <tr>
                <td>
                    <asp:Button ID="btnLogout" runat="server" Width="209" CssClass="CommandButton" Text="Exit Application" OnClick="BtnLogout_Click" />
                </td>
                <td>&nbsp;</td>
                <td>&nbsp;</td>
            </tr>
        </table>
    </div>
</asp:Content>

<asp:Content ID="Content3" ContentPlaceHolderID="scripts" runat="server">
</asp:Content>
