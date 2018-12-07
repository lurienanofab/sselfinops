<%@ Page Title="Costing Formula Configuration" Language="C#" MasterPageFile="~/FinOpsMaster.Master" AutoEventWireup="true" CodeBehind="ConFormula.aspx.cs" Inherits="sselFinOps.ConFormula" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    <div class="section">
        <h2>
            <asp:Literal runat="server" ID="litHeader">Configure Costing Formulas</asp:Literal>
        </h2>
        <table class="report-table" border="1" style="width: 685px;">
            <tr>
                <td class="GridHeader" colspan="2">Please select the type of formula you wish to develop</td>
            </tr>
            <tr>
                <td colspan="2">
                    <asp:RadioButtonList runat="server" ID="rblFormulaType" RepeatDirection="Horizontal" AutoPostBack="True" OnSelectedIndexChanged="RblFormulaType_SelectedIndexChanged">
                        <asp:ListItem Value="0">Room</asp:ListItem>
                        <asp:ListItem Value="1">Tool</asp:ListItem>
                        <asp:ListItem Value="2">StoreInv</asp:ListItem>
                        <asp:ListItem Value="3">StoreJE</asp:ListItem>
                    </asp:RadioButtonList>
                </td>
            </tr>
            <tr>
                <td colspan="2">
                    <asp:Label runat="server" ID="lblInfo" Visible="False">User notes go here</asp:Label>
                </td>
            </tr>
            <tr>
                <td colspan="2">
                    <asp:TextBox ID="txtFormula" runat="server" Width="672" Height="250" TextMode="MultiLine" Visible="False">calcCost =</asp:TextBox>
                </td>
            </tr>
            <tr>
                <td colspan="2">
                    <asp:Label runat="server" ID="lblError" CssClass="WarningText" Visible="False"></asp:Label>
                </td>
            </tr>
            <tr>
                <td style="text-align: center;">
                    <asp:Button runat="server" ID="btnValidate" CssClass="StoreButton" Text="Validate Formula" Enabled="False" OnClick="BtnValidate_Click"></asp:Button>
                </td>
                <td style="text-align: center;">
                    <asp:Button runat="server" ID="btnRevert" CssClass="QuitStoreButton" Text="Revert to Saved" Enabled="False" OnClick="BtnRevert_Click"></asp:Button>
                </td>
            </tr>
            <tr>
                <td colspan="2">
                    <asp:DataGrid runat="server" ID="dgSample" AutoGenerateColumns="False">
                        <AlternatingItemStyle BackColor="#FFFFC0"></AlternatingItemStyle>
                        <HeaderStyle BackColor="#C0FFFF"></HeaderStyle>
                    </asp:DataGrid>
                </td>
            </tr>
            <tr>
                <td colspan="2">
                    <asp:Button runat="server" ID="btnSave" Text="Save Changes" CausesValidation="False" OnClick="BtnSave_Click" CssClass="report-button"></asp:Button>
                    <asp:LinkButton runat="server" ID="btnBack" Text="&larr; Back to Main Page" OnClick="BackButton_Click" CssClass="back-link"></asp:LinkButton>
                </td>
            </tr>
        </table>
    </div>
</asp:Content>

<asp:Content ID="Content3" ContentPlaceHolderID="scripts" runat="server">
</asp:Content>
