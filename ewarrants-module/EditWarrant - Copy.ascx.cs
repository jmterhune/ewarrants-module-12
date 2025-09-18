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

using System;
using DotNetNuke.Entities.Users;
using tjc.Modules.eWarrants.Components;
using DotNetNuke.Services.Exceptions;
using System.Web.UI.WebControls;
using System.Collections.Generic;
using System.Linq;
using DotNetNuke.Services.Mail;
using DotNetNuke.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using System.Diagnostics;

namespace tjc.Modules.eWarrants
{
    /// -----------------------------------------------------------------------------
    /// <summary>   
    /// The Edit class is used to manage content
    /// 
    /// Typically your edit control would be used to create new content, or edit existing content within your module.
    /// The ControlKey for this control is "Edit", and is defined in the manifest (.dnn) file.
    /// 
    /// Because the control inherits from ewarrantsModuleBase you have access to any custom properties
    /// defined there, as well as properties from DNN such as PortalId, ModuleId, TabId, UserId and many more.
    /// 
    /// </summary>
    /// -----------------------------------------------------------------------------
    public partial class EditWarrant : ewarrantsModuleBase
    {
        private readonly INavigationManager _navigationManager;
        public EditWarrant()
        {
            _navigationManager = DependencyProvider.GetRequiredService<INavigationManager>();
        }

        #region Properties & Members
        private string coverJudgeName = "";
        private bool isOutofOffice = false;
        private UserInfo Judgeinfo = new UserInfo();
        public string uploadHandler = "";
        #endregion

        #region Methods
        private void BindCounties(int agencyId)
        {
            var aCtl = new AgencyController();
            Agency objAgency = aCtl.GetAgency(agencyId, ModuleId);
            IEnumerable<County> counties = objAgency.Counties;

            if (counties.Count() > 0)
            {
                foreach (County c in counties)
                {
                    drpCounty.Items.Add(new ListItem(c.CountyName, c.CountyID.ToString()));
                }
                drpCounty.Items.Insert(0, new ListItem("< Select County >", ""));
                if (drpCounty.Items.Count > 2)
                    divCounty.Visible = true;
                else
                    drpCounty.SelectedIndex = 1;
            }
            else
                DotNetNuke.UI.Skins.Skin.AddModuleMessage(this, "Your agencies county list has not been properly configured.  Please contact your site administrator with this message.", DotNetNuke.UI.Skins.Controls.ModuleMessage.ModuleMessageType.RedError);
        }

        private void BindOffenseTypes()
        {
            var ctl = new JudgeTypeController();
            IEnumerable<JudgeType> judgeTypes = ctl.GetJudgeTypes(ModuleId);

            rblOffenseType.DataSource = judgeTypes.Where(jt => jt.JudgeTypeName != "RPO"); //exclude RPOs. Separate form for those.
            rblOffenseType.DataTextField = "JudgeTypeName";
            rblOffenseType.DataValueField = "JudgeTypeID";
            rblOffenseType.DataBind();
            foreach (ListItem item in rblOffenseType.Items)
            {
                if (item.Text.Contains("CBS"))
                {
                    item.Attributes.Add("title", "Compel Blood Screening");
                    item.Attributes.Add("data-toggle", "tooltip");
                    item.Attributes.Add("data-placement", "top");
                }
            }
        }

        private UserInfo GetCoverJudge(int coverJudgeId)
        {
            return UserController.GetUserById(PortalId, coverJudgeId);
        }

        private void SendEmailResponse(string agencyName, string warrantId, WarrantType warrantType, bool SendNow)
        {
            string noreplyEmail = "webhelp@jud12.flcourts.org";
            string leoEmail = UserInfo.Email;
            var sCtl = new SettingController();
            Setting objSettings = sCtl.GetSettings(ModuleId);
            if (objSettings != null)
                noreplyEmail = objSettings.SenderEmail;
            var ctl = new NotificationController();
            Judgeinfo = UserController.GetUserById(PortalId, Int32.Parse(hdJudgeId.Value));
            if (Judgeinfo != null)
            {
                string judgeEmail = Judgeinfo.Email;
                string judgeProfileContact1 = Helper.GetProfileValue(Judgeinfo, "Email2");
                string judgeProfileContact2 = Helper.GetProfileValue(Judgeinfo, "Email3");
                string subject;
                string userDisplay;
                string body;
                string warrantTypeName = "warrant";
                switch (warrantType)
                {
                    case WarrantType.Search:
                        warrantTypeName = "Search Warrant";
                        break;
                    case WarrantType.Arrest:
                        warrantTypeName = "Arrest Warrant";
                        break;
                    case WarrantType.PickupOrder:
                        warrantTypeName = "Juvenile Pickup Order";
                        break;
                    case WarrantType.RPO:
                        warrantTypeName = "Risk Protection Order";
                        break;
                    case WarrantType.CBS:
                        warrantTypeName = "Compel Blood Screening";
                        break;
                    case WarrantType.CivilForfeiture:
                        warrantTypeName = "Civil Forfeiture";
                        break;
                    default:
                        break;
                }
                IEnumerable<Notification> colNotifications = ctl.GetCurrentNotificationsByJudge(Judgeinfo.UserID);
                if (colNotifications.Count() > 0)
                {
                    Notification objNotification = colNotifications.FirstOrDefault();
                    isOutofOffice = true;
                    if (objNotification.CoveringJudgeID > 0)
                    {
                        UserInfo CoverJudgeInfo;
                        CoverJudgeInfo = GetCoverJudge(objNotification.CoveringJudgeID);
                        judgeEmail = CoverJudgeInfo.Email;
                        coverJudgeName = CoverJudgeInfo.DisplayName;
                        subject = $"New {warrantTypeName} Forwarded from  " + Judgeinfo.DisplayName.Replace(" ", " ") + " for Your Review";
                        body = $"A new {warrantTypeName} (ID: {warrantId})  has been submitted by {UserInfo.DisplayName.Replace(" ", " ")} from {agencyName + Environment.NewLine + Environment.NewLine}" +
                            $"{Judgeinfo.DisplayName.Replace(" & nbsp; ", " ")} is out of the office and has asked that {warrantTypeName} be forwarded to you during their absence.";

                        if (SendNow)
                        {
                            Mail.SendEmail(noreplyEmail, judgeEmail, subject, body);
                            if (!string.IsNullOrEmpty(judgeProfileContact1) && judgeProfileContact1.Contains("@"))
                                Mail.SendEmail(noreplyEmail, judgeProfileContact1, subject, body);
                            if (!string.IsNullOrEmpty(judgeProfileContact2) && judgeProfileContact2.Contains("@"))
                                Mail.SendEmail(noreplyEmail, judgeProfileContact2, subject, body);
                            if (HasUserOptedForSms())
                                Helper.SendSmsNotifications(Judgeinfo, objSettings.TelnyxApiKey, objSettings.TelnyxPhoneNumber, subject, body);
                        }
                        subject = "Out of Office Notice";
                        body = $"The Honorable {Judgeinfo.DisplayName.Replace(" ", " ")} is Out of the Office. {Environment.NewLine + Environment.NewLine}" +
                         $"Your {warrantTypeName} notification has been forwarded to the Honorable " + coverJudgeName;
                        Mail.SendEmail(noreplyEmail, leoEmail, subject, body);
                    }
                    else
                    {
                        subject = $"New {warrantTypeName} Submitted for Review";
                        userDisplay = UserInfo.DisplayName.Replace(" ", " ");
                        body = $"A new {warrantTypeName} has been submitted by " + userDisplay + " from " + agencyName;
                        if (SendNow)
                        {
                            Mail.SendEmail(leoEmail, noreplyEmail, judgeEmail, subject, body);
                            if (!string.IsNullOrEmpty(judgeProfileContact1) && judgeProfileContact1.Contains("@"))
                                Mail.SendEmail(noreplyEmail, judgeProfileContact1, subject, body);
                            if (!string.IsNullOrEmpty(judgeProfileContact2) && judgeProfileContact2.Contains("@"))
                                Mail.SendEmail(noreplyEmail, judgeProfileContact2, subject, body);
                            if (HasUserOptedForSms())
                                Helper.SendSmsNotifications(Judgeinfo, objSettings.TelnyxApiKey, objSettings.TelnyxPhoneNumber, subject, body);
                        }
                        subject = "Out of Office Notice";
                        body = " The Honorable " + Judgeinfo.DisplayName.Replace(" ", " ") + " is Out of the Office. Please review the notice below" + Environment.NewLine + Environment.NewLine;
                        body += objNotification.Message;
                        Mail.SendEmail(noreplyEmail, leoEmail, subject, body);
                    }
                }
                else
                {
                    subject = $"New {warrantTypeName} Submitted for Review";
                    userDisplay = UserInfo.DisplayName.Replace(" ", " ");
                    body = $"A new {warrantTypeName} (ID: " + warrantId + ") has been submitted by " + userDisplay + " from " + agencyName;

                    if (SendNow)
                    {
                        Mail.SendEmail(leoEmail, noreplyEmail, judgeEmail, subject, body);
                        if (!string.IsNullOrEmpty(judgeProfileContact1) && judgeProfileContact1.Contains("@"))
                            Mail.SendEmail(noreplyEmail, judgeProfileContact1, subject, body);
                        if (!string.IsNullOrEmpty(judgeProfileContact2) && judgeProfileContact2.Contains("@"))
                            Mail.SendEmail(noreplyEmail, judgeProfileContact2, subject, body);
                        if (HasUserOptedForSms())
                            Helper.SendSmsNotifications(Judgeinfo, objSettings.TelnyxApiKey, objSettings.TelnyxPhoneNumber, subject, body);
                    }
                }
            }
            else
                DotNetNuke.UI.Skins.Skin.AddModuleMessage(this, "The selected Judge's record could not be found. No email notification can be sent", DotNetNuke.UI.Skins.Controls.ModuleMessage.ModuleMessageType.RedError);
        }


        #endregion

        /// -----------------------------------------------------------------------------
        ///         ''' <summary>
        ///         ''' Page_Load runs when the control is loaded
        ///         ''' </summary>
        ///         ''' <remarks>
        ///         ''' </remarks>
        ///         ''' <history>
        ///         ''' </history>
        ///         ''' ----------------------------------------------------------------------------- 
        protected void Page_Load(System.Object sender, System.EventArgs e)
        {
            try
            {
                // The upload configuration will be available in the handler
                uploadHandler = TemplateSourceDirectory + "/Handlers/WarrantHandler.ashx";
                if (Page.IsPostBack == false)
                {
                    if (string.IsNullOrEmpty(JudgeRole))
                    {
                        DotNetNuke.UI.Skins.Skin.AddModuleMessage(this, "The Warrants Application has not been configured. Please contact the site administrator", DotNetNuke.UI.Skins.Controls.ModuleMessage.ModuleMessageType.RedError);
                        pnlWarrant.Visible = false;
                        cmdUpdate.Visible = false;
                        return;
                    }
                    var ctl = new AgencyController();
                    IEnumerable<AgencyUserListItem> colAgencyUser = ctl.GetUserAgencies(UserId, ModuleId).Where(a => a.IsAdmin == false);
                    if (colAgencyUser.Count() > 0 & !IsSiteAdmin)
                    {
                        foreach (AgencyUserListItem item in colAgencyUser)
                            drpAgency.Items.Add(new ListItem(item.AgencyName, item.AgencyID.ToString()));
                        if (drpAgency.Items.Count > 1)
                            dvAgency.Visible = true;
                        if (AgencyId > 0)
                            drpAgency.SelectedValue = AgencyId.ToString();
                        BindOffenseTypes();
                        BindCounties(Int32.Parse(drpAgency.SelectedValue));
                    }
                    else
                    {
                        DotNetNuke.UI.Skins.Skin.AddModuleMessage(this, "Invalid User Access.  Please contact the site administrator with the error.", DotNetNuke.UI.Skins.Controls.ModuleMessage.ModuleMessageType.RedError);
                        pnlWarrant.Visible = false;
                        cmdUpdate.Visible = false;
                        return;
                    }
                }
            }
            catch (Exception exc)
            {
                Exceptions.ProcessModuleLoadException(this, exc);
            }
        }

        /// -----------------------------------------------------------------------------
        ///         ''' <summary>
        ///         ''' cmdCancel_Click runs when the cancel button is clicked
        ///         ''' </summary>
        ///         ''' <remarks>
        ///         ''' </remarks>
        ///         ''' <history>
        ///         ''' </history>
        ///         ''' ----------------------------------------------------------------------------- 
        protected void cmdCancel_Click(object sender, EventArgs e)
        {
            try
            {
                if (!string.IsNullOrEmpty(hdFileId.Value))
                {
                    var ctl = new FileController();
                    ctl.DeleteFile(System.Convert.ToInt32(hdFileId.Value));
                }
                Response.Redirect(_navigationManager.NavigateURL(), true);
            }
            catch (Exception exc)
            {
                Exceptions.ProcessModuleLoadException(this, exc);
            }
        }

        /// -----------------------------------------------------------------------------
        ///         ''' <summary>
        ///         ''' cmdUpdate_Click runs when the update button is clicked
        ///         ''' </summary>
        ///         ''' <remarks>
        ///         ''' </remarks>
        ///         ''' <history>
        ///         ''' </history>
        ///         ''' ----------------------------------------------------------------------------- 
        protected void cmdUpdate_Click(object sender, EventArgs e)
        {
            try
            {
                var ctl = new WarrantController();

                bool isJuvenile = false;
                if (rblOffenseType.SelectedItem.Text == "Juvenile")
                    isJuvenile = true;
                int warrantId = -1;
                if (chkAffidavit.Checked & chkWarrant.Checked)
                {
                    bool SendNow = true;
                    Warrant objwarrant = new Warrant
                    {
                        ModuleID = ModuleId,
                        AgencyID = Int32.Parse(drpAgency.SelectedValue),
                        CreatedByUserID = UserId,
                        CreatedDate = DateTime.Now,
                        Title = txtTitle.Text,
                        CountyName = drpCounty.SelectedItem.Text,
                        StateAttorney = txtSaName.Text,
                        Status = WarrantStatus.New,
                        Defendant = txtDefendant.Text,
                        AdministeredOathAgency = txtOAgency.Text,
                        AdministeredOathId = txtOID.Text,
                        AdministeredOathName = txtOName.Text,
                    };

                    if (Int32.TryParse(hdJudgeId.Value, out int judgeId))
                        objwarrant.JudgeUserID = judgeId;
                    if (!string.IsNullOrEmpty(rblWarrantType.SelectedValue))
                        objwarrant.WarrantType = (WarrantType)Int32.Parse(rblWarrantType.SelectedValue);
                    if (rblOffenseType.SelectedItem.Text.ToLower().StartsWith("cbs"))
                        objwarrant.WarrantType = WarrantType.CBS;
                    if (rblOffenseType.SelectedItem.Text.ToLower().StartsWith("civil"))
                        objwarrant.WarrantType = WarrantType.CivilForfeiture;

                    if (Int32.TryParse(hdFileId.Value, out int fileId))
                    {
                        Warrant existingWarrant = ctl.GetWarrantByFileId(fileId);
                        if (existingWarrant != null)
                        {
                            DotNetNuke.UI.Skins.Skin.AddModuleMessage(this, "Detected duplicate warrant file, warrant not added. This usually happens when the submit button is double clicked or the submitted page is refreshed.<br /><br /><strong>Please select the Cancel Button to Reset the Form</strong>", DotNetNuke.UI.Skins.Controls.ModuleMessage.ModuleMessageType.RedError);
                            return;
                        }
                        objwarrant.FileID = fileId;
                    }
                    else
                    {
                        DotNetNuke.UI.Skins.Skin.AddModuleMessage(this, "The file ID could not be found.  Please cancel this process and try again.", DotNetNuke.UI.Skins.Controls.ModuleMessage.ModuleMessageType.RedError);
                        return;
                    }
                    if (rblEmergency.SelectedValue == "0")
                        SendNow = false;
                    else
                        objwarrant.NotificationSentDate = DateTime.Now;
                    objwarrant.IsJuvenile = isJuvenile;
                    ctl.CreateWarrant(objwarrant);
                    warrantId = objwarrant.WarrantID;
                    try
                    {
                        var aCtl = new AgencyController();
                        Agency objAgency = aCtl.GetAgency(Int32.Parse(drpAgency.SelectedValue), ModuleId);

                        SendEmailResponse(objAgency.AgencyName, warrantId.ToString(), objwarrant.WarrantType, SendNow);
                    }
                    catch (Exception ex)
                    {
                        if (isJuvenile)
                            DotNetNuke.UI.Skins.Skin.AddModuleMessage(this, "The Juvenile Pickup Order was successfully upload, however, there was an error sending the  email.", DotNetNuke.UI.Skins.Controls.ModuleMessage.ModuleMessageType.RedError);
                        else
                            DotNetNuke.UI.Skins.Skin.AddModuleMessage(this, "The warrant was successfully upload, however, there was an error sending the  email.", DotNetNuke.UI.Skins.Controls.ModuleMessage.ModuleMessageType.RedError);
                        Exceptions.ProcessModuleLoadException(this, ex, false);
                    }
                }
                else
                {
                    if (isJuvenile)
                        DotNetNuke.UI.Skins.Skin.AddModuleMessage(this, "The Juvenile Pickup Order, PCA, and Affidavit must be included.  Both checkboxes must be checked.", DotNetNuke.UI.Skins.Controls.ModuleMessage.ModuleMessageType.RedError);
                    else
                        DotNetNuke.UI.Skins.Skin.AddModuleMessage(this, "The Warrant and Affidavit must be included.  Both checkboxes must be checked.", DotNetNuke.UI.Skins.Controls.ModuleMessage.ModuleMessageType.RedError);
                    return;
                }

                // refresh cache
                DotNetNuke.Entities.Modules.ModuleController.SynchronizeModule(ModuleId);
                // Redirect back to the portal home page
                string notificationMessage = "";
                if (isOutofOffice)
                {
                    string judgeName = Judgeinfo.DisplayName;
                    notificationMessage = judgeName + " is out of the office. ";
                    if (!string.IsNullOrEmpty(coverJudgeName))
                    {
                        if (isJuvenile)
                            notificationMessage += " Your Juvenile Pickup Order has been forwarded to " + coverJudgeName;
                        else
                            notificationMessage += " Your warrant has been forwarded to " + coverJudgeName;
                    }
                    else
                        notificationMessage += " You will receive an email with further information from the Judge";
                    if (coverJudgeName.ToLower() == "smwhyte" && rblEmergency.SelectedValue == "1")
                        notificationMessage = "<strong style='color:red'>IMPORTANT!! If this is an emergency, please notify Judge Whyte by phone at 941-713-1353.</strong><br />" + notificationMessage;

                    if (!string.IsNullOrEmpty(notificationMessage))
                        Response.Redirect(_navigationManager.NavigateURL("", "msg=" + notificationMessage), true);
                    else
                        Response.Redirect(_navigationManager.NavigateURL(), true);
                }
                else
                {
                    if (coverJudgeName.ToLower() == "smwhyte" && rblEmergency.SelectedValue == "1")
                        notificationMessage = "<strong style='color:red'>IMPORTANT!! If this is an emergency, please notify Judge Whyte by phone at 941-713-1353.</strong><br />" + notificationMessage;

                    if (!string.IsNullOrEmpty(notificationMessage))
                        Response.Redirect(_navigationManager.NavigateURL("", "msg=" + notificationMessage), true);
                    else
                        Response.Redirect(_navigationManager.NavigateURL(), true);
                }
            }
            catch (Exception exc)
            {
                Exceptions.ProcessModuleLoadException(this, exc);
            }
        }

        protected void valPCACheck_ServerValidate(object source, ServerValidateEventArgs args)
        {
            args.IsValid = chkPCA.Checked;
        }

        protected void valWarrant_ServerValidate(object source, ServerValidateEventArgs args)
        {
            args.IsValid = chkWarrant.Checked;
        }

        protected void chkAffidavitRequired_ServerValidate(object source, ServerValidateEventArgs args)
        {
            args.IsValid = chkAffidavit.Checked;
        }

        protected void chkWarrantRequired_ServerValidate(object source, ServerValidateEventArgs args)
        {
            args.IsValid = chkWarrant.Checked;
        }

        protected override void Render(System.Web.UI.HtmlTextWriter writer)
        {
            var ctl = new JudgeController();
            IEnumerable<Judge> judges = ctl.GetJudges(ModuleId);
            foreach (var j in judges)
                Page.ClientScript.RegisterForEventValidation(drpJudge.UniqueID, j.JudgeID.ToString());
            Page.ClientScript.RegisterForEventValidation(drpJudge.UniqueID, "");
            base.Render(writer);
        }

        protected void rblOffenseType_DataBound(object sender, EventArgs e)
        {
            foreach (ListItem li in rblOffenseType.Items)
                li.Attributes.Add("onclick", "FillJudges()");
        }

        protected void valEmergency_ServerValidate(object source, ServerValidateEventArgs args)
        {
            args.IsValid = false;
            if (Int32.TryParse(hdJudgeId.Value, out int judgeId))
            {
                var ctl = new JudgeController();
                Judge judge = ctl.GetJudge(judgeId, ModuleId);
                bool NotifyJudge = true;
                if (DateTime.Now.TimeOfDay > judge.DayEnd.TimeOfDay | DateTime.Now.TimeOfDay < judge.DayStart.TimeOfDay)
                    NotifyJudge = false;
                if (NotifyJudge)
                    args.IsValid = true;
                else if (rblEmergency.SelectedIndex >= 0)
                    args.IsValid = true;
            }
        }

        protected void valOName_ServerValidate(object source, ServerValidateEventArgs args)
        {
            if (!string.IsNullOrEmpty(rblWarrantType.SelectedValue))
            {
                if (!string.IsNullOrEmpty(txtOName.Text.Trim()))
                    args.IsValid = true;
                else
                    args.IsValid = false;
            }
            else
                args.IsValid = true;
        }

        protected void valOAgency_ServerValidate(object source, ServerValidateEventArgs args)
        {
            if (!string.IsNullOrEmpty(txtOAgency.Text.Trim()))
                args.IsValid = true;
            else
                args.IsValid = false;
        }

        protected void valOID_ServerValidate(object source, ServerValidateEventArgs args)
        {
            if (!string.IsNullOrEmpty(txtOID.Text.Trim()))
                args.IsValid = true;
            else
                args.IsValid = false;
        }
        protected void drpAgency_SelectedIndexChanged(object sender, EventArgs e)
        {
            drpCounty.Items.Clear();
            BindCounties(Int32.Parse(drpAgency.SelectedValue));
            rblOffenseType.SelectedIndex = -1;
            drpJudge.SelectedIndex = -1;
            foreach (ListItem li in rblOffenseType.Items)
                li.Attributes.Add("onclick", "FillJudges()");
        }
    }
}