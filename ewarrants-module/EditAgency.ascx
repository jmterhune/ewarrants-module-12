<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="EditAgency.ascx.cs" Inherits="tjc.Modules.eWarrants.EditAgency" %>
<%@ Register TagPrefix="dnn" TagName="Label" Src="~/controls/LabelControl.ascx" %>
<div class="dnnForm dnnClear">
    <div class="dnnFormItem dnnFormHelp dnnClear">
        <span class="dnnFormRequired"><%=Localization.GetString("RequiredFields", Localization.SharedResourceFile)%></span>
    </div>

    <h2 id="dnnPanel-Personal" class="dnnFormSectionHead">Agency Information</h2>
    <fieldset>
        <legend></legend>
        <div class="dnnFormItem">
            <dnn:Label ID="lblAgencyName" runat="server" Text="Agency Name" ControlName="txtAgencyName" CssClass="dnnFormRequired" />
            <asp:TextBox ID="txtAgencyName" runat="server" Columns="40" MaxLength="250" />
            <asp:RequiredFieldValidator ID="valAgencyName" runat="server" Display="dynamic" SetFocusOnError="true"
                CssClass="dnnFormMessage dnnFormError" ErrorMessage="Agency Name is Required" ControlToValidate="txtAgencyName" />
        </div>
        <div class="dnnFormItem">
            <dnn:Label ID="lblParentAgency" runat="server" Text="Parent Agency"
                ControlName="txtParentAgency" />
            <asp:DropDownList runat="server" ID="drpParentAgency" DataTextField="AgencyName" DataValueField="AgencyID">
            </asp:DropDownList>
        </div>
        <div class="dnnFormItem">
            <dnn:Label ID="lblCounties" runat="server" Text="County Jurisdiction" CssClass="dnnFormRequired"
                ControlName="lstSelectedCounties" />
            <asp:CheckBoxList ID="chkCounties" RepeatLayout="Flow" RepeatDirection="Horizontal" runat="server" DataTextField="CountyName" DataValueField="CountyID">
            </asp:CheckBoxList>
            <asp:CustomValidator ID="valCounties" Display="Dynamic" SetFocusOnError="true" ErrorMessage="Please select at least one county."
                 CssClass="dnnFormMessage dnnFormError" ClientValidationFunction="ValidateCheckBoxList" runat="server" />
        </div>
        <div class="dnnFormItem">

            <dnn:Label ID="lblEmailAddress" runat="server" Text="E-mail Address" ControlName="txtEmail" CssClass="dnnFormRequired" />
            <asp:TextBox ID="txtEmail" runat="server" Columns="30" MaxLength="256" />
            <asp:RequiredFieldValidator ID="valEmail" runat="server" Display="dynamic" SetFocusOnError="true"
                CssClass="dnnFormMessage dnnFormError" ErrorMessage="E-mail address is Required" ControlToValidate="txtEmail" />
            <asp:RegularExpressionValidator ID="valIsEmail" runat="server" Display="dynamic"
                SetFocusOnError="true" CssClass="dnnFormMessage dnnFormError" ErrorMessage="The value entered is not a valid E-mail address"
                ControlToValidate="txtEmail" ValidationExpression="\w+([-+.']\w+)*@\w+([-.]\w+)*\.\w+([-.]\w+)*" />
        </div>
    </fieldset>

    <ul class="dnnActions dnnClear">
        <li>
            <asp:LinkButton ID="btnSave" runat="server" Text="Save" OnClick="btnSave_Click" CssClass="btn btn-primary" /></li>
        <li>
            <asp:HyperLink ID="lnkCancel" runat="server" class="btn btn-danger" Text="Return" /></li>
    </ul>
</div>
<script type="text/javascript">
    
    function ValidateCheckBoxList(sender, args) {
        var checkBoxList = document.getElementById("<%=chkCounties.ClientID %>");
        var checkboxes = checkBoxList.getElementsByTagName("input");
        var isValid = false;
        for (var i = 0; i < checkboxes.length; i++) {
            if (checkboxes[i].checked) {
                isValid = true;
                break;
            }
        }
        args.IsValid = isValid;
    }
</script>
