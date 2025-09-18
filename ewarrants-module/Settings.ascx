<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="Settings.ascx.cs" Inherits="tjc.Modules.eWarrants.Settings" %>
<%@ Register TagName="Label" TagPrefix="dnn" Src="~/controls/labelcontrol.ascx" %>

<div class="dnnForm dnnClear">
    <h2 class="dnnFormSectionHead"><a href="#">General Settings</a></h2>
    <fieldset class="dnnClear">
        <div class="dnnFormItem">
            <dnn:label id="lblDemoMode" runat="server" controlname="chkDemoMode" suffix=":"></dnn:label>
            <asp:CheckBox ID="chkDemoMode" runat="server" />
        </div>

        <div class="dnnFormItem">
            <dnn:label id="lblSergeant" runat="server" controlname="drpSergeantRole" suffix=":"></dnn:label>
            <asp:DropDownList ID="drpSergeantRole" runat="server"></asp:DropDownList>
        </div>
        <div class="dnnFormItem">
            <dnn:label id="lblJudgeRole" runat="server" controlname="drpJudgeRole" suffix=":"></dnn:label>
            <asp:DropDownList ID="drpJudgeRole" AutoPostBack="true" runat="server"></asp:DropDownList>
        </div>
        <div class="dnnFormItem">
            <dnn:label id="lblAdminJudge" runat="server" controlname="drpAdminJudge" suffix=":"></dnn:label>
            <asp:DropDownList ID="drpAdminJudge" runat="server"></asp:DropDownList>
        </div>
        <div class="dnnFormItem">
            <dnn:label id="lblJaRole" runat="server" controlname="drpJARole" suffix=":"></dnn:label>
            <asp:DropDownList ID="drpJARole" AutoPostBack="true" runat="server"></asp:DropDownList>
        </div>
        <div class="dnnFormItem">
            <dnn:label id="lblSenderEmail" runat="server" controlname="txtSenderEmail" suffix=":"></dnn:label>
            <asp:TextBox ID="txtSenderEmail" runat="server" MaxLength="250"></asp:TextBox>
        </div>

        <div class="dnnFormItem">
            <dnn:label id="lblNotificationEmail" runat="server" controlname="txtNotificationEmail" suffix=":"></dnn:label>
            <asp:TextBox ID="txtNotificationEmail" runat="server" MaxLength="250"></asp:TextBox>
        </div>
        <div class="dnnFormItem">
            <dnn:label id="lblDeleteThreshold" runat="server" controlname="txtDeleteThreshold" suffix=":"></dnn:label>
            <asp:TextBox ID="txtDeleteThreshold" runat="server" MaxLength="2" Width="50"></asp:TextBox>
        </div>
        <div class="dnnFormItem">
            <dnn:label id="lblSignedThreshold" runat="server" controlname="txtSignedThreshold" suffix=":"></dnn:label>
            <asp:TextBox ID="txtSignedThreshold" runat="server" MaxLength="2" Width="50"></asp:TextBox>
        </div>
        <div class="dnnFormItem">
            <dnn:label id="lblClaimedThreshold" runat="server" controlname="txtClaimedThreshold" suffix=":"></dnn:label>
            <asp:TextBox ID="txtClaimedThreshold" runat="server" MaxLength="2" Width="50"></asp:TextBox>
        </div>
        <div class="dnnFormItem">
            <dnn:label id="lblCompletedThreshold" runat="server" controlname="txtCompletedThreshold" suffix=":"></dnn:label>
            <asp:TextBox ID="txtCompletedThreshold" runat="server" MaxLength="2" Text="0" Width="50"></asp:TextBox>
        </div>

        <div class="dnnFormItem">
            <dnn:label id="lblJAPage" runat="server" controlname="cboJAPage" suffix=":" text="JA Notification Page"></dnn:label>
            <asp:DropDownList ID="cboJAPage" runat="server" CssClass="NormalTextBox" Width="200" DataValueField="TabID" DataTextField="IndentedTabName" AutoPostBack="false" />
        </div>
        <div class="dnnFormItem">
            <dnn:label id="lblContactListUrl" runat="server" controlname="cboContactList" suffix=":"></dnn:label>
            <asp:DropDownList ID="cboContactList" runat="server" CssClass="NormalTextBox" Width="200" DataValueField="TabID" DataTextField="IndentedTabName" AutoPostBack="false" />
        </div>
        <div class="dnnFormItem">
            <dnn:label id="lblDeSotoClerk" runat="server" controlname="txtDeSotoClerk" suffix=":"></dnn:label>
            <asp:TextBox ID="txtDeSotoClerk" runat="server" MaxLength="250"></asp:TextBox>
        </div>
        <div class="dnnFormItem">
            <dnn:label id="lblManateeClerk" runat="server" controlname="txtManateeClerk" suffix=":"></dnn:label>
            <asp:TextBox ID="txtManateeClerk" runat="server" MaxLength="250"></asp:TextBox>
        </div>
        <div class="dnnFormItem">
            <dnn:label id="lblSarasotaClerk" runat="server" controlname="txtSarasotaClerk" suffix=":"></dnn:label>
            <asp:TextBox ID="txtSarasotaClerk" runat="server" MaxLength="250"></asp:TextBox>
        </div>

    </fieldset>

    <h2 class="dnnFormSectionHead"><a href="#">RPO Email Settings</a></h2>
    <fieldset class="dnnClear">
        <div class="dnnFormItem">
            <dnn:label id="lblJudgeType" runat="server" controlname="drpJudgeTypes" suffix=":" text="RPO Type"></dnn:label>
            <asp:DropDownList ID="drpJudgeTypes" runat="server" CssClass="NormalTextBox" AutoPostBack="false" />
        </div>

        <div class="dnnFormItem">
            <dnn:label id="lblCC" runat="server" controlname="txtCC" suffix=":" text="RPO CC List"></dnn:label>
            <asp:TextBox ID="txtCC" runat="server" MaxLength="2000" TextMode="MultiLine" Rows="3"></asp:TextBox>
        </div>
        <div class="dnnFormItem">
            <dnn:label id="lblRPODesoto" runat="server" controlname="txtRPODeSoto" suffix=":" text="DeSoto Email List"></dnn:label>
            <asp:TextBox ID="txtRPODeSoto" runat="server" MaxLength="2000" TextMode="MultiLine" Rows="3"></asp:TextBox>
        </div>
        <div class="dnnFormItem">
            <dnn:label id="lblDesotoAOJ" runat="server" controlname="drpDesotoAOJ" suffix=":" text="DeSoto Afer Hours Judge"></dnn:label>
            <asp:DropDownList ID="drpDesotoAOJ" runat="server" CssClass="NormalTextBox" AutoPostBack="false" />
        </div>

        <div class="dnnFormItem">
            <dnn:label id="lblRPOManatee" runat="server" controlname="txtRPOManatee" suffix=":" text="Manatee Email List"></dnn:label>
            <asp:TextBox ID="txtRPOManatee" runat="server" MaxLength="2000" TextMode="MultiLine" Rows="3"></asp:TextBox>
        </div>
        <div class="dnnFormItem">
            <dnn:label id="lblManateeAOJ" runat="server" controlname="drpManateeAOJ" suffix=":" text="Manatee Afer Hours Judge"></dnn:label>
            <asp:DropDownList ID="drpManateeAOJ" runat="server" CssClass="NormalTextBox" AutoPostBack="false" />
        </div>
        <div class="dnnFormItem">
            <dnn:label id="lblRPOSarasota" runat="server" controlname="txtRPOSarasota" suffix=":" text="Sarasota Email List"></dnn:label>
            <asp:TextBox ID="txtRPOSarasota" runat="server" MaxLength="2000" TextMode="MultiLine" Rows="3"></asp:TextBox>
        </div>
        <div class="dnnFormItem">
            <dnn:label id="lblSarasotaAOJ" runat="server" controlname="drpSarasotaAOJ" suffix=":" text="Sarasota Afer Hours Judge"></dnn:label>
            <asp:DropDownList ID="drpSarasotaAOJ" runat="server" CssClass="NormalTextBox" AutoPostBack="false" />
        </div>

        <div class="dnnFormItem">
            <dnn:label id="lblAttachemntDirectory" runat="server" controlname="txtAttachemntDirectory" suffix=":" text="Attachment Directory"></dnn:label>
            <asp:TextBox ID="txtAttachemntDirectory" runat="server" MaxLength="2000"></asp:TextBox>
        </div>

    </fieldset>

    <h2 class="dnnFormSectionHead"><a href="#">Telnyx SMS Settings</a></h2>
    <fieldset class="dnnClear">
        <div class="dnnFormItem">
            <dnn:label id="lblTelnyxApiKey" runat="server" controlname="txtTelnyxApiKey" suffix=":" text="Auth Token"></dnn:label>
            <asp:TextBox ID="txtTelnyxApiKey" runat="server" MaxLength="50"></asp:TextBox>
        </div>
        <div class="dnnFormItem">
            <dnn:label id="lblTelnyxPhoneNumber" runat="server" controlname="txtTelnyxPhoneNumber" suffix=":" text="Phone Number"></dnn:label>
            <asp:TextBox ID="txtTelnyxPhoneNumber" runat="server" MaxLength="50"></asp:TextBox>
        </div>
    </fieldset>
</div>

