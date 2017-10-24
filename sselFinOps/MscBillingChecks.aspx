<%@ Page Title="" Language="C#" MasterPageFile="~/FinOpsMaster.Master" AutoEventWireup="true" CodeBehind="MscBillingChecks.aspx.cs" Inherits="sselFinOps.MscBillingChecks" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <style type="text/css">
        .grid {
            border-collapse: separate;
            border-spacing: 2px;
        }

            .grid th,
            .grid td {
                padding: 3px;
                border: solid 1px #CCCCCC;
            }

            .grid th {
                background-color: #DFDFDF;
            }

        .section-title {
            padding: 2px;
            background-color: #DDDDFF;
            border: solid 1px #CCCCCC;
            font-weight: bold;
            color: #003366;
        }

        .section-content {
            padding: 5px;
        }
    </style>
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    <div class="section">
        <h2>Billing Checks</h2>
        <div class="criteria">
            <table class="criteria-table">
                <tr>
                    <td>Select period:</td>
                    <td>
                        <lnf:PeriodPicker runat="server" ID="pp1" />
                    </td>
                </tr>
                <tr>
                    <td>Select client:</td>
                    <td>
                        <asp:DropDownList runat="server" ID="ddlClient" DataTextField="DisplayName" DataValueField="ClientID" CssClass="report-select" AppendDataBoundItems="true">
                            <asp:ListItem Value="0">-- Select --</asp:ListItem>
                        </asp:DropDownList>
                    </td>
                </tr>
            </table>
            <div class="criteria-item">
                <asp:Button runat="server" ID="btnReport" Text="Retrieve Data" OnClick="btnReport_Click" CssClass="report-button" />
                <asp:LinkButton runat="server" ID="btnBack" Text="&laquo; Back to Main Page" OnClick="BackButton_Click" CssClass="back-link"></asp:LinkButton>
            </div>
        </div>
    </div>
    <div class="section">
        <div class="section-title">
            Days With Data
        </div>
        <div class="section-content">
            <asp:Repeater runat="server" ID="rptDaysWithData">
                <HeaderTemplate>
                    <table class="grid">
                        <tr>
                            <th>ChargeCategory</th>
                            <th>DaysWithDataCount</th>
                            <th>TotalDaysInMonth</th>
                        </tr>
                </HeaderTemplate>
                <ItemTemplate>
                    <tr>
                        <td class="item">
                            <%# Eval("ChargeCategory")%>
                        </td>
                        <td class="item" style="text-align: right;">
                            <%# Eval("DaysWithDataCount")%>
                        </td>
                        <td class="item" style="text-align: right;">
                            <%# Eval("TotalDaysInMonth")%>
                        </td>
                    </tr>
                </ItemTemplate>
                <FooterTemplate>
                    </table>
                </FooterTemplate>
            </asp:Repeater>
            <asp:Panel runat="server" ID="panDaysWithDataNoData" Visible="false">
                <div class="note">
                    No data was found.
                </div>
            </asp:Panel>
        </div>
    </div>
    <div class="section">
        <div class="section-title">
            Subsidy Comparison
        </div>
        <div class="section-content">
            <asp:Repeater runat="server" ID="rptSubsidyComparison">
                <HeaderTemplate>
                    <table class="grid">
                        <tr>
                            <th>TableName</th>
                            <th>TotalSubsidy</th>
                        </tr>
                </HeaderTemplate>
                <ItemTemplate>
                    <tr>
                        <td class="item">
                            <%# Eval("TableName")%>
                        </td>
                        <td class="item" style="text-align: right;">
                            <%# Eval("TotalSubsidy", "{0:C}")%>
                        </td>
                    </tr>
                </ItemTemplate>
                <FooterTemplate>
                    </table>
                </FooterTemplate>
            </asp:Repeater>
            <asp:Panel runat="server" ID="panSubsidyComparisonNoData" Visible="false">
                <div class="note">
                    No data was found.
                </div>
            </asp:Panel>
        </div>
    </div>
</asp:Content>

<asp:Content ID="Content3" ContentPlaceHolderID="scripts" runat="server">
</asp:Content>
