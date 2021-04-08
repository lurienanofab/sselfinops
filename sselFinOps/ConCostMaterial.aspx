<%@ Page Title="" Language="C#" MasterPageFile="~/FinOpsMaster.Master" AutoEventWireup="true" CodeBehind="ConCostMaterial.aspx.cs" Inherits="sselFinOps.ConCostMaterial" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <link rel="stylesheet" href="styles/costtable.css" />

    <style>
        .tct-table {
            border-collapse: collapse;
        }

        .tr-head {
            border-top: solid 2px #585858;
        }

        .tr-top {
            border-top: solid 2px #585858;
        }

        .tct-table th {
            background-color: #dcdcdc;
            border: 1px solid #aaaaaa;
            padding: 3px;
        }

        .tct-table th {
            font-family: Arial;
            font-size: 10pt;
        }
    </style>
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
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
                <td colspan="2">
                    <asp:Table ID="tblmaterial" runat="server" CssClass="costs-table">
                        <asp:TableHeaderRow>
                            <asp:TableHeaderCell CssClass="right-border-thick">Resource Name</asp:TableHeaderCell>
                            <asp:TableHeaderCell>Acct Per</asp:TableHeaderCell>
                            <asp:TableHeaderCell>Per Use Charge</asp:TableHeaderCell>
                            <asp:TableHeaderCell CssClass="right-border-thick">Per Period Charge</asp:TableHeaderCell>

                            <asp:TableHeaderCell>Acct Per</asp:TableHeaderCell>
                            <asp:TableHeaderCell>Per Use Charge</asp:TableHeaderCell>
                            <asp:TableHeaderCell CssClass="right-border-thick">Per Period Charge</asp:TableHeaderCell>

                            <asp:TableHeaderCell>Acct Per</asp:TableHeaderCell>
                            <asp:TableHeaderCell>Per Use Charge</asp:TableHeaderCell>
                            <asp:TableHeaderCell CssClass="right-border-thick">Per Period Charge</asp:TableHeaderCell>


                        </asp:TableHeaderRow>
                    </asp:Table>
                </td>
            </tr>
            <tr>
                <td colspan="2">
                    <asp:Button runat="server" ID="btnSave2" CssClass="report-button" Text="Save Changes" OnClick="BtnSave_Click"></asp:Button>
                    <asp:LinkButton runat="server" ID="btnBack2" CssClass="back-link" Text="&laquo; Back To Main Page" OnClick="BackButton_Click"></asp:LinkButton>
                    <asp:Literal runat="server" ID="litSaveMessage2"></asp:Literal>
                </td>
            </tr>
        </table>
    </div>
</asp:Content>

<asp:Content ID="Content3" ContentPlaceHolderID="scripts" runat="server">
    <script>
        //$("tr:even").css("background-color", "#e5e5e5");
        $('.costs-table > tbody  > tr').each(function () {
            if ($(this).css("background-color") == "transparent") {
                $(this).css("background-color", "#e5e5e5");
            }
        });


        var orgTypes = ['UMICH', 'OTHER_ACAD', 'NON_ACAD'];
        var chargTypes = ['.ddlAcct_', '.txtUSAGE_', '.txtPERIOD_'];
        var onChangeBackgroundColor = '#F78181';

        var onKeyChange = function (eachcomp) {
            if ($.isNumeric($(eachcomp).val())) {
                $(eachcomp).keydown(function () { $(this).css('background-color', onChangeBackgroundColor); });
            } else {
                //$(eachcomp).val("");
            }
        };

        var onValChange = function (eachcomp) {
            $(eachcomp).change(function () { $(this).css('background-color', onChangeBackgroundColor); });
        };

        orgTypes.forEach(function (eachorg) {
            chargTypes.forEach(function (eachtype) {
                var comp = eachtype + eachorg;
                if (eachtype.match("^.txt")) {
                    $(comp).change(onKeyChange(comp));
                } else {
                    $(comp).change(onValChange(comp));
                }
            });

            //var perAcct = ".ddl" + eachorg;
            //$(perAcct).change(onValChange(perAcct));

        });
    </script>
</asp:Content>
