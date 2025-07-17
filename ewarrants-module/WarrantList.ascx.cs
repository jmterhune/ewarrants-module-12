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
        private Encryptor encrypt = new Encryptor();
        public string p_counties;
        private AgencyUserListItem AgencyUserInfo = new AgencyUserListItem();
        private bool isJudge
        {
            get => ViewState["IsJudge"] != null && Convert.ToBoolean(ViewState["IsJudge"]);
            set => ViewState["IsJudge"] = value;
        }

        public WarrantList()
        {
            _navigationManager = DependencyProvider.GetRequiredService<INavigationManager>();
        }

        private void BindData()
        {
            var wCtl = new WarrantController();
            if (isJudge)
            {
                IEnumerable<WarrantListItem> colWarrants = wCtl.GetWarrantListItems(ModuleId, StartDate, EndDate.AddDays(1));
                rptWarrants.DataSource = chkShowAssigned.Checked ? colWarrants.Where(w => w.JudgeUserID == UserId) : colWarrants;
            }
            else if (IsSeargeant)
                rptWarrants.DataSource = wCtl.GetWarrantListItemsBySergeant(ModuleId, UserId, StartDate, EndDate.AddDays(1));
            else
                rptWarrants.DataSource = wCtl.GetWarrantListItemsByUser(ModuleId, UserId, StartDate, EndDate.AddDays(1));
            rptWarrants.DataBind();
        }

        private void SendEmailResponse(WarrantController ctl, int warrantId)
        {
            try
            {
                var sCtl = new SettingController();
                Setting objSettings = sCtl.GetSettings(ModuleId);
                if (objSettings == null || string.IsNullOrEmpty(objSettings.TelnyxApiKey) || string.IsNullOrEmpty(objSettings.TelnyxPhoneNumber))
                {
                    DotNetNuke.UI.Skins.Skin.AddModuleMessage(this, "The Telnyx API Key and Phone Number are not configured. Please contact the site administrator.", DotNetNuke.UI.Skins.Controls.ModuleMessage.ModuleMessageType.RedError);
                    return;
                }

                string leoAddress = UserInfo.Email;
                string noreplyAddress = Settings.Contains("fromemail") ? Settings["fromemail"].ToString() : "webhelp@jud12.flcourts.org";

                Warrant objwarrant = ctl.GetWarrant(warrantId);
                UserInfo judge = UserController.GetUserById(PortalId, objwarrant.JudgeUserID);
                NotificationController nCtl = new NotificationController();
                if (judge != null)
                {
                    var colNotifications = nCtl.GetCurrentNotificationsByJudge(objwarrant.JudgeUserID);
                    if (colNotifications?.Any() == true)
                    {
                        Notification objNotification = colNotifications.FirstOrDefault();
                        if (objNotification.CoveringJudgeID > 0)
                            judge = UserController.GetUserById(PortalId, objNotification.CoveringJudgeID);
                    }
                    string judgeAddress = judge.Email;
                    string judgeProfileContact1 = judge.Profile.GetPropertyValue("Email2") ?? "";
                    string judgeProfileContact2 = judge.Profile.GetPropertyValue("Email3") ?? "";
                    string subject = $"Warrant ID: {warrantId} Deleted";
                    string body = $"Officer {UserInfo.LastName}, {UserInfo.FirstName} has deleted the warrant with ID: {warrantId}";
                    Mail.SendEmail(leoAddress, noreplyAddress, judgeAddress, subject, body);
                    if (!string.IsNullOrEmpty(judgeProfileContact1) && judgeProfileContact1.Contains("@"))
                        Mail.SendEmail(noreplyAddress, judgeProfileContact1, subject, body);
                    if (!string.IsNullOrEmpty(judgeProfileContact2) && judgeProfileContact2.Contains("@"))
                        Mail.SendEmail(noreplyAddress, judgeProfileContact2, subject, body);
                    if (HasUserOptedForSms())
                        Helper.SendSmsNotifications(judge, objSettings.TelnyxApiKey, objSettings.TelnyxPhoneNumber, subject, body);
                }
                else
                    DotNetNuke.UI.Skins.Skin.AddModuleMessage(this, "The selected Judge's record could not be found. No email notification can be sent", DotNetNuke.UI.Skins.Controls.ModuleMessage.ModuleMessageType.RedError);
            }
            catch (Exception ex)
            {
                Exceptions.LogException(ex);
            }
        }
        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);
            isJudge = IsJudge;
            var aCtl = new AgencyController();
            AgencyUserInfo = aCtl.GetAgencyUser(UserId, ModuleId) ?? new AgencyUserListItem();
            if (AgencyUserInfo.UserID > 0)
                this.ModuleConfiguration.ModuleTitle = IsSeargeant ? "Document List" : $"Document List for {UserInfo.DisplayName}";
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
                    if (AgencyUserInfo.UserID > 0)
                    {
                        toolbar.HasAgencyUser = true;
                        toolbar.AgencyUserIsAdmin = AgencyUserInfo.IsAdmin;
                    }
                    toolbar.ContactListUrl = ContactPage;
                    toolbar.SergeantRoleName = SergeantRoleName;
                    txtStartDate.Text = StartDate.ToShortDateString();
                    txtEndDate.Text = EndDate.ToShortDateString();
                    if (string.IsNullOrEmpty(JudgeRoleName) || string.IsNullOrEmpty(AdminJudgeRole) || string.IsNullOrEmpty(SergeantRoleName))
                    {
                        DotNetNuke.UI.Skins.Skin.AddModuleMessage(this, "The Application Settings have not been set. Please contact the site administrator to configure settings.", DotNetNuke.UI.Skins.Controls.ModuleMessage.ModuleMessageType.YellowWarning);
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

                    bool _isSiteAdmin = IsSiteAdmin;
                    if (Request.QueryString["msg"] != null)
                        DotNetNuke.UI.Skins.Skin.AddModuleMessage(this, Request.QueryString["msg"].ToString().Trim(), DotNetNuke.UI.Skins.Controls.ModuleMessage.ModuleMessageType.YellowWarning);

                    if (IsAdminJudge || _isSiteAdmin)
                    {
                        if (_isSiteAdmin)
                        {
                            rptWarrants.Visible = false;
                            dvFilters.Visible = false;
                        }
                    }
                    else if (AgencyUserInfo.UserID <= 0 && !isJudge)
                    {
                        rptWarrants.Visible = false;
                        dvFilters.Visible = false;
                        DotNetNuke.UI.Skins.Skin.AddModuleMessage(this, $"You have not been assigned to a law enforcement agency. Please contact <a href=\"mailto:{PortalSettings.Email}\">support</a> to report this error.", DotNetNuke.UI.Skins.Controls.ModuleMessage.ModuleMessageType.BlueInfo);
                    }
                    else if (AgencyUserInfo.UserID > 0)
                    {
                        var cCtl = new CountyController();
                        IEnumerable<County> colCounties = cCtl.GetCountiesByAgency(AgencyUserInfo.AgencyID);
                        p_counties = string.Join(";", colCounties.Select(c => c.CountyName.ToLower())) + ";";
                        p_counties = p_counties.Trim(';');
                        if (AgencyUserInfo.IsAdmin)
                        {
                            rptWarrants.Visible = false;
                            dvFilters.Visible = false;
                        }
                    }
                    if (isJudge)
                    {
                        var jCtl = new JudgeController();
                        Judge judge = jCtl.GetJudge(UserId);
                        chkShowAssigned.Visible = true;
                    }
                    BindData();
                }
            }
            catch (Exception exc)
            {
                Exceptions.ProcessModuleLoadException(this, exc);
            }
        }

        protected string GetPhoneNumbersJson()
        {
            return Helper.GetPhoneNumbersJson(UserInfo);
        }

        protected void btnSaveOptIn_Click(object sender, EventArgs e)
        {
            try
            {
                var user = UserController.GetUserById(PortalId, UserId);
                bool consent = chkSmsConsent.Checked;
                var phoneNumbers = new List<string>();

                // Collect phone numbers from dynamic inputs
                foreach (var control in upSmsModal.Controls.OfType<Panel>().FirstOrDefault()?.Controls.OfType<TextBox>() ?? new List<TextBox>())
                {
                    if (control.CssClass.Contains("phone-number-input") && !string.IsNullOrWhiteSpace(control.Text))
                        phoneNumbers.Add(control.Text.Trim());
                }
                // Add the default phone number input if it exists
                if (!string.IsNullOrWhiteSpace(txtPhoneNumber.Text))
                    phoneNumbers.Add(txtPhoneNumber.Text.Trim());

                if (!consent && !phoneNumbers.Any())
                {
                    lblPhoneError.Text = "Please enter at least one phone number or opt out.";
                    lblPhoneError.Visible = true;
                    return;
                }

                if (consent)
                {
                    if (!phoneNumbers.Any())
                    {
                        lblPhoneError.Text = "At least one phone number is required to opt in.";
                        lblPhoneError.Visible = true;
                        return;
                    }

                    var phoneUtil = PhoneNumberUtil.GetInstance();
                    var validNumbers = new List<string>();
                    foreach (var number in phoneNumbers)
                    {
                        try
                        {
                            var parsedNumber = phoneUtil.Parse(number, null);
                            if (!phoneUtil.IsValidNumber(parsedNumber))
                            {
                                lblPhoneError.Text = $"Invalid phone number: {number}. Please use international format (e.g., +1234567890).";
                                lblPhoneError.Visible = true;
                                return;
                            }
                            validNumbers.Add(phoneUtil.Format(parsedNumber, PhoneNumberFormat.E164));
                        }
                        catch (NumberParseException)
                        {
                            lblPhoneError.Text = $"Invalid phone number format: {number}.";
                            lblPhoneError.Visible = true;
                            return;
                        }
                    }
                    user.Profile.SetProfileProperty("PhoneNumber", string.Join(",", validNumbers));
                }
                else
                    user.Profile.SetProfileProperty("PhoneNumber", "");
                user.Profile.SetProfileProperty("SmsConsent", consent.ToString());
                UserController.UpdateUser(PortalId, user);

                DotNetNuke.UI.Skins.Skin.AddModuleMessage(this, consent ? "SMS preferences saved successfully!" : "You have opted out of SMS notifications.", DotNetNuke.UI.Skins.Controls.ModuleMessage.ModuleMessageType.GreenSuccess);
            }
            catch (Exception ex)
            {
                DotNetNuke.UI.Skins.Skin.AddModuleMessage(this, $"Error saving SMS preferences: {ex.Message}", DotNetNuke.UI.Skins.Controls.ModuleMessage.ModuleMessageType.RedError);
            }
        }

        protected void btnOptOut_Click(object sender, EventArgs e)
        {
            try
            {
                var user = UserController.GetUserById(PortalId, UserId);
                user.Profile.SetProfileProperty("PhoneNumber", "");
                user.Profile.SetProfileProperty("SmsConsent", "False");
                UserController.UpdateUser(PortalId, user);
                DotNetNuke.UI.Skins.Skin.AddModuleMessage(this, "You have opted out of SMS notifications.", DotNetNuke.UI.Skins.Controls.ModuleMessage.ModuleMessageType.GreenSuccess);
            }
            catch (Exception ex)
            {
                DotNetNuke.UI.Skins.Skin.AddModuleMessage(this, $"Error opting out: {ex.Message}", DotNetNuke.UI.Skins.Controls.ModuleMessage.ModuleMessageType.RedError);
            }
        }

        protected void txtStartDate_TextChanged(object sender, EventArgs e)
        {
            if (DateTime.TryParse(txtStartDate.Text, out DateTime startDate))
            {
                StartDate = startDate;
                BindData();
            }
        }

        protected void txtEndDate_TextChanged(object sender, EventArgs e)
        {
            if (DateTime.TryParse(txtEndDate.Text, out DateTime endDate))
            {
                EndDate = endDate;
                BindData();
            }
        }

        protected void dptUpdate_Unload(object sender, EventArgs e)
        {
            MethodInfo methodInfo = typeof(ScriptManager).GetMethods(BindingFlags.NonPublic | BindingFlags.Instance).Where(i => i.Name.Equals("System.Web.UI.IScriptManagerInternal.RegisterUpdatePanel")).First();
            methodInfo.Invoke(ScriptManager.GetCurrent(Page), new object[] { sender as UpdatePanel });
        }

        protected void rptWarrants_ItemCommand(object source, RepeaterCommandEventArgs e)
        {
            var ctl = new WarrantController();
            int warrantId = Convert.ToInt32(e.CommandArgument);
            if (e.CommandName.ToLower() == "release")
            {
                Warrant objwarrant = ctl.GetWarrant(warrantId);
                if (objwarrant != null)
                {
                    objwarrant.Status = WarrantStatus.New;
                    ctl.UpdateWarrant(objwarrant);
                    BindData();
                }
                else
                    DotNetNuke.UI.Skins.Skin.AddModuleMessage(this, "Error Retrieving Document Information", DotNetNuke.UI.Skins.Controls.ModuleMessage.ModuleMessageType.RedError);
            }
            if (e.CommandName.ToLower() == "sign")
            {
                string viewer = "WarrantView";
                Encryptor encryptor = new Encryptor();
                var jCtl = new JudgeController();
                Judge judge = jCtl.GetJudge(UserId);
                string encryptedValue = encryptor.QueryStringEncode(warrantId.ToString(), UserInfo.Username);
                string url = $"{TemplateSourceDirectory}/{viewer}.aspx?ModuleId={ModuleId}&enc={encryptedValue}";
                Warrant objwarrant = ctl.GetWarrant(warrantId);
                if (objwarrant != null)
                {
                    if (objwarrant.Status == WarrantStatus.UnderReview && objwarrant.ReviewedByJudgeUserID != UserId)
                    {
                        DotNetNuke.UI.Skins.Skin.AddModuleMessage(this, "This Document is currently under review by another judge.", DotNetNuke.UI.Skins.Controls.ModuleMessage.ModuleMessageType.RedError);
                        BindData();
                        return;
                    }
                    else
                    {
                        objwarrant.ReviewedByJudgeUserID = UserId;
                        objwarrant.Status = WarrantStatus.UnderReview;
                        ctl.UpdateWarrant(objwarrant);
                    }
                }
                Response.Redirect(url, true);
            }
            if (e.CommandName.ToLower() == "delete")
            {
                Log objLog = new Log
                {
                    UserID = UserInfo.UserID,
                    EventTime = DateTime.Now,
                    EventDescription = "Deleted Record",
                    WarrantID = warrantId,
                    IP = GetIp()
                };
                var ctlLog = new LogController();
                ctlLog.CreateLog(objLog);
                try
                {
                    SendEmailResponse(ctl, warrantId);
                    ctl.DeleteWarrant(warrantId);
                    BindData();
                }
                catch (Exception ex)
                {
                    var message = DotNetNuke.UI.Skins.Skin.GetModuleMessageControl("", ex.Message + "<br />" + ex.StackTrace, DotNetNuke.UI.Skins.Controls.ModuleMessage.ModuleMessageType.YellowWarning);
                    gridmessage.Controls.Add(message);
                }
            }
        }

        protected void rptWarrants_ItemDataBound(object sender, RepeaterItemEventArgs e)
        {
            if (e.Item.ItemType == ListItemType.Item || e.Item.ItemType == ListItemType.AlternatingItem)
            {
                RepeaterItem item = e.Item;
                LinkButton lnkSign = (LinkButton)item.FindControl("lnkSign");
                LinkButton cmdRelease = (LinkButton)item.FindControl("cmdRelease");
                LinkButton cmdDelete = (LinkButton)item.FindControl("cmdDelete");
                Repeater rptAttachments = (Repeater)item.FindControl("rptAttachments");
                Warrant objWarrant = (Warrant)e.Item.DataItem;
                cmdRelease.Visible = false;
                if (objWarrant.Status == WarrantStatus.New && !isJudge)
                    cmdDelete.Visible = true;
                else
                    cmdDelete.Visible = false;
                if (objWarrant.Status == WarrantStatus.Complete && !isJudge)
                {
                    System.Web.UI.HtmlControls.HtmlTableRow tr = (System.Web.UI.HtmlControls.HtmlTableRow)e.Item.FindControl("tablerow");
                    tr.Attributes.Add("style", "background-color:#ffeeba;");
                    lblDeleteNotice.Visible = true;
                }

                int warrantId = objWarrant.WarrantID;
                if (isJudge)
                {
                    int judgeId = objWarrant.ReviewedByJudgeUserID;
                    WarrantStatus statusId = objWarrant.Status;
                    if (statusId != WarrantStatus.New)
                    {
                        lnkSign.Visible = false;
                        if (judgeId == UserId && statusId == WarrantStatus.UnderReview)
                        {
                            lnkSign.Visible = true;
                            cmdRelease.Visible = true;
                        }
                    }
                    else
                    {
                        lnkSign.Visible = true;
                        cmdRelease.Visible = false;
                    }
                }

                HyperLink lnkWarrant = (HyperLink)e.Item.FindControl("lnkWarrant");
                if (lnkWarrant != null)
                {
                    Encryptor encryptor = new Encryptor();
                    string encryptedValue = encryptor.QueryStringEncode(objWarrant.FileID.ToString(), UserInfo.Username);
                    lnkWarrant.NavigateUrl = TemplateSourceDirectory + "/Handlers/PdfHandler.ashx?enc=" + encryptedValue;
                    if (!isJudge)
                        lnkWarrant.Attributes.Add("data-stat", objWarrant.Status.ToString().ToLower());
                }
                if (objWarrant.WarrantType == WarrantType.RPO)
                {
                    var aCtl = new AttachmentController();
                    IEnumerable<Attachment> attachments = aCtl.GetAttachmentsByWarrant(warrantId);
                    rptAttachments.DataSource = attachments;
                    rptAttachments.DataBind();
                }
            }
        }

        protected void rptWarrants_ItemCreated(object sender, RepeaterItemEventArgs e)
        {
            if (e.Item.ItemType == ListItemType.Item || e.Item.ItemType == ListItemType.AlternatingItem)
            {
                RepeaterItem item = e.Item;
                var sm = ScriptManager.GetCurrent(Page);
                LinkButton cmdRelease = (LinkButton)item.FindControl("cmdRelease");
                LinkButton cmdDelete = (LinkButton)item.FindControl("cmdDelete");
                sm.RegisterAsyncPostBackControl(cmdRelease);
                sm.RegisterAsyncPostBackControl(cmdDelete);
            }
        }

        protected void chkShowAssigned_CheckedChanged(object sender, EventArgs e)
        {
            BindData();
        }

        protected void chkTouchViewer_CheckedChanged(object sender, EventArgs e)
        {
            var ctl = new JudgeController();
            Judge judge = ctl.GetJudge(UserId);
            ctl.UpdateJudge(judge);
        }
    }
}