using DotNetNuke.Abstractions;
using DotNetNuke.Entities.Users;
using DotNetNuke.Services.Exceptions;
using DotNetNuke.Services.Mail;
using DotNetNuke.Web.UI;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Web.UI;
using System.Web.UI.WebControls;
using tjc.Modules.eWarrants.Components;
using PhoneNumbers;
using Newtonsoft.Json;

namespace tjc.Modules.eWarrants
{
    public partial class WarrantList : ewarrantsModuleBase
    {
        private readonly INavigationManager _navigationManager;
        private readonly Encryptor _encryptor = new Encryptor();
        public string p_counties;
        private AgencyUserListItem AgencyUserInfo = new AgencyUserListItem();
        private bool isJudge
        {
            get { return ViewState["IsJudge"] != null && Convert.ToBoolean(ViewState["IsJudge"]); }
            set { ViewState["IsJudge"] = value; }
        }

        public WarrantList()
        {
            _navigationManager = DependencyProvider.GetRequiredService<INavigationManager>();
        }

        private void BindData()
        {
            try
            {
                var wCtl = new WarrantController();
                IEnumerable<WarrantListItem> colWarrants;

                if (isJudge)
                {
                    colWarrants = wCtl.GetWarrantListItems(ModuleId, StartDate, EndDate.AddDays(1)) ?? Enumerable.Empty<WarrantListItem>();
                    rptWarrants.DataSource = chkShowAssigned.Checked ? colWarrants.Where(w => w.JudgeUserID == UserId) : colWarrants;
                }
                else if (IsSeargeant)
                {
                    colWarrants = wCtl.GetWarrantListItemsBySergeant(ModuleId, UserId, StartDate, EndDate.AddDays(1)) ?? Enumerable.Empty<WarrantListItem>();
                    rptWarrants.DataSource = colWarrants;
                }
                else
                {
                    colWarrants = wCtl.GetWarrantListItemsByUser(ModuleId, UserId, StartDate, EndDate.AddDays(1)) ?? Enumerable.Empty<WarrantListItem>();
                    rptWarrants.DataSource = colWarrants;
                }

                rptWarrants.DataBind();
            }
            catch (Exception exc)
            {
                Exceptions.ProcessModuleLoadException(this, exc);
                DotNetNuke.UI.Skins.Skin.AddModuleMessage(this, "Error loading warrant list. Please try again or contact support.", DotNetNuke.UI.Skins.Controls.ModuleMessage.ModuleMessageType.RedError);
            }
        }

        private void SendEmailResponse(WarrantController ctl, int warrantId)
        {
            try
            {
                var sCtl = new SettingController();
                Setting objSettings = sCtl.GetSettings(ModuleId);

                string leoAddress = UserInfo != null ? UserInfo.Email : throw new InvalidOperationException("User email not available.");
                string noreplyAddress = Settings.Contains("fromemail") ? Settings["fromemail"].ToString() : "webhelp@jud12.flcourts.org";

                Warrant objWarrant = ctl.GetWarrant(warrantId);
                if (objWarrant == null)
                    throw new InvalidOperationException("Warrant not found.");
                UserInfo judge = UserController.GetUserById(PortalId, objWarrant.JudgeUserID);
                if (judge == null)
                {
                    DotNetNuke.UI.Skins.Skin.AddModuleMessage(this, "The selected judge's record could not be found. No email notification can be sent.", DotNetNuke.UI.Skins.Controls.ModuleMessage.ModuleMessageType.RedError);
                    return;
                }

                var nCtl = new NotificationController();
                var colNotifications = nCtl.GetCurrentNotificationsByJudge(objWarrant.JudgeUserID);
                if (colNotifications != null && colNotifications.Any())
                {
                    Notification objNotification = colNotifications.FirstOrDefault();
                    if (objNotification.CoveringJudgeID > 0)
                        judge = UserController.GetUserById(PortalId, objNotification.CoveringJudgeID) ?? judge;
                }

                string judgeAddress = judge.Email;
                string judgeProfileContact1 = Helper.GetProfileValue(judge, "Email2") ?? "";
                string judgeProfileContact2 = Helper.GetProfileValue(judge, "Email3") ?? "";
                string subject = "Warrant ID: " + warrantId + " Deleted";
                string body = "Officer " + UserInfo.LastName + ", " + UserInfo.FirstName + " has deleted the warrant with ID: " + warrantId;

                Mail.SendEmail(noreplyAddress, judgeAddress, subject, body);
                Helper.SendToAlternateContact(noreplyAddress, judgeProfileContact1, subject, body);
                Helper.SendToAlternateContact(noreplyAddress, judgeProfileContact2, subject, body);
                if (HasUserOptedForSms(objWarrant.JudgeUserID))
                {
                    Task.Run(() => Helper.SendSmsNotifications(judge, subject, body)).GetAwaiter().GetResult();
                }
            }
            catch (Exception exc)
            {
                Exceptions.LogException(exc);
                DotNetNuke.UI.Skins.Skin.AddModuleMessage(this, "Error sending email notifications. Please try again or contact support.", DotNetNuke.UI.Skins.Controls.ModuleMessage.ModuleMessageType.RedError);
            }
        }

        protected override void OnInit(EventArgs e)
        {
            try
            {
                base.OnInit(e);
                isJudge = IsJudge;
                var aCtl = new AgencyController();
                AgencyUserInfo = aCtl.GetAgencyUser(UserId, ModuleId) ?? new AgencyUserListItem();
                if (AgencyUserInfo.UserID > 0)
                    this.ModuleConfiguration.ModuleTitle = IsSeargeant ? "Document List" : "Document List for " + UserInfo.DisplayName;
            }
            catch (Exception exc)
            {
                Exceptions.ProcessModuleLoadException(this, exc);
            }
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            try
            {
                if (!Page.IsPostBack)
                {
                    if (DotNetNuke.Framework.AJAX.IsInstalled())
                        DotNetNuke.Framework.AJAX.RegisterScriptManager();

                    toolbar.AdminJudge = AdminJudgeRole;
                    toolbar.IsAdminJudge = IsAdminJudge;
                    toolbar.IsJudge = isJudge;
                    toolbar.IsSiteAdmin = IsSiteAdmin;
                    toolbar.JudgeRoleName = JudgeRoleName;
                    toolbar.NavAddWarrant = EditUrl("addWarrant");
                    toolbar.NavAddRPO = EditUrl("rpo");
                    toolbar.NavClerkFTP = EditUrl("adminFTP");
                    toolbar.NavAdminAgency = EditUrl("adminagency");
                    toolbar.NavAdminCounty = EditUrl("admincounty");
                    toolbar.NavAnnotations = EditUrl("annotations");
                    toolbar.NavJudgeSign = EditUrl("judgesign");
                    toolbar.NavManageUsers = EditUrl("users");
                    toolbar.NavOutofOffice = EditUrl("cover");
                    toolbar.NavStatus = EditUrl("wadmin");
                    toolbar.HasAgencyUser = AgencyUserInfo.UserID > 0;
                    toolbar.AgencyUserIsAdmin = AgencyUserInfo.IsAdmin;
                    toolbar.ContactListUrl = ContactPage;
                    toolbar.SergeantRoleName = SergeantRoleName;
                    toolbar.NavSms = EditUrl("sms");
                    txtStartDate.Text = StartDate.ToShortDateString();
                    txtEndDate.Text = EndDate.ToShortDateString();

                    if (string.IsNullOrEmpty(JudgeRoleName) || string.IsNullOrEmpty(AdminJudgeRole) || string.IsNullOrEmpty(SergeantRoleName))
                    {
                        DotNetNuke.UI.Skins.Skin.AddModuleMessage(this, "The application settings have not been set. Please contact the site administrator to configure settings.", DotNetNuke.UI.Skins.Controls.ModuleMessage.ModuleMessageType.YellowWarning);
                        rptWarrants.Visible = false;
                        dvFilters.Visible = false;
                        return;
                    }

                    if (Settings.Contains("hours") && !isJudge)
                        CompletedWarrantsThreshold = Convert.ToInt32(Settings["hours"]);

                    if (CompletedWarrantsThreshold > 0)
                        ltMessage.Text = string.Format(ltMessage.Text, CompletedWarrantsThreshold.ToString());
                    else
                        ltMessage.Visible = false;

                    if (Request.QueryString["msg"] != null)
                        DotNetNuke.UI.Skins.Skin.AddModuleMessage(this, Request.QueryString["msg"].ToString().Trim(), DotNetNuke.UI.Skins.Controls.ModuleMessage.ModuleMessageType.YellowWarning);

                    if (IsAdminJudge || IsSiteAdmin)
                    {
                        if (IsSiteAdmin)
                        {
                            rptWarrants.Visible = false;
                            dvFilters.Visible = false;
                        }
                    }
                    else if (AgencyUserInfo.UserID <= 0 && !isJudge)
                    {
                        rptWarrants.Visible = false;
                        dvFilters.Visible = false;
                        DotNetNuke.UI.Skins.Skin.AddModuleMessage(this, "You have not been assigned to a law enforcement agency. Please contact <a href=\"mailto:" + PortalSettings.Email + "\">support</a> to report this error.", DotNetNuke.UI.Skins.Controls.ModuleMessage.ModuleMessageType.BlueInfo);
                    }
                    else if (AgencyUserInfo.UserID > 0)
                    {
                        var cCtl = new CountyController();
                        var colCounties = cCtl.GetCountiesByAgency(AgencyUserInfo.AgencyID) ?? Enumerable.Empty<County>();
                        p_counties = string.Join(";", colCounties.Select(c => c.CountyName.ToLower())).Trim(';');
                        if (AgencyUserInfo.IsAdmin)
                        {
                            rptWarrants.Visible = false;
                            dvFilters.Visible = false;
                        }
                    }

                    if (isJudge)
                    {
                        var jCtl = new JudgeController();
                        var judge = jCtl.GetJudge(UserId);
                        chkShowAssigned.Visible = judge != null;
                    }

                    BindData();
                }
            }
            catch (Exception exc)
            {
                Exceptions.ProcessModuleLoadException(this, exc);
                DotNetNuke.UI.Skins.Skin.AddModuleMessage(this, "Error loading page. Please try again or contact support.", DotNetNuke.UI.Skins.Controls.ModuleMessage.ModuleMessageType.RedError);
            }
        }

        protected void txtStartDate_TextChanged(object sender, EventArgs e)
        {
            try
            {
                if (DateTime.TryParse(txtStartDate.Text, out DateTime startDate))
                {
                    StartDate = startDate;
                    BindData();
                }
            }
            catch (Exception exc)
            {
                Exceptions.ProcessModuleLoadException(this, exc);
                DotNetNuke.UI.Skins.Skin.AddModuleMessage(this, "Error updating start date. Please try again or contact support.", DotNetNuke.UI.Skins.Controls.ModuleMessage.ModuleMessageType.RedError);
            }
        }

        protected void txtEndDate_TextChanged(object sender, EventArgs e)
        {
            try
            {
                if (DateTime.TryParse(txtEndDate.Text, out DateTime endDate))
                {
                    EndDate = endDate;
                    BindData();
                }
            }
            catch (Exception exc)
            {
                Exceptions.ProcessModuleLoadException(this, exc);
                DotNetNuke.UI.Skins.Skin.AddModuleMessage(this, "Error updating end date. Please try again or contact support.", DotNetNuke.UI.Skins.Controls.ModuleMessage.ModuleMessageType.RedError);
            }
        }

        protected void dptUpdate_Unload(object sender, EventArgs e)
        {
            try
            {
                MethodInfo methodInfo = typeof(ScriptManager).GetMethods(BindingFlags.NonPublic | BindingFlags.Instance)
                    .FirstOrDefault(i => i.Name.Equals("System.Web.UI.IScriptManagerInternal.RegisterUpdatePanel"));
                if (methodInfo != null)
                    methodInfo.Invoke(ScriptManager.GetCurrent(Page), new object[] { sender as UpdatePanel });
            }
            catch (Exception exc)
            {
                Exceptions.ProcessModuleLoadException(this, exc);
            }
        }

        protected void rptWarrants_ItemCommand(object source, RepeaterCommandEventArgs e)
        {
            try
            {
                var ctl = new WarrantController();
                int warrantId = Convert.ToInt32(e.CommandArgument);

                switch (e.CommandName.ToLower())
                {
                    case "release":
                        Warrant releaseWarrant = ctl.GetWarrant(warrantId);
                        if (releaseWarrant != null)
                        {
                            releaseWarrant.Status = WarrantStatus.New;
                            ctl.UpdateWarrant(releaseWarrant);
                            BindData();
                        }
                        else
                        {
                            DotNetNuke.UI.Skins.Skin.AddModuleMessage(this, "Error retrieving document information.", DotNetNuke.UI.Skins.Controls.ModuleMessage.ModuleMessageType.RedError);
                        }
                        break;

                    case "sign":
                        string viewer = "WarrantView";
                        var jCtl = new JudgeController();
                        Judge judge = jCtl.GetJudge(UserId);
                        string encryptedValue = _encryptor.QueryStringEncode(warrantId.ToString(), UserInfo.Username);
                        string url = TemplateSourceDirectory + "/" + viewer + ".aspx?ModuleId=" + ModuleId + "&enc=" + encryptedValue;
                        Warrant signWarrant = ctl.GetWarrant(warrantId);
                        if (signWarrant != null)
                        {
                            if (signWarrant.Status == WarrantStatus.UnderReview && signWarrant.ReviewedByJudgeUserID != UserId)
                            {
                                DotNetNuke.UI.Skins.Skin.AddModuleMessage(this, "This document is currently under review by another judge.", DotNetNuke.UI.Skins.Controls.ModuleMessage.ModuleMessageType.RedError);
                                BindData();
                                return;
                            }
                            signWarrant.ReviewedByJudgeUserID = UserId;
                            signWarrant.Status = WarrantStatus.UnderReview;
                            ctl.UpdateWarrant(signWarrant);
                            Response.Redirect(url, true);
                        }
                        else
                        {
                            DotNetNuke.UI.Skins.Skin.AddModuleMessage(this, "Error retrieving document information.", DotNetNuke.UI.Skins.Controls.ModuleMessage.ModuleMessageType.RedError);
                        }
                        break;

                    case "delete":
                        var log = new Log
                        {
                            UserID = UserInfo.UserID,
                            EventTime = DateTime.Now,
                            EventDescription = "Deleted Record",
                            WarrantID = warrantId,
                            IP = GetIp()
                        };
                        var ctlLog = new LogController();
                        ctlLog.CreateLog(log);
                        SendEmailResponse(ctl, warrantId);
                        ctl.DeleteWarrant(warrantId);
                        BindData();
                        break;
                }
            }
            catch (Exception exc)
            {
                Exceptions.ProcessModuleLoadException(this, exc);
                DotNetNuke.UI.Skins.Skin.AddModuleMessage(this, "Error processing command: " + exc.Message, DotNetNuke.UI.Skins.Controls.ModuleMessage.ModuleMessageType.YellowWarning);
            }
        }

        protected void rptWarrants_ItemDataBound(object sender, RepeaterItemEventArgs e)
        {
            if (e.Item.ItemType == ListItemType.Item || e.Item.ItemType == ListItemType.AlternatingItem)
            {
                try
                {
                    RepeaterItem item = e.Item;
                    var lnkSign = (LinkButton)item.FindControl("lnkSign");
                    var cmdRelease = (LinkButton)item.FindControl("cmdRelease");
                    var cmdDelete = (LinkButton)item.FindControl("cmdDelete");
                    var rptAttachments = (Repeater)item.FindControl("rptAttachments");
                    Warrant objWarrant = (Warrant)e.Item.DataItem;

                    cmdRelease.Visible = false;
                    cmdDelete.Visible = objWarrant.Status == WarrantStatus.New && !isJudge;

                    if (objWarrant.Status == WarrantStatus.Complete && !isJudge)
                    {
                        var tr = (System.Web.UI.HtmlControls.HtmlTableRow)e.Item.FindControl("tablerow");
                        tr.Attributes.Add("style", "background-color:#ffeeba;");
                        lblDeleteNotice.Visible = true;
                    }

                    if (isJudge)
                    {
                        int judgeId = objWarrant.ReviewedByJudgeUserID;
                        WarrantStatus statusId = objWarrant.Status;
                        lnkSign.Visible = statusId == WarrantStatus.New || (judgeId == UserId && statusId == WarrantStatus.UnderReview);
                        cmdRelease.Visible = judgeId == UserId && statusId == WarrantStatus.UnderReview;
                    }

                    var lnkWarrant = (HyperLink)e.Item.FindControl("lnkWarrant");
                    if (lnkWarrant != null)
                    {
                        string encryptedValue = _encryptor.QueryStringEncode(objWarrant.FileID.ToString(), UserInfo.Username);
                        lnkWarrant.NavigateUrl = TemplateSourceDirectory + "/Handlers/PdfHandler.ashx?enc=" + encryptedValue;
                        if (!isJudge)
                            lnkWarrant.Attributes.Add("data-stat", objWarrant.Status.ToString().ToLower());
                    }

                    if (objWarrant.WarrantType == WarrantType.RPO)
                    {
                        var aCtl = new AttachmentController();
                        var attachments = aCtl.GetAttachmentsByWarrant(objWarrant.WarrantID) ?? Enumerable.Empty<Attachment>();
                        rptAttachments.DataSource = attachments;
                        rptAttachments.DataBind();
                    }
                }
                catch (Exception exc)
                {
                    Exceptions.ProcessModuleLoadException(this, exc);
                }
            }
        }

        protected void rptWarrants_ItemCreated(object sender, RepeaterItemEventArgs e)
        {
            if (e.Item.ItemType == ListItemType.Item || e.Item.ItemType == ListItemType.AlternatingItem)
            {
                try
                {
                    var sm = ScriptManager.GetCurrent(Page);
                    var cmdRelease = (LinkButton)e.Item.FindControl("cmdRelease");
                    var cmdDelete = (LinkButton)e.Item.FindControl("cmdDelete");
                    sm.RegisterAsyncPostBackControl(cmdRelease);
                    sm.RegisterAsyncPostBackControl(cmdDelete);
                }
                catch (Exception exc)
                {
                    Exceptions.ProcessModuleLoadException(this, exc);
                }
            }
        }

        protected void chkShowAssigned_CheckedChanged(object sender, EventArgs e)
        {
            try
            {
                BindData();
            }
            catch (Exception exc)
            {
                Exceptions.ProcessModuleLoadException(this, exc);
                DotNetNuke.UI.Skins.Skin.AddModuleMessage(this, "Error updating filter. Please try again or contact support.", DotNetNuke.UI.Skins.Controls.ModuleMessage.ModuleMessageType.RedError);
            }
        }

        protected void chkTouchViewer_CheckedChanged(object sender, EventArgs e)
        {
            try
            {
                var ctl = new JudgeController();
                var judge = ctl.GetJudge(UserId);
                if (judge != null)
                    ctl.UpdateJudge(judge);
            }
            catch (Exception exc)
            {
                Exceptions.ProcessModuleLoadException(this, exc);
                DotNetNuke.UI.Skins.Skin.AddModuleMessage(this, "Error updating viewer settings. Please try again or contact support.", DotNetNuke.UI.Skins.Controls.ModuleMessage.ModuleMessageType.RedError);
            }
        }
    }
}