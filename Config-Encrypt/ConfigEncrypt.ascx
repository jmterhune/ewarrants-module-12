<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="ConfigEncrypt.ascx.cs" Inherits="tjc.Modules.Utilities.ConfigEncryption" %>
<%@ Register TagPrefix="dnn" Namespace="DotNetNuke.Web.Client.ClientResourceManagement" Assembly="DotNetNuke.Web.Client" %>

<div class="dnnFormMessage dnnFormInfo">
    Select Protect to Encrypt the appsettings and connection string sections of your webconfig file. Select UnProtect to decrypt the sections for editing.
</div>
<fieldset>
    <legend></legend>
    <div class="dnnFormItem">
        <asp:LinkButton ID="cmdProtect" runat="server" CssClass="dnnPrimaryAction">Protect</asp:LinkButton>

        <asp:LinkButton ID="cmdUnProtect" runat="server" CssClass="dnnPrimaryAction"> UnProtect</asp:LinkButton>
    </div>
</fieldset>
