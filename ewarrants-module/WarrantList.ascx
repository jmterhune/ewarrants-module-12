<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="WarrantList.ascx.cs" Inherits="tjc.Modules.eWarrants.WarrantList" %>
<%@ Register TagPrefix="dnn" Namespace="DotNetNuke.Web.Client.ClientResourceManagement" Assembly="DotNetNuke.Web.Client" %>
<%@ Register TagPrefix="tb" TagName="ToolBar" Src="controls/ToolBar.ascx" %>

<div id="warrantForm">
    <div style="margin-top: 15px;" id="warrantList">
        <asp:PlaceHolder ID="gridmessage" runat="server" EnableViewState="false" />
    </div>
    <asp:UpdatePanel ID="dptUpdate" runat="server" RenderMode="Block" OnUnload="dptUpdate_Unload">
        <ContentTemplate>
            <asp:Literal ID="lblDeleteNotice" runat="server" Visible="false">
                <p class="alert alert-danger"><em class="fa fa-warning"></em> The warrants highlighted below have been completed and will be automatically deleted 24 hours after viewing.</p>
            </asp:Literal>

            <tb:ToolBar runat="server" ID="toolbar" />

            <div class="form-inline" id="dvFilters" runat="server">
                <div class="form-group">
                    <asp:Label ID="lblStartDate" CssClass="control-label" runat="server" AssociatedControlID="txtStartDate">Start Date:</asp:Label>
                    <asp:TextBox ID="txtStartDate" AutoPostBack="true" OnTextChanged="txtStartDate_TextChanged" runat="server" MaxLength="10" CssClass="form-control date" />
                </div>
                <div class="form-group">
                    <asp:Label ID="lblEndDate" CssClass="control-label" runat="server" AssociatedControlID="txtEndDate">End Date:</asp:Label>
                    <asp:TextBox ID="txtEndDate" AutoPostBack="true" OnTextChanged="txtEndDate_TextChanged" runat="server" MaxLength="50" CssClass="form-control date" />
                </div>
                <div class="form-group">
                    <button id="cmdRefresh" class="btn btn-success"><em class="fas fa-sync-alt"></em> Refresh List</button>
                    <asp:CheckBox ID="chkShowAssigned" CssClass="checker" AutoPostBack="true" OnCheckedChanged="chkShowAssigned_CheckedChanged" runat="server" Visible="false" Checked="true" Text="Show Only My warrants" />
                    <asp:CheckBox ID="chkTouchViewer" CssClass="ms-2 checker" AutoPostBack="true" OnCheckedChanged="chkTouchViewer_CheckedChanged" runat="server" Visible="false" Text="Alternate Viewer" />
                    <a href="#" id="lnkSmsPreferences" class="ms-2" data-bs-toggle="modal" data-bs-target="#smsOptInModal">Manage SMS Preferences</a>
                </div>
            </div>
            <asp:Repeater runat="server" ID="rptWarrants" OnItemCreated="rptWarrants_ItemCreated" OnItemCommand="rptWarrants_ItemCommand" OnItemDataBound="rptWarrants_ItemDataBound">
                <HeaderTemplate>
                    <table id="warrants" summary="List of Warrants" class="table table-striped">
                        <thead class="bg-primary">
                            <tr>
                                <th> </th>
                                <th>ID</th>
                                <th>Title</th>
                                <th>Defendant</th>
                                <th>Submitted By</th>
                                <th>Agency</th>
                                <th>Judge</th>
                                <th>SA</th>
                                <th>Created</th>
                                <th>Reviewed</th>
                                <th>Status</th>
                                <th class="nowrap"> </th>
                            </tr>
                        </thead>
                        <tbody>
                </HeaderTemplate>
                <ItemTemplate>
                    <tr id="tablerow" runat="server">
                        <td class="command command-icon">
                            <asp:LinkButton ID="lnkSign" runat="server" Visible="false" CommandName="sign" CommandArgument='<%#Eval("WarrantId") %>'>
                                <em class="fa fa-edit"> </em>
                            </asp:LinkButton>
                        </td>
                        <td><%#DataBinder.Eval(Container.DataItem, "WarrantId")%></td>
                        <td>
                            <ul class="attachmentList">
                                <li>
                                    <asp:HyperLink ID="lnkWarrant" CssClass="warrantLink" Target="_blank" runat="server"><%#Eval("title")%></asp:HyperLink>
                                </li>
                                <asp:Repeater ID="rptAttachments" runat="server">
                                    <ItemTemplate>
                                        <li class="attachment">
                                            <a href='<%# TemplateSourceDirectory + "/handlers/attachmentViewer.ashx?aid=" + Eval("FileID") %>' target="_blank"><%#Eval("Description") %></a>
                                        </li>
                                    </ItemTemplate>
                                </asp:Repeater>
                            </ul>
                        </td>
                        <td><%#DataBinder.Eval(Container.DataItem, "Defendant")%></td>
                        <td>
                            <span class="toolTip" data-toggle="tooltip" data-placement="top" data-html="true" title='<%#GetUserInfo(Int32.Parse(Eval("CreatedByUserId").ToString()))%>'><%#DataBinder.Eval(Container.DataItem, "CreatedByName")%></span>
                        </td>
                        <td><%#DataBinder.Eval(Container.DataItem, "AgencyName")%></td>
                        <td><%#DataBinder.Eval(Container.DataItem, "JudgeName")%></td>
                        <td><%#Eval("StateAttorney") != null && !string.IsNullOrEmpty(Eval("StateAttorney").ToString()) ? Eval("StateAttorney") : "Not Approved"%></td>
                        <td><%#Convert.ToDateTime(Eval("CreatedDate")).ToString("MM/dd/yyyy h:mm tt")%></td>
                        <td><%#FormatDate(Convert.ToDateTime(Eval("ReviewedDate")).ToString())%></td>
                        <td>
                            <span data-toggle="tooltip" class="toolTip" data-placement="top" title='<%#Eval("statusTooltip")%>'><%#DataBinder.Eval(Container.DataItem, "statusTooltip").ToString().Substring(0,1)%></span>
                        </td>
                        <td class="command command-icon">
                            <asp:LinkButton ID="cmdRelease" runat="server" CommandArgument='<%#DataBinder.Eval(Container.DataItem, "WarrantId").ToString() %>' CommandName="Release">
                                <em class="fa fa-lock" data-toggle="tooltip" data-placement="top" title="Release Warrant"></em>
                            </asp:LinkButton>
                            <asp:LinkButton ID="cmdDelete" runat="server" ToolTip="Delete Warrant" CommandName="Delete" OnClientClick="return confirm('Delete this Warrant?')" CommandArgument='<%#DataBinder.Eval(Container.DataItem, "WarrantId").ToString() %>'>
                                <em class="fa fa-trash"></em>
                            </asp:LinkButton>
                        </td>
                    </tr>
                </ItemTemplate>
                <FooterTemplate>
                    </tbody>
                    </table>
                </FooterTemplate>
            </asp:Repeater>
        </ContentTemplate>
    </asp:UpdatePanel>

    <asp:Literal ID="ltMessage" runat="server">
        <p class="alert alert-info" style="margin-top: 20px"><em class="fa fa-info-circle"></em> <strong>Please Note:</strong> Warrants will be automatically deleted {0} hours after you or another agency member view a signed or rejected warrant.</p>
    </asp:Literal>

    <div class="dnnFormMessage dnnFormInfo" id="juvpickupForm" style="clear: both; background-color: whitesmoke; border-color: #ccc;">
        <strong>Form Downloads</strong>
        <ul style="margin-bottom: 0; margin-top: 10px;">
            <li id="ddl"><a href="/Portals/0/Forms/DeSoto-Juvenile-Pick-Up-Order.docx">DeSoto County Juvenile Pickup Order</a></li>
            <li id="mdl"><a href="/Portals/0/Forms/Manatee-Juvenile-Pick-Up-Order.docx">Manatee County Juvenile Pickup Order</a></li>
            <li id="sdl"><a href="/Portals/0/Forms/Sarasota-Juvenile-Pick-Up-Order.docx">Sarasota County Juvenile Pickup Order</a></li>
        </ul>
    </div>

    <!-- SMS Opt-In Modal -->
    <div class="modal fade" id="smsOptInModal" tabindex="-1" aria-labelledby="smsOptInModalLabel" aria-hidden="true">
        <div class="modal-dialog">
            <div class="modal-content">
                <div class="modal-header">
                    <h5 class="modal-title" id="smsOptInModalLabel">SMS Notification Preferences</h5>
                    <button type="button" class="btn-close" data-bs-dismiss="modal" aria-label="Close"></button>
                </div>
                <asp:UpdatePanel ID="upSmsModal" runat="server">
                    <ContentTemplate>
                        <div class="modal-body">
                            <div id="phoneNumbersContainer">
                                <div class="form-group mb-3 phone-number-group">
                                    <asp:Label ID="lblPhoneNumber" runat="server" AssociatedControlID="txtPhoneNumber" CssClass="control-label">Phone Number (e.g., +1234567890):</asp:Label>
                                    <div class="input-group">
                                        <asp:TextBox ID="txtPhoneNumber" runat="server" CssClass="form-control phone-number-input" Placeholder="Enter phone number in international format" />
                                        <button type="button" class="btn btn-outline-danger remove-phone" style="display:none;"><em class="fa fa-trash"></em></button>
                                    </div>
                                </div>
                            </div>
                            <asp:Label ID="lblPhoneError" runat="server" CssClass="text-danger" Visible="false"></asp:Label>
                            <button type="button" id="btnAddPhone" class="btn btn-outline-primary mb-3">Add Another Phone Number</button>
                            <div class="form-check mb-3">
                                <asp:CheckBox ID="chkSmsConsent" runat="server" CssClass="form-check-input" />
                                <asp:Label ID="lblSmsConsent" runat="server" AssociatedControlID="chkSmsConsent" CssClass="form-check-label">
                                    I agree to receive SMS notifications (e.g., warrant updates, alerts). Up to 5 messages per month. Msg & data rates may apply. Reply STOP to unsubscribe.
                                </asp:Label>
                            </div>
                        </div>
                        <div class="modal-footer">
                            <asp:Button ID="btnOptOut" runat="server" Text="Opt Out" CssClass="btn btn-secondary" OnClick="btnOptOut_Click" CausesValidation="false" />
                            <asp:Button ID="btnSaveOptIn" runat="server" Text="Save" CssClass="btn btn-primary" OnClick="btnSaveOptIn_Click" />
                        </div>
                    </ContentTemplate>
                </asp:UpdatePanel>
            </div>
        </div>
    </div>
</div>

<dnn:DnnCssInclude runat="server" FilePath="~/Resources/Shared/components/TimePicker/Themes/jquery-ui.min.css" />
<dnn:DnnCssInclude runat="server" FilePath="https://cdnjs.cloudflare.com/ajax/libs/jquery-timepicker/1.10.0/jquery.timepicker.min.css" />
<dnn:DnnJsInclude runat="server" FilePath="https://cdnjs.cloudflare.com/ajax/libs/jquery-timepicker/1.10.0/jquery.timepicker.min.js" />
<dnn:DnnJsInclude runat="server" FilePath="https://cdn.datatables.net/1.11.3/js/jquery.dataTables.min.js" />
<dnn:DnnJsInclude runat="server" FilePath="https://cdn.datatables.net/1.11.3/js/dataTables.bootstrap5.min.js" />
<dnn:DnnCssInclude runat="server" FilePath="https://cdn.datatables.net/1.11.3/css/dataTables.bootstrap5.min.css" />
<dnn:DnnJsInclude runat="server" FilePath="https://cdn.jsdelivr.net/npm/bootstrap@5.1.3/dist/js/bootstrap.bundle.min.js" />

<script type="text/javascript">
    var p_counties = "<%=p_counties%>";
    function pageLoad(sender, args) {
        $(document).ready(function () {
            $('#warrantForm label').addClass("control-label");
            $("#<%= txtStartDate.ClientID%>").datepicker();
            $("#<%= txtEndDate.ClientID%>").datepicker();

            $('[data-toggle="tooltip"]').tooltip();
            $(".toolbar").append($("#cmdRefresh"));

            $("#cmdRefresh").on("click", function (e) {
                e.preventDefault();
                refreshGrid();
            });

            bindDataTable();
            Sys.WebForms.PageRequestManager.getInstance().add_endRequest(bindDataTable);

            if (p_counties.length > 0) {
                if (p_counties.indexOf("sarasota") < 0) { $("#sdl").toggle(); }
                if (p_counties.indexOf("manatee") < 0) { $("#mdl").toggle(); }
                if (p_counties.indexOf("desoto") < 0) { $("#ddl").toggle(); }
            } else {
                $("#juvpickupForm").hide();
            }

            // Check if user needs to opt in/out for SMS
            var hasOpted = '<%= HasUserOptedForSms() ? "true" : "false" %>';
            if (hasOpted === "false") {
                $('#smsOptInModal').modal('show');
            }

            // Populate existing phone numbers
            var phoneNumbers = '<%= GetPhoneNumbersJson() %>';
            if (phoneNumbers) {
                var numbers = JSON.parse(phoneNumbers);
                if (numbers.length > 0) {
                    $('#phoneNumbersContainer').empty();
                    numbers.forEach(function (number) {
                        addPhoneNumberField(number);
                    });
                }
            }

            // Handle adding new phone number fields
            $('#btnAddPhone').on('click', function () {
                addPhoneNumberField('');
            });

            // Handle removing phone number fields
            $(document).on('click', '.remove-phone', function () {
                if ($('.phone-number-group').length > 1) {
                    $(this).closest('.phone-number-group').remove();
                }
            });

            // Update remove button visibility
            function updateRemoveButtons() {
                $('.remove-phone').each(function () {
                    $(this).toggle($('.phone-number-group').length > 1);
                });
            }

            // Add phone number field
            function addPhoneNumberField(number) {
                var newField = `
                    <div class="form-group mb-3 phone-number-group">
                        <label class="control-label">Phone Number (e.g., +1234567890):</label>
                        <div class="input-group">
                            <input type="text" class="form-control phone-number-input" value="${number}" placeholder="Enter phone number in international format" />
                            <button type="button" class="btn btn-outline-danger remove-phone"><em class="fa fa-trash"></em></button>
                        </div>
                    </div>`;
                $('#phoneNumbersContainer').append(newField);
                updateRemoveButtons();
            }
        });
    }

    function bindDataTable() {
        var warrants = $('#warrants').DataTable({
            "aoColumns": [
                { "bSortable": false },
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                { "bSortable": false }
            ],
            "bStateSave": true,
            retrieve: true,
            paging: false,
            order: [[1, 'desc']]
        });
        $('#warrants_wrapper label').css("font-weight:700");
    }

    function refreshGrid() {
        window.location.replace("/");
    }
</script>