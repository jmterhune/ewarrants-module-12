<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="EditAnnotation.ascx.cs" Inherits="tjc.Modules.eWarrants.EditAnnotation" %>
<%@ Register TagPrefix="dnn" TagName="Label" Src="~/controls/LabelControl.ascx" %>
<div class="dnnFormMessage dnnFormInfo">To add a custom text item, enter your text in the Custom Text field and click the save button.  To remove a custom text Item, click the delete button next to the item you wish to delete.</div>
<asp:Repeater ID="rptAnnotations" runat="server" EnableViewState="true">
    <HeaderTemplate><div class="listAnnotations">
        <h4>Custom Text List</h4>
        <ul id="customTextList">
    </HeaderTemplate>
    <FooterTemplate></ul></div><hr /></FooterTemplate>
    <ItemTemplate>
        <li><span class="listItem"><%#Eval("AnnotationText") %></span>
            <asp:LinkButton ID="cmdDelete" runat="server" OnClientClick="return confirm('Delete this Custom Text?');" OnClick="cmdDelete_Click"  CausesValidation="false" CommandArgument='<%#Eval("AnnotationId") %>' CommandName="Delete">
           <i class="fa fa-trash" title="Delete Text"></i>
                 </asp:LinkButton>

        </li>
    </ItemTemplate>
</asp:Repeater>

<style>
    .dnnFormItem .dnnLabel{ min-width:100px; width:100px;}
</style>
<div class="dnnForm dnnClear">
    <div class="dnnFormItem">
        <dnn:Label ID="lblAnnotationText" runat="server" ControlName="txtAnnotation" Text="Custom Text:" CssClass="dnnFormRequired"></dnn:Label>
        <asp:TextBox ID="txtAnnotation" runat="server" TextMode="MultiLine" Rows="3" />
        <asp:RequiredFieldValidator ID="valJudge" ControlToValidate="txtAnnotation"
            CssClass="dnnFormMessage dnnFormError" Display="Dynamic" ErrorMessage="No Text Entered" runat="server" />
    </div>

    <ul class="dnnActions dnnClear">
        <li>
            <asp:LinkButton ID="cmdUpdate" OnClick="cmdUpdate_Click" runat="server" Text="Save" CssClass="btn btn-primary" /></li>
        <li>
            <asp:LinkButton ID="cmdCancel" OnClick="cmdCancel_Click" runat="server" Text="Close" CausesValidation="false" CssClass="btn btn-danger" />
        </li>
    </ul>
</div>

