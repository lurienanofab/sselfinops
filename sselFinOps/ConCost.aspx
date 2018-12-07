<%@ Page Title="Cost Configuration" Language="C#" MasterPageFile="~/FinOpsMaster.Master" AutoEventWireup="true" CodeBehind="ConCost.aspx.cs" Inherits="sselFinOps.ConCost" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <link rel="stylesheet" type="text/css" href="styles/costtable.css" />
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    <input type="hidden" runat="server" id="hidModifiedColor" class="modified-color" />
    <div class="section">
        <h2>
            <asp:Literal runat="server" ID="litHeader">Configure Costs</asp:Literal>
        </h2>
        <table class="report-table" border="1">
            <tr>
                <td colspan="2">
                    <asp:Button runat="server" ID="btnSave1" CssClass="report-button" Text="Save Changes" OnClick="BtnSave_Click"></asp:Button>
                    <asp:LinkButton runat="server" ID="btnBack1" CssClass="back-link" Text="&larr; Back To Main Page" OnClick="BackButton_Click"></asp:LinkButton>
                    <asp:Literal runat="server" ID="litSaveMessage1"></asp:Literal>
                </td>
            </tr>
            <tr>
                <td class="GridHeader" colspan="2">
                    <asp:Literal runat="server" ID="litAuxCostHdr">Monthly Auxilliary Costs</asp:Literal>
                </td>
            </tr>
            <tr>
                <td colspan="2">
                    <div class="aux-costs-container">
                        <asp:Table runat="server" ID="tblAuxCost" CssClass="costs-table">
                        </asp:Table>
                    </div>
                </td>
            </tr>
            <tr>
                <td class="GridHeader" colspan="2">
                    <asp:Literal ID="litCostHdr" runat="server">Costs</asp:Literal>
                </td>
            </tr>
            <tr>
                <td colspan="2">
                    <div style="background-color: #D3D3D3; padding: 3px; border-bottom: solid 1px #808080">
                        Upload Cost Worksheet:
                        <asp:FileUpload runat="server" ID="FileUpload1" /><asp:Button runat="server" ID="btnUploadCostWorksheet" Text="Upload" OnClick="BtnUploadCostWorksheet_Click" /><asp:Literal runat="server" ID="litUploadMessage"></asp:Literal>
                        <div style="font-style: italic; color: #606060;">
                            You can upload a properly formated Excel file (*.xls, *.xlsx) to load values in the form below. Click
                            <asp:HyperLink runat="server" ID="hypTemplate" NavigateUrl="~/Spreadsheets.ashx?path=templates/sample-tool-cost-worksheet.xls">here</asp:HyperLink>
                            for an example file.
                        </div>
                    </div>
                    <div class="costs-container">
                        <asp:Table runat="server" ID="tblCost" CssClass="costs-table">
                        </asp:Table>
                    </div>
                </td>
            </tr>
            <tr>
                <td colspan="2">
                    <asp:Button runat="server" ID="btnSave2" CssClass="report-button" Text="Save Changes" OnClick="BtnSave_Click"></asp:Button>
                    <asp:LinkButton runat="server" ID="btnBack2" CssClass="back-link" Text="&larr; Back To Main Page" OnClick="BackButton_Click"></asp:LinkButton>
                    <asp:Literal runat="server" ID="litSaveMessage2"></asp:Literal>
                </td>
            </tr>
        </table>
    </div>
    <asp:Literal runat="server" ID="litDebug"></asp:Literal>
</asp:Content>

<asp:Content ID="Content3" ContentPlaceHolderID="scripts" runat="server">
    <script type="text/javascript">
        function markModified(target, index) {
            var color = $(".modified-color").val();
            $('.modified-checkbox.index-' + index + ' > input[type="checkbox"]').prop('checked', true);
            $(target).css({ "background-color": color });
        }
    </script>
</asp:Content>
