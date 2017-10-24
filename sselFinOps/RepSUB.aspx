<%@ Page Title="SUB Reports" Language="C#" MasterPageFile="~/FinOpsMaster.Master" AutoEventWireup="true" CodeBehind="RepSUB.aspx.cs" Inherits="sselFinOps.RepSUB" Async="true" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <style>
        .controls {
            border-collapse: collapse;
        }

            .controls td {
                padding: 10px 5px 5px 5px;
            }

            .controls td {
            }

        .sub-report {
            border-collapse: collapse;
        }

            .sub-report td, .sub-report th {
                padding: 3px;
            }
    </style>
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    <div class="section">
        <h2>Service Unit Billing - All Reports</h2>
        <div class="criteria">
            <table class="criteria-table">
                <tr>
                    <td>Start period:</td>
                    <td>
                        <lnf:PeriodPicker runat="server" ID="ppStart" StartPeriod="2009-01-01" />
                    </td>
                </tr>
                <tr>
                    <td>End period:</td>
                    <td>
                        <lnf:PeriodPicker runat="server" ID="ppEnd" StartPeriod="2009-01-01" />
                    </td>
                </tr>
            </table>
        </div>
    </div>
    <div class="section" style="border: solid 1px #AAAAAA;">
        <table class="controls">
            <tr>
                <td colspan="5">
                    <asp:CheckBox runat="server" ID="chkHTML" Text="HTML ONLY" OnCheckedChanged="chkHTML_CheckChanged" AutoPostBack="true" />
                </td>
            </tr>
            <tr>
                <td>
                    <asp:Button ID="btnReportLabtime" runat="server" Text="Labtime Report" Width="180" OnClick="btnReportLabtime_Click" />
                </td>
                <td>
                    <asp:Button ID="btnRoomJUA" runat="server" Text="JU A" CommandArgument="A" Width="65" OnCommand="btnRoomJU_Command" />
                </td>
                <td>
                    <asp:Button ID="btnRoomJUB" runat="server" Text="JU B" CommandArgument="B" Width="65" OnCommand="btnRoomJU_Command" />
                </td>
                <td>
                    <asp:Button ID="btnRoomJUC" runat="server" Text="JU C" CommandArgument="C" Width="65" OnCommand="btnRoomJU_Command" />
                </td>
                <td>
                    <asp:Button ID="btnAllRoomJU" runat="server" Text="All Room JU" OnClick="btnAllRoomJU_Click" Width="100" Enabled="false" />
                </td>
            </tr>
            <tr>
                <td>
                    <asp:Button ID="btnReportTool" runat="server" Text="Tool Report" Width="180" OnClick="btnReportTool_Click" />
                </td>
                <td>
                    <asp:Button ID="btnToolJUA" runat="server" Text="JU A" CommandArgument="A" Width="65" OnCommand="btnToolJU_Command" />
                </td>
                <td>
                    <asp:Button ID="btnToolJUB" runat="server" Text="JU B" CommandArgument="B" Width="65" OnCommand="btnToolJU_Command" />
                </td>
                <td>
                    <asp:Button ID="btnToolJUC" runat="server" Text="JU C" CommandArgument="C" Width="65" OnCommand="btnToolJU_Command" />
                </td>
                <td>
                    <asp:Button ID="btnAllToolJU" runat="server" Text="All Tool JU" OnClick="btnAllToolJU_Click" Width="100" Enabled="false" />
                </td>
            </tr>
            <tr>
                <td>
                    <asp:Button ID="btnReportStore" runat="server" Text="Store Report" Width="180" OnClick="btnReportStore_Click" />
                </td>
                <td colspan="4">&nbsp;</td>
            </tr>
            <tr>
                <td>
                    <asp:Button ID="btnAllSUB" runat="server" Text="All SUB" Width="180" OnClick="btnAllSUB_Click" Enabled="false" />
                </td>
                <td>
                    <asp:Button ID="btnAllJUA" runat="server" Text="All JU A" CommandArgument="A" OnCommand="btnAllJU_Command" Width="65" Enabled="false" />
                </td>
                <td>
                    <asp:Button ID="btnAllJUB" runat="server" Text="All JU B" CommandArgument="B" OnCommand="btnAllJU_Command" Width="65" Enabled="false" />
                </td>
                <td>
                    <asp:Button ID="btnAllJUC" runat="server" Text="All JU C" CommandArgument="C" OnCommand="btnAllJU_Command" Width="65" Enabled="false" />
                </td>
                <td>
                    <asp:Button ID="btnAllJU" runat="server" Text="All JU" CommandArgument="all" OnCommand="btnAllJU_Command" Width="100" Enabled="false" />
                </td>
            </tr>
            <tr>
                <td colspan="5" style="padding-top: 20px;">
                    <asp:Button ID="btnReportStoreTwoAccounts" runat="server" Text="Deprecated:Store Report (two credit accounts)" OnClick="btnReportStoreTwoAccounts_Click" />
                </td>
            </tr>
            <tr>
                <td colspan="5" style="padding-top: 20px;">
                    <asp:HyperLink ID="hypBack" runat="server" NavigateUrl="~" Text="&larr; Back to Main Page "></asp:HyperLink>
                </td>
            </tr>
        </table>
    </div>
    <div class="section">
        <asp:Literal runat="server" ID="litError"></asp:Literal>
        <div style="padding-bottom: 5px;">
            <div style="float: left;">
                <asp:Label ID="lblTotal" runat="server"></asp:Label>
            </div>
            <div style="float: left; padding-left: 10px;">
                <asp:Literal runat="server" ID="litLink"></asp:Literal>
            </div>
            <div style="clear: both;">
            </div>
        </div>
        <asp:GridView ID="gvSUB" runat="server" AutoGenerateColumns="false" CssClass="sub-report gridview highlight" GridLines="None">
            <HeaderStyle CssClass="header" />
            <RowStyle CssClass="row" />
            <AlternatingRowStyle CssClass="altrow" />
            <Columns>
                <asp:TemplateField Visible="false">
                    <ItemTemplate>
                        <%# Container.DataItemIndex + 1%>
                    </ItemTemplate>
                </asp:TemplateField>
                <asp:TemplateField HeaderText="Type" Visible="false">
                    <ItemTemplate>
                        <%# Eval("ChargeType")%>
                    </ItemTemplate>
                </asp:TemplateField>
                <asp:TemplateField HeaderText="Period" Visible="false">
                    <ItemTemplate>
                        <%#string.Format("{0}", Eval("Period").Equals(DateTime.MinValue) ? string.Empty : Convert.ToDateTime(Eval("Period")).ToString("MMM-yyyy"))%>
                    </ItemTemplate>
                </asp:TemplateField>
                <asp:BoundField DataField="ItemDescription" HeaderText="User" Visible="true" />
                <asp:BoundField DataField="ShortCode" HeaderText="ShortCode" Visible="true" />
                <asp:TemplateField HeaderText="Usage Charge">
                    <ItemStyle HorizontalAlign="Right" />
                    <ItemTemplate>
                        <%#string.Format("{0}", string.IsNullOrEmpty(Eval("UsageCharge").ToString()) ? string.Empty : Convert.ToDouble(Eval("UsageCharge")).ToString("0.00"))%>
                    </ItemTemplate>
                </asp:TemplateField>
                <asp:TemplateField HeaderText="Subsidy Amount">
                    <ItemStyle HorizontalAlign="Right" />
                    <ItemTemplate>
                        <%#string.Format("{0}", string.IsNullOrEmpty(Eval("SubsidyDiscount").ToString()) ? string.Empty : Convert.ToDouble(Eval("SubsidyDiscount")).ToString("0.00"))%>
                    </ItemTemplate>
                </asp:TemplateField>
                <asp:TemplateField HeaderText="Billed Charge">
                    <ItemStyle HorizontalAlign="Right" />
                    <ItemTemplate>
                        <%#string.Format("{0}", string.IsNullOrEmpty(Eval("BilledCharge").ToString()) ? string.Empty : Convert.ToDouble(Eval("BilledCharge")).ToString("0.00"))%>
                    </ItemTemplate>
                </asp:TemplateField>
            </Columns>
        </asp:GridView>
        <asp:GridView runat="server" ID="gvJU" AutoGenerateColumns="false" CssClass="gridview highlight" GridLines="None">
            <HeaderStyle CssClass="header" />
            <RowStyle CssClass="row" />
            <AlternatingRowStyle CssClass="altrow" />
            <Columns>
                <asp:TemplateField Visible="false">
                    <ItemTemplate>
                        <%#Container.DataItemIndex + 1%>
                    </ItemTemplate>
                </asp:TemplateField>
                <asp:TemplateField HeaderText="Type" Visible="false">
                    <ItemTemplate>
                        <%#Eval("ChargeType")%>
                    </ItemTemplate>
                </asp:TemplateField>
                <asp:TemplateField HeaderText="JU" Visible="false" ItemStyle-HorizontalAlign="Center">
                    <ItemTemplate>
                        <%#Eval("JournalUnitType")%>
                    </ItemTemplate>
                </asp:TemplateField>
                <asp:TemplateField HeaderText="Period" Visible="false">
                    <ItemTemplate>
                        <%#string.Format("{0}", Eval("Period").Equals(DateTime.MinValue) ? string.Empty : Convert.ToDateTime(Eval("Period")).ToString("MMM-yyyy"))%>
                    </ItemTemplate>
                </asp:TemplateField>
                <asp:BoundField DataField="Account" HeaderText="Account" />
                <asp:BoundField DataField="FundCode" HeaderText="Fund" />
                <asp:BoundField DataField="DeptID" HeaderText="Department" />
                <asp:BoundField DataField="ProgramCode" HeaderText="Program" />
                <asp:BoundField DataField="Class" HeaderText="Class" />
                <asp:BoundField DataField="ProjectGrant" HeaderText="Project" />
                <asp:BoundField DataField="MerchandiseAmount" HeaderText="Monetary Amount" ItemStyle-HorizontalAlign="Right" />
                <asp:BoundField DataField="DepartmentalReferenceNumber" HeaderText="JournalLine" />
                <asp:BoundField DataField="ItemDescription" HeaderText="Line Description" />
            </Columns>
        </asp:GridView>
    </div>
</asp:Content>

<asp:Content ID="Content3" ContentPlaceHolderID="scripts" runat="server">
</asp:Content>
