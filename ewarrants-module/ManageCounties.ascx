<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="ManageCounties.ascx.cs" Inherits="tjc.Modules.eWarrants.ManageCounties" %>
<%@ Register TagPrefix="dnn" TagName="Label" Src="~/controls/LabelControl.ascx" %>
<%@ Register TagPrefix="dnn" Namespace="DotNetNuke.Web.Client.ClientResourceManagement" Assembly="DotNetNuke.Web.Client" %>
<div id="listsForm">
    <asp:UpdatePanel ID="dptUpdate" runat="server" RenderMode="Block" OnUnload="UpdatePanel_Unload">
        <ContentTemplate>
            <asp:Literal ID="ltMessage" runat="server" />
            <div class="form-inline">
                <div class="form-group col-auto">
                    <asp:Label ID="lblCounty" CssClass="control-label" runat="server" AssociatedControlID="txtCounty">County Name:</asp:Label>
                    <asp:TextBox ID="txtCounty" runat="server" MaxLength="50" CssClass="form-control" />
                    <asp:HiddenField ID="hdCountyId" runat="server" />
                </div>
                <div class="form-group col-auto">
                    <asp:Button ID="cmdSaveCounty" runat="server" OnClick="cmdSaveCounty_Click" CssClass="btn btn-primary" Text="Add County" />
                    <asp:HyperLink ID="lnkCancelCounty" Visible="false" runat="server" CssClass="btn btn-danger" Text="Cancel" />
                </div>
            </div>
            <asp:Repeater runat="server" ID="rptCounties" OnItemCommand="rptCounties_ItemCommand" OnItemCreated="rptCounties_ItemCreated">
                <HeaderTemplate>

                    <table id="counties" summary="List of Counties" class="table table-striped">
                        <thead class="bg-primary">
                            <tr>
                                <th>&nbsp;</th>
                                <th>County Name
                                </th>
                                <th>&nbsp;</th>
                            </tr>
                        </thead>
                        <tbody>
                </HeaderTemplate>

                <ItemTemplate>
                    <tr>
                        <td class="command command-icon">
                            <asp:LinkButton ID="cmdEdit" runat="server" CommandName="edit" CommandArgument='<%#Eval("countyId") %>'>
                            <em class="fa fa-pencil">&nbsp;</em>
                            </asp:LinkButton>
                        </td>

                        <td><%#DataBinder.Eval(Container.DataItem, "CountyName")%></td>
                        <td class="command command-icon">
                            <asp:LinkButton ID="cmdDelete" runat="server" CommandName="delete" OnClientClick="return confirm('Delete this County?');" CommandArgument='<%#Eval("countyId") %>'>
                            <em class="fa fa-trash">&nbsp;</em>
                            </asp:LinkButton>
                        </td>
                    </tr>
                </ItemTemplate>
                <FooterTemplate>
                    </tbody></table>
                </FooterTemplate>
            </asp:Repeater>
            <hr style="border-color: #ccc; border-style: dashed; margin-bottom: 0px" />
            <h2>Manage Division Types</h2>
            <div class="form-inline">
                <div class="form-group col-auto">
                    <asp:Label ID="lblDivisionType" CssClass="control-label" runat="server" AssociatedControlID="txtDivisionType">Division Type:</asp:Label>
                    <asp:TextBox ID="txtDivisionType" runat="server" MaxLength="50" CssClass="form-control" />
                    <asp:HiddenField ID="hdDivisionTypeId" runat="server" />
                </div>
                <div class="form-group col-auto">
                    <asp:Button ID="cmdAddDivisionType" runat="server" OnClick="cmdAddDivisionType_Click" CssClass="btn btn-primary" Text="Add Division Type" />
                    <asp:HyperLink ID="lnkCancelDivisionType" Visible="false" runat="server" CssClass="btn btn-danger" Text="Cancel" />
                </div>
            </div>

            <asp:Repeater runat="server" ID="rptTypes" OnItemCommand="rptTypes_ItemCommand" OnItemCreated="rptTypes_ItemCreated">
                <HeaderTemplate>

                    <table id="types" summary="List of Division Types" class="table table-striped">
                        <thead class="bg-primary">
                            <tr>
                                <th>&nbsp;
                                </th>
                                <th>Division Type
                                </th>
                                <th>&nbsp;</th>
                            </tr>
                        </thead>
                        <tbody>
                </HeaderTemplate>

                <ItemTemplate>
                    <tr>
                        <td class="command command-icon">
                            <asp:LinkButton ID="cmdEdit" runat="server" CommandName="edit" CommandArgument='<%#Eval("JudgeTypeId") %>'>
                            <em class="fa fa-pencil">&nbsp;</em>
                            </asp:LinkButton>
                        </td>

                        <td><%#DataBinder.Eval(Container.DataItem, "JudgeTypeName")%></td>
                        <td class="command command-icon">
                            <asp:LinkButton ID="cmdDelete" runat="server" CommandName="delete" OnClientClick="return confirm('Delete this Division Type?');" CommandArgument='<%#Eval("JudgeTypeId") %>'>
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
        <Triggers>
            <asp:AsyncPostBackTrigger ControlID="cmdSaveCounty" />
            <asp:AsyncPostBackTrigger ControlID="cmdAddDivisionType" />
        </Triggers>
    </asp:UpdatePanel>

    <ul class="dnnActions dnnClear">
        <li>
            <asp:HyperLink ID="lnkCancel" runat="server" class="btn btn-danger" Text="Return" /></li>
    </ul>
</div>
<dnn:DnnJsInclude runat="server" FilePath="/Resources/Libraries/DataTables/jquery.dataTables.min.js" />
<dnn:DnnJsInclude runat="server" FilePath="/Resources/Libraries/DataTables/dataTables.bootstrap5.min.js" />
<dnn:DnnCssInclude runat="server" FilePath="/Resources/Libraries/DataTables/dataTables.bootstrap5.min.css" />


<script type="text/javascript">
    function pageLoad(sender, args) {
        $(document).ready(function () {
            $('[data-toggle="tooltip"]').tooltip()
            bindDataTable();
            Sys.WebForms.PageRequestManager.getInstance().add_endRequest(bindDataTable); // bind data table on every UpdatePanel refresh
        });
    }

    function bindDataTable() {
        var counties = $('#counties').DataTable({
            "aoColumns": [
                { "bSortable": false },
                null,
                { "bSortable": false }
            ],
            "bStateSave": true,
            retrieve: true,
            paging: false
        });

        var types = $('#types').DataTable({
            "aoColumns": [
                { "bSortable": false },
                null,
                { "bSortable": false }
            ],
            "bStateSave": true,
            retrieve: true,
            paging: false
        });

        $('#users_wrapper label').css("font-weight:700");
    }

</script>
