<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="EditRPO.ascx.cs" Inherits="tjc.Modules.eWarrants.EditRPO" %>
<%@ Register TagPrefix="dnn" TagName="Label" Src="~/controls/LabelControl.ascx" %>
<div class="dnnForm dnnClear">
    <div class="alert alert-warning"><em class="fa fa-warning"></em> <strong>IMPORTANT!</strong> When submitting a RPO, submit the Proposed Order as a TIFF or PDF file without including the affidavit and petition. Attach the affidavit and petition separately as PDF files using the Attachments field. <span style="color: red">Do not check any of the boxes in the Proposed Order (For Judge Use Only).</span></div>

    <div class="dnnFormItem dnnFormHelp dnnClear">
        <span class="dnnFormRequired"><%=Localization.GetString("RequiredFields", Localization.SharedResourceFile)%></span>
    </div>

    <asp:Panel ID="pnlRPO" runat="server">
        <h2 id="dnnPanel-RPO" class="dnnFormSectionHead">Select Proposed Order to Upload</h2>
        <div class="filebox">
            <div id="upload-overlay" class="overlay" style="display: none;">
                <div class="spinner"></div>
            </div>
            <p class="alert alert-info" style="margin-top: 10px;"><em class="fa fa-info-circle"></em> To upload a document for review, click "<strong>Choose File</strong>" and select a TIFF or PDF file.</p>
            <div class="dnnFormItem clearfix">
                <asp:FileUpload ID="uplWarrant" runat="server" CssClass="fileUpload" ClientIDMode="Static" accept=".pdf,.tif,.tiff" onchange="checkExtension(this.value);" ToolTip="Select File to Upload" />
                <asp:Button ID="cmdUpload" CssClass="btn btn-primary sr-only" CausesValidation="false" runat="server" Text="Upload Selected File" Enabled="false" />
                <span id="fileInputWarning" class="dnnFormMessage dnnFormError" style="display: none;">Please choose a TIFF or PDF file to upload.</span>
            </div>
            <p class="info clearfix"></p>
        </div>

        <h2 id="dnnPanel-FileInfo" class="dnnFormSectionHead">Enter RPO Information</h2>
        <div class="dnnFormItem" id="dvAgency" runat="server" visible="false">
            <dnn:Label ID="lblAgency" runat="server" ControlName="drpAgency" Text="Select Agency" Suffix=":" CssClass="dnnFormRequired" HelpText="Select the agency you wish to use"></dnn:Label>
            <asp:DropDownList ID="drpAgency" OnSelectedIndexChanged="drpAgency_SelectedIndexChanged" runat="server" AutoPostBack="true"></asp:DropDownList>
        </div>

        <div class="dnnFormItem">
            <dnn:Label ID="lblTitle" runat="server" ControlName="txtTitle" Text="Title" Suffix=":" CssClass="dnnFormRequired" HelpText="Enter RPO Title"></dnn:Label>
            <asp:TextBox ID="txtTitle" runat="server" MaxLength="50" CssClass="firstText NormalTextBox" TabIndex="1" />
            <asp:RequiredFieldValidator ID="valTitle" ControlToValidate="txtTitle" CssClass="dnnFormMessage dnnFormError" Display="Dynamic" ErrorMessage="Please enter a descriptive title" runat="server" />
        </div>

        <div class="dnnFormItem">
            <dnn:Label ID="lblDefendantName" runat="server" ControlName="txtRespondent" Text="Respondent" Suffix=":" CssClass="dnnFormRequired" HelpText="Enter Respondent Name or 'unknown respondent'."></dnn:Label>
            <asp:TextBox ID="txtRespondent" runat="server" CssClass="NormalTextBox" MaxLength="250" placeholder="Enter 'unknown respondent' where applicable." TabIndex="2" />
            <asp:RequiredFieldValidator ID="valDefendant" ControlToValidate="txtRespondent" CssClass="dnnFormMessage dnnFormError" Display="Dynamic" ErrorMessage="Please enter the Respondent's Name" runat="server" />
        </div>
        <div id="divCounty" runat="server" class="dnnFormItem tjcToggleCountyList">
            <dnn:Label ID="lblCounty" runat="server" ControlName="drpCounty" Text="County" Suffix=":" CssClass="dnnFormRequired" HelpText="Select County"></dnn:Label>
            <asp:DropDownList ID="drpCounty" runat="server" onchange="clearOffenseList();" TabIndex="3"></asp:DropDownList>
            <asp:RequiredFieldValidator ID="valCounty" ControlToValidate="drpCounty" CssClass="dnnFormMessage dnnFormError" Display="Dynamic" ErrorMessage="You must select a county" runat="server" />
        </div>
        <div id="JudgeContainer" class="dnnFormItem tjcToggleJudgeList">
            <dnn:Label ID="lblJudge" runat="server" ControlName="drpJudge" Text="Judge" Suffix=":" CssClass="dnnFormRequired" HelpText="Select a Judge from the List"></dnn:Label>
            <asp:DropDownList ID="drpJudge" runat="server" onchange="setJudgeId();" TabIndex="5"></asp:DropDownList>
            <asp:RequiredFieldValidator ID="valJudge" ControlToValidate="drpJudge" CssClass="dnnFormMessage dnnFormError" Display="Dynamic" ErrorMessage="You must select a judge" runat="server" />
        </div>

        <div class="dnnFormMessage dnnFormError tjcToggleMessage" style="margin-left: 34.7%; max-width: 427px; clear: both">
            No judges available for the selected division.<br />
            Please select a different division or contact the site administrator.
        </div>
        <div class="dnnFormItem clearfix">
            <dnn:Label ID="lblupload" runat="server" ControlName="attachmentUpload" Text="Upload Attachments" Suffix=":" HelpText="Select the Attachment Type and then select the file to upload" />
            <div style="position: relative;">
                <div id="attach-overlay" class="overlay" style="display: none;">
                    <div class="spinner"></div>
                </div>
                <asp:DropDownList ID="drpAttachmentType" runat="server" CssClass="pull-left smallDropdown">
                    <asp:ListItem Text="<Select Attachment Type>" Value="" />
                    <asp:ListItem>Petition</asp:ListItem>
                    <asp:ListItem>Affidavit</asp:ListItem>
                    <asp:ListItem>Other</asp:ListItem>
                </asp:DropDownList>
                <asp:FileUpload ID="uplAttachments" runat="server" ClientIDMode="Static" Enabled="false" CssClass="fileUpload" accept=".pdf" onchange="checkAttachment(this.value);" ToolTip="Select Attachment Type First" />
                <asp:Button ID="cmdAddAttachment" CssClass="btn btn-primary sr-only" CausesValidation="false" runat="server" Text="Upload Selected File" Enabled="false" />
                <span id="fileAttachmentWarning" class="dnnFormMessage dnnFormError" style="display: none;">Please choose a PDF file to upload.</span>
            </div>
        </div>
        <div class="dnnFormItem">
            <div class="formFieldAdjust">
                <span class="attachmentInfo"></span>
                <ul id="attachmentList" class="attachments"></ul>
            </div>
        </div>

        <div class="dnnFormItem">
            <dnn:Label ID="lblRPO" runat="server" ControlName="chkRPO" Text="Proposed Order Included?" Suffix=":" CssClass="dnnFormRequired" HelpText="Check to confirm that the RPO is included in the upload" />
            <asp:CheckBox ID="chkRPO" runat="server" CssClass="RPOChk" TabIndex="7" />
            <asp:CustomValidator runat="server" ID="valRPOCheck" EnableClientScript="true" OnServerValidate="chkRPORequired_ServerValidate" CssClass="dnnFormMessage dnnFormError" ClientValidationFunction="chkRPORequired_ClientValidate">RPO must be included</asp:CustomValidator>
        </div>
        <div id="petitionField" class="dnnFormItem">
            <dnn:Label ID="lblPetition" runat="server" ControlName="chkPetition" Text="Petition Attached?" Suffix=":" CssClass="dnnFormRequired warrantLabel" HelpText="Check to confirm that the Petition is included in the upload" />
            <asp:CheckBox ID="chkPetition" runat="server" CssClass="petitionChk" TabIndex="9" />
            <asp:CustomValidator runat="server" ID="valpetitionCheck" EnableClientScript="true" OnServerValidate="valPetitionCheck_ServerValidate" CssClass="dnnFormMessage dnnFormError" ClientValidationFunction="chkPetitionRequired_ClientValidate">Petition must be included</asp:CustomValidator>
        </div>

        <div class="dnnFormItem">
            <dnn:Label ID="lblAffidavit" runat="server" ControlName="chkAffidavit" Text="Affidavit Attached?" Suffix=":" CssClass="dnnFormRequired" HelpText="Check to confirm that the Affidavit is included in the upload" />
            <asp:CheckBox ID="chkAffidavit" runat="server" CssClass="affidavitChk" TabIndex="8" />
            <asp:CustomValidator runat="server" ID="valAffidavitCheck" EnableClientScript="true" CssClass="dnnFormMessage dnnFormError" OnServerValidate="chkAffidavitRequired_ServerValidate" ClientValidationFunction="chkAffidavitRequired_ClientValidate">Affidavit must be included</asp:CustomValidator>
        </div>
        <asp:HiddenField ID="hdFileId" runat="server" />
        <asp:HiddenField ID="hdJudgeId" runat="server" />
        <asp:HiddenField ID="hdAttachmentIds" runat="server" />
    </asp:Panel>
    <ul class="dnnActions dnnClear">
        <li><asp:LinkButton ID="cmdUpdate" OnClick="cmdUpdate_Click" runat="server" Text="Update" CssClass="btn btn-primary submitbutton" /></li>
        <li><asp:LinkButton ID="cmdCancel" OnClick="cmdCancel_Click" runat="server" Text="Cancel" CausesValidation="false" CssClass="btn btn-danger" /></li>
    </ul>
</div>
<script type="text/javascript">
    var submitCount = 0;
    var validExtensions = { 'pdf': 1, 'tif': 1, 'tiff': 1 };
    var moduleId = "<%=ModuleId%>";
    var tabId = "<%=TabId%>";

 

    // Prevent double form submission
    function preventDoubleSubmission() {
        if (submitCount > 0) {
            showAlert("Submission Error", "Form is already being submitted. Please wait.");
            return false;
        }
        submitCount++;
        var submitButton = $(".submitbutton");
        submitButton.prop("disabled", true).addClass("disabled").text("Submitting...");
        $("#upload-overlay").show();
        return true;
    }

    // Reset submission state on failure
    function resetSubmissionState() {
        submitCount = 0;
        var submitButton = $(".submitbutton");
        submitButton.prop("disabled", false).removeClass("disabled").text("Update");
        $("#upload-overlay").hide();
    }

    // Check file extension for main RPO upload
    function checkExtension(filename) {
        if (!filename) {
            showAlert("Error", "No file selected for upload.");
            return false;
        }
        var ext = filename.split('.').pop().toLowerCase();
        var submitEl = document.getElementById('<%=cmdUpload.ClientID%>');
        if (validExtensions[ext]) {
            $(".info").html("");
            $("#<%=cmdUpload.ClientID%>").trigger("click");
            submitEl.disabled = false;
            return true;
        } else {
            $(".info").html("<span class='NormalRed'>Invalid file type. Please choose a TIFF or PDF file.</span>");
            submitEl.disabled = true;
            return false;
        }
    }

    // Check file extension for attachments
    function checkAttachment(filename) {
        if (!filename) {
            showAlert("Error", "No file selected for attachment.");
            return false;
        }
        var ext = filename.split('.').pop().toLowerCase();
        var submitEl = document.getElementById('<%=cmdAddAttachment.ClientID%>');
        if (ext === "pdf") {
            $(".attachmentInfo").html("");
            $("#<%=cmdAddAttachment.ClientID%>").trigger("click");
            submitEl.disabled = false;
            return true;
        } else {
            $(".attachmentInfo").html("<span class='NormalRed'>Invalid file type. Please choose a PDF file.</span>");
            submitEl.disabled = true;
            return false;
        }
    }

    // Delete attachment via AJAX
    function deleteAttachment(fileId) {
        if (!fileId) {
            showAlert("Error", "Invalid file ID for deletion.");
            return false;
        }
        var data = new FormData();
        data.append("fileId", fileId);
        $.ajax({
            url: "<%=AttachmentHandler%>",
            type: "POST",
            data: data,
            contentType: false,
            processData: false,
            success: function (result) {
                var listItem = $("li[data-fileid='" + fileId + "']");
                listItem.remove();
                updateAttachmentIds();
            },
            error: function (xhr, status, error) {
                showAlert("Error", "Failed to delete attachment: " + (xhr.statusText || "Unknown error"));
                console.error("Attachment deletion failed:", error);
            }
        });
        return false;
    }

    // Update hidden field with attachment IDs
    function updateAttachmentIds() {
        var fileList = "";
        $('li[data-fileid]').each(function () {
            var id = $(this).data("fileid");
            fileList = fileList ? fileList + "," + id : id;
        });
        $("#<%=hdAttachmentIds.ClientID%>").val(fileList);
    }

    // Show alert message
    function showAlert(title, text) {
        $.dnnAlert({
            okText: "OK",
            title: title,
            text: text
        });
    }

    // Write message for main file upload
    function writeMessage(fileId) {
        if (fileId === "0") {
            $(".info").html("<span class='NormalRed'>Unable to upload file. Please ensure the file is in TIFF or PDF format.</span>");
        } else {
            $("#fileInputWarning").fadeOut();
            $(".info").html("<span class='NormalRed'>RPO File Captured.</span>");
            $(".firstText").focus();
        }
        $("#upload-overlay").hide();
    }

    // Write message for attachment upload
    function writeAttachmentMessage(filename) {
        if (!filename) {
            $(".attachmentInfo").html("<span class='NormalRed'>Unable to upload file. Please ensure the file is in PDF format.</span>");
        } else {
            $("#fileAttachmentWarning").fadeOut();
            $(".attachmentInfo").html("<span class='NormalRed'>Attachment Captured.</span>");
        }
        var upload = $("#<%=uplAttachments.ClientID%>");
        upload.parent().addClass("disabledWrapper").html(upload.parent().html().replace(filename, "Choose File"));
        upload.attr("disabled", true).attr("title", "Select Attachment Type First");
        $(".attachmentInfo").html("");
        $("#<%=drpAttachmentType.ClientID%>").val("");
        $("#attach-overlay").hide();
    }

    $(document).ready(function () {
        // Initialize UI state
        if ($("#<%=hdFileId.ClientID%>").val()) {
            $(".info").html("<span class='NormalRed'>RPO File Captured.</span>");
        }
        $("#uplWarrant").on("click", function () { $("#upload-overlay").show(); });
        $("#uplAttachments").on("click", function () { $("#attach-overlay").show(); });

        // Handle main file upload
        $("#<%=cmdUpload.ClientID%>").click(function (evt) {
            evt.preventDefault();
            var fileUpload = $("#<%=uplWarrant.ClientID%>").get(0);
            var files = fileUpload.files;
            if (files.length === 0) {
                $(".info").html("<span class='NormalRed'>Please choose a file to upload.</span>");
                $("#upload-overlay").hide();
                return false;
            }
            var data = new FormData();
            data.append(files[0].name, files[0]);
            $.ajax({
                url: "<%=UploadHandler%>",
                type: "POST",
                data: data,
                contentType: false,
                processData: false,
                success: function (fileId) {
                    $("#<%=hdFileId.ClientID%>").val(fileId);
                    writeMessage(fileId);
                },
                error: function (xhr, status, error) {
                    showAlert("Upload Error", "Failed to upload file: " + (xhr.statusText || "Unknown error"));
                    console.error("File upload failed:", error);
                    $("#upload-overlay").hide();
                }
            });
        });

        // Handle attachment upload
        $("#<%=cmdAddAttachment.ClientID%>").click(function (evt) {
            evt.preventDefault();
            var upload = $("#<%=uplAttachments.ClientID%>");
            if (!upload.is(":enabled")) return false;

            var selectedAttachmentType = $("#<%=drpAttachmentType.ClientID%>").val();
            var fileUpload = $("#<%=uplAttachments.ClientID%>").get(0);
            var files = fileUpload.files;
            if (files.length === 0) {
                $(".attachmentInfo").html("<span class='NormalRed'>Please choose a file to upload.</span>");
                $("#attach-overlay").hide();
                return false;
            }
            if (!selectedAttachmentType) {
                $(".attachmentInfo").html("<span class='NormalRed'>Please select an attachment type.</span>");
                $("#attach-overlay").hide();
                return false;
            }
            var filename = files[0].name;
            var data = new FormData();
            data.append(filename, files[0]);
            data.append("attachmentType", selectedAttachmentType);
            data.append("moduleId", moduleId);
            data.append("tabId", tabId);
            $.ajax({
                url: "<%=AttachmentHandler%>",
                type: "POST",
                data: data,
                contentType: false,
                processData: false,
                success: function (fileId) {
                    var fileIdList = $("#<%=hdAttachmentIds.ClientID%>").val();
                    $("#<%=hdAttachmentIds.ClientID%>").val(fileIdList ? fileIdList + "," + fileId : fileId);
                    $("#attachmentList").append("<li data-fileType='" + selectedAttachmentType + "' data-fileId='" + fileId + "'><span class='fileType'>" + selectedAttachmentType + "</span> <a onclick=\"deleteAttachment('" + fileId + "')\"><em class='fa fa-trash'></em></a></li>");
                    writeAttachmentMessage(filename);
                },
                error: function (xhr, status, error) {
                    showAlert("Attachment Upload Error", "Failed to upload attachment: " + (xhr.statusText || "Unknown error"));
                    console.error("Attachment upload failed:", error);
                    $("#<%=drpAttachmentType.ClientID%>").val("");
                    upload.attr("disabled", true).attr("title", "Select Attachment Type First").parent().addClass("disabledWrapper");
                    $("#attach-overlay").hide();
                }
            });
        });

        // Enable/disable attachment upload based on type selection
        $("#<%=drpAttachmentType.ClientID%>").change(function () {
            var upload = $("#<%=uplAttachments.ClientID%>");
            if ($(this).val()) {
                upload.attr("disabled", false).attr("title", "Select File to Upload").parent().removeClass("disabledWrapper");
            } else {
                upload.attr("disabled", true).attr("title", "Select Attachment Type First").parent().addClass("disabledWrapper");
            }
        });

        // Initialize UI elements
        $(".saApproved").hide();
        $(".tjcToggleDisplay").hide();
        $(".tjcToggleMessage").hide();
        $(".tjcToggleJudgeList").hide();
        $("#dvEmergency").hide();

        // Prevent double submission
        $(".submitbutton").click(function (e) {
            if (!preventDoubleSubmission()) {
                e.preventDefault();
                return false;
            }
            if ($("#<%=hdFileId.ClientID%>").val() === "") {
                showAlert("Submission Error", "No file uploaded. Please upload a TIFF or PDF file.");
                $("#fileInputWarning").show();
                resetSubmissionState();
                return false;
            }
            return true;
        });

        // Validate affidavit attachment
        $(".affidavitChk input:checkbox").click(function () {
            var affidavit = $("li[data-fileType='Affidavit']");
            if (!affidavit.length && $(this).is(":checked")) {
                $(this).prop("checked", false);
                showAlert("Attachment Validation", "No Affidavit attachment found. Please upload an Affidavit as a separate PDF.");
            }
        });

        // Validate petition attachment
        $(".petitionChk input:checkbox").click(function () {
            var petition = $("li[data-fileType='Petition']");
            if (!petition.length && $(this).is(":checked")) {
                $(this).prop("checked", false);
                showAlert("Attachment Validation", "No Petition attachment found. Please upload a Petition as a separate PDF.");
            }
        });

        // Auto-select county if only one is available
        if ($("#<%=drpCounty.ClientID%> option").length <= 2) {
            $(".tjcToggleCountyList").hide();
            $("#<%=drpCounty.ClientID%> option:eq(1)").prop("selected", true);
        }

        // Disable attachment upload button initially
        setTimeout(function () {
            $("#<%=uplAttachments.ClientID%>").parent().addClass("disabledWrapper");
        }, 300);

        fillJudges();
    });

    function clearOffenseList() {
        $(".tjcToggleJudgeList").hide();
        fillJudges();
    }

    function chkRPORequired_ClientValidate(sender, e) {
        e.IsValid = $(".RPOChk input:checkbox").is(":checked");
    }

    function chkAffidavitRequired_ClientValidate(sender, e) {
        e.IsValid = $(".affidavitChk input:checkbox").is(":checked");
    }

    function chkPetitionRequired_ClientValidate(sender, e) {
        e.IsValid = $(".petitionChk input:checkbox").is(":checked");
    }

    function setJudgeId() {
        var judgeId = $("#<%=drpJudge.ClientID%> option:selected").val();
        var url = "/DesktopModules/tjc.Modules/eWarrants/api/HttpData/notifyjudge/" + moduleId + "/" + judgeId;
        $("#<%=hdJudgeId.ClientID%>").val(judgeId);
        $.ajax({
            type: "GET",
            dataType: "json",
            url: url,
            success: function (j) {
                if (j.Available === false) {
                    $("#dvEmergency").show();
                    $("#judgeTime").text(j.StartOfDay || "(No time specified)");
                } else {
                    $("#dvEmergency").hide();
                    $("#judgeTime").text("(No time specified)");
                }
            },
            error: function (xhr, status, error) {
                showAlert("Error", "Error retrieving judge availability. Please contact the site administrator.");
                console.error("Judge availability fetch failed:", error);
                resetSubmissionState();
            }
        });
    }

    function fillJudges() {
        var countyName = $("#<%=drpCounty.ClientID%> option:selected").val();
        var judgeTypeId = "<%=JudgeTypeID %>";
        var judgeList = $("#<%=drpJudge.ClientID%>");
        var url = "/DesktopModules/tjc.Modules/eWarrants/api/HttpData/rpojudges/" + moduleId + "/";
        var choices = "";
        if (countyName && judgeTypeId) {
            url += countyName + "/" + judgeTypeId;
            $.ajax({
                type: "GET",
                dataType: "json",
                url: url,
                success: function (j) {
                    if (!j || j.length === 0) {
                        showAlert("No Judges Found", "No judges available for RPOs. Please contact the site administrator.");
                        $(".tjcToggleJudgeList").hide();
                        choices = "<option value=''>No Judges Found</option>";
                        judgeList.html(choices);
                    } else {
                        $(".tjcToggleJudgeList").fadeIn("slow");
                        choices = "<option value=''>< Select Judge ></option>";
                        for (var i = 0; i < j.length; i++) {
                            choices += "<option value='" + j[i].Value + "'>" + j[i].Text + "</option>";
                        }
                        judgeList.html(choices);
                    }
                },
                error: function (xhr, status, error) {
                    showAlert("Error", "Error retrieving judge names. Please refresh the page and try again.");
                    console.error("Judge list fetch failed:", error);
                    resetSubmissionState();
                }
            });
        } else {
            choices = "<option value=''>No Judges for Selected Options</option>";
            judgeList.html(choices);
        }
    }
</script>