<%@ Page Title="Configure Auxilliary Costs" Language="C#" MasterPageFile="~/FinOpsMaster.Master" AutoEventWireup="true" CodeBehind="AuxCostConfiguration.aspx.cs" Inherits="sselFinOps.AuxCostConfiguration" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    <div class="section">
        <h2>Configure  Auxilliary Cost Parameters</h2>
        <table class="report-table" border="1">
            <tr>
                <td class="GridHeader" colspan="2">Room Cost Auxilliary Parameters</td>
            </tr>
            <tr>
                <td colspan="2">
                    <asp:DataGrid runat="server" ID="dgRoomAuxCost" CssClass="aux-cost-table" GridLines="None" ShowFooter="True" AutoGenerateColumns="false" OnItemCommand="DgRoomAuxCost_ItemCommand" OnItemDataBound="DgRoomAuxCost_ItemDataBound">
                        <HeaderStyle CssClass="aux-cost-header" />
                        <ItemStyle CssClass="aux-cost-row" />
                        <AlternatingItemStyle CssClass="aux-cost-altrow" />
                        <FooterStyle CssClass="aux-cost-footer" />
                        <Columns>
                            <asp:TemplateColumn HeaderText="Parameter">
                                <ItemTemplate>
                                    <asp:Label runat="server" ID="lblRoomAuxCost"></asp:Label>
                                </ItemTemplate>
                                <FooterTemplate>
                                    <asp:TextBox runat="server" ID="txtRoomAuxCost"></asp:TextBox>
                                </FooterTemplate>
                            </asp:TemplateColumn>
                            <asp:TemplateColumn HeaderText="Allow Per Use">
                                <HeaderStyle HorizontalAlign="Center"></HeaderStyle>
                                <ItemStyle HorizontalAlign="Center"></ItemStyle>
                                <FooterStyle HorizontalAlign="Center"></FooterStyle>
                                <ItemTemplate>
                                    <asp:Label runat="server" ID="lblRoomPerUse"></asp:Label>
                                </ItemTemplate>
                                <FooterTemplate>
                                    <asp:CheckBox runat="server" ID="chkRoomPerUse" Checked="False"></asp:CheckBox>
                                </FooterTemplate>
                            </asp:TemplateColumn>
                            <asp:TemplateColumn HeaderText="Allow Per Period">
                                <HeaderStyle HorizontalAlign="Center"></HeaderStyle>
                                <ItemStyle HorizontalAlign="Center"></ItemStyle>
                                <FooterStyle HorizontalAlign="Center"></FooterStyle>
                                <ItemTemplate>
                                    <asp:Label runat="server" ID="lblRoomPerPeriod"></asp:Label>
                                </ItemTemplate>
                                <FooterTemplate>
                                    <asp:CheckBox runat="server" ID="chkRoomPerPeriod" Checked="False"></asp:CheckBox>
                                </FooterTemplate>
                            </asp:TemplateColumn>
                            <asp:TemplateColumn HeaderText="Description">
                                <ItemTemplate>
                                    <asp:Label runat="server" ID="lblRoomDescription"></asp:Label>
                                </ItemTemplate>
                                <FooterTemplate>
                                    <asp:TextBox runat="server" ID="txtRoomDescription"></asp:TextBox>
                                </FooterTemplate>
                            </asp:TemplateColumn>
                            <asp:TemplateColumn>
                                <FooterStyle HorizontalAlign="Center"></FooterStyle>
                                <FooterTemplate>
                                    <asp:Button ID="btnAddRoomParm" runat="server" Text="ADD" CommandName="AddNewRow"></asp:Button>
                                </FooterTemplate>
                            </asp:TemplateColumn>
                        </Columns>
                    </asp:DataGrid>
                </td>
            </tr>
            <tr>
                <td class="GridHeader" colspan="2">Tool Cost Auxilliary Parameters</td>
            </tr>
            <tr>
                <td colspan="2">
                    <asp:DataGrid runat="server" ID="dgToolAuxCost" CssClass="aux-cost-table" GridLines="None" ShowFooter="True" AutoGenerateColumns="False" OnItemCommand="DgToolAuxCost_ItemCommand" OnItemDataBound="DgToolAuxCost_ItemDataBound">
                        <HeaderStyle CssClass="aux-cost-header" />
                        <ItemStyle CssClass="aux-cost-row" />
                        <AlternatingItemStyle CssClass="aux-cost-altrow" />
                        <FooterStyle CssClass="aux-cost-footer" />
                        <Columns>
                            <asp:TemplateColumn HeaderText="Parameter">
                                <ItemTemplate>
                                    <asp:Label runat="server" ID="lblToolAuxCost"></asp:Label>
                                </ItemTemplate>
                                <FooterTemplate>
                                    <asp:TextBox runat="server" ID="txtToolAuxCost"></asp:TextBox>
                                </FooterTemplate>
                            </asp:TemplateColumn>
                            <asp:TemplateColumn HeaderText="Allow Per Use">
                                <HeaderStyle HorizontalAlign="Center"></HeaderStyle>
                                <ItemStyle HorizontalAlign="Center"></ItemStyle>
                                <FooterStyle HorizontalAlign="Center"></FooterStyle>
                                <ItemTemplate>
                                    <asp:Label runat="server" ID="lblToolPerUse"></asp:Label>
                                </ItemTemplate>
                                <FooterTemplate>
                                    <asp:CheckBox runat="server" ID="chkToolPerUse" Checked="False"></asp:CheckBox>
                                </FooterTemplate>
                            </asp:TemplateColumn>
                            <asp:TemplateColumn HeaderText="Allow Per Period">
                                <HeaderStyle HorizontalAlign="Center"></HeaderStyle>
                                <ItemStyle HorizontalAlign="Center"></ItemStyle>
                                <FooterStyle HorizontalAlign="Center"></FooterStyle>
                                <ItemTemplate>
                                    <asp:Label runat="server" ID="lblToolPerPeriod"></asp:Label>
                                </ItemTemplate>
                                <FooterTemplate>
                                    <asp:CheckBox runat="server" ID="chkToolPerPeriod" Checked="False"></asp:CheckBox>
                                </FooterTemplate>
                            </asp:TemplateColumn>
                            <asp:TemplateColumn HeaderText="Description">
                                <ItemTemplate>
                                    <asp:Label runat="server" ID="lblToolDescription"></asp:Label>
                                </ItemTemplate>
                                <FooterTemplate>
                                    <asp:TextBox runat="server" ID="txtToolDescription"></asp:TextBox>
                                </FooterTemplate>
                            </asp:TemplateColumn>
                            <asp:TemplateColumn>
                                <FooterStyle HorizontalAlign="Center"></FooterStyle>
                                <FooterTemplate>
                                    <asp:Button ID="btnAddToolParm" runat="server" Text="ADD" CommandName="AddNewRow"></asp:Button>
                                </FooterTemplate>
                            </asp:TemplateColumn>
                        </Columns>
                    </asp:DataGrid>
                </td>
            </tr>
            <tr>
                <td colspan="2" class="GridHeader">Store Cost Auxilliary Parameters</td>
            </tr>
            <tr>
                <td colspan="2">
                    <asp:DataGrid runat="server" ID="dgStoreAuxCost" CssClass="aux-cost-table" GridLines="None" ShowFooter="True" AutoGenerateColumns="False" OnItemCommand="DgStoreAuxCost_ItemCommand" OnItemDataBound="DgStoreAuxCost_ItemDataBound">
                        <HeaderStyle CssClass="aux-cost-header" />
                        <ItemStyle CssClass="aux-cost-row" />
                        <AlternatingItemStyle CssClass="aux-cost-altrow" />
                        <FooterStyle CssClass="aux-cost-footer" />
                        <Columns>
                            <asp:TemplateColumn HeaderText="Parameter">
                                <ItemTemplate>
                                    <asp:Label runat="server" ID="lblStoreAuxCost"></asp:Label>
                                </ItemTemplate>
                                <FooterTemplate>
                                    <asp:TextBox runat="server" ID="txtStoreAuxCost"></asp:TextBox>
                                </FooterTemplate>
                            </asp:TemplateColumn>
                            <asp:TemplateColumn HeaderText="Allow Per Use">
                                <HeaderStyle HorizontalAlign="Center"></HeaderStyle>
                                <ItemStyle HorizontalAlign="Center"></ItemStyle>
                                <FooterStyle HorizontalAlign="Center"></FooterStyle>
                                <ItemTemplate>
                                    <asp:Label runat="server" ID="lblStorePerUse"></asp:Label>
                                </ItemTemplate>
                                <FooterTemplate>
                                    <asp:CheckBox runat="server" ID="chkStorePerUse" Checked="False"></asp:CheckBox>
                                </FooterTemplate>
                            </asp:TemplateColumn>
                            <asp:TemplateColumn HeaderText="Allow Per Period">
                                <HeaderStyle HorizontalAlign="Center"></HeaderStyle>
                                <ItemStyle HorizontalAlign="Center"></ItemStyle>
                                <FooterStyle HorizontalAlign="Center"></FooterStyle>
                                <ItemTemplate>
                                    <asp:Label runat="server" ID="lblStorePerPeriod"></asp:Label>
                                </ItemTemplate>
                                <FooterTemplate>
                                    <asp:CheckBox runat="server" ID="chkStorePerPeriod" Checked="False"></asp:CheckBox>
                                </FooterTemplate>
                            </asp:TemplateColumn>
                            <asp:TemplateColumn HeaderText="Description">
                                <ItemTemplate>
                                    <asp:Label runat="server" ID="lblStoreDescription"></asp:Label>
                                </ItemTemplate>
                                <FooterTemplate>
                                    <asp:TextBox runat="server" ID="txtStoreDescription"></asp:TextBox>
                                </FooterTemplate>
                            </asp:TemplateColumn>
                            <asp:TemplateColumn>
                                <FooterStyle HorizontalAlign="Center"></FooterStyle>
                                <FooterTemplate>
                                    <asp:Button ID="btnAddStoreParm" runat="server" Text="ADD" CommandName="AddNewRow"></asp:Button>
                                </FooterTemplate>
                            </asp:TemplateColumn>
                        </Columns>
                    </asp:DataGrid>
                </td>
            </tr>
            <tr>
                <td colspan="2">
                    <asp:Button runat="server" ID="btnSave" CssClass="report-button" Text="Save Changes" OnClick="BtnSave_Click" />
                    <asp:LinkButton runat="server" ID="btnBack" Text="&larr; Back to Main Page" OnClick="BackButton_Click"></asp:LinkButton>
                </td>
            </tr>
        </table>
    </div>
</asp:Content>

<asp:Content runat="server" ID="Content3" ContentPlaceHolderID="scripts">
</asp:Content>
