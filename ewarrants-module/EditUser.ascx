<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="EditUser.ascx.cs" Inherits="tjc.Modules.eWarrants.EditUser" %>
<%@ Register TagPrefix="dnn" TagName="Label" Src="~/controls/LabelControl.ascx" %>
<div class="dnnForm dnnClear">
    <div class="dnnFormItem dnnFormHelp dnnClear">
        <p class="dnnFormRequired"><span><%=Localization.GetString("RequiredFields", Localization.SharedResourceFile)%></span></p>
    </div>

    <h2 id="dnnPanel-Personal" class="dnnFormSectionHead">Personal Information</h2>
    <fieldset>
        <legend></legend>
        <div class="dnnFormItem">
            <dnn:Label ID="lblFirstName" runat="server" Text="First Name" ControlName="txtFirstName" />
            <asp:TextBox ID="txtFirstName" runat="server" CssClass="dnnFormRequired" Columns="20" MaxLength="50" />
            <asp:RequiredFieldValidator ID="valFirstName" runat="server" Display="dynamic" SetFocusOnError="true"
                CssClass="dnnFormMessage dnnFormError" ErrorMessage="First Name is Required" ControlToValidate="txtFirstName" />
        </div>
        <div class="dnnFormItem">
            <dnn:Label ID="lblLastName" runat="server" Text="Last Name"
                ControlName="txtLastName" />
            <asp:TextBox ID="txtLastName" runat="server" CssClass="dnnFormRequired" Columns="20" MaxLength="50" />
            <asp:RequiredFieldValidator ID="valLastName" runat="server" Display="dynamic" SetFocusOnError="true"
                CssClass="dnnFormMessage dnnFormError" ErrorMessage="Last Name is required" ControlToValidate="txtLastName" />
        </div>

        <div class="dnnFormItem">

            <dnn:Label ID="lblEmailAddress" runat="server" Text="E-mail Address" ControlName="txtEmail" />
            <asp:TextBox ID="txtEmail" runat="server" Columns="30" MaxLength="256" CssClass="dnnFormRequired" />
            <asp:RequiredFieldValidator ID="valEmail" runat="server" Display="dynamic" SetFocusOnError="true"
                CssClass="dnnFormMessage dnnFormError" ErrorMessage="E-mail address is Required" ControlToValidate="txtEmail" />
            <asp:RegularExpressionValidator ID="valIsEmail" runat="server" Display="dynamic"
                SetFocusOnError="true" CssClass="dnnFormMessage dnnFormError" ErrorMessage="The value entered is not a valid E-mail address"
                ControlToValidate="txtEmail" ValidationExpression="\w+([-+.']\w+)*@\w+([-.]\w+)*\.\w+([-.]\w+)*" />
            <asp:RegularExpressionValidator ID="valUserNoSpace" runat="server" Display="dynamic"
                SetFocusOnError="true" CssClass="dnnFormMessage dnnFormError" ErrorMessage="Usernames can not have spaces"
                ControlToValidate="txtUserName" ValidationExpression="^[^\s]+$" />
        </div>
    </fieldset>
    <h2 id="dnnPanel-Account" class="dnnFormSectionHead">Account Information</h2>
    <fieldset>
        <legend></legend>
        <div class="dnnFormItem">

            <dnn:Label ID="lblUserName" runat="server" Text="Username" ControlName="txtUserName" />
            <asp:TextBox ID="txtUserName" CssClass="dnnFormRequired" MaxLength="100" runat="server" Columns="20" />
            <asp:RequiredFieldValidator ID="valUserName" runat="server" Display="dynamic" SetFocusOnError="true"
                CssClass="dnnFormMessage dnnFormError" ErrorMessage="Username is Required" ControlToValidate="txtUserName" />
           
            <span id="userMessage"></span>
        </div>

        <div runat="server" id="pChecks">
            <div class="dnnFormItem">
                <asp:CheckBox ID="chkNotify" runat="server" Checked="true"  TextAlign="Left"  Text="Notify User by Email?"
                    CssClass="SubHead" />
            </div>
            <div class="dnnFormItem">
                <asp:CheckBox ID="chkAdmin" runat="server"  TextAlign="Left" Text="Set as Account Admin?"
                    CssClass="SubHead" />
            </div>
        </div>
    </fieldset>


    <ul class="dnnActions dnnClear">
        <li>
            <asp:LinkButton ID="btnSave" runat="server" OnClick="btnSave_Click" Text="Save" CssClass="btn btn-primary" /></li>
        <li>
            <asp:HyperLink ID="lnkCancel" runat="server" class="btn btn-danger" Text="Return" /></li>
    </ul>
</div>
