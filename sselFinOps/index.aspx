<%@ Page Title="FinOps Main Page" Language="C#" MasterPageFile="~/FinOpsMaster.Master" AutoEventWireup="true" CodeBehind="index.aspx.cs" Inherits="sselFinOps.index" %>

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
                    <asp:Label ID="lblRep" runat="server" Height="24" Width="200" CssClass="ButtonGroupHeader" Visible="false">Financial Reports</asp:Label>
                </td>
                <td style="text-align: center;">
                    <asp:Label ID="lblCon" runat="server" Height="24" Width="200" CssClass="ButtonGroupHeader" Visible="false">Configuration</asp:Label>
                </td>
                <td style="text-align: center;">
                    <asp:Label ID="lblDat" runat="server" CssClass="ButtonGroupHeader" Height="24" Width="200" Visible="false">Summaries</asp:Label>
                </td>
            </tr>
            <tr>
                <td>
                    <asp:Button ID="btnRepMiscCharge" runat="server" CssClass="CommandButton" Visible="false" Text="Misc. Charges" OnClick="FinOpsButton_Click" />
                </td>
                <td>
                    <asp:Button ID="btnConHolidays" runat="server" CssClass="CommandButton" Visible="false" Text="Define Holidays" ToolTip="Specify official University and SSEL holidays" OnClick="FinOpsButton_Click" />
                </td>
                <td>
                    <asp:Button ID="btnDatHistorical" runat="server" CssClass="CommandButton" Visible="false" Text="Historical Reports" ToolTip="Generates a high level report by organization showing total lab time, tool usage and store purchases" OnClick="FinOpsButton_Click" />
                </td>
            </tr>
            <tr>
                <td>
                    <asp:Button ID="btnRepSUB" runat="server" CssClass="CommandButton" Visible="false" Text="SUB Reports" ToolTip="All SUB Reports" OnClick="FinOpsButton_Click" />
                    <asp:Button ID="btnRepOther" runat="server" CssClass="CommandButton" Visible="false" Text="External Misc. Charges" ToolTip="Used to enter other charges incurred by non-UM organizations" OnClick="FinOpsButton_Click" />
                </td>
                <td>
                    <asp:Button ID="btnConGlobal" runat="server" CssClass="CommandButton" Visible="false" Text="Configure Global Parameters" ToolTip="Specify monthly/yearly cost caps, etc" OnClick="FinOpsButton_Click" />
                </td>
                <td>
                    <asp:Button runat="server" ID="btnDatSubsidyTiers" CssClass="CommandButton" Visible="false" Text="Subsidy Tiers Report" ToolTip="View the tiers used to compute subsidy amounts in a given period." OnClick="FinOpsButton_Click" />
                </td>
            </tr>
            <tr>
                <td>
                    <asp:Button runat="server" ID="btnRepInvoice" CssClass="CommandButton" Visible="false" Text="External Org Invoice" ToolTip="Creates a UM Invoice spreadsheet for external (non-UMich) organizations" OnClick="FinOpsButton_Click" />
                </td>
                <td>
                    <asp:Button runat="server" ID="btnConAuxCost" CssClass="CommandButton" Text="Define Aux Cost Parameters" Visible="false" ToolTip="Create auxilliary costing parameters, such as caps, penalties, etc." OnClick="FinOpsButton_Click" />
                </td>
                <td>
                    <asp:Button runat="server" ID="btnDatCurrentCosts" CssClass="CommandButton" Text="Current Costs Report" Visible="false" ToolTip="Generate current Costs PDF files" OnClick="FinOpsButton_Click" />
                </td>
            </tr>
            <tr>
                <td>
                    <asp:Button ID="btnRepLabTimeJE" runat="server" CssClass="CommandButton" Visible="false" Text="Labtime JE" ToolTip="Creates a UM Journal Entry spreadsheet for internal user time-in-lab usage" OnClick="FinOpsButton_Click" />
                    <asp:Button ID="btnMscToolBillingReport" runat="server" CssClass="CommandButton" Visible="false" Text="Tool Billing Report" ToolTip="Display tool billing data by reservation" OnClick="FinOpsButton_Click" />
                </td>
                <td>
                    <asp:Button ID="btnConRoomCost" runat="server" CssClass="CommandButton" Visible="false" Text="Configure Room Charges" CommandArgument="?ItemType=Room" ToolTip="Used to define the cost model and values for each of the access controlled room within the SSEL" OnClick="FinOpsButton_Click" />
                </td>
                <td style="text-align: center;">
                    <asp:Label ID="lblMsc" runat="server" CssClass="ButtonGroupHeader" Width="200" Height="24" Visible="false">Other</asp:Label>
                </td>
            </tr>
            <tr>
                <td>
                    <asp:Button ID="btnRepToolUseJE" runat="server" CssClass="CommandButton" Visible="false" Text="Tool Use JE" ToolTip="Creates a UM Journal Entry spreadsheet for internal user tool usage" OnClick="FinOpsButton_Click" />
                    <a href="/reports/dispatch/manager-usage-summary?returnTo=/sselfinops" class="command-button">Manager Usage Summary</a>
                </td>
                <td>
                    <asp:Button ID="btnConStoreCost" runat="server" CssClass="CommandButton" Text="Configure Store Charges" Visible="false" CommandArgument="?ItemType=Store" ToolTip="Used to define the cost parameters for the SSEL store" OnClick="FinOpsButton_Click" />
                </td>
                <td>
                    <asp:Button ID="btnMscInternalSpecial" runat="server" CssClass="CommandButton" Visible="false" Text="Per Usage Reports" ToolTip="Calculate the total amount to bill for special cases" OnClick="FinOpsButton_Click" />
                </td>
            </tr>
            <tr>
                <td>
                    <asp:Button ID="btnRepSUBStore" runat="server" CssClass="CommandButton" Visible="false" Text="Old SUB (2007 June)" ToolTip="All SUB Reports Before July 2009" OnClick="FinOpsButton_Click" />
                </td>
                <td>
                    <asp:Button ID="btnConToolCost" runat="server" CssClass="CommandButton" Text="Configure Tool Charges" Visible="false" CommandArgument="?ItemType=Tool" ToolTip="Used to define the cost model and values for each of the reservable tools within the SSEL" OnClick="FinOpsButton_Click" />
                </td>
                <td>
                    <asp:Button ID="btnMscExp" runat="server" CssClass="CommandButton" Visible="false" Text="Experimental Cost Config" ToolTip="For generating income results based on experimental cost values" OnClick="FinOpsButton_Click" />
                </td>
            </tr>
            <tr>
                <td>
                    <asp:Button runat="server" ID="btnRepMaterialsJE" CssClass="CommandButton" Visible="false" Text="Materials JE" ToolTip="Creates a UM Journal Entry spreadsheet for items purchased form the on-line store" OnClick="FinOpsButton_Click" />
                </td>
                <td>
                    <asp:Button runat="server" ID="btnConFormula" CssClass="CommandButton" Visible="false" Text="Configure Costing Formulas" ToolTip="Used to define the formulas for calculating costs" OnClick="FinOpsButton_Click" />
                </td>
                <td>
                    <asp:Button ID="btnMscExp2" runat="server" CssClass="CommandButton" Visible="false" Text="Experimental Cost Config 2" OnClick="FinOpsButton_Click" />
                </td>
            </tr>
            <tr>
                <td>&nbsp;</td>
                <td>
                    <asp:Button ID="btnConRemoteProcessing" runat="server" CssClass="CommandButton" Visible="false" Text="Configure Remote Processing" ToolTip="Remote Processing users and accouts mapping" OnClick="FinOpsButton_Click" />
                </td>
                <td>
                    <asp:Button runat="server" ID="btnMscBillingChecks" CssClass="CommandButton" Visible="false" Text="Billing Checks" ToolTip="Used to check if billing data is accurate" OnClick="FinOpsButton_Click" />
                </td>
            </tr>
            <tr>
                <td>&nbsp;</td>
                <td>
                    <asp:Button ID="btnConOrgRecharge" runat="server" CssClass="CommandButton" Visible="false" Text="Configure Recharge Accounts" ToolTip="Link an organization to a particular recharge account" OnClick="FinOpsButton_Click" />
                </td>
                <td>
                    <a href="/reports/dispatch/durations-report?returnTo=/sselfinops" class="command-button">Durations Report</a>
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
                    <asp:Button ID="btnLogout" runat="server" Width="209" CssClass="CommandButton" Text="Exit Application" OnClick="btnLogout_Click" />
                </td>
                <td>&nbsp;</td>
                <td>&nbsp;</td>
            </tr>
        </table>
    </div>
</asp:Content>

<asp:Content ID="Content3" ContentPlaceHolderID="scripts" runat="server">
</asp:Content>
