<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="EditRPO.ascx.cs" Inherits="tjc.Modules.eWarrants.EditRPO" %>
<%@ Register TagPrefix="dnn" TagName="Label" Src="~/controls/LabelControl.ascx" %>
<div class="dnnForm dnnClear">
    <div class="alert alert-warning"><em class="fa fa-warning"></em>&nbsp;<strong>IMPORTANT!</strong> When submitting a RPO, submit the Proposed Order as a Tiff or PDF file without including the affidavit and petition. Attach the affidavit and petition separately as PDF files using the Attachments field. <span style="color: red">Do not check any of the boxes in the Proposed Order (For Judge Use Only).</span></div>

    <div class="dnnFormItem dnnFormHelp dnnClear">
        <span class="dnnFormRequired"><%=Localization.GetString("RequiredFields", Localization.SharedResourceFile)%></span>
    </div>

    <asp:Panel ID="pnlRPO" runat="server">
        <h2 id="dnnPanel-RPO" class="dnnFormSectionHead">Select Proposed Order to Upload</h2>
        <div class="filebox">
            <div id="upload-overlay" class="overlay" style="display: none;">
                <div class="spinner"></div>
            </div>
            <p class="alert alert-info" style="margin-top: 10px;"><em class="fa fa-info-circle"></em>&nbsp;To upload a document for review, click the button below and browse for the file on your computer.</p>
            <div class="dnnFormItem clearfix">
                <asp:FileUpload ID="uplWarrant" runat="server" CssClass="fileUpload" ClientIDMode="Static" accept=".pdf,.tif,.tiff" onchange='check_extension(this.value);' ToolTip="Select File to Upload" />
                <asp:Button ID="cmdUpload" CssClass="btn btn-primary sr-only" CausesValidation="false" runat="server" Text="Upload Selected File" Enabled="false" />
                <span id="fileInputWarning" class="dnnFormMessage dnnFormError" style="display: none; right: auto; left: 25%;">Please Choose File to Upload</span>

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
            <asp:RequiredFieldValidator ID="valTitle" ControlToValidate="txtTitle"
                CssClass="dnnFormMessage dnnFormError" Display="Dynamic" ErrorMessage="Please enter a descriptive title" runat="server" />
        </div>

        <div class="dnnFormItem">
            <dnn:Label ID="lblDefendantName" runat="server" ControlName="txtRespondent" Text="Respondent" Suffix=":" CssClass="dnnFormRequired" HelpText="Enter Respondent Name or unknown respondent."></dnn:Label>
            <asp:TextBox ID="txtRespondent" runat="server" CssClass="NormalTextBox" MaxLength="250" placeholder='Enter "unknown respondent" where applicable.' TabIndex="2" />
            <asp:RequiredFieldValidator ID="valDefendant" ControlToValidate="txtRespondent"
                CssClass="dnnFormMessage dnnFormError" Display="Dynamic" ErrorMessage="Please enter the Respondent's Name" runat="server" />
        </div>
        <div id="divCounty" runat="server" class="dnnFormItem tjcToggleCountyList">
            <dnn:Label ID="lblCounty" runat="server" ControlName="drpCounty" Text="County" Suffix=":" CssClass="dnnFormRequired" HelpText="Select County"></dnn:Label>
            <asp:DropDownList ID="drpCounty" runat="server" onchange="ClearOffenseList()" TabIndex="3"></asp:DropDownList>
            <asp:RequiredFieldValidator ID="valCounty" ControlToValidate="drpCounty"
                CssClass="dnnFormMessage dnnFormError" Display="Dynamic" ErrorMessage="You must select a county" runat="server" />
        </div>
        <div id="JudgeContainer" class="dnnFormItem tjcToggleJudgeList">
            <dnn:Label ID="lblJudge" runat="server" ControlName="drpJudge" Text="Judge" Suffix=":" CssClass="dnnFormRequired" HelpText="Select a Judge from the List"></dnn:Label>
            <asp:DropDownList ID="drpJudge" runat="server" onchange="SetJudgeId()" TabIndex="5"></asp:DropDownList>
            <asp:RequiredFieldValidator ID="valJudge" ControlToValidate="drpJudge"
                CssClass="dnnFormMessage dnnFormError" Display="Dynamic" ErrorMessage="You must select a judge" runat="server" />
        </div>

        <div class="dnnFormMessage dnnFormError tjcToggleMessage" style="margin-left: 34.7%; max-width: 427px; clear: both">
            No Judges exits for the selected division.<br />
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
                <asp:FileUpload ID="uplAttachments" runat="server" ClientIDMode="Static" Enabled="false" CssClass="fileUpload" accept=".pdf" onchange='check_attachment(this.value);' ToolTip="Select Attachment Type First" />
                <asp:Button ID="cmdAddAttachment" CssClass="btn btn-primary sr-only" CausesValidation="false" runat="server" Text="Upload Selected File" Enabled="false" />
                <span id="fileAttachmentWarning" class="dnnFormMessage dnnFormError" style="display: none; right: auto; left: 25%;">Please Choose File to Upload</span>
            </div>
        </div>
        <div class="dnnFormItem">
            <div class="formFieldAdjust">
                <span class="attachmentInfo"></span>
                <ul id="attachmentList" class="attachments">
                </ul>
            </div>
        </div>

        <div class="dnnFormItem">
            <dnn:Label ID="lblRPO" runat="server" ControlName="chkRPO" Text="Proposed Order Included?" Suffix=":" CssClass="dnnFormRequired" HelpText="Check to Confirm that the RPO is Included in the Upload" />
            <asp:CheckBox ID="chkRPO" runat="server" CssClass="RPOChk" TabIndex="7" />
            <asp:CustomValidator runat="server" ID="valRPOCheck" EnableClientScript="true"
                OnServerValidate="chkRPORequired_ServerValidate" CssClass="dnnFormMessage dnnFormError"
                ClientValidationFunction="chkRPORequired_ClientValidate">RPO must be included</asp:CustomValidator>
        </div>
        <div id="petitionField" class="dnnFormItem ">
            <dnn:Label ID="lblPetition" runat="server" ControlName="chkPetition" Text="Petition Attached?" Suffix=":" CssClass="dnnFormRequired warrantLabel" HelpText="Check to Confirm that the Petition is Included in the Upload" />
            <asp:CheckBox ID="chkPetition" runat="server" CssClass="petitionChk" TabIndex="9" />
            <asp:CustomValidator runat="server" ID="valpetitionCheck" EnableClientScript="true"
                OnServerValidate="valPetitionCheck_ServerValidate" CssClass="dnnFormMessage dnnFormError"
                ClientValidationFunction="chkPetitionRequired_ClientValidate">petition must be included</asp:CustomValidator>
        </div>

        <div class="dnnFormItem">
            <dnn:Label ID="lblAffidavit" runat="server" ControlName="chkAffidavit" Text="Affidavit Attached?" Suffix=":" CssClass="dnnFormRequired" HelpText="Check to Confirm that the Affidavit is Included in the Upload" />
            <asp:CheckBox ID="chkAffidavit" runat="server" CssClass="affidavitChk" TabIndex="8" />
            <asp:CustomValidator runat="server" ID="valAffidavitCheck" EnableClientScript="true" CssClass="dnnFormMessage dnnFormError"
                OnServerValidate="chkAffidavitRequired_ServerValidate"
                ClientValidationFunction="chkAffidavitRequired_ClientValidate">Affidavit must be included</asp:CustomValidator>
        </div>
        <asp:HiddenField ID="hdFileId" runat="server" />
        <asp:HiddenField ID="hdJudgeId" runat="server" />
        <asp:HiddenField ID="hdAttachmentIds" runat="server" />

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
    function check_attachment(filename) {
        var ext = filename.split('.').pop().toLowerCase();
        var submitEl = document.getElementById('<%=cmdAddAttachment.ClientID%>');
        if (ext === "pdf") {
            $(".info").html("");
            $( "#<%=cmdAddAttachment.ClientID%>").trigger("click");
            submitEl.disabled = false;
            return true;
        } else {
            $(".attachmentInfo").html("<span class='NormalRed'>Invalid File Type, please choose a PDF file!</span>");
            submitEl.disabled = true;
            return false;
        }
    }
    function DeleteAttachment(fileId) {
        var data = new FormData();
        data.append("fileId", fileId);
        var options = {};
        options.url = "<%=attachmentHandler%>";
        options.type = "POST";
        options.data = data;
        options.contentType = false;
        options.processData = false;
        options.success = function (result) {
            var listItem = $("li[data-fileid='" + fileId + "']");
            listItem.remove();
            var fileList = "";
            $('li[data-fileid]').each(function () {
                var id = $(this).data("fileid");
                if (fileList == "") {
                    fileList = id;
                } else {
                    fileList = fileList + "," + id;
                }
            });
            $("#<%=hdAttachmentIds.ClientID%>").val(fileList);
        };
        options.error = function (err) { alert(err.statusText); };
        $.ajax(options);

        return false;
    }
    $(document).ready(function () {
        if ($("#<%=hdFileId.ClientID%>").val() != "") {
            $(".info").html("<span class='NormalRed'>RPO File Captured.</span>");
        }
        $("#uplWarrant").on("click", function (e) {
            $("#upload-overlay").show();
        });
        $("#uplAttachments").on("click", function (e) {
            $("#attach-overlay").show();
        });

        //send file async
        $("#<%=cmdUpload.ClientID%>").click(function (evt) {
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
                $("#<%=hdFileId.ClientID%>").val(fileId);
                WriteMessage(fileId);
            };
            options.error = function (err) {
                alert(err.statusText);
            };
            $.ajax(options);
            evt.preventDefault();
            $("#upload-overlay").hide();
        });

        //send attachment async
        $("#<%=cmdAddAttachment.ClientID%>").click(function (evt) {
            $("#attach-overlay").show();

            var upload = $("#<%=uplAttachments.ClientID%>");

            if (upload.is(':enabled')) {
                var selectedAttachmentType = $("#<%=drpAttachmentType.ClientID%>").val();
                var fileUpload = $("#<%=uplAttachments.ClientID%>").get(0);
                var files = fileUpload.files;
                if (files.length == 0) {
                    $(".attachmentInfo").html("<span class='NormalRed'>Please Choose a File!</span>");
                    return false;
                }
                var filename = files[0].name;
                if (selectedAttachmentType === "") {
                    $(".attachmentInfo").html("<span class='NormalRed'>Please Choose an Attachment Type!</span>");
                    return false;
                }
                var data = new FormData();
                data.append(filename, files[0]);
                data.append("attachmentType", selectedAttachmentType);
                data.append("moduleId",<%=ModuleId%>);
                data.append("tabId",<%=TabId%>);
                var options = {};
                options.url = "<%=attachmentHandler%>";
                options.type = "POST";
                options.data = data;
                options.contentType = false;
                options.processData = false;
                options.success = function (fileId) {
                    $("#attach-overlay").hide();
                    var fileIdList = $("#<%=hdAttachmentIds.ClientID%>").val();
                    if (fileIdList.length == 0) {
                        $("#<%=hdAttachmentIds.ClientID%>").val(fileId);
                    } else {
                        $("#<%=hdAttachmentIds.ClientID%>").val(fileIdList + "," + fileId);
                    }
                    $("#attachmentList").append("<li data-fileType='" + selectedAttachmentType + "' data-fileId='" + fileId + "'><span class='fileType'>" + selectedAttachmentType + "</span>&nbsp;<a onclick=\"DeleteAttachment('" + fileId + "')\"><em class='fa fa-trash'></em></></li>");
                    WriteAttachmentMessage(filename);
                };
                options.error = function (err) {
                    alert(err.statusText);
                    setTimeout(function () {
                        $("#attach-overlay").hide();
                        upload.parent().addClass("disabledWrapper");
                        //upload.parent().text("Choose File");
                        upload.attr("disabled", true);
                        upload.attr("title", "Select Attachment Type First");
                        $(".attachmentInfo").html('');
                    }, 1000);

                    $("#<%=drpAttachmentType.ClientID%>").val('');

                };

                $.ajax(options);

                evt.preventDefault();
            }
        });

        //check attachment type and enable upload if file type selected
        $("#<%=drpAttachmentType.ClientID%>").change(function () {
            var upload = $("#<%=uplAttachments.ClientID%>");
            if ($(this).val() != "") {
                upload.attr("disabled", false);
                upload.parent().removeClass("disabledWrapper");
                upload.attr("title", "Select File to Upload");
            } else {
                upload.attr("disabled", true);
                upload.parent().addClass("disabledWrapper");
                upload.attr("title", "Select Attachment Type First");
            }
        });

        $(".saApproved").hide();
        $(".tjcToggleDisplay").hide();
        $(".tjcToggleMessage").hide();
        $(".tjcToggleJudgeList").hide();
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

        //Validate Affidavit is uploaded
        $(".affidavitChk  input:checkbox").click(function () {
            var affidavit = $("li[data-fileType='Affidavit']");
            if (!affidavit.length) {
                $(this).prop('checked', false);
                ShowAlert("attachment Validation", "Attachment of type 'Affidavit' was not found. Please verify that you have uploaded the Affidavit as a separate PDF.");
            }
        });

        //validate Petision is uploaded
        $(".petitionChk  input:checkbox").click(function () {
            var petition = $("li[data-fileType='Petition']");
            if (!petition.length) {
                $(this).prop('checked', false);
                ShowAlert("Attachment Validation", "Attachment of type 'Petition' was not found. Please verify that you have uploaded the Petition as a separate PDF.");
            }
        });

        $(".tjcShowText").click(function (e) { $(".tjcToggleDisplay").toggle(); });

        // Auto Select County if only one available
        if ($("#<%=drpCounty.ClientID%> option").length <= 2) {
            $(".tjcToggleCountyList").hide()
            $("#<%=drpCounty.ClientID%> option:eq(1)").prop("selected", true);
        }
        //Disable the attachment Button
        setTimeout(function () {
            $("#<%=uplAttachments.ClientID%>").parent().addClass("disabledWrapper");
        }, 300);
        FillJudges();
    });
    function ClearOffenseList() {
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
            $(".info").html("<span class='NormalRed'>RPO File Captured.</span>");
            $(".firstText").focus();
        }
    }
    function WriteAttachmentMessage(filename) {
        if (filename == "") {
            $(".attachmentInfo").html("<span class='NormalRed'>Unable to upload file. Please make sure the file is in PDF format.</span>");
        } else {
            $("#fileAttachmentWarning").fadeOut();
            $(".attachmentInfo").html("<span class='NormalRed'>Attachment Captured.</span>");
        }
        var upload = $("#<%=uplAttachments.ClientID%>");
        upload.parent().addClass("disabledWrapper");
        var html = upload.parent().html();
        upload.parent().html(html.replace(filename, "Choose File"));
        //upload.parent().text("Choose File");
        upload.attr("disabled", true);
        upload.attr("title", "Select Attachment Type First");
        $(".attachmentInfo").html('');
        $("#<%=drpAttachmentType.ClientID%>").val('');


    }
    function chkRPORequired_ClientValidate(sender, e) {
        e.IsValid = $(".RPOChk input:checkbox").is(':checked');
    }
    function chkAffidavitRequired_ClientValidate(sender, e) {
        e.isValid = $(".affidavitChk input:checkbox").is(':checked');
    }
    function chkPetitionRequired_ClientValidate(sender, e) {
        e.IsValid = $(".petitionChk input:checkbox").is(':checked');
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
            }
        });
    }
    function FillJudges() {
        countyName = $("#<%=drpCounty.ClientID%> option:selected").val();
        judgeTypeId = "<%=JudgeTypeID %>";
        judgeList = $('#<%=drpJudge.ClientID%>');
        var ModuleId = '<%= ModuleId %>';
        var Url = "/DesktopModules/tjc.Modules/eWarrants/api/HttpData/rpojudges/" + ModuleId + "/";
        var choices = '';
        if (countyName != "" && judgeTypeId != undefined) {
            Url += countyName + "/" + judgeTypeId;
            $.ajax({
                type: "GET",
                dataType: "json",
                url: Url,
                success: function (j) {
                    if (j.length == 0) {
                        ShowAlert("No Judges Found", "No Judges exit for RPOs.<br />Please contact the site administrator.");
                        $(".tjcToggleJudgeList").hide();
                        choices += '<option value="">No Judges Found</option>';
                        judgeList.html(choices);
                    } else {
                        $(".tjcToggleJudgeList").fadeIn("slow");
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
