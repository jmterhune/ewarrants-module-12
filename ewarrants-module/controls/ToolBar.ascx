<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="ToolBar.ascx.cs" Inherits="tjc.Modules.eWarrants.controls.ToolBar" %>
<div id="tbGroup" runat="server" class="toolbar btn-group mb-2" role="group" aria-label="Warrant Toolbar">
    <asp:HyperLink ID="lnkSms" Visible="false" runat="server" CssClass="btn btn-warning"><em class='fa fa-phone'></em> Manage SMS Preferences</asp:HyperLink>
    <asp:HyperLink ID="lnkJudges" Visible="false" runat="server" CssClass="btn btn-primary"><em class='fa fa-gavel'></em> Manage Judges</asp:HyperLink>
    <asp:HyperLink ID="lnkAgencies" Visible="false" runat="server" CssClass="btn btn-primary"><em class='fa fa-star'></em> Manage Agencies</asp:HyperLink>
    <asp:HyperLink ID="lnkLists" Visible="false" runat="server" CssClass="btn btn-primary"><em class='fa fa-list'></em> Manage Lists</asp:HyperLink>
    <asp:HyperLink ID="lnkChangeStatus" Visible="false" runat="server" CssClass="btn btn-primary"><em class='fa fa-bar-chart'></em> Change Status</asp:HyperLink>
    <asp:HyperLink ID="lnkUsers" Visible="false" runat="server" CssClass="btn btn-primary"><em class='fa fa-users'></em> Manage Users</asp:HyperLink>
    <asp:HyperLink ID="lnkCustomText" Visible="false" runat="server" CssClass="btn btn-primary"><em class='fa fa-keyboard-o'></em> Manage Custom Text</asp:HyperLink>
    <asp:HyperLink ID="lnkWarrant" runat="server" CssClass="btn btn-primary"><em class='fa fa-upload'></em> Upload Warrant</asp:HyperLink>
    <asp:HyperLink ID="lnkRPO" runat="server" CssClass="btn btn-danger"><em class='fa fa-upload'></em> Upload RPO</asp:HyperLink>
    <asp:HyperLink ID="lnkOutofOffice" Visible="false" runat="server" CssClass="btn btn-primary"><em class='fa fa-calendar'></em> Out of Office Notice</asp:HyperLink>
    <asp:HyperLink ID="lnkContactInfo" runat="server" CssClass="btn btn-primary"><em class='fa fa-phone'></em> Contact Information</asp:HyperLink>
</div>
