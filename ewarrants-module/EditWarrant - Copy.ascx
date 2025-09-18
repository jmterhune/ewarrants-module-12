<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="EditWarrant.ascx.cs" Inherits="tjc.Modules.eWarrants.EditWarrant" %>
<%@ Register TagPrefix="dnn" TagName="Label" Src="~/controls/LabelControl.ascx" %>
<div class="dnnForm dnnClear">
    <div class="dnnFormItem dnnFormHelp dnnClear">
        <span class="dnnFormRequired"><%=Localization.GetString("RequiredFields", Localization.SharedResourceFile)%></span>
    </div>

    <asp:Panel ID="pnlWarrant" runat="server">

        <h2 id="dnnPanel-Warrants" class="dnnFormSectionHead">Select File to Upload</h2>

        <div class="filebox">

            <div id="upload-overlay" class="overlay" style="display: none;">
                <div class="spinner"></div>
            </div>

            <p class="alert alert-info" style="margin-top: 10px;"><em class="fa fa-info-circle"></em>&nbsp;To upload a document for review, click "<strong>Choose File</strong>" and browse for the file on your computer.</p>
            <div class="dnnFormItem clearfix">
                <asp:FileUpload ID="uplWarrant" runat="server" CssClass="fileUpload" accept=".pdf,.tif,.tiff" onchange='check_extension(this.value);' ToolTip="Select File to Upload" />
                <asp:Button ID="cmdUpload" CssClass="btn btn-primary sr-only" CausesValidation="false" runat="server" Text="Upload Selected File" Enabled="false" />
                <span id="fileInputWarning" class="dnnFormMessage dnnFormError" style="display: none; right: auto; left: 25%;">Please Choose File to Upload</span>

            </div>
            <p class="info clearfix"></p>
        </div>
        <h2 id="dnnPanel-FileInfo" class="dnnFormSectionHead">Enter Warrant or Order Information</h2>
        <div class="dnnFormItem" id="dvAgency" runat="server" visible="false">
            <dnn:label id="lblAgency" runat="server" controlname="drpAgency" text="Select Agency" suffix=":" cssclass="dnnFormRequired" helptext="Select the agency you wish to use"></dnn:label>
            <asp:DropDownList ID="drpAgency" OnSelectedIndexChanged="drpAgency_SelectedIndexChanged" runat="server" AutoPostBack="true"></asp:DropDownList>
        </div>

        <div class="dnnFormItem">
            <dnn:label id="lblTitle" runat="server" controlname="txtTitle" text="Title" suffix=":" cssclass="dnnFormRequired" helptext="Enter Warrant Title"></dnn:label>
            <asp:TextBox ID="txtTitle" runat="server" MaxLength="50" CssClass="firstText NormalTextBox" />
            <asp:RequiredFieldValidator ID="valTitle" ControlToValidate="txtTitle"
                CssClass="dnnFormMessage dnnFormError" Display="Dynamic" ErrorMessage="Please enter a descriptive title" runat="server" />
        </div>

        <div class="dnnFormItem">
            <dnn:label id="lblDefendantName" runat="server" controlname="txtDefendant" text="Defendant" suffix=":" cssclass="dnnFormRequired" helptext="Enter Defendant Name or unknown defendant."></dnn:label>
            <asp:TextBox ID="txtDefendant" runat="server" CssClass="NormalTextBox" MaxLength="250" placeholder='Enter "unknown defandant" where applicable.' />
            <asp:RequiredFieldValidator ID="valDefendant" ControlToValidate="txtDefendant"
                CssClass="dnnFormMessage dnnFormError" Display="Dynamic" ErrorMessage="Please enter the Defendant's Name" runat="server" />
        </div>
        <div id="divCounty" runat="server" class="dnnFormItem tjcToggleCountyList">
            <dnn:label id="lblCounty" runat="server" controlname="drpCounty" text="County" suffix=":" cssclass="dnnFormRequired" helptext="Select County"></dnn:label>
            <asp:DropDownList ID="drpCounty" runat="server" onchange="ClearOffenseList()"></asp:DropDownList>
            <asp:RequiredFieldValidator ID="valCounty" ControlToValidate="drpCounty"
                CssClass="dnnFormMessage dnnFormError" Display="Dynamic" ErrorMessage="You must select a county" runat="server" />
        </div>
        <div class="dnnFormItem">
            <dnn:label id="lblOffenseType" runat="server" controlname="rblOffenseType" text="Division Type" suffix=":" cssclass="dnnFormRequired" helptext="Select Division Type"></dnn:label>
            <asp:RadioButtonList ID="rblOffenseType" OnDataBound="rblOffenseType_DataBound" runat="server" RepeatDirection="Horizontal" CssClass="dnnFormRadioButtons" RepeatColumns="4" RepeatLayout="Flow"></asp:RadioButtonList>
            <asp:RequiredFieldValidator ID="valOffenseType" ControlToValidate="rblOffenseType"
                CssClass="dnnFormMessage dnnFormError" Display="Dynamic" ErrorMessage="You must select an Division Type" runat="server" />
        </div>

        <div id="JudgeContainer" class="dnnFormItem tjcToggleJudgeList">
            <div id="misMessage" class="dnnFormMessage dnnFormWarning" style="margin-left: 34.65%; max-width: 405px;">Please select the South County Judge for Misdemeanor Warrants in South County.  Do not use the alphabet assignment.</div>

            <dnn:label id="lblJudge" runat="server" controlname="drpJudge" text="Judge" suffix=":" cssclass="dnnFormRequired" helptext="Select a Judge from the List"></dnn:label>
            <asp:DropDownList ID="drpJudge" runat="server" onchange="SetJudgeId()"></asp:DropDownList><br />
            <asp:RequiredFieldValidator ID="valJudge" ControlToValidate="drpJudge"
                CssClass="dnnFormMessage dnnFormError" Display="Dynamic" ErrorMessage="You must select a judge" runat="server" />
        </div>

        <div class="dnnFormMessage dnnFormError tjcToggleMessage" style="margin-left: 34.7%; max-width: 427px; clear: both">
            No Judges exits for the selected division.<br />
            Please select a different division or contact the site administrator.
            
        </div>

        <div class="dnnFormItem warranttype">
            <dnn:label id="lblWarrantType" runat="server" controlname="rblWarrantType" text="Warrant Type" suffix=":" cssclass="dnnFormRequired" helptext="Select Warrant Type"></dnn:label>
            <asp:RadioButtonList runat="server" ID="rblWarrantType" RepeatDirection="Horizontal" CssClass="dnnFormRadioButtons" RepeatLayout="Flow">
                <asp:ListItem Value="1" Text="Arrest Warrant" />
                <asp:ListItem Value="2" Text="Search Warrant" />
                <asp:ListItem Value="3" Text="Pickup Order" />
            </asp:RadioButtonList>
            <asp:RequiredFieldValidator ID="valWarrantType" runat="server" ControlToValidate="rblWarrantType" CssClass="dnnFormMessage dnnFormError"
                Display="Dynamic" ErrorMessage="Warrant Type is Required" />
        </div>
        <div class="dnnFormItem warrant-included">
            <dnn:label id="lblWarrant" runat="server" controlname="chkWarrant" text="Warrant Included?" suffix=":" cssclass="dnnFormRequired" helptext="Check to Confirm that the Warrant is Included in the Upload" />
            <asp:CheckBox ID="chkWarrant" runat="server" Text=" " CssClass="warrantChk checkbox" />
            <asp:CustomValidator runat="server" ID="valWarrantCheck" EnableClientScript="true"
                OnServerValidate="chkWarrantRequired_ServerValidate" CssClass="dnnFormMessage dnnFormError"
                ClientValidationFunction="chkWarrantRequired_ClientValidate">Warrant must be included</asp:CustomValidator>
        </div>
        <div class="dnnFormItem">
            <dnn:label id="lblAffidavit" runat="server" controlname="chkAffidavit" text="Affidavit Included?" suffix=":" cssclass="dnnFormRequired" helptext="Check to Confirm that the Affidavit is Included in the Upload" />
            <asp:CheckBox ID="chkAffidavit" runat="server" Text=" " CssClass="affidavitChk checkbox" />
            <asp:CustomValidator runat="server" ID="valAffidavitCheck" EnableClientScript="true" CssClass="dnnFormMessage dnnFormError"
                OnServerValidate="chkAffidavitRequired_ServerValidate"
                ClientValidationFunction="chkAffidavitRequired_ClientValidate">Affidavit must be included</asp:CustomValidator>
        </div>
        <div id="pcaField" class="dnnFormItem ">
            <dnn:label id="lblPCA" runat="server" controlname="chkPCA" text="PCA Included?" suffix=":" cssclass="dnnFormRequired warrantLabel" helptext="Check to Confirm that the PCA is Included in the Upload" />
            <asp:CheckBox ID="chkPCA" runat="server" CssClass="pcaChk checkbox" Text=" " />
            <asp:CustomValidator runat="server" ID="valPCACheck" EnableClientScript="true"
                OnServerValidate="valPCACheck_ServerValidate" CssClass="dnnFormMessage dnnFormError"
                ClientValidationFunction="chkPCARequired_ClientValidate">PCA must be included</asp:CustomValidator>
        </div>

        <div class="dnnFormItem saApproved">
            <dnn:label id="lblsaApproved" runat="server" controlname="chkSaApproved" text="State Attorney Approved?" suffix=":" helptext="Check to Confirm that the State Attorney has Approved the Warrant" />
            <asp:CheckBox ID="chkSaApproved" runat="server" CssClass="tjcShowText checkbox" Text=" " />
        </div>
        <div class="dnnFormItem tjcToggleDisplay">
            <dnn:label id="lblsaName" runat="server" controlname="txtSaName" text="State Attorney Name" suffix=":" />
            <asp:TextBox ID="txtSaName" runat="server" MaxLength="100" />
            <asp:CustomValidator ID="valSAName" runat="server" ErrorMessage="State Attorney Name is Required" ClientValidationFunction="txtSaName_ClientValidate" CssClass="dnnFormMessage dnnFormError"
                Display="Dynamic"></asp:CustomValidator>
        </div>
        <div class="dnnFormItem" id="dvEmergency">
            <dnn:label id="lblEmergency" runat="server" controlname="rblEmergency" cssclass="dnnFormRequired" text="Is this an emergency?" suffix=":" helptext="If this is an emergency the Judge will be notified immediately otherwise the notification will be sent in the next day" />
            <asp:RadioButtonList ID="rblEmergency" runat="server" RepeatDirection="Horizontal" RepeatLayout="Flow" CssClass="dnnFormRadioButtons">
                <asp:ListItem Text="Yes" Value="1" />
                <asp:ListItem Text="No" Value="0" />
            </asp:RadioButtonList>
            <asp:CustomValidator ID="valEmergency" runat="server" ClientValidationFunction="rblEmergency_ClientValidate" OnServerValidate="valEmergency_ServerValidate" ErrorMessage="You must select Yes or No" Display="Dynamic" CssClass="dnnFormMessage dnnFormError"></asp:CustomValidator>
            <p class="dnnClear dnnFormMessage dnnFormInfo" style="margin-left: 35%; width: 47%; max-width: 445px">Selecting Yes will send the warrant notification immediately to the Judge. Selecting No will postpone the notification until <span id="judgeTime"></span>.</p>
        </div>
        <div id="affidavitInfo" style="display: none">
            <h2 class="dnnFormSectionHead">Who Administered the Oath on the Affidavit</h2>
            <div class="dnnFormItem">
                <dnn:label id="lblOName" runat="server" controlname="txtOName" text="Name" suffix=":" cssclass="dnnFormRequired" helptext="Enter the name of the person who administered the oath on the affidavit"></dnn:label>
                <asp:TextBox ID="txtOName" runat="server" MaxLength="50" CssClass="NormalTextBox" />
                <asp:CustomValidator ID="valOName" ControlToValidate="txtOName" ValidateEmptyText="true" ClientValidationFunction="Validate_AgentName" OnServerValidate="valOName_ServerValidate"
                    CssClass="dnnFormMessage dnnFormError" Display="Dynamic" ErrorMessage="Name is Required" runat="server" />
            </div>
            <div class="dnnFormItem">
                <dnn:label id="lblOAgency" runat="server" controlname="txtOAgency" text="Agency" suffix=":" cssclass="dnnFormRequired" helptext="Enter the agency for the person who administered the oath on the affidavit.  If a Notary Public, enter Notary"></dnn:label>
                <asp:TextBox ID="txtOAgency" runat="server" MaxLength="50" CssClass="NormalTextBox" />
                <asp:CustomValidator ID="valOAgency" ControlToValidate="txtOAgency" ValidateEmptyText="true" ClientValidationFunction="Validate_Agency" OnServerValidate="valOAgency_ServerValidate"
                    CssClass="dnnFormMessage dnnFormError" Display="Dynamic" ErrorMessage="Agency is Required" runat="server" />
            </div>
            <div class="dnnFormItem">
                <dnn:label id="lblOID" runat="server" controlname="txtOID" text="ID Number" suffix=":" cssclass="dnnFormRequired" helptext="Enter the ID number of the person who administered the oath on the affidavit. If Notary Public, enter Notary"></dnn:label>
                <asp:TextBox ID="txtOID" runat="server" MaxLength="50" CssClass="NormalTextBox" />
                <asp:CustomValidator ID="valOID" ControlToValidate="txtOID" ValidateEmptyText="true" ClientValidationFunction="Validate_AgentID" OnServerValidate="valOID_ServerValidate"
                    CssClass="dnnFormMessage dnnFormError" Display="Dynamic" ErrorMessage="ID Number is Required" runat="server" />
            </div>
        </div>
        <asp:HiddenField ID="hdFileId" runat="server" />
        <asp:HiddenField ID="hdJudgeId" runat="server" />
    </asp:Panel>
    <ul class="dnnActions dnnClear">
        <li>
            <asp:LinkButton ID="cmdUpdate" OnClick="cmdUpdate_Click" runat="server" Text="Update" CssClass="btn btn-primary submitbutton" /></li>
        <li>
            <asp:LinkButton ID="cmdCancel" OnClick="cmdCancel_Click" runat="server" Text="Cancel" CausesValidation="false" CssClass="btn btn-danger" />
        </li>
    </ul>
</div>
<script type="text/javascript">
    var submit = 0;
    var extensionHash = {
        'pdf': 1,
        'tif': 1,
        'tiff': 1,
    };
    function check_extension(filename) {
        var ext = filename.split('.').pop().toLowerCase();
        var submitEl = document.getElementById('<%=cmdUpload.ClientID%>');
        if (extensionHash[ext]) {
            $(".info").html("");
            $( "#<%=cmdUpload.ClientID%>").trigger("click");
            submitEl.disabled = false;
            return true;
        } else {
            $(".info").html("<span class='NormalRed'>Invalid File Type, please choose a Tiff or PDF file!</span>");
            submitEl.disabled = true;
            return false;
        }
    }
    $(document).ready(function () {
        if ($("#<%=hdFileId.ClientID%>").val() != "") {
            $(".info").html("<span class='NormalRed'>Warrant File Captured.</span>");
        }
        $('[data-toggle="tooltip"]').tooltip();
        $('.fileUpload').click(function () { $("#upload-overlay").show(); });
        $("#<%=cmdUpload.ClientID%>").click(function (evt) {
            //$("#upload-overlay").show();
            var fileUpload = $("#<%=uplWarrant.ClientID%>").get(0);
            var files = fileUpload.files;
            if (files.length == 0) {
                $(".info").html("<span class='NormalRed'>Please Choose a File!</span>");
                return false;
            }
            var data = new FormData();
            data.append(files[0].name, files[0]);
            var options = {};
            options.url = "<%=uploadHandler%>";
            options.type = "POST";
            options.data = data;
            options.contentType = false;
            options.processData = false;
            options.success = function (fileId) {
                $("#upload-overlay").hide();
                $("#<%=hdFileId.ClientID%>").val(fileId);
                WriteMessage(fileId);
            };
            options.error = function (err) {
                $("#upload-overlay").hide();
                alert(err.statusText);
            };

            $.ajax(options);

            evt.preventDefault();
        });
        $(".warranttype").hide();
        $(".saApproved").hide();
        $(".tjcToggleDisplay").hide();
        $(".tjcToggleMessage").hide();
        $(".tjcToggleJudgeList").hide();
        $("#<%=chkWarrant.ClientID%>").css("visibility", "visible").css("opacity", "0");
        $("#<%=chkAffidavit.ClientID%>").css("visibility", "visible").css("opacity", "0");
        $("#<%=chkPCA.ClientID%>").css("visibility", "visible").css("opacity", "0");
        $("#<%=chkSaApproved.ClientID%>").css("visibility", "visible").css("opacity", "0");
        $(".dnnFormRadioButtons input").css("visibility", "visible").css("opacity", "0");
        $("input").each(function (i) { $(this).attr('tabindex', i + 1); });

        $("#misMessage").hide();
        $("#pcaField").hide();
        $("#dvEmergency").hide();
        $(".submitbutton").click(function (e) {
            if ($("#<%=hdFileId.ClientID%>").val() === "") {
                $("#fileInputWarning").show();
                return false;
            }
            if (++submit > 1) {
                return false;
            }
            return true;
        });

        $(".tjcShowText").click(function (e) { $(".tjcToggleDisplay").toggle(); });
        var pickupOrder = $("[id*=rblWarrantType] input[value=" + 3 + "]");
        var warrantTypes = $("[id*=rblWarrantType] input[value=" + 1 + "]");
        $('.tjcToggleCountyList').change(function () {

            $('#<%= rblOffenseType.ClientID%> input:disabled').prop("checked", false);
        });
        $("#<%=rblOffenseType.ClientID%> input").click(function () {
            $('#<%= rblWarrantType.ClientID%> input:checked').prop("checked", false);
            $(".warrant-included").show();
            $(".warrant-included input").prop("checked", false);
            $("#misMessage").hide();
            var index = $("#<%=rblOffenseType.ClientID%> input").index(this);
            if (index == 1) {
                var county = $("#<%=drpCounty.ClientID%> option:selected").text();
                if (county.toLowerCase() == 'sarasota') {
                    $("#misMessage").show();
                }
            }
            if (index < 3) {
                pickupOrder.next().hide().next().hide();
                pickupOrder.hide();
                warrantTypes.next().fadeIn("slow").next().fadeIn("slow");
                warrantTypes.show();
                $("#pcaField").hide();
                $(".saApproved").fadeIn("slow");
                $(".pcaChk input").prop("checked", true);
                $("#<%=lblWarrant.ClientID%>_lblLabel").text("Warrant Included?");
                $("#<%=valWarrantCheck.ClientID%>").text("Warrant must be included");
                $("#affidavitInfo").fadeIn()
                $(".warranttype").fadeIn("slow");
            } else {
                if (index == 3) {
                    $("#affidavitInfo").hide()
                    pickupOrder.next().fadeIn("slow").next().fadeIn("slow");
                    pickupOrder.show();
                    warrantTypes.next().hide().next().hide();
                    warrantTypes.hide();
                    $(".warranttype").fadeIn("slow");
                }
                if (index > 3) {
                    $("#pcaField").hide();
                    $("#affidavitInfo").fadeIn()
                    $(".warranttype").hide();
                    $(".warrant-included input").prop("checked", true);
                    $(".warrant-included").hide();
                    warrantTypes.prop("checked", true);
                    $(".pcaChk input").prop("checked", true);
                }
            }
        });

        $("#<%=rblWarrantType.ClientID%> input").click(function () {
            var index = $("#<%=rblWarrantType.ClientID%> input").index(this);
            if (index <= 1) {
                if (index == 1) {
                    ShowAlert("Important!", "Search warrants are required to be reviewed by the State Attorney's Office before submission unless a true emergency prevents such review.");
                }
                $(".pcaChk input").prop("checked", true);
                $("#pcaField").hide();
                $("#<%=lblWarrant.ClientID%>_lblLabel").text("Warrant Included?");
                $("#<%=valWarrantCheck.ClientID%>").text("Warrant must be included");

                $("#affidavitInfo").fadeIn()
                $(".saApproved").fadeIn("slow");
            } else {
                $("#pcaField").fadeIn("slow");
                $(".saApproved").hide();
                $(".pcaChk input").prop("checked", false);
                $("#<%=lblWarrant.ClientID%>_lblLabel").text("Pickup Order Included?");
                $("#<%=valWarrantCheck.ClientID%>").text("Pickup Order must be included");
                $("#affidavitInfo").fadeOut()
            }
        });

        if ($("#<%=drpCounty.ClientID%> option").length <= 2) {
            $(".tjcToggleCountyList").hide()
            $("#<%=drpCounty.ClientID%> option:eq(1)").prop("selected", true);
        }
        FillJudges();
    });
    function ClearOffenseList() {
        $('#<%= rblOffenseType.ClientID%> input:checked').prop("checked", false);
        $('#<%= rblWarrantType.ClientID%> input:checked').prop("checked", false);
        //$(".warranttype").hide();
        $(".tjcToggleJudgeList").hide();
        FillJudges();
    }
    function ShowAlert(title, text) {
        $.dnnAlert({
            okText: 'OK',
            title: title,
            text: text
        });
    }
    function WriteMessage(fileId) {
        if (fileId == "0") {
            $(".info").html("<span class='NormalRed'>Unable to upload file. Please make sure the file is in Tiff or PDF format.</span>");
        } else {
            $("#fileInputWarning").fadeOut();
            $(".info").html("<span class='NormalRed'>Warrant File Captured.</span>");
            $(".firstText").focus();
        }
    }
    function chkWarrantRequired_ClientValidate(sender, e) {
        e.IsValid = $(".warrantChk input:checkbox").is(':checked');
    }
    function chkAffidavitRequired_ClientValidate(sender, e) {
        e.IsValid = $(".affidavitChk input:checkbox").is(':checked');
    }
    function chkPCARequired_ClientValidate(sender, e) {
        e.IsValid = $(".pcaChk input:checkbox").is(':checked');
    }
    function rblEmergency_ClientValidate(sender, e) {
        if ($("#dvEmergency").is(":visible")) {
            var rbl = $("#<%=rblEmergency.ClientID %> input:checked").length;
            if (rbl == 0) { e.IsValid = false } else { e.IsValid = true }
        } else {
            e.IsValid = true;
        }
    }
    function txtSaName_ClientValidate(sender, e) {
        if ($("#<%=chkSaApproved.ClientID%> input:checkbox").is(':checked')) {

            if ($("#<%=txtSaName.ClientID%>").val() == "") {
                e.IsValid = false
            } else {
                e.IsValid = true
            }
        } else {
            e.IsValid = true;
        }
    }
    function Validate_AgentID(sender, e) {
        if ($("#affidavitInfo").is(":visible")) {
            var txt = $("#<%=txtOID.ClientID %>").val();
            var l = $.trim(txt).length;
            if (l == 0) { e.IsValid = false } else { e.IsValid = true }
        } else {
            e.IsValid = true;
        }
    }
    function Validate_AgentName(sender, e) {
        if ($("#affidavitInfo").is(":visible")) {
            var txt = $("#<%=txtOName.ClientID %>").val();
            var l = $.trim(txt).length;
            if (l == 0) { e.IsValid = false } else { e.IsValid = true }
        } else {
            e.IsValid = true;
        }
    }
    function Validate_Agency(sender, e) {
        if ($("#affidavitInfo").is(":visible")) {
            var txt = $("#<%=txtOAgency.ClientID %>").val();
            var l = $.trim(txt).length;
            if (l == 0) { e.IsValid = false } else { e.IsValid = true }
        } else {
            e.IsValid = true;
        }
    }
    function SetJudgeId() {
        var judgeId = $("#<%=drpJudge.ClientID%> option:selected").val();
        var ModuleId = '<%=ModuleId%>';
        $('#<%=hdJudgeId.ClientID%>').val(judgeId);
        var Url = "/DesktopModules/tjc.Modules/eWarrants/api/HttpData/notifyjudge/" + ModuleId + "/" + judgeId;
        var result = "";
        $.ajax({
            type: "GET",
            dataType: "json",
            url: Url,
            success: function (j) {
                if (j.Available === false) {
                    $("#dvEmergency").show();
                    $("#judgeTime").text(j.StartOfDay);
                } else {
                    $("#dvEmergency").hide();
                    $("#judgeTime").text("(No time specified)");
                }
            },
            error: function () {
                ShowAlert("Error!", "Error retrieving judge availability.<br />Please contact the site administrator with this message.");
                //$(".tjcToggleMessage p").html();
                //$(".tjcToggleMessage").show();
            }
        });
    }
    function FillJudges() {
        countyName = $("#<%=drpCounty.ClientID%> option:selected").val();
        selectedOffense = $('#<%= rblOffenseType.ClientID%> input:checked');
        judgeTypeId = selectedOffense.val();
        judgeList = $('#<%=drpJudge.ClientID%>');
        var ModuleId = '<%=ModuleId%>';
        var Url = "/DesktopModules/tjc.Modules/eWarrants/api/HttpData/judges/" + ModuleId + "/";
        var choices = '';
        if (countyName != "" && judgeTypeId != undefined) {
            Url += countyName + "/" + judgeTypeId;
            $.ajax({
                type: "GET",
                dataType: "json",
                url: Url,
                success: function (j) {
                    if (j.length == 0) {
                        ShowAlert("No Judges Found", "No Judges exits for the selected division.<br />Please select a different division or contact the site administrator.");
                        $(".tjcToggleJudgeList").hide();
                        selectedOffense.prop('checked', false);
                        selectedOffense.attr('disabled', true);
                        $('#<%= rblWarrantType.ClientID%> input:checked').removeAttr("checked");
                        $('#<%= rblWarrantType.ClientID%> input').attr("disabled", true);
                        choices += '<option value="">No Judges Found</option>';
                        judgeList.html(choices);
                    } else {
                        $(".tjcToggleJudgeList").fadeIn("slow");
                        $('#<%= rblWarrantType.ClientID%> input').attr("disabled", false);
                        choices += '<option value="">< Select Judge ></option>';
                        for (var i = 0; i < j.length; i++) {
                            c = j[i];
                            choices += '<option value="' + c.Value + '">' + c.Text + '</option>';
                        }
                        judgeList.html(choices);
                    }
                },
                error: function () {
                    ShowAlert("Error!", "Error retrieving judge names.<br />Please refresh the page and try again.");
                }
            });
        } else {
            choices += '<option value="">No Judges for Selected Options</option>';
            judgeList.html(choices);
        }
    }
</script>
