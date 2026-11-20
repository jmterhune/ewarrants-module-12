<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="ManageAgencyUsers.ascx.cs" Inherits="tjc.Modules.eWarrants.ManageAgencyUsers" %>
<%@ Register TagPrefix="dnn" TagName="Label" Src="~/controls/LabelControl.ascx" %>
<%@ Register TagPrefix="dnn" Namespace="DotNetNuke.Web.Client.ClientResourceManagement" Assembly="DotNetNuke.Web.Client" %>
<div class="dnnForm dnnClear">
    <asp:UpdatePanel ID="dptUpdate" runat="server" RenderMode="Block" OnUnload="UpdatePanel_Unload">
        <ContentTemplate>
            <asp:HyperLink ID="lnkAddUser" runat="server" CssClass="btn btn-primary"><em class="fa fa-plus-circle"></em> Add New Agency User</asp:HyperLink>

            <asp:Repeater runat="server" ID="rptUsers" OnItemCommand="rptUsers_ItemCommand" OnItemCreated="rptUsers_ItemCreated" OnItemDataBound="rptUsers_ItemDataBound" >
                <HeaderTemplate>

                    <table id="users" summary="List of Users" class="table table-striped">
                        <thead class="bg-primary">
                            <tr>
                                <th>DisplayName
                                </th>
                                <th>Username
                                </th>
                                <th class="text-center">Reset Password</th>
                                <th class="text-center">Change Email</th>
                                <th class="text-center">Is Admin</th>
                                <th class="text-center">Delete</th>
                            </tr>
                        </thead>
                        <tbody>
                </HeaderTemplate>

                <ItemTemplate>
                    <tr id="tablerow" runat="server">
                        <td><%#DataBinder.Eval(Container.DataItem, "DisplayName")%></td>
                        <td><%#DataBinder.Eval(Container.DataItem, "Username")%></td>
                        <td class="command command-icon">
                            <asp:LinkButton ID="cmdPasswordReset" runat="server"  CommandName="reset" CommandArgument='<%#Eval("userId") %>'>
                                <em class="fa fa-key">&nbsp;</em>
                            </asp:LinkButton>

                        </td>
                        <td class="command command-icon">
                            <asp:LinkButton ID="cmdEmailEdit" runat="server"  CommandName="email" CommandArgument='<%#Eval("userId") %>'>
                                <em class="fa fa-envelope">&nbsp;</em>
                            </asp:LinkButton>

                        </td>
                        <td class="command command-icon">
                            <asp:LinkButton ID="cmdSetAdmin" runat="server"  CommandName="setadmin" CommandArgument='<%#Eval("userId") %>'>
                                <em class="fa fa-square">&nbsp;</em>
                            </asp:LinkButton>

                        </td>
                        <td class="command command-icon">
                            <asp:LinkButton ID="cmdDelete" runat="server" CommandName="delete"  OnClientClick="return confirm('Delete this User?');" CommandArgument='<%#Eval("userId") %>'>
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
    <div class="clearfix" style="margin-top:10px;">
        <div class="alert alert-danger">***A maximum of three users can be assigned admin rights.***</div>
        <ul class="dnnActions dnnClear">

            <li>
                <asp:HyperLink ID="lnkCancel" runat="server" class="btn btn-danger" Text="Return" /></li>
        </ul>
    </div>
</div>

<dnn:DnnJsInclude runat="server" FilePath="/Resources/Libraries/DataTables/jquery.dataTables.min.js" />
<dnn:DnnJsInclude runat="server" FilePath="/Resources/Libraries/DataTables/dataTables.bootstrap5.min.js" />
<dnn:DnnCssInclude runat="server" FilePath="/Resources/Libraries/DataTables/dataTables.bootstrap5.min.css" />


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
                { "bSortable": false },
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
