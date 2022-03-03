<%@ Page Title="Misc Charges" Language="C#" MasterPageFile="~/FinOpsMaster.Master" AutoEventWireup="true" CodeBehind="RepMiscCharge.aspx.cs" Inherits="sselFinOps.RepMiscCharge" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <style type="text/css">
        .description-item input {
            width: 340px;
        }

        .unitcost-item {
            text-align: right;
        }

            .unitcost-item input {
                width: 80px;
            }

        .quantity-item,
        .totalcost-item,
        .subsidy-item,
        .userpayment-item {
            text-align: right;
        }

        .command-item {
            text-align: center;
        }

        .insertdate-item {
            text-align: right;
            width: 100px;
        }

        span.report-text {
            padding: 0;
        }

            span.report-text input {
                padding: 4px;
            }

        span.validation-error {
            color: #ff0000;
        }

        .report-text.date-picker {
            width: 80px;
        }

        .debug {
            font-family: 'Courier New';
            color: #ff0000;
        }
    </style>
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    <div class="section">
        <h2>Miscellaneous Charges</h2>
        <div class="criteria">
            <table class="criteria-table">
                <tr>
                    <td>Select period:</td>
                    <td>
                        <lnf:PeriodPicker runat="server" ID="PeriodPicker1" StartPeriod="2009-01-01" AutoPostBack="true" OnSelectedPeriodChanged="PeriodPicker1_SelectedPeriodChanged" />
                    </td>
                </tr>
            </table>
        </div>
    </div>
    <div class="section">
        <h3>Add new entry for miscellaneous billing</h3>
        <div class="criteria">
            <table class="criteria-table">
                <tr>
                    <td>Client:</td>
                    <td>
                        <asp:DropDownList ID="ddlClient" runat="server" AutoPostBack="true" DataValueField="ClientID" DataTextField="DisplayName" CssClass="user-select report-select" OnDataBound="DdlClient_DataBound" OnSelectedIndexChanged="DdlClient_SelectedIndexChanged">
                        </asp:DropDownList>
                        <asp:Label runat="server" ID="lblUserValidation" CssClass="validation-error" Visible="false"></asp:Label>
                    </td>
                </tr>
                <tr>
                    <td>Account:</td>
                    <td>
                        <asp:DropDownList ID="ddlAccount" runat="server" DataValueField="AccountID" DataTextField="AccountName" CssClass="acct-select report-select">
                        </asp:DropDownList>
                        <asp:Label runat="server" ID="lblAccountValidation" CssClass="validation-error" Visible="false"></asp:Label>
                        <asp:ObjectDataSource ID="odsAccount" runat="server" TypeName="sselFinOps.AppCode.BLL.AccountBL" SelectMethod="GetAccountsByClientIDAndDate">
                            <SelectParameters>
                                <asp:ControlParameter ControlID="ddlClient" Type="Int32" Name="clientId" PropertyName="SelectedValue" />
                                <asp:ControlParameter ControlID="PeriodPicker1" Type="Int32" Name="year" PropertyName="SelectedYear" />
                                <asp:ControlParameter ControlID="PeriodPicker1" Type="Int32" Name="month" PropertyName="SelectedMonth" />
                            </SelectParameters>
                        </asp:ObjectDataSource>
                    </td>
                </tr>
                <tr>
                    <td>Usage type:</td>
                    <td>
                        <asp:DropDownList runat="server" ID="ddlSUBType" CssClass="report-select">
                            <asp:ListItem Value="Room" Text="Room"></asp:ListItem>
                            <asp:ListItem Value="Store" Text="Store"></asp:ListItem>
                            <asp:ListItem Value="Tool" Text="Tool"></asp:ListItem>
                        </asp:DropDownList>
                        <span style="font-size: small; font-style: italic; color: #808080;">&larr; This is ignored if it's external account</span>
                    </td>
                </tr>
                <tr>
                    <td>Apply period:</td>
                    <td>
                        <asp:TextBox runat="server" ID="txtActDate" CssClass="date-picker report-text apply-period"></asp:TextBox>
                        <img src="images/calendar.gif" alt="" class="calendar-icon" />
                    </td>
                </tr>
                <tr>
                    <td>Quantity:</td>
                    <td>
                        <asp:TextBox runat="server" ID="txtQuantity" Width="55" CssClass="report-text"></asp:TextBox>
                        <asp:Label runat="server" ID="lblQuantityValidation" CssClass="validation-error" Visible="false"></asp:Label>
                    </td>
                </tr>
                <tr>
                    <td>Unit cost:</td>
                    <td>
                        <asp:TextBox runat="server" ID="txtCost" Width="55" CssClass="report-text"></asp:TextBox>
                        <asp:Label runat="server" ID="lblCostValidation" CssClass="validation-error" Visible="false"></asp:Label>
                    </td>
                </tr>
                <tr>
                    <td>Description:</td>
                    <td>
                        <asp:TextBox runat="server" ID="txtDesc" TextMode="MultiLine" Height="100" Width="200"></asp:TextBox>
                        <span style="font-size: small; font-style: italic; color: #808080;">&larr; Anything after the 49th character will be truncated on the invoice</span>
                        <div>
                            <asp:Label runat="server" ID="lblDescriptionValidation" CssClass="validation-error" Visible="false"></asp:Label>
                        </div>
                    </td>
                </tr>
            </table>
            <div class="criteria-item">
                <asp:Button runat="server" ID="btnSave" Text="Create New Entry" CssClass="report-button" OnClick="BtnSave_Click" />
                <asp:LinkButton runat="server" ID="btnBack2" OnClick="BackButton_Click" CssClass="back-link">&larr; Back to Main Page</asp:LinkButton>
                <asp:Literal runat="server" ID="litDebug"></asp:Literal>
            </div>
        </div>
    </div>
    <div class="section">
        <asp:HiddenField runat="server" ID="hidPeriod" />
        <asp:GridView runat="server" ID="gvMiscCharge" AutoGenerateColumns="false" DataKeyNames="ExpID" AllowSorting="true" CssClass="gridview highlight" GridLines="None" OnRowDeleting="GvMiscCharge_RowDeleting" OnRowEditing="GvMiscCharge_RowEditing" OnRowCancelingEdit="GvMiscCharge_RowCancelingEdit" OnRowUpdating="GvMiscCharge_RowUpdating" OnRowCommand="GvMiscCharge_RowCommand">
            <HeaderStyle CssClass="header" />
            <RowStyle CssClass="row" />
            <AlternatingRowStyle CssClass="altrow" />
            <Columns>
                <asp:CommandField ShowDeleteButton="true" ButtonType="Link" DeleteText="Delete" />
                <asp:BoundField HeaderText="Name" DataField="DisplayName" ReadOnly="true" SortExpression="DisplayName" />
                <asp:BoundField HeaderText="Account" DataField="AccountName" ReadOnly="true" SortExpression="AccountName" />
                <asp:BoundField HeaderText="ShortCode" DataField="ShortCode" ReadOnly="true" SortExpression="ShortCode" />
                <asp:BoundField HeaderText="SUB<br />Type" DataField="SUBType" ReadOnly="true" SortExpression="SUBType" HtmlEncode="false" />
                <asp:TemplateField HeaderText="Period">
                    <ItemTemplate>
                        <%#Eval("Period", "{0:M/d/yyyy}")%>
                    </ItemTemplate>
                    <EditItemTemplate>
                        <div style="width: 85px;">
                            <asp:TextBox runat="server" ID="txtActDate" CssClass="date-picker report-text" Text='<%#Eval("Period", "{0:M/d/yyyy}")%>'></asp:TextBox>
                        </div>
                    </EditItemTemplate>
                </asp:TemplateField>
                <asp:BoundField HeaderText="Insert Date" DataField="ActDate" ReadOnly="true" SortExpression="ExpID" ItemStyle-CssClass="insertdate-item" DataFormatString="{0:M/d/yyyy'<br>'h:mm:ss tt}" HtmlEncode="false" />
                <asp:BoundField HeaderText="Description" DataField="Description" ItemStyle-CssClass="description-item" />
                <asp:BoundField HeaderText="Quantity" DataField="Quantity" ItemStyle-CssClass="quantity-item" />
                <asp:BoundField HeaderText="UnitCost" DataField="UnitCost" DataFormatString="{0:C}" HtmlEncode="false" ItemStyle-CssClass="unitcost-item" />
                <asp:BoundField HeaderText="Total<br />(w/o Subsidy)" DataField="TotalCost" DataFormatString="{0:C}" HtmlEncode="false" ItemStyle-CssClass="totalcost-item" ReadOnly="true" />
                <asp:BoundField HeaderText="Subsidy" DataField="SubsidyDiscount" DataFormatString="{0:C}" HtmlEncode="false" ReadOnly="true" ItemStyle-CssClass="subsidy-item" />
                <asp:BoundField HeaderText="User<br />Payment" DataField="UserPayment" DataFormatString="{0:C}" HtmlEncode="false" ReadOnly="true" ItemStyle-CssClass="userpayment-item" />
                <asp:CommandField ShowEditButton="true" ButtonType="Link" UpdateText="Update" CancelText="Cancel" EditText="Edit" ItemStyle-CssClass="command-item" />
                <asp:TemplateField>
                    <ItemStyle HorizontalAlign="Center" />
                    <ItemTemplate>
                        <asp:LinkButton runat="server" ID="btnRecalcSubsidy" CommandName="recalc" CommandArgument='<%#Eval("ClientID")%>'>Recalculate</asp:LinkButton>
                    </ItemTemplate>
                </asp:TemplateField>
            </Columns>
            <EmptyDataTemplate>
                -- There is no record for this period --
            </EmptyDataTemplate>
        </asp:GridView>
    </div>
</asp:Content>

<asp:Content runat="server" ID="Content3" ContentPlaceHolderID="scripts">
    <script>
        $(window).scrollposition();

        $(".date-picker").datepicker({
            "dateFormat": "m/d/yy"
        });

        $(".calendar-icon").on("click", function (e) {
            $(".apply-period").datepicker("show");
        });
    </script>
</asp:Content>
