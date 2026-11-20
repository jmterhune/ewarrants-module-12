<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="EditJudge.ascx.cs" Inherits="tjc.Modules.eWarrants.EditJudge" %>
<%@ Register TagPrefix="dnn" TagName="Label" Src="~/controls/LabelControl.ascx" %>
<%@ Register TagPrefix="dnn" Namespace="DotNetNuke.Web.Client.ClientResourceManagement" Assembly="DotNetNuke.Web.Client" %>
<div class="dnnForm dnnClear">
    <div class="dnnFormItem dnnFormHelp dnnClear">
        <span class="dnnFormRequired"><%=Localization.GetString("RequiredFields", Localization.SharedResourceFile)%></span>
    </div>
    <div class="dnnFormItem labelnowidth">
        <dnn:Label ID="lblJudge" runat="server" ControlName="drpJudge" Text="Judge:" CssClass="dnnFormRequired"></dnn:Label>
        <asp:DropDownList ID="drpJudge" ClientIDMode="static"  runat="server"></asp:DropDownList>
        <asp:RequiredFieldValidator ID="valJudge" ControlToValidate="drpJudge"
            CssClass="dnnFormMessage dnnFormError" Display="Dynamic" ErrorMessage="You must select a judge" runat="server" />
    </div>
    <div class="dnnFormItem labelnowidth">
        <dnn:Label ID="lblOffenseType" runat="server" ControlName="rblOffenseType" Text="Division Type" Suffix=":" CssClass="dnnFormRequired"></dnn:Label>
        <asp:CheckBoxList ID="cklOffenseType" ClientIDMode="static"  runat="server" RepeatDirection="Horizontal" CssClass="dnnFormRadioButtons"></asp:CheckBoxList>

    </div>
    <div class="dnnFormItem labelnowidth">
        <dnn:Label ID="lblCounty" runat="server" ControlName="drpCounty" Text="Counties:" CssClass="dnnFormRequired"></dnn:Label>
        <asp:CheckBoxList ID="cklCounties" ClientIDMode="static"  runat="server" RepeatDirection="Horizontal" CssClass="dnnFormRadioButtons"></asp:CheckBoxList>
        <asp:CustomValidator runat="server" ID="cvmodulelist" ClientValidationFunction="ValidateCountyList" ErrorMessage="Please Select At least one County"></asp:CustomValidator>
    </div>
    <div class="dnnFormItem labelnowidth">
        <dnn:Label ID="lblDayStart" runat="server" ControlName="tpDayStart" Text="Daily Availability:" CssClass="dnnFormRequired"></dnn:Label>
        <asp:TextBox ID="txtDayStart" ClientIDMode="static"  runat="server" MaxLength="10" CssClass="date" /><span> to </span>
        <asp:TextBox ID="txtDayEnd" ClientIDMode="static"  runat="server" MaxLength="10" CssClass="date" />
    </div>
    <div class="dnnFormItem clearfix">

        <dnn:Label ID="lblupload" runat="server" ControlName="signatureUpload" Text="Upload Signatures" Suffix=":" HelpText="Select the Image Type and then select the file to upload" />
        <div style="position: relative;">
            <div id="sign-overlay" class="overlay" style="display: none;">
                <div class="spinner"></div>
            </div>
            <asp:DropDownList ID="drpSignatureType" ClientIDMode="Static" runat="server" CssClass="pull-left smallDropdown">
                <asp:ListItem Text="<Select Image Type>" Value="" />
                <asp:ListItem Value="s">Signature</asp:ListItem>
                <asp:ListItem Value="i">Initial</asp:ListItem>
            </asp:DropDownList>
            <asp:FileUpload ID="uplSignatures" ClientIDMode="static" runat="server" Enabled="false" CssClass="fileUpload" accept=".jpeg,.png,.jpg" onchange='check_signature(this.value);' ToolTip="Select Image Type First" />
            <asp:Button ID="cmdAddSignature" ClientIDMode="static"  CssClass="btn btn-primary sr-only" CausesValidation="false" runat="server" Text="Upload Selected File" Enabled="false" />
            <span id="fileSignatureWarning" class="dnnFormMessage dnnFormError" style="display: none; right: auto; left: 25%;">Please Choose File to Upload</span>
        </div>
    </div>
    <div class="dnnFormItem">
        <div class="formFieldAdjust">
            <span class="signatureInfo"></span>
            <ul id="signatureList" class="signatures">
            </ul>
        </div>
    </div>
    <asp:HiddenField ID="hdSignatureIds" ClientIDMode="Static" runat="server" />

    <ul class="dnnActions dnnClear">
        <li>
            <asp:LinkButton ID="cmdUpdate" ClientIDMode="static"  runat="server" OnClick="cmdUpdate_Click" Text="Save" CssClass="btn btn-primary" /></li>
        <li>
            <asp:LinkButton ID="cmdCancel" ClientIDMode="static"  runat="server" OnClick="cmdCancel_Click" Text="Cancel" CausesValidation="false" CssClass="btn btn-danger" />
        </li>
    </ul>
    <asp:Panel ID="pnlSignatures" ClientIDMode="static"  runat="server" Visible="false">
        <h2 id="dnnPanel-Signatures" class="dnnFormSectionHead">Signatures</h2>
        <div class="row">
            <div class="col-md-6">
                <strong>Signature</strong>
                <br />
                <asp:Image ID="imgSignature" runat="server" CssClass="img-fluid img-thumbnail" />
            </div>
            <div class="col-md-6">
                <strong>Initial</strong>
                <br />
                <asp:Image ID="imgInitial" runat="server" CssClass="img-fluid img-thumbnail"  />
            </div>
        </div>

    </asp:Panel>
</div>
<dnn:DnnCssInclude runat="server" FilePath="~/Resources/Shared/components/TimePicker/Themes/jquery-ui.min.css" />
<dnn:DnnCssInclude runat="server" FilePath="~/Resources/Shared/components/TimePicker/jquery.timepicker.css" />
<dnn:DnnJsInclude runat="server" FilePath="~/Resources/Shared/components/TimePicker/jquery.timepicker.js" />

<script type="text/javascript">
    var extensionHash = {
        '.jpeg': 1,
        '.png': 1,
        '.jpg': 1,
    };

    function check_signature(filename) {
        var re = /\..+$/;
        var ext = filename.match(re);
        var submitEl = document.getElementById('cmdAddSignature');
        if (extensionHash[ext]) {
            $(".info").html("");
            $( "#cmdAddSignature").trigger("click");
            submitEl.disabled = false;
            return true;
        } else {
            $(".signatureInfo").html("<span class='NormalRed'>Invalid Image Type, please choose a JPEG or PNG!</span>");
            submitEl.disabled = true;
            return false;
        }
    }
    function DeleteSignature(fileId) {
        var data = new FormData();
        data.append("fileId", fileId);
        data.append("actionType", "delete");  // Assuming the handler supports 'delete' actionType

        $.ajax({
            url: "<%=fileHandler%>",
                type: "POST",
                data: data,
                contentType: false,
                processData: false,
                headers: { "RequestVerificationToken": $('input[name="__RequestVerificationToken"]').val() },
                beforeSend: function () {
                    $("#sign-overlay").show();
                },
                success: function (result) {
                    $("#sign-overlay").hide();
                    var listItem = $("li[data-fileId='" + fileId + "']");
                    listItem.remove();

                    // Update hdSignatureIds by rebuilding the comma-separated list without the deleted item
                    var fileIdList = "";
                    $("li[data-fileId]").each(function () {
                        var currentFileId = $(this).data("fileId");
                        var currentType = $(this).data("type");
                        var valueSet = currentType + ":" + currentFileId;
                        if (fileIdList === "") {
                            fileIdList = valueSet;
                        } else {
                            fileIdList += "," + valueSet;
                        }
                    });
                    $("#hdSignatureIds").val(fileIdList);

                    $(".signatureInfo").html("<span class='dnnFormMessage dnnFormSuccess'>Signature deleted successfully.</span>");
                },
                error: function (xhr, status, error) {
                    $("#sign-overlay").hide();
                    $(".signatureInfo").html("<span class='dnnFormMessage dnnFormError'>Error deleting signature: " + error + "</span>");
                }
            });
       return false;
   }

    $(document).ready(function () {
        //Disable the signature Button
        setTimeout(function () {
            $("#uplSignatures").parent().addClass("disabledWrapper");
        }, 300);
        if ($.fn.timepicker) { $(".date").timepicker({}); }

        //send signature async
        $("#cmdAddSignature").click(function (evt) {
            $("#sign-overlay").show();
            var upload = $("#uplSignatures");
            var selectedSignatureType = $("#drpSignatureType").val();
            var selectedText = $("#drpSignatureType option:selected").text();
            var fileUpload = $("#uplSignatures").get(0);
            var files = fileUpload.files;
            if (files.length == 0) {
                $(".signatureInfo").html("<span class='dnnFormMessage dnnFormError'>Please Choose a Signature Image!</span>");
                $("#sign-overlay").hide();
                return false;
            }
            if (selectedSignatureType === "") {
                $(".signatureInfo").html("<span class='dnnFormMessage dnnFormError'>Please Choose a Signature Type!</span>");
                $("#sign-overlay").hide();
                return false;
            }

            var filename = files[0].name;
            var data = new FormData();
            data.append(filename, files[0]);
            data.append("signatureType", selectedSignatureType);
            data.append("actionType", "insert");

            $.ajax({
                url: "<%=fileHandler%>",
                type: "POST",
                data: data,
                contentType: false,
                processData: false,
                headers: { "RequestVerificationToken": $('input[name="__RequestVerificationToken"]').val() },
                success: function (fileId) {
                    $("#sign-overlay").hide();
                    var fileIdList = $("#hdSignatureIds").val();
                    var valueSet = selectedSignatureType + ":" + fileId;
                    if (fileIdList.length == 0) {
                        $("#hdSignatureIds").val(valueSet);
                    } else {
                        $("#hdSignatureIds").val(fileIdList + "," + valueSet);
                    }
                    $("#signatureList").append("<li data-type='" + selectedSignatureType + "' data-fileId='" + fileId + "'><span class='fileType'>" + selectedText + "</span>&nbsp;<a onclick=\"DeleteSignature('" + fileId + "')\"><em class='fa fa-trash'></em></a></li>");
                    WritesignatureMessage(filename);
                },
                error: function (xhr, status, error) {
                    $("#sign-overlay").hide();
                    upload.parent().addClass("disabledWrapper");
                    upload.attr("disabled", true);
                    upload.attr("title", "Select Signature Type First");
                    $(".signatureInfo").html("<span class='dnnFormMessage dnnFormError'>Error uploading signature: " + error + "</span>");
                    $("#drpSignatureType").val('');
                }
            });
            evt.preventDefault();
        });

        //check signature type and enable upload if file type selected
        $("#drpSignatureType").change(function () {
            var upload = $("#uplSignatures");
            if ($(this).val() != "") {
                upload.attr("disabled", false);
                upload.parent().removeClass("disabledWrapper");
                upload.attr("title", "Select File to Upload");
            } else {
                upload.attr("disabled", true);
                upload.parent().addClass("disabledWrapper");
                upload.attr("title", "Select Signature Type First");
            }
        });
    });

    function ValidateCountyList(source, args) {
        var cklCounties = document.getElementById('cklCounties');
        var chkListinputs = cklCounties.getElementsByTagName("input");
        for (var i = 0; i < chkListinputs.length; i++) {
            if (chkListinputs[i].checked) {
                args.IsValid = true;
                return;
            }
        }
        args.IsValid = false;
    }

    function ShowAlert(title, text) {
        $.dnnAlert({
            okText: 'OK',
            title: title,
            text: text
        });
    }

    function WritesignatureMessage(filename) {
        if (filename == "") {
            $(".signatureInfo").html("<span class='NormalRed'>Unable to upload file. Please make sure the file JPEG or PNG.</span>");
        } else {
            $("#fileSignatureWarning").fadeOut();
            $(".signatureInfo").html("<span class='NormalRed'>Signature Captured.</span>");
        }
        var upload = $("#uplSignatures");
        upload.parent().addClass("disabledWrapper");
        var html = upload.parent().html();
        upload.parent().html(html.replace(filename, "Choose File"));
        upload.attr("disabled", true);
        upload.attr("title", "Select signature Type First");
        $(".signatureInfo").html('');
        $("#drpSignatureType").val('');
    }
</script>
