/*
' Copyright (c) 2020  Joe Terhune
'  All rights reserved.
' 
' THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED
' TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL
' THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF
' CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER
' DEALINGS IN THE SOFTWARE.
' 
*/

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

namespace tjc.Modules.eWarrants
{
    /// -----------------------------------------------------------------------------
    /// <summary>
    /// The View class displays the content
    /// 
    /// Typically your view control would be used to display content or functionality in your module.
    /// 
    /// View may be the only control you have in your project depending on the complexity of your module
    /// 
    /// Because the control inherits from ewarrantsModuleBase you have access to any custom properties
    /// defined there, as well as properties from DNN such as PortalId, ModuleId, TabId, UserId and many more.
    /// 
    /// </summary>
    /// -----------------------------------------------------------------------------
    public partial class WarrantList : ewarrantsModuleBase
    {

        //--------------Members-------------------
        #region Memebers
        private readonly INavigationManager _navigationManager;
        private Encryptor encrypt = new Encryptor();
        public string p_counties;
        private AgencyUserListItem AgencyUserInfo = new AgencyUserListItem();
        //private bool isJudge;
        private bool isJudge
        {
            get
            {
                if (ViewState["IsJudge"] != null)
                {
                    return System.Convert.ToBoolean(ViewState["IsJudge"]);
                }
                return false;
            }
            set
            {
                ViewState["IsJudge"] = value;
            }
        }
        #endregion

        //--------------Methods-----------------------------------
        #region Methods
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

                if (chkShowAssigned.Checked)
                    rptWarrants.DataSource = colWarrants.Where(w => w.JudgeUserID == UserId);
                else
                    rptWarrants.DataSource = colWarrants;

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
                string leoAddress = UserInfo.Email;
                string noreplyAddress = "webhelp@jud12.flcourts.org";
                if (Settings.Contains("fromemail"))
                {
                    noreplyAddress = Settings["fromemail"].ToString();
                }

                Warrant objwarrant = ctl.GetWarrant(warrantId);
                UserInfo judge = UserController.GetUserById(PortalId, objwarrant.JudgeUserID);
                NotificationController nCtl = new NotificationController();
                if (judge != null)
                {
                    var colNotifications = nCtl.GetCurrentNotificationsByJudge(objwarrant.JudgeUserID);
                    if (colNotifications != null && colNotifications.Count() > 0)
                    {
                        Notification objNotification = colNotifications.FirstOrDefault();
                        if (objNotification.CoveringJudgeID > 0)
                            judge = UserController.GetUserById(PortalId, objNotification.CoveringJudgeID);
                    }
                    string judgeAddress = judge.Email;
                    string judgeProfileContact1 = "";
                    string judgeProfileContact2 = "";
                    string subject = string.Format("Warrant ID: {0} Deleted", warrantId);
                    string body = string.Format("Officer {0}, {1} has deleted the warrant with ID: {2}", UserInfo.LastName, UserInfo.FirstName, warrantId);
                    if (judge.Profile.ProfileProperties["Email2"] != null)
                        judgeProfileContact1 = judge.Profile.ProfileProperties["Email2"].PropertyValue;
                    if (judge.Profile.ProfileProperties["Email3"] != null)
                        judgeProfileContact2 = judge.Profile.ProfileProperties["Email3"].PropertyValue;
                    if (judgeProfileContact1 != "")
                        SendToAlternateContact(objSettings.TwilioAccountSid, objSettings.TwilioAuthToken, objSettings.TwilioPhoneNumber, noreplyAddress, judgeProfileContact1, subject, body);
                    if (judgeProfileContact2 != "")
                        SendToAlternateContact(objSettings.TwilioAccountSid, objSettings.TwilioAuthToken, objSettings.TwilioPhoneNumber, noreplyAddress, judgeProfileContact1, subject, body);
                    DotNetNuke.Services.Mail.Mail.SendEmail(leoAddress, noreplyAddress, judgeAddress, subject, body);
                }
                else
                    DotNetNuke.UI.Skins.Skin.AddModuleMessage(this, "The selected Judge's record could not be found. No email notification can be sent", DotNetNuke.UI.Skins.Controls.ModuleMessage.ModuleMessageType.RedError);
            }
            catch (Exception ex)
            {
                Exceptions.LogException(ex);
            }
        }
        protected async void SendToAlternateContact(string accountSid, string authToken, string fromPhoneNumber, string replyEmail, string contactPoint, string subject, string body)
        {

            if (string.IsNullOrEmpty(accountSid) || string.IsNullOrEmpty(authToken) || string.IsNullOrEmpty(fromPhoneNumber))
            {
                return;
            }
            if (contactPoint.Contains("@"))
            {
                // If contactPoint is an email address, send an email instead of SMS
                Mail.SendEmail(replyEmail, contactPoint, subject, body);
                return;
            }
            else
            {
                var smsService = new TwilioSmsService(accountSid, authToken, fromPhoneNumber);
                await smsService.SendSmsAsync(contactPoint, body);
            }
        }
        #endregion

        //--------------Event Handlers-----------------------
        #region Event Handlers
        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);
            isJudge = IsJudge;
            var aCtl = new AgencyController();
            AgencyUserInfo = aCtl.GetAgencyUser(UserId, ModuleId);
            if (AgencyUserInfo != null)
            {
                if (IsSeargeant)
                {
                    this.ModuleConfiguration.ModuleTitle = "Document List";
                }
                else
                {
                    this.ModuleConfiguration.ModuleTitle = "Document List for " + UserInfo.DisplayName;
                }
            }
            else
            {
                AgencyUserInfo = new AgencyUserListItem();
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
                    if (AgencyUserInfo.UserID > 0)
                    {
                        toolbar.HasAgencyUser = true;
                        toolbar.AgencyUserIsAdmin = AgencyUserInfo.IsAdmin;
                    }
                    toolbar.ContactListUrl = ContactPage;
                    toolbar.SergeantRoleName = SergeantRoleName;
                    txtStartDate.Text = StartDate.ToShortDateString();
                    txtEndDate.Text = EndDate.ToShortDateString();
                    if (string.IsNullOrEmpty(JudgeRoleName) | string.IsNullOrEmpty(AdminJudgeRole) | string.IsNullOrEmpty(SergeantRoleName))
                    {
                        DotNetNuke.UI.Skins.Skin.AddModuleMessage(this, "The Application Settings have not been set.  Please contact the site administrator to configure settings.", DotNetNuke.UI.Skins.Controls.ModuleMessage.ModuleMessageType.YellowWarning);
                        rptWarrants.Visible = false;
                        dvFilters.Visible = false;
                        return;
                    }

                    if (Settings.Contains("hours") && !isJudge)
                        CompletedWarrantsThreshold = System.Convert.ToInt32(Settings["hours"]);
                    if (CompletedWarrantsThreshold > 0)
                        ltMessage.Text = string.Format(ltMessage.Text, CompletedWarrantsThreshold.ToString());
                    else
                        ltMessage.Visible = false;

                    bool _isSiteAdmin = IsSiteAdmin;
                    if (Request.QueryString["msg"] != null)
                        DotNetNuke.UI.Skins.Skin.AddModuleMessage(this, Request.QueryString["msg"].ToString().Trim(), DotNetNuke.UI.Skins.Controls.ModuleMessage.ModuleMessageType.YellowWarning);

                    if (IsAdminJudge | _isSiteAdmin)
                    {
                        if (_isSiteAdmin)
                        {
                            rptWarrants.Visible = false;
                            dvFilters.Visible = false;
                        }
                    }
                    else if (AgencyUserInfo.UserID <= 0 & !isJudge)
                    {
                        rptWarrants.Visible = false;
                        dvFilters.Visible = false;

                        DotNetNuke.UI.Skins.Skin.AddModuleMessage(this, "You have not been assigned to a law enforcement agency. Please contact <a href=\"mailto:" + PortalSettings.Email + "\">support</a> to report this error.", DotNetNuke.UI.Skins.Controls.ModuleMessage.ModuleMessageType.BlueInfo);
                    }
                    else if (AgencyUserInfo.UserID > 0)
                    {
                        var cCtl = new CountyController();

                        IEnumerable<County> colCounties = cCtl.GetCountiesByAgency(AgencyUserInfo.AgencyID);
                        foreach (County c in colCounties)
                            p_counties += c.CountyName.ToLower() + ";";
                        if (string.IsNullOrEmpty(p_counties))
                            p_counties = p_counties.Trim(';');
                        if (AgencyUserInfo.IsAdmin)
                        {
                            rptWarrants.Visible = false;
                            dvFilters.Visible = false;
                        }
                    }
                    if (isJudge)
                    {
                        var jCtl=new JudgeController();
                        Judge judge=jCtl.GetJudge(UserId);
                        chkShowAssigned.Visible = true;
                    }
                    BindData();
                }
            }
            catch (Exception exc) //Module failed to load
            {
                Exceptions.ProcessModuleLoadException(this, exc);
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
            int warrantId = System.Convert.ToInt32(e.CommandArgument);
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
                //if (judge != null)
                //    if (judge.TouchEnabledViewer)
                //        viewer = "DocViewer";
                string encryptedValue = encryptor.QueryStringEncode(warrantId.ToString(), UserInfo.Username);
                string url = string.Format("{0}/{1}.aspx?ModuleId={2}&enc={3}", TemplateSourceDirectory, viewer, ModuleId, encryptedValue);
                Warrant objwarrant = ctl.GetWarrant(warrantId);
                if (objwarrant != null)
                {
                    if (objwarrant.Status == WarrantStatus.UnderReview & objwarrant.ReviewedByJudgeUserID != UserId)
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
                if (objWarrant.Status == WarrantStatus.New & !isJudge)
                    cmdDelete.Visible = true;
                else
                    cmdDelete.Visible = false;
                if (objWarrant.Status == WarrantStatus.Complete & !isJudge)
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
                        if (judgeId == UserId & statusId == WarrantStatus.UnderReview)
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
            //if (chkTouchViewer.Checked)
            //    judge.TouchEnabledViewer = true;
            //else
            //    judge.TouchEnabledViewer = false;
            ctl.UpdateJudge(judge);
        }
        #endregion

    }
}