<%@ Page Title="Configure Global Cost parameters" Language="C#" MasterPageFile="~/FinOpsMaster.Master" AutoEventWireup="true" CodeBehind="ConGlobal.aspx.cs" Inherits="sselFinOps.ConGlobal" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    <div class="section">
        <h2>Configure Global Cost Parameters</h2>
        <table class="report-table" border="1">
            <tr>
                <td>General lab account:</td>
                <td>
                    <asp:DropDownList runat="server" ID="ddlGenLabAcct" CssClass="report-select" DataTextField="Name" DataValueField="AccountID">
                    </asp:DropDownList>
                </td>
            </tr>
            <tr>
                <td>General lab credit account:</td>
                <td>
                    <asp:DropDownList runat="server" ID="ddlGenLabCredit" CssClass="report-select" DataTextField="Name" DataValueField="AccountID">
                    </asp:DropDownList>
                </td>
            </tr>
            <tr>
                <td>Subsidy credit account:</td>
                <td>
                    <asp:DropDownList runat="server" ID="ddlSubsidyCreditAcct" CssClass="report-select" DataTextField="Name" DataValueField="AccountID">
                    </asp:DropDownList>
                </td>
            </tr>
            <tr>
                <td>Adminstrative assistant:</td>
                <td>
                    <asp:DropDownList runat="server" ID="ddlAdmin" CssClass="report-select" DataTextField="DisplayName" DataValueField="ClientID">
                    </asp:DropDownList>
                </td>
            </tr>
            <tr>
                <td>Business day when data locked:</td>
                <td>
                    <asp:TextBox runat="server" ID="txtBusinessDay" CssClass="report-text"></asp:TextBox>
                </td>
            </tr>
            <tr>
                <td>Days after which access is not auto re-enabled:</td>
                <td>
                    <asp:TextBox runat="server" ID="txtAccessToOld" CssClass="report-text"></asp:TextBox>
                </td>
            </tr>
            <tr>
                <td colspan="2">
                    <asp:Button runat="server" ID="btnSave" CssClass="report-button StoreButton" Text="Save Changes" CausesValidation="False" OnClick="btnSave_Click"></asp:Button>
                    <asp:LinkButton runat="server" ID="btnBack" Text="&larr; Back to Main Page" OnClick="BackButton_Click" CssClass="back-link"></asp:LinkButton>
                </td>
            </tr>
        </table>
    </div>
</asp:Content>

<asp:Content ID="Content3" ContentPlaceHolderID="scripts" runat="server">
</asp:Content>