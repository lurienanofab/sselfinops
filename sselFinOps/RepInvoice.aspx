<%@ Page Title="" Language="C#" MasterPageFile="~/FinOpsMaster.Master" AutoEventWireup="true" CodeBehind="RepInvoice.aspx.cs" Inherits="sselFinOps.RepInvoice" %>

<%@ Import Namespace="sselFinOps" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <link rel="stylesheet" href="styles/invoice.css?v=20170125" />
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    <div class="invoice" data-ajaxurl="/webapi/billing/report/billing-summary">
        <div class="section">
            <h2>External Organization Invoice</h2>
            <div class="criteria">
                <div class="criteria-item">
                    <table style="border-collapse: collapse; margin: 0;">
                        <tr>
                            <td style="padding: 2px;">Select period:</td>
                            <td style="padding: 2px;">
                                <lnf:PeriodPicker runat="server" ID="pp1" AutoPostBack="true" OnSelectedPeriodChanged="Pp1_SelectedPeriodChanged" />
                            </td>
                        </tr>
                    </table>
                </div>
                <div class="criteria-item">
                    <asp:CheckBox runat="server" ID="chkShowRemote" Text="Includes Remote Processing" Checked="false" AutoPostBack="true" OnCheckedChanged="ChkShowRemote_CheckedChanged" CssClass="show-remote" />
                </div>
                <div class="criteria-item">
                    <asp:LinkButton runat="server" ID="btnBack" Text="&larr; Back to Main Page" OnClick="BackButton_Click"></asp:LinkButton>
                </div>
            </div>
        </div>
        <div class="section">
            <asp:Panel runat="server" ID="panWarning" Visible="false">
                <div class="WarningText" style="padding-bottom: 10px;">
                    No organization with charges for selected period
                </div>
            </asp:Panel>
            <asp:Panel runat="server" ID="panSummary" Visible="true">
                <div class="summary">
                    <div class="title">
                        Summary
                    </div>
                    <div class="content">
                        <asp:Repeater runat="server" ID="rptUsageSummary">
                            <HeaderTemplate>
                                <div class="usage-summary">
                            </HeaderTemplate>
                            <ItemTemplate>
                                <div style="margin-top: 5px;">
                                    <strong><%#Eval("ChargeTypeName")%>:</strong>
                                    <span><%#Eval("TotalCharges", "{0:C}")%></span>
                                </div>
                            </ItemTemplate>
                            <FooterTemplate>
                                </div>
                            </FooterTemplate>
                        </asp:Repeater>
                        <div style="margin-top: 10px;">
                            <asp:Button runat="server" ID="btnDownloadAll" Text="Create Zip File For All Invoices" OnClick="BtnDownloadAll_Click" OnClientClick="downloadAll(this);" UseSubmitBehavior="false" />
                            <div class="download-all-working" style="display: none;">Working...</div>
                            <asp:Literal runat="server" ID="litDownloadAllWorkingMessage"></asp:Literal>
                        </div>
                    </div>
                </div>
            </asp:Panel>
            <asp:Panel runat="server" ID="panDetail">
                <div class="detail">
                    <asp:Repeater runat="server" ID="rptInvoice">
                        <HeaderTemplate>
                            <table class="detail-table">
                                <thead>
                                    <tr>
                                        <th style="width: 300px;">Organization</th>
                                        <th>Account</th>
                                        <th style="width: 100px;">PO Expire</th>
                                        <th style="width: 100px; text-align: right;">Funds</th>
                                        <th style="width: 100px; text-align: right;">Total</th>
                                        <th style="width: 50px;">&nbsp;</th>
                                    </tr>
                                </thead>
                                <tbody>
                        </HeaderTemplate>
                        <ItemTemplate>
                            <tr>
                                <td>
                                    <%#Eval("Header.OrgName") %>
                                </td>
                                <td>
                                    <%#Eval("Header.AccountName") %>
                                </td>
                                <td class='<%#GetExpireCssClass((DateTime?)Eval("Header.PoEndDate"))%>'>
                                    <%#Eval("Header.PoEndDate", "{0:d-MMM-yyyy}") %>
                                </td>
                                <td style="text-align: right;">
                                    <%#FormatDecimal((decimal)Eval("Header.PoRemainingFunds"))%>
                                </td>
                                <td style="text-align: right;">
                                    <%#Eval("Usage.TotalCharges", "{0:C}")%>
                                </td>
                                <td class="controls" style="text-align: center;">
                                    <asp:HyperLink runat="server" ID="hypViewExcel" ImageUrl="~/images/xls.png" NavigateUrl='<%#GetViewUrl((int)Eval("Header.AccountID"), "Excel")%>' ToolTip="Create Excel Invoice"></asp:HyperLink>
                                    <asp:HyperLink runat="server" ID="hypViewHtml" ImageUrl="~/images/html.png" NavigateUrl='<%#GetViewUrl((int)Eval("Header.AccountID"), "Html")%>' ToolTip="Display Invoice"></asp:HyperLink>
                                </td>
                            </tr>
                        </ItemTemplate>
                        <FooterTemplate>
                            </tbody>
                            </table>
                        </FooterTemplate>
                    </asp:Repeater>
                </div>
            </asp:Panel>
        </div>
    </div>
</asp:Content>

<asp:Content ID="Content3" ContentPlaceHolderID="scripts" runat="server">
    <script>
        function downloadAll(btn) {
            btn.disabled = true;
            $(".download-all-working").css({ "display": "inline-block" }).html("Working...");
        }
    </script>
</asp:Content>
