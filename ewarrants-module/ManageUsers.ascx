<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="ManageUsers.ascx.cs" Inherits="tjc.Modules.eWarrants.ManageUsers" %>
<%@ Register TagPrefix="dnn" TagName="Label" Src="~/controls/LabelControl.ascx" %>
<%@ Register TagPrefix="dnn" Namespace="DotNetNuke.Web.Client.ClientResourceManagement" Assembly="DotNetNuke.Web.Client" %>
<div class="dnnForm dnnClear">
    <div class="btn-group" role="group" aria-label="Manage User Toolbar">
        <asp:HyperLink ID="lnkAddUser" runat="server" CssClass="btn btn-primary"><em class="fa fa-plus-circle"></em> Add New User</asp:HyperLink>
        <asp:LinkButton ID="btnExistingUser" OnClick="btnExistingUser_Click" runat="server" Text="<em class='fa fa-user'></em> Add Existing User" CausesValidation="false" CssClass="btn btn-success" />
    </div>
    <asp:UpdatePanel ID="dptUpdate" runat="server" RenderMode="Block" OnUnload="UpdatePanel_Unload">
        <ContentTemplate>

            <asp:Repeater runat="server" ID="rptUsers" OnItemCommand="rptUsers_ItemCommand" OnItemCreated="rptUsers_ItemCreated" OnItemDataBound="rptUsers_ItemDataBound">
                <HeaderTemplate>

                    <table id="users" summary="List of Users" class="table table-striped">
                        <thead class="bg-primary">
                            <tr>
                                <th>Display Name
                                </th>
                                <th>Username
                                </th>
                                <th class="text-center">Is Admin</th>
                                <th class="text-center">Delete</th>
                            </tr>
                        </thead>
                        <tbody>
                </HeaderTemplate>

                <ItemTemplate>
                    <tr>
                        <td><%#DataBinder.Eval(Container.DataItem, "DisplayName")%></td>
                        <td><%#DataBinder.Eval(Container.DataItem, "Username")%></td>
                        <td class="command command-icon">
                            <asp:LinkButton ID="cmdSetAdmin" runat="server" CommandName="setadmin" CommandArgument='<%#Eval("userId") %>'>
                                <em class="fa fa-square">&nbsp;</em>
                            </asp:LinkButton>

                        </td>
                        <td class="command command-icon">
                            <asp:LinkButton ID="cmdDelete" runat="server" CommandName="delete" OnClientClick="return confirm('Delete this User?');" CommandArgument='<%#Eval("userId") %>'>
                                <em class="fa fa-trash">&nbsp;</em>
                            </asp:LinkButton>

                        </td>

                    </tr>

                </ItemTemplate>
                <FooterTemplate>
                    </tbody></table>
                </FooterTemplate>
            </asp:Repeater>
        </ContentTemplate>
    </asp:UpdatePanel>

    <ul class="dnnActions dnnClear">
        <li>
            <asp:HyperLink ID="lnkCancel" runat="server" class="btn btn-danger" Text="Return" /></li>
    </ul>
</div>
<dnn:DnnJsInclude runat="server" FilePath="https://cdn.datatables.net/1.11.3/js/jquery.dataTables.min.js" />
<dnn:DnnJsInclude runat="server" FilePath="https://cdn.datatables.net/1.11.3/js/dataTables.bootstrap5.min.js" />
<dnn:DnnCssInclude runat="server" FilePath="https://cdn.datatables.net/1.11.3/css/dataTables.bootstrap5.min.css" />


<script type="text/javascript">
    function pageLoad(sender, args) {
        $(document).ready(function () {
            $('.TitleH2').html('<%=ModuleTitle%>');
            $('[data-toggle="tooltip"]').tooltip()
            bindDataTable();
            Sys.WebForms.PageRequestManager.getInstance().add_endRequest(bindDataTable); // bind data table on every UpdatePanel refresh
        });
    }

    function bindDataTable() {
        var users = $('#users').DataTable({
            "aoColumns": [
                null,
                null,
                { "bSortable": false },
                { "bSortable": false }
            ],
            "bStateSave": true,
            retrieve: true,
            paging: false
        });
        $('#users_wrapper label').css("font-weight:700");
    }

</script>
