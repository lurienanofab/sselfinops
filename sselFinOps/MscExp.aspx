<%@ Page Title="" Language="C#" MasterPageFile="~/FinOpsMaster.Master" AutoEventWireup="true" CodeBehind="MscExp.aspx.cs" Inherits="sselFinOps.MscExp" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    <div class="section">
        <h2>Historical Report</h2>
        <div class="criteria">
            <table class="criteria-table">
                <tr>
                    <td>Start period:</td>
                    <td>
                        <lnf:PeriodPicker runat="server" ID="pp1" />
                        <span style="padding-left: 20px;">Number of months:</span>
                        <asp:TextBox runat="server" ID="txtNumMonths" MaxLength="12" Width="30" CssClass="report-text">1</asp:TextBox>
                    </td>
                </tr>
            </table>
            <div class="criteria-item">
                <asp:LinkButton runat="server" ID="btnConRoomCost" CommandArgument="Room" Text="Configure Exp Room Charges" OnCommand="ConfigureCost_Command" />
                |
                <asp:LinkButton runat="server" ID="btnConToolCost" CommandArgument="Tool" Text="Configure Exp Tool Charges" OnCommand="ConfigureCost_Command" />
                |
                <asp:LinkButton runat="server" ID="btnConFormula" Text="Configure Exp Costing Formulas" OnClick="BtnConFormula_Click" />
            </div>
            <div class="criteria-item">
                <asp:Button runat="server" ID="btnReport" CssClass="report-button" Text="Caclulate Experimental Costs" OnClick="BtnReport_Click" />
                <asp:LinkButton runat="server" ID="btnBack" Text="&laquo; Back to Main Page" OnClick="BackButton_Click" CssClass="back-link"></asp:LinkButton>
            </div>
        </div>
    </div>
    <div class="section">
        <table runat="server" id="Table1" border="1" visible="false">
            <tr>
                <td>
                    <asp:DataGrid ID="dgCost" runat="server" BorderColor="LightGray" BorderStyle="Ridge" BorderWidth="1px" HeaderStyle-Wrap="false" HeaderStyle-Font-Bold="true" HeaderStyle-BackColor="LightGrey" AlternatingItemStyle-BackColor="Linen" CellPadding="2" GridLines="Horizontal" AutoGenerateColumns="False">
                        <FooterStyle CssClass="GridText" BackColor="LightGray"></FooterStyle>
                        <EditItemStyle CssClass="GridText" BackColor="#66FFFF"></EditItemStyle>
                        <AlternatingItemStyle CssClass="GridText" BackColor="Linen"></AlternatingItemStyle>
                        <ItemStyle CssClass="GridText" BackColor="White"></ItemStyle>
                        <HeaderStyle Font-Bold="True" Wrap="False" CssClass="GridText" BackColor="LightGray"></HeaderStyle>
                        <Columns>
                            <asp:BoundColumn DataField="DisplayName" HeaderText="Client Name">
                                <HeaderStyle Width="200px" />
                            </asp:BoundColumn>
                            <asp:BoundColumn DataField="OrgName" HeaderText="Org Name">
                                <HeaderStyle Width="200px" />
                            </asp:BoundColumn>
                            <asp:BoundColumn DataField="RoomCost" DataFormatString="{0:$#,##0}" HeaderText="Room Costs" ItemStyle-HorizontalAlign="Right">
                                <HeaderStyle Width="70px" />
                            </asp:BoundColumn>
                            <asp:BoundColumn DataField="ToolCost" DataFormatString="{0:$#,##0}" HeaderText="Tool Costs" ItemStyle-HorizontalAlign="Right">
                                <HeaderStyle Width="70px" />
                            </asp:BoundColumn>
                        </Columns>
                    </asp:DataGrid>
                </td>
            </tr>
        </table>
    </div>
</asp:Content>

<asp:Content ID="Content3" ContentPlaceHolderID="scripts" runat="server">
</asp:Content>
