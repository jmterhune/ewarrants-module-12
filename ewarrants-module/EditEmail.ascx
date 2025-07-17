<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="EditEmail.ascx.cs" Inherits="tjc.Modules.eWarrants.EditEmail" %>
<%@ Register TagPrefix="dnn" TagName="Label" Src="~/controls/LabelControl.ascx" %>
<div class="dnnForm dnnClear">

    <fieldset>
        <legend></legend>
        <div class="dnnFormItem">

            <dnn:Label ID="lblEmailAddress" runat="server" Text="E-mail Address" ControlName="txtEmail" />
            <asp:TextBox ID="txtEmail" runat="server" Columns="30" MaxLength="256" CssClass="dnnFormRequired" />
            <asp:RequiredFieldValidator ID="valEmail" runat="server" Display="dynamic" SetFocusOnError="true"
                CssClass="dnnFormMessage dnnFormError" ErrorMessage="E-mail address is Required" ControlToValidate="txtEmail" />
            <asp:RegularExpressionValidator ID="valIsEmail" runat="server" Display="dynamic"
                SetFocusOnError="true" CssClass="dnnFormMessage dnnFormError" ErrorMessage="The value entered is not a valid E-mail address"
                ControlToValidate="txtEmail" ValidationExpression="\w+([-+.']\w+)*@\w+([-.]\w+)*\.\w+([-.]\w+)*" />
        </div>
    </fieldset>
    <ul class="dnnActions dnnClear">
        <li>
            <asp:LinkButton ID="btnSave" runat="server" Text="Save" CssClass="btn btn-primary" /></li>
        <li>
            <asp:HyperLink ID="lnkCancel" runat="server" class="btn btn-danger" Text="Return" /></li>
    </ul>
</div>