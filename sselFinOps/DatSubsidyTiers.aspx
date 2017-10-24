<%@ Page Title="" Language="C#" MasterPageFile="~/FinOpsMaster.Master" AutoEventWireup="true" CodeBehind="DatSubsidyTiers.aspx.cs" Inherits="sselFinOps.DatSubsidyTiers" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <style type="text/css">
        .tier-group {
            font-size: 12pt;
            margin-bottom: 30px;
        }

        .tier-group-title {
            font-weight: bold;
            font-style: italic;
            font-size: 12pt;
        }

        .tier-group-table {
            border-collapse: collapse;
            table-layout: fixed;
            width: 100%;
        }

            .tier-group-table th, .tier-group-table td {
                padding: 3px;
                font-weight: normal;
                font-size: 12pt;
            }

            .tier-group-table th {
                border-bottom: solid 1px #222222;
                text-align: left;
            }

        .tier-nodata {
            margin-top: 5px;
        }
    </style>
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    <div class="section">
        <h2>Subsidy Tiers Report</h2>
        <div class="criteria">
            <table class="criteria-table">
                <tr>
                    <td>Select period:</td>
                    <td>
                        <lnf:PeriodPicker runat="server" ID="pp1" />
                    </td>
                </tr>
            </table>
            <div class="criteria-item">
                <asp:Button runat="server" ID="btnReport" Text="Retrieve Data" CssClass="report-button" OnClick="btnReport_Click" />
                <asp:LinkButton runat="server" ID="btnBack" Text="&larr; Back to Main Page" CssClass="back-link" OnClick="BackButton_Click"></asp:LinkButton>
            </div>
        </div>
    </div>
    <div class="section">
        <h4>Selected Period:
            <asp:Literal runat="server" ID="litPeriod"></asp:Literal>
        </h4>

        <div class="tier-group">
            <asp:Repeater runat="server" ID="rptSubsidyTiersGroup0">
                <HeaderTemplate>
                    <div class="tier-group-title">
                        For Existing LNF Users
                    </div>
                    <table class="tier-group-table">
                        <thead>
                            <tr>
                                <th style="width: 250px;">Annual Fees Per User</th>
                                <th style="width: 60px;">LNF</th>
                                <th style="width: 60px;">PI</th>
                                <th style="width: 100px;">User Charge</th>
                                <th>Running Total</th>
                            </tr>
                        </thead>
                        <tbody>
                </HeaderTemplate>
                <ItemTemplate>
                    <tr>
                        <td>
                            <%# Eval("AnnualFees")%>
                        </td>
                        <td>
                            <%# Eval("LNF")%>
                        </td>
                        <td>
                            <%# Eval("PI")%>
                        </td>
                        <td>
                            <%# Eval("UserCharge")%>
                        </td>
                        <td>
                            <%# Eval("RunningTotal")%>
                        </td>
                    </tr>
                </ItemTemplate>
                <FooterTemplate>
                    </tbody> </table>
                </FooterTemplate>
            </asp:Repeater>

            <asp:Panel runat="server" ID="panNoData0" Visible="false" CssClass="tier-nodata">
                <em style="color: #808080;">There is no subsidy for Existing Users in this period.</em>
            </asp:Panel>
        </div>

        <div class="tier-group">
            <asp:Repeater runat="server" ID="rptSubsidyTiersGroup1">
                <HeaderTemplate>
                    <div class="tier-group-title">
                        For New LNF Users
                    </div>
                    <table class="tier-group-table">
                        <thead>
                            <tr>
                                <th style="width: 250px;">Annual Fees Per User</th>
                                <th style="width: 60px;">LNF</th>
                                <th style="width: 60px;">PI</th>
                                <th style="width: 100px;">User Charge</th>
                                <th>Running Total</th>
                            </tr>
                        </thead>
                        <tbody>
                </HeaderTemplate>
                <ItemTemplate>
                    <tr>
                        <td>
                            <%# Eval("AnnualFees")%>
                        </td>
                        <td>
                            <%# Eval("LNF")%>
                        </td>
                        <td>
                            <%# Eval("PI")%>
                        </td>
                        <td>
                            <%# Eval("UserCharge")%>
                        </td>
                        <td>
                            <%# Eval("RunningTotal")%>
                        </td>
                    </tr>
                </ItemTemplate>
                <FooterTemplate>
                    </tbody> </table>
                </FooterTemplate>
            </asp:Repeater>

            <asp:Panel runat="server" ID="panNoData1" Visible="false" CssClass="tier-nodata">
                <em style="color: #808080;">There is no subsidy for New Users in this period.</em>
            </asp:Panel>
        </div>

        <div class="tier-group">
            <asp:Repeater runat="server" ID="rptSubsidyTiersGroup2">
                <HeaderTemplate>
                    <div class="tier-group-title">
                        For LNF Users Working With New Untenured Faculty Members
                    </div>
                    <table class="tier-group-table">
                        <thead>
                            <tr>
                                <th style="width: 250px;">Annual Fees Per User</th>
                                <th style="width: 60px;">LNF</th>
                                <th style="width: 60px;">PI</th>
                                <th style="width: 100px;">User Charge</th>
                                <th>Running Total</th>
                            </tr>
                        </thead>
                        <tbody>
                </HeaderTemplate>
                <ItemTemplate>
                    <tr>
                        <td>
                            <%# Eval("AnnualFees")%>
                        </td>
                        <td>
                            <%# Eval("LNF")%>
                        </td>
                        <td>
                            <%# Eval("PI")%>
                        </td>
                        <td>
                            <%# Eval("UserCharge")%>
                        </td>
                        <td>
                            <%# Eval("RunningTotal")%>
                        </td>
                    </tr>
                </ItemTemplate>
                <FooterTemplate>
                    </tbody> </table>
                </FooterTemplate>
            </asp:Repeater>

            <asp:Panel runat="server" ID="panNoData2" Visible="false" CssClass="tier-nodata">
                <em style="color: #808080;">There is no subsidy for New Faculty Users in this period.</em>
            </asp:Panel>
        </div>

    </div>
</asp:Content>

<asp:Content ID="Content3" ContentPlaceHolderID="scripts" runat="server">
</asp:Content>
