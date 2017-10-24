<%@ Page Title="" Language="C#" MasterPageFile="~/FinOpsMaster.Master" AutoEventWireup="true" CodeBehind="RepInvoiceView.aspx.cs" Inherits="sselFinOps.RepInvoiceView" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <link rel="stylesheet" href="styles/invoice.css?v=20151119">

    <style>
        .error-message {
            color: red;
        }

        .export-button {
            padding: 5px;
        }
    </style>
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    <asp:Literal runat="server" ID="litErrorMessage"></asp:Literal>
    <div class="section">
        <h2>External Organization Invoice</h2>
        <div class="criteria">
            <div class="criteria-item">
                <asp:HyperLink runat="server" ID="hypBack" Text="&larr; Back to Invoice List" NavigateUrl="~/RepInvoice.aspx"></asp:HyperLink>
            </div>
            <div class="criteria-item">
                <asp:LinkButton runat="server" ID="btnBack" Text="&larr; Back to Main Page" OnClick="BackButton_Click"></asp:LinkButton>
            </div>
        </div>
    </div>
    <hr />
    <div class="section">
        <div style="font-size: 22pt; margin-bottom: 20px;">
            <asp:Literal runat="server" ID="litAccountName"></asp:Literal>
        </div>
        <div class="detail">
            <asp:Repeater runat="server" ID="rptInvoice">
                <HeaderTemplate>
                    <table class="detail-table">
                        <thead>
                            <tr>
                                <th>Description</th>
                                <th style="width: 40px; text-align: right;">Qty</th>
                                <th style="width: 60px; text-align: right;">Cost</th>
                                <th style="width: 60px; text-align: right;">Total</th>
                            </tr>
                        </thead>
                        <tbody>
                </HeaderTemplate>
                <ItemTemplate>
                    <tr>
                        <td><%#Eval("Description")%></td>
                        <td style="text-align: right;"><%#Eval("Quantity")%></td>
                        <td style="text-align: right;"><%#Eval("Cost", "{0:C}")%></td>
                        <td style="text-align: right;"><%#Eval("LineTotal", "{0:C}")%></td>
                    </tr>
                </ItemTemplate>
                <FooterTemplate>
                    </tbody>
                </table>
                </FooterTemplate>
            </asp:Repeater>
            <div style="text-align: right; font-weight: bold; border-top: solid 1px #ccc; padding: 5px;">
                <asp:Literal runat="server" ID="litInvoiceTotal"></asp:Literal>
            </div>
        </div>
        <asp:Button runat="server" ID="btnCreateExcelInvoice" Text="Save Excel Invoice" OnClick="btnCreateExcelInvoice_Click" CssClass="export-button"></asp:Button>
    </div>
</asp:Content>

<asp:Content ID="Content3" ContentPlaceHolderID="scripts" runat="server">
</asp:Content>
