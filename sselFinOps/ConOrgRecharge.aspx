<%@ Page Title="" Language="C#" MasterPageFile="~/FinOpsMaster.Master" AutoEventWireup="true" CodeBehind="ConOrgRecharge.aspx.cs" Inherits="sselFinOps.ConOrgRecharge" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <style type="text/css">
        .inactive {
            color: #FF0000;
            text-decoration: line-through;
        }

        .messages {
            font-style: italic;
            color: #808080;
        }

        .org-recharge-table td {
            height: 26px;
        }
    </style>
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    <div class="section">
        <h2>Configure Recharge Accounts</h2>
        <input type="hidden" runat="server" id="hidAjaxUrl" class="ajax-url" />
        <div style="margin-bottom: 10px; color: #002244;">
            Use this page to override the default recharge account for an organization. The default is normally determined by the organization's Charge Type (Internal, External Academic, or External Non-Academic). You may edit the Enable Date by clicking it. Pressing the Enter or Tab keys will save, or pressing Esc will cancel.
        </div>
        <div style="margin-bottom: 10px; color: #002244; font-weight: bold;">
            <asp:HyperLink runat="server" NavigateUrl="~">&larr; Back to Main Page</asp:HyperLink>
        </div>
        <hr />
        <div class="criteria">
            <strong>Add or Update Recharge Account</strong>
            <table class="criteria-table">
                <tr>
                    <td>Organization</td>
                    <td>
                        <asp:DropDownList runat="server" ID="ddlOrg" DataTextField="OrgName" DataValueField="OrgID"></asp:DropDownList>
                    </td>
                </tr>
                <tr>
                    <td>Account</td>
                    <td>
                        <asp:DropDownList runat="server" ID="ddlAccount" DataTextField="AccountName" DataValueField="AccountID"></asp:DropDownList>
                    </td>
                </tr>
            </table>
            <div class="criteria-item">
                <asp:Button runat="server" ID="btnAddOrUpdate" Text="Add or Update" OnClick="btnAddOrUpdate_Click" />
            </div>
        </div>
        <hr />
        <div style="width: 1000px; margin-top: 15px;">
            <asp:Repeater runat="server" ID="rptOrgRecharge">
                <HeaderTemplate>
                    <table class="org-recharge-table" style="width: 100%;">
                        <thead>
                            <tr>
                                <th>Organization</th>
                                <th>Account</th>
                                <th>ShortCode</th>
                                <th>Project</th>
                                <th>Enable Date</th>
                                <th style="width: 20px;">&nbsp;</th>
                            </tr>
                        </thead>
                        <tbody>
                </HeaderTemplate>
                <ItemTemplate>
                    <tr data-org-recharge-id='<%#Eval("OrgRechargeID")%>'>
                        <td><%#Eval("OrgName")%></td>
                        <td class='<%#Eval("AccountCssClass")%>'><%#Eval("AccountName")%></td>
                        <td><%#Eval("ShortCode")%></td>
                        <td><%#Eval("Project")%></td>
                        <td class="enable-date"><%#Eval("EnableDate")%></td>
                        <td style="text-align: center;">
                            <asp:ImageButton runat="server" ID="btnDisable" ImageUrl='<%#GetStaticUrl("images/delete.png")%>' CommandArgument='<%#Eval("OrgRechargeID")%>' CommandName="disable" OnCommand="Row_Command" ToolTip="Disable" />
                        </td>
                    </tr>
                </ItemTemplate>
                <FooterTemplate>
                    </tbody>
                </table>
                </FooterTemplate>
            </asp:Repeater>
            <div style="clear: both;"></div>
            <div class="messages"></div>
        </div>
    </div>
</asp:Content>

<asp:Content ID="Content3" ContentPlaceHolderID="scripts" runat="server">
    <script type="text/javascript">
        $(".org-recharge-table").dataTable({
            "pagingType": "full_numbers",
            "columnDefs": [
                { "targets": [5], "orderable": false, "searchable": false }
            ],
            "drawCallback": function (oSettings) {
                $(".messages").html("");
                if ($(".org-recharge-table td.inactive").length > 0) {
                    $(".messages").html("A red account name with a line through it means the account is inactive.");
                }
            },
            "initComplete": function (oSettings, json) {
                $(".paging_full_numbers", this.closest(".dataTables_wrapper")).css({
                    "margin-top": "10px",
                    "-webkit-touch-callout": "none",
                    "-webkit-user-select": "none",
                    "-khtml-user-select": "none",
                    "-moz-user-select": "none",
                    "-ms-user-select": "none",
                    "user-select": "none"
                });
            }
        });

        var saveEnableDate = function (cell, value, callback) {
            var row = cell.closest("tr");
            var id = row.data("org-recharge-id");
            $.ajax({
                "url": $(".ajax-url").val(),
                "type": "POST",
                "data": { "Command": "udpate-org-recharge", "OrgRechargeID": id, "EnableDate": value },
                "dataType": "json",
                "success": function (data, textStatus, xhr) {
                    if (data.Success)
                        cell.html(data.Data);
                    else {
                        cell.html(cell.data("orig"));
                        alert(data.Message);
                    }
                },
                "error": function (xhr, textStatus, errorThrown) {
                    cell.html(cell.data("orig"));
                    alert(errorThrown);
                    console.log(xhr.responseText);
                },
                "complete": function (xhr, textStatus) {
                    if (typeof callback == "function")
                        callback();
                }
            });
        }

        var resetCell = function (cell) {
            cell.bind("click", handleCellClick);
            cell.data("orig", null);
        }

        var handleTextBoxEvent = function (cell, textbox) {
            var orig = cell.data("orig");
            var update = textbox.val();
            if (!isNaN(new Date(update).valueOf())) {
                saveEnableDate(cell, update, function () {
                    resetCell(cell);
                });
            }
            else {
                cell.html(orig);
                resetCell(cell);
            }
        }

        var handleCellClick = function (e) {
            var cell = $(this);
            cell.unbind("click");
            var orig = cell.text();
            cell.data("orig", orig);
            var textbox = $("<input/>").attr("type", "text").val(orig).on("keydown", function (e) {
                console.log(e.keyCode);
                if (e.keyCode == 13) {
                    e.preventDefault();
                    handleTextBoxEvent(cell, $(this));
                }
                else if (e.keyCode == 27) {
                    e.preventDefault();
                    cell.html(orig);
                    resetCell(cell);
                }
            }).on("blur", function (e) {
                handleTextBoxEvent(cell, $(this));
            });
            cell.html(textbox);
            textbox.focus().select();
        }

        $(".enable-date").on("click", handleCellClick);
    </script>
</asp:Content>
