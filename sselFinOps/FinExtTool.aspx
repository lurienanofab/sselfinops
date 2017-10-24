<%@ Page Title="" Language="C#" MasterPageFile="~/FinOpsMaster.Master" AutoEventWireup="true" CodeBehind="FinExtTool.aspx.cs" Inherits="sselFinOps.FinExtTool" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    <div class="section">
        <h2>External Users Tool Charges</h2>
        <div class="criteria">
            <table class="criteria-table">
                <tr>
                    <td>Select period:</td>
                    <td>
                        <lnf:PeriodPicker runat="server" ID="pp1" AutoPostBack="true" OnSelectedPeriodChanged="pp1_SelectedPeriodChanged" />
                    </td>
                </tr>
            </table>
            <div class="criteria-item">
                <asp:LinkButton runat="server" ID="btnBack" Text="&laquo; Back to Main Page" OnClick="BackButton_Click" CssClass="back-link"></asp:LinkButton>
            </div>
        </div>
    </div>
    <div class="section">
        <table runat="server" id="Table1" border="1" visible="false">
            <tr>
                <td>
                    <asp:Label ID="lblWarning" runat="server" Visible="False" CssClass="WarningText">No tool charges for the selected period</asp:Label>
                </td>
            </tr>
            <tr>
                <td style="vertical-align: top;">
                    <asp:DataGrid ID="dgTool" runat="server" CellPadding="2" CellSpacing="2">
                        <HeaderStyle Font-Size="Small" Font-Names="Arial" Font-Bold="True" BackColor="#FFFFC0"></HeaderStyle>
                    </asp:DataGrid>
                </td>
            </tr>
        </table>
    </div>
</asp:Content>

<asp:Content ID="Content3" ContentPlaceHolderID="scripts" runat="server">
</asp:Content>
