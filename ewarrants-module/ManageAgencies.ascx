<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="ManageAgencies.ascx.cs" Inherits="tjc.Modules.eWarrants.ManageAgencies" %>
<%@ Register TagPrefix="dnn" TagName="Label" Src="~/controls/LabelControl.ascx" %>
<%@ Register TagPrefix="dnn" Namespace="DotNetNuke.Web.Client.ClientResourceManagement" Assembly="DotNetNuke.Web.Client" %>
<asp:UpdatePanel ID="dptUpdate" runat="server" RenderMode="Block" OnUnload="UpdatePanel_Unload">
    <ContentTemplate>
        <asp:HyperLink ID="lnkAddAgency" runat="server" CssClass="btn btn-primary"><em class="fa fa-plus-circle"></em> Add New Agency</asp:HyperLink>

        <asp:Repeater runat="server" ID="rptAgencies" OnItemCommand="rptAgencies_ItemCommand" OnItemCreated="rptAgencies_ItemCreated" OnItemDataBound="rptAgencies_ItemDataBound">
            <HeaderTemplate>
                <table id="agencies" summary="List of Agencies" class="table table-striped">
                    <thead class="bg-primary">
                        <tr>
                            <th>&nbsp;</th>
                            <th>&nbsp;</th>
                            <th>Agency Name</th>
                            <th>Parent Agency</th>
                            <th>County</th>
                            <th>Email</th>
                            <th>&nbsp;</th>
                        </tr>
                    </thead>
                    <tbody>
            </HeaderTemplate>

            <ItemTemplate>
                <tr>
                    <td class="command command-icon">
                        <asp:HyperLink ID="lnkEdit" runat="server">
                            <em class="fa fa-pencil">&nbsp;</em>
                        </asp:HyperLink>
                    </td>
                    <td class="command command-icon">
                        <asp:HyperLink ID="lnkUsers" runat="server">
                            <em class="fa fa-users">&nbsp;</em>
                        </asp:HyperLink>
                    </td>
                    <td><%#DataBinder.Eval(Container.DataItem, "AgencyName")%></td>
                    <td><%#DataBinder.Eval(Container.DataItem, "ParentAgencyName")%></td>
                    <td><%#DataBinder.Eval(Container.DataItem, "CountyList")%></td>
                    <td><a href='mailto:<%#DataBinder.Eval(Container.DataItem, "EmailAddress")%>'><%#DataBinder.Eval(Container.DataItem, "EmailAddress")%></a></td>
                    <td class="command command-icon">
                        <asp:LinkButton ID="cmdDelete" runat="server" CommandName="delete" OnClientClick="return confirm('Delete this Agency?');" CommandArgument='<%#Eval("agencyId") %>'>
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

<dnn:DnnJsInclude runat="server" FilePath="https://cdn.datatables.net/1.11.3/js/jquery.dataTables.min.js" />
<dnn:DnnJsInclude runat="server" FilePath="https://cdn.datatables.net/1.11.3/js/dataTables.bootstrap5.min.js" />
<dnn:DnnCssInclude runat="server" FilePath="https://cdn.datatables.net/1.11.3/css/dataTables.bootstrap5.min.css" />


<script type="text/javascript">
    function pageLoad(sender, args) {
        $(document).ready(function () {
            $('[data-toggle="tooltip"]').tooltip()
            bindDataTable();
            Sys.WebForms.PageRequestManager.getInstance().add_endRequest(bindDataTable); // bind data table on every UpdatePanel refresh
        });
    }

    function bindDataTable() {
        var agencies = $('#agencies').DataTable({
            "aoColumns": [
                { "bSortable": false },
                { "bSortable": false },
                null,
                null,
                null,
                null,
                { "bSortable": false }
            ],
            "bStateSave": true,
            retrieve: true,
            paging: false
        });
        $('#agencies_wrapper label').css("font-weight:700");
    }

</script>
