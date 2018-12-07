<%@ Page Title="" Language="C#" MasterPageFile="~/FinOpsMaster.Master" AutoEventWireup="true" CodeBehind="DatHistorical.aspx.cs" Inherits="sselFinOps.DatHistorical" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <style type="text/css">
        .period {
            font-weight: bold;
            background-color: #CCCCFF;
        }

        .header {
            font-size: small;
            font-family: Arial;
            font-weight: bold;
            background-color: #FFFF00;
        }
    </style>
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    <div class="section">
        <h2>Historical Report</h2>
        <div class="criteria">
            <table class="criteria-table">
                <tr>
                    <td>Organization:</td>
                    <td>
                        <asp:DropDownList runat="server" ID="ddlOrg" CssClass="report-select" AutoPostBack="True" OnSelectedIndexChanged="DdlOrg_SelectedIndexChanged">
                        </asp:DropDownList>
                    </td>
                </tr>
                <tr>
                    <td>Time frame:</td>
                    <td>
                        <asp:RadioButtonList runat="server" ID="rblTimeFrame" RepeatDirection="Horizontal" TextAlign="Left" AutoPostBack="True" OnSelectedIndexChanged="RblTimeFrame_SelectedIndexChanged">
                            <asp:ListItem Value="-3">3 months</asp:ListItem>
                            <asp:ListItem Value="-6">6 months</asp:ListItem>
                            <asp:ListItem Value="-12" Selected="True">1 Year</asp:ListItem>
                            <asp:ListItem Value="-24">2 Years</asp:ListItem>
                            <asp:ListItem Value="-36">3 Years</asp:ListItem>
                            <asp:ListItem Value="0">All time</asp:ListItem>
                        </asp:RadioButtonList>
                    </td>
                </tr>
                <tr>
                    <td>Select an account:</td>
                    <td>
                        <asp:DropDownList runat="server" ID="ddlAccount" CssClass="report-select" Width="300" AutoPostBack="True" OnSelectedIndexChanged="DdlAccount_SelectedIndexChanged">
                        </asp:DropDownList>
                        <span style="padding-left: 10px;">
                            <asp:RadioButtonList runat="server" ID="rblAcctDisplay" RepeatDirection="Horizontal" TextAlign="Left" AutoPostBack="True" RepeatLayout="Flow" OnSelectedIndexChanged="RblAcctDisplay_SelectedIndexChanged">
                                <asp:ListItem Value="Name" Selected="True">Name</asp:ListItem>
                                <asp:ListItem Value="Number">Number</asp:ListItem>
                                <asp:ListItem Value="Project">Project</asp:ListItem>
                                <asp:ListItem Value="ShortCode">Short Code</asp:ListItem>
                            </asp:RadioButtonList>
                        </span>
                    </td>
                </tr>
            </table>
            <div class="criteria-item" style="color: #FF0000;">
                NOTE: Since this report is done by account, it does not reflect cost caps
            </div>
            <div class="criteria-item">
                <asp:LinkButton runat="server" ID="btnBack" Text="&larr; Back to Main Page" OnClick="BackButton_Click" CssClass="back-link"></asp:LinkButton>
            </div>
        </div>
    </div>
    <div class="section">
        <table runat="server" id="Table1" border="1" visible="false">
            <tr>
                <td>
                    <asp:Label ID="lblWarning" runat="server" CssClass="WarningText" Visible="False">No charges for the selected period</asp:Label>
                </td>
            </tr>
            <tr>
                <td>
                    <asp:DataGrid runat="server" ID="dgReport" AutoGenerateColumns="False" OnItemDataBound="DgReport_ItemDataBound">
                        <HeaderStyle CssClass="header" />
                        <Columns>
                            <asp:BoundColumn DataField="Period" HeaderText="Period" DataFormatString="{0: MMM yyyy}">
                                <HeaderStyle Width="100px" />
                            </asp:BoundColumn>
                            <asp:BoundColumn DataField="DisplayName" HeaderText="User">
                                <HeaderStyle Width="150px" />
                            </asp:BoundColumn>
                            <asp:BoundColumn DataField="Room" HeaderText="Room Cost" DataFormatString="{0:$#,##0.00}">
                                <HeaderStyle Width="90px" />
                            </asp:BoundColumn>
                            <asp:BoundColumn DataField="Tool" HeaderText="Tool Cost" DataFormatString="{0:$#,##0.00}">
                                <HeaderStyle Width="90px" />
                            </asp:BoundColumn>
                            <asp:BoundColumn DataField="StoreInv" HeaderText="Store Cost" DataFormatString="{0:$#,##0.00}">
                                <HeaderStyle Width="90px" />
                            </asp:BoundColumn>
                            <asp:BoundColumn DataField="Misc" HeaderText="Misc Cost" DataFormatString="{0:$#,##0.00}">
                                <HeaderStyle Width="90px" />
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
