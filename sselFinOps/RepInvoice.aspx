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
                                <lnf:PeriodPicker runat="server" ID="pp1" AutoPostBack="true" OnSelectedPeriodChanged="pp1_SelectedPeriodChanged" />
                            </td>
                        </tr>
                    </table>
                </div>
                <div class="criteria-item">
                    <asp:CheckBox runat="server" ID="chkShowRemote" Text="Includes Remote Processing" Checked="false" AutoPostBack="true" OnCheckedChanged="chkShowRemote_CheckedChanged" CssClass="show-remote" />
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
                            <asp:Button runat="server" ID="btnDownloadAll" Text="Create Zip File For All Invoices" OnClick="btnDownloadAll_Click" OnClientClick="downloadAll(this);" UseSubmitBehavior="false" />
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
                                    <a href='<%#GetViewUrl((int)Eval("Header.OrgAcctID"), "Excel")%>'>
                                        <img src="images/xls.png" alt="Create Excel Invoice" title="Create Excel Invoice" />
                                    </a>
                                    <a href='<%#GetViewUrl((int)Eval("Header.OrgAcctID"), "Html")%>'>
                                        <img src="images/html.png" alt="Display Invoice" title="Display Invoice" />
                                    </a>
                                </td>
                            </tr>
                        </ItemTemplate>
                        <FooterTemplate>
                            </tbody>
                            </table>
                        </FooterTemplate>
                    </asp:Repeater>

                    <asp:DataGrid runat="server" ID="zdgOrg" TabIndex="26" AutoGenerateColumns="False" DataKeyField="OrgAcctID" CssClass="detail-table" AlternatingItemStyle-BackColor="Linen" GridLines="None" OnItemDataBound="dgOrg_ItemDataBound">
                        <EditItemStyle CssClass="GridText" BackColor="#66FFFF"></EditItemStyle>
                        <AlternatingItemStyle CssClass="altitem"></AlternatingItemStyle>
                        <ItemStyle CssClass="item"></ItemStyle>
                        <HeaderStyle CssClass="header"></HeaderStyle>
                        <Columns>
                            <asp:TemplateColumn HeaderText="Organization">
                                <ItemStyle Width="300"></ItemStyle>
                                <ItemTemplate>
                                    <asp:Label ID="lblOrg" runat="server"></asp:Label>
                                </ItemTemplate>
                            </asp:TemplateColumn>
                            <asp:TemplateColumn HeaderText="Account">
                                <ItemStyle Width="300"></ItemStyle>
                                <ItemTemplate>
                                    <asp:Label ID="lblAccount" runat="server"></asp:Label>
                                </ItemTemplate>
                            </asp:TemplateColumn>
                            <asp:TemplateColumn HeaderText="PO Expire">
                                <ItemStyle Width="100"></ItemStyle>
                                <ItemTemplate>
                                    <asp:Label ID="lblExpire" runat="server"></asp:Label>
                                </ItemTemplate>
                            </asp:TemplateColumn>
                            <asp:TemplateColumn HeaderText="Funds" HeaderStyle-CssClass="center">
                                <ItemStyle Width="100" HorizontalAlign="Right"></ItemStyle>
                                <ItemTemplate>
                                    <asp:Label ID="lblFunds" runat="server"></asp:Label>
                                </ItemTemplate>
                            </asp:TemplateColumn>
                            <asp:TemplateColumn>
                                <ItemStyle Width="35" HorizontalAlign="Center"></ItemStyle>
                                <ItemTemplate>
                                    <asp:ImageButton ID="btnExcelOrgInvoice" runat="server" ImageUrl="~/images/xls.png" AlternateText="Create Excel Invoice" ToolTip="Create Excel Invoice" CommandName="ExcelReport" CausesValidation="false" OnCommand="OrgInvoice_Command"></asp:ImageButton>
                                    <a href="RepInvoiceView.aspx?">
                                        <img src="images/html.png" alt="Display Invoice" /></a>
                                    <asp:ImageButton ID="btnHtmlOrgInvoice" runat="server" ImageUrl="~/images/html.png" AlternateText="Display Invoice" ToolTip="Display Invoice" CommandName="HtmlReport" CausesValidation="false" OnCommand="OrgInvoice_Command"></asp:ImageButton>
                                </ItemTemplate>
                            </asp:TemplateColumn>
                        </Columns>
                        <PagerStyle HorizontalAlign="Right" Mode="NumericPages"></PagerStyle>
                    </asp:DataGrid>
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
