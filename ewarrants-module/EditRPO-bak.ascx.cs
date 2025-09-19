using Atalasoft.Imaging.Codec;
using Atalasoft.Imaging.Codec.Pdf;
using DotNetNuke.Abstractions;
using DotNetNuke.Entities.Users;
using DotNetNuke.Services.Exceptions;
using DotNetNuke.Services.Mail;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web.UI.WebControls;
using tjc.Modules.eWarrants.Components;

namespace tjc.Modules.eWarrants
{
    public partial class EditRPO : ewarrantsModuleBase
    {
        private readonly INavigationManager _navigationManager;
        private string coverJudgeName = "";
        private bool isOutofOffice = false;
        private UserInfo Judgeinfo;

        public string UploadHandler => TemplateSourceDirectory + "/Handlers/WarrantHandler.ashx";
        public string AttachmentHandler => TemplateSourceDirectory + "/Handlers/AttachmentHandler.ashx";

        public EditRPO()
        {
            _navigationManager = DependencyProvider.GetRequiredService<INavigationManager>();
        }

        private void BindCountyList(int agencyId)
        {
            try
            {
                var ctl = new AgencyController();
                Agency objAgency = ctl.GetAgency(agencyId, ModuleId);
                if (objAgency == null || !objAgency.Counties.Any())
                {
                    DotNetNuke.UI.Skins.Skin.AddModuleMessage(this, "Your agency's county list is not properly configured. Please contact your site administrator.", DotNetNuke.UI.Skins.Controls.ModuleMessage.ModuleMessageType.RedError);
                    return;
                }

                foreach (County c in objAgency.Counties)
                {
                    drpCounty.Items.Add(new ListItem(c.CountyName, c.CountyID.ToString()));
                }
                drpCounty.Items.Insert(0, new ListItem("< Select County >", ""));
                divCounty.Visible = drpCounty.Items.Count > 2;
                if (drpCounty.Items.Count == 2)
                    drpCounty.SelectedIndex = 1;
            }
            catch (Exception exc)
            {
                Exceptions.ProcessModuleLoadException(this, exc);
                DotNetNuke.UI.Skins.Skin.AddModuleMessage(this, "Error loading county list. Please try again or contact support.", DotNetNuke.UI.Skins.Controls.ModuleMessage.ModuleMessageType.RedError);
            }
        }

        private UserInfo GetCoverJudge(int coverJudgeId)
        {
            return UserController.GetUserById(PortalId, coverJudgeId);
        }

        private void SendEmailResponse(string agencyName, string warrantId)
        {
            try
            {
                string noreplyEmail = "webhelp@jud12.flcourts.org";
                string leoEmail = UserInfo != null ? UserInfo.Email : throw new InvalidOperationException("User email not available.");
                var sCtl = new SettingController();
                Setting objSettings = sCtl.GetSettings(ModuleId);
                if (objSettings != null)
                    noreplyEmail = objSettings.SenderEmail;

                if (!Int32.TryParse(hdJudgeId.Value, out int judgeId))
                {
                    DotNetNuke.UI.Skins.Skin.AddModuleMessage(this, "Invalid judge ID. No notification sent.", DotNetNuke.UI.Skins.Controls.ModuleMessage.ModuleMessageType.RedError);
                    return;
                }

                Judgeinfo = UserController.GetUserById(PortalId, judgeId);
                if (Judgeinfo == null)
                {
                    DotNetNuke.UI.Skins.Skin.AddModuleMessage(this, "The selected judge's record could not be found. No email notification can be sent.", DotNetNuke.UI.Skins.Controls.ModuleMessage.ModuleMessageType.RedError);
                    return;
                }

                string judgeEmail = Judgeinfo.Email;
                string warrantTypeName = "RPO";
                string subject, body, userDisplay;
                string judgeProfileContact1 = Helper.GetProfileValue(Judgeinfo, "Email2");
                string judgeProfileContact2 = Helper.GetProfileValue(Judgeinfo, "Email3");
                var nCtl = new NotificationController();
                var colNotifications = nCtl.GetCurrentNotificationsByJudge(Judgeinfo.UserID);

                if (colNotifications != null && colNotifications.Any())
                {
                    Notification objNotification = colNotifications.FirstOrDefault();
                    if (objNotification.CoveringJudgeID > 0)
                    {
                        var coverJudgeInfo = GetCoverJudge(objNotification.CoveringJudgeID);
                        coverJudgeName = coverJudgeInfo != null ? coverJudgeInfo.DisplayName : "";
                        judgeEmail = coverJudgeInfo != null ? coverJudgeInfo.Email : judgeEmail;
                        subject = "New " + warrantTypeName + " Forwarded from " + Judgeinfo.DisplayName.Replace(" ", " ") + " for Your Review";
                        userDisplay = UserInfo.DisplayName.Replace(" ", " ");
                        body = "A new " + warrantTypeName + " (ID: " + warrantId + ") has been submitted by " + userDisplay + " from " + agencyName + Environment.NewLine;
                        body += Judgeinfo.DisplayName.Replace(" ", " ") + " is out of the office and has asked that this " + warrantTypeName + " be forwarded to you during their absence.";
                        Mail.SendEmail(noreplyEmail, judgeEmail, subject, body);
                        Helper.SendToAlternateContact(noreplyEmail, judgeProfileContact1, subject, body);
                        Helper.SendToAlternateContact(noreplyEmail, judgeProfileContact2, subject, body);
                        if (HasUserOptedForSms(objNotification.CoveringJudgeID))
                        {
                            Task.Run(() => Helper.SendSmsNotifications(coverJudgeInfo, subject, body)).GetAwaiter().GetResult();
                        }

                        subject = "Out of Office Notice";
                        body = "The Honorable " + Judgeinfo.DisplayName.Replace(" ", " ") + " is Out of the Office." + Environment.NewLine;
                        body += "Your " + warrantTypeName + " notification has been forwarded to the Honorable " + coverJudgeName;
                        Mail.SendEmail(noreplyEmail, leoEmail, subject, body);
                        isOutofOffice = true;
                    }
                    else
                    {
                        subject = "New " + warrantTypeName + " Submitted for Review";
                        userDisplay = UserInfo.DisplayName.Replace(" ", " ");
                        body = "A new " + warrantTypeName + " has been submitted by " + userDisplay + " from " + agencyName;
                        Mail.SendEmail(noreplyEmail, judgeEmail, subject, body);
                        Helper.SendToAlternateContact(noreplyEmail, judgeProfileContact1, subject, body);
                        Helper.SendToAlternateContact(noreplyEmail, judgeProfileContact2, subject, body);
                        if (HasUserOptedForSms(Judgeinfo.UserID))
                        {
                            Task.Run(() => Helper.SendSmsNotifications(Judgeinfo, subject, body)).GetAwaiter().GetResult();
                        }

                        subject = "Out of Office Notice";
                        body = "The Honorable " + Judgeinfo.DisplayName.Replace(" ", " ") + " is Out of the Office. Please review the notice below" + Environment.NewLine;
                        body += objNotification.Message;
                        Mail.SendEmail(noreplyEmail, leoEmail, subject, body);
                        isOutofOffice = true;
                    }
                }
                else
                {
                    subject = "New " + warrantTypeName + " Submitted for Review";
                    userDisplay = UserInfo.DisplayName.Replace(" ", " ");
                    body = "A new " + warrantTypeName + " (ID: " + warrantId + ") has been submitted by " + userDisplay + " from " + agencyName;
                    Mail.SendEmail(noreplyEmail, judgeEmail, subject, body);
                    Helper.SendToAlternateContact(noreplyEmail, judgeProfileContact1, subject, body);
                    Helper.SendToAlternateContact(noreplyEmail, judgeProfileContact2, subject, body);
                    if (HasUserOptedForSms(Judgeinfo.UserID))
                    {
                        Task.Run(() => Helper.SendSmsNotifications(Judgeinfo, subject, body)).GetAwaiter().GetResult();
                    }
                }
            }
            catch (Exception exc)
            {
                Exceptions.ProcessModuleLoadException(this, exc);
                DotNetNuke.UI.Skins.Skin.AddModuleMessage(this, "Error sending email notifications. Please try again or contact support.", DotNetNuke.UI.Skins.Controls.ModuleMessage.ModuleMessageType.RedError);
            }
        }

        private byte[] GetImage(int id)
        {
            try
            {
                var ctl = new FileController();
                Components.File objWarrantImage = ctl.GetFile(id);
                if (objWarrantImage == null)
                    throw new InvalidOperationException("Warrant image not found.");
                return objWarrantImage.Bytes;
            }
            catch (Exception exc)
            {
                Exceptions.LogException(exc);
                throw;
            }
        }

        private void SendRPOHearingNotifications(string county, Warrant objWarrant, List<string> attachments)
        {
            PdfImageCollection col = null;
            try
            {
                if (objWarrant == null || string.IsNullOrEmpty(objWarrant.Defendant))
                    throw new ArgumentNullException("objWarrant");
                var leoUser = UserController.Instance.GetUserById(PortalId, objWarrant.CreatedByUserID);
                if (leoUser == null)
                    throw new InvalidOperationException("LEO user not found.");
                string leo = leoUser.DisplayName;
                string agency = objWarrant.Agency != null ? objWarrant.Agency.AgencyName : throw new InvalidOperationException("Agency not found.");
                string warrantTitle = objWarrant.Title;
                string[] emailString = string.IsNullOrEmpty(RPO_CC_Email) ? new string[0] : RPO_CC_Email.Split(new[] { ';' }).Where(s => !string.IsNullOrEmpty(s)).ToArray();

                col = new PdfImageCollection();
                byte[] imageData = GetImage(objWarrant.FileID);
                Helper.AddPdfDecoder();
                int pages = RegisteredDecoders.GetImageInfo(new MemoryStream(imageData)).FrameCount;

                for (int i = 0; i < pages; i++)
                    col.Add(new PdfImage(new MemoryStream(imageData), i, PdfCompressionType.Auto));

                var pdf = new PdfEncoder
                {
                    JpegQuality = 85,
                    SizeMode = PdfPageSizeMode.FitToPage,
                    PageSize = new Size(612, 792)
                };
                pdf.Metadata = new PdfMetadata(warrantTitle, "Circuit Court", "RPO File", "", "", "", DateTime.Now, DateTime.Now);
                byte[] pdfBytes = pdf.ToByteArray(col, null);

                IEnumerable<string> emailList = null;
                switch (county.ToLower())
                {
                    case "manatee":
                        emailList = emailString.Union(RPO_ManateeEmai.Split(new[] { ';' }).Where(s => !string.IsNullOrEmpty(s)));
                        break;
                    case "desoto":
                        emailList = emailString.Union(RPO_DeSotoEmail.Split(new[] { ';' }).Where(s => !string.IsNullOrEmpty(s)));
                        break;
                    case "sarasota":
                        emailList = emailString.Union(RPO_SarasotaEmail.Split(new[] { ';' }).Where(s => !string.IsNullOrEmpty(s)));
                        break;
                    default:
                        throw new ArgumentException("Invalid county: " + county);
                }

                var emailAttach = new List<DotNetNuke.Services.Mail.MailAttachment>();
                foreach (var attachment in attachments ?? new List<string>())
                {
                    emailAttach.Add(new DotNetNuke.Services.Mail.MailAttachment(attachment));
                }
                emailAttach.Add(new DotNetNuke.Services.Mail.MailAttachment("rpo-" + WarrantId + ".pdf", pdfBytes, "application/pdf"));

                string noreplyEmail = "webhelp@jud12.flcourts.org";
                var sCtl = new SettingController();
                Setting objSettings = sCtl.GetSettings(ModuleId);
                if (objSettings != null)
                    noreplyEmail = objSettings.SenderEmail;

                string toAddress = string.Join(",", emailList);
                string subject = "12th Circuit Court RPO Notification";
                string body = "<p><strong>Submitted By:</strong> " + leo + " from " + agency + "</p><p><strong>Respondent:</strong> " + objWarrant.Defendant + "</p><p>Submitted RPO Attached</p>";

                Mail.SendEmail(noreplyEmail, noreplyEmail, toAddress.Trim(), subject, body, emailAttach);
            }
            catch (Exception exc)
            {
                Exceptions.ProcessModuleLoadException(this, exc);
                DotNetNuke.UI.Skins.Skin.AddModuleMessage(this, "Error sending RPO hearing notifications. Please try again or contact support.", DotNetNuke.UI.Skins.Controls.ModuleMessage.ModuleMessageType.RedError);
            }
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            try
            {
                if (!Page.IsPostBack)
                {
                    if (string.IsNullOrEmpty(JudgeRoleName))
                    {
                        DotNetNuke.UI.Skins.Skin.AddModuleMessage(this, "The application has not been configured. Please contact the site administrator.", DotNetNuke.UI.Skins.Controls.ModuleMessage.ModuleMessageType.RedError);
                        pnlRPO.Visible = false;
                        cmdUpdate.Visible = false;
                        return;
                    }

                    var ctl = new AgencyController();
                    var colAgencyUser = ctl.GetUserAgencies(UserId, ModuleId) != null ? ctl.GetUserAgencies(UserId, ModuleId).Where(a => !a.IsAdmin) : Enumerable.Empty<AgencyUserListItem>();
                    if (!colAgencyUser.Any() && !IsSiteAdmin)
                    {
                        DotNetNuke.UI.Skins.Skin.AddModuleMessage(this, "Invalid user access. Please contact the site administrator.", DotNetNuke.UI.Skins.Controls.ModuleMessage.ModuleMessageType.RedError);
                        pnlRPO.Visible = false;
                        cmdUpdate.Visible = false;
                        return;
                    }

                    foreach (var item in colAgencyUser)
                    {
                        drpAgency.Items.Add(new ListItem(item.AgencyName, item.AgencyID.ToString()));
                    }
                    dvAgency.Visible = drpAgency.Items.Count > 1;
                    BindCountyList(Int32.Parse(drpAgency.SelectedValue));
                }
            }
            catch (Exception exc)
            {
                Exceptions.ProcessModuleLoadException(this, exc);
                DotNetNuke.UI.Skins.Skin.AddModuleMessage(this, "Error loading page. Please try again or contact support.", DotNetNuke.UI.Skins.Controls.ModuleMessage.ModuleMessageType.RedError);
            }
        }

        protected void cmdCancel_Click(object sender, EventArgs e)
        {
            try
            {
                if (!string.IsNullOrEmpty(hdFileId.Value) && Int32.TryParse(hdFileId.Value, out int fileId))
                {
                    var ctl = new FileController();
                    ctl.DeleteFile(fileId);
                }
                Response.Redirect(_navigationManager.NavigateURL(), true);
            }
            catch (Exception exc)
            {
                Exceptions.ProcessModuleLoadException(this, exc);
                DotNetNuke.UI.Skins.Skin.AddModuleMessage(this, "Error cancelling operation. Please try again or contact support.", DotNetNuke.UI.Skins.Controls.ModuleMessage.ModuleMessageType.RedError);
            }
        }

        protected void cmdUpdate_Click(object sender, EventArgs e)
        {
            try
            {
                if (!chkAffidavit.Checked || !chkRPO.Checked || !chkPetition.Checked)
                {
                    DotNetNuke.UI.Skins.Skin.AddModuleMessage(this, "The RPO, Affidavit, and Petition must be included. All checkboxes must be checked.", DotNetNuke.UI.Skins.Controls.ModuleMessage.ModuleMessageType.RedError);
                    return;
                }

                var ctl = new WarrantController();
                if (!Int32.TryParse(hdFileId.Value, out int fileId))
                {
                    DotNetNuke.UI.Skins.Skin.AddModuleMessage(this, "The file ID could not be found. Please cancel this process and try again.", DotNetNuke.UI.Skins.Controls.ModuleMessage.ModuleMessageType.RedError);
                    return;
                }

                Warrant existingWarrant = ctl.GetWarrantByFileId(fileId);
                if (existingWarrant != null)
                {
                    DotNetNuke.UI.Skins.Skin.AddModuleMessage(this, "Detected duplicate RPO file. Document not added. Please select the Cancel button to reset the form.", DotNetNuke.UI.Skins.Controls.ModuleMessage.ModuleMessageType.RedError);
                    return;
                }

                var objWarrant = new Warrant
                {
                    ModuleID = ModuleId,
                    AgencyID = Int32.Parse(drpAgency.SelectedValue),
                    CreatedByUserID = UserId,
                    CreatedDate = DateTime.Now,
                    Title = txtTitle.Text,
                    CountyName = drpCounty.SelectedItem.Text,
                    Status = WarrantStatus.New,
                    Defendant = txtRespondent.Text,
                    NotificationSentDate = DateTime.Now,
                    WarrantType = WarrantType.RPO,
                    IsJuvenile = false,
                    FileID = fileId
                };

                if (Int32.TryParse(hdJudgeId.Value, out int judgeId))
                    objWarrant.JudgeUserID = judgeId;

                ctl.CreateWarrant(objWarrant);
                var fileNames = new List<string>();

                if (!string.IsNullOrEmpty(hdAttachmentIds.Value))
                {
                    string[] fileList = hdAttachmentIds.Value.Split(new[] { ',' }).Where(s => !string.IsNullOrEmpty(s)).ToArray();
                    var aCtl = new AttachmentController();
                    foreach (var fileIdStr in fileList)
                    {
                        if (Int32.TryParse(fileIdStr, out int attachmentId))
                        {
                            var xref = new AttachmentWarrantXref { FileID = attachmentId, WarrantID = objWarrant.WarrantID };
                            aCtl.CreateAttachmentWarrantXref(xref);
                        }
                    }
                    fileNames = aCtl.GetAttachmentsByWarrant(objWarrant.WarrantID).Select(a => a.FileName).ToList();
                }

                var aCtlAgency = new AgencyController();
                Agency objAgency = aCtlAgency.GetAgency(Int32.Parse(drpAgency.SelectedValue), ModuleId);
                SendEmailResponse(objAgency.AgencyName, objWarrant.WarrantID.ToString());
                SendRPOHearingNotifications(drpCounty.SelectedItem.Text.ToLower(), objWarrant, fileNames);

                DotNetNuke.Entities.Modules.ModuleController.SynchronizeModule(ModuleId);

                string redirectUrl = isOutofOffice
                    ? _navigationManager.NavigateURL("", "msg=" + Judgeinfo.DisplayName + " is out of the office. " + (string.IsNullOrEmpty(coverJudgeName) ? "You will receive an email with further information from the Judge" : "Your submission has been forwarded to " + coverJudgeName))
                    : _navigationManager.NavigateURL();

                Response.Redirect(redirectUrl, true);
            }
            catch (Exception exc)
            {
                Exceptions.ProcessModuleLoadException(this, exc);
                DotNetNuke.UI.Skins.Skin.AddModuleMessage(this, "Error submitting RPO. Please try again or contact support.", DotNetNuke.UI.Skins.Controls.ModuleMessage.ModuleMessageType.RedError);
            }
        }

        protected void valPetitionCheck_ServerValidate(object source, ServerValidateEventArgs args)
        {
            args.IsValid = chkPetition.Checked;
        }

        protected void valRPO_ServerValidate(object source, ServerValidateEventArgs args)
        {
            args.IsValid = chkRPO.Checked;
        }

        protected void chkAffidavitRequired_ServerValidate(object source, ServerValidateEventArgs args)
        {
            args.IsValid = chkAffidavit.Checked;
        }

        protected void chkRPORequired_ServerValidate(object source, ServerValidateEventArgs args)
        {
            args.IsValid = chkRPO.Checked;
        }

        protected override void Render(System.Web.UI.HtmlTextWriter writer)
        {
            try
            {
                var ctl = new JudgeController();
                var judges = ctl.GetJudges(ModuleId) != null ? ctl.GetJudges(ModuleId) : Enumerable.Empty<Judge>();
                foreach (var j in judges)
                    Page.ClientScript.RegisterForEventValidation(drpJudge.UniqueID, j.JudgeID.ToString());

                Page.ClientScript.RegisterForEventValidation(drpJudge.UniqueID, "");
                base.Render(writer);
            }
            catch (Exception exc)
            {
                Exceptions.ProcessModuleLoadException(this, exc);
            }
        }

        protected void valEmergency_ServerValidate(object source, ServerValidateEventArgs args)
        {
            args.IsValid = false;
            if (Int32.TryParse(hdJudgeId.Value, out int judgeId))
            {
                var ctl = new JudgeController();
                var judge = ctl.GetJudge(judgeId, ModuleId);
                if (judge != null)
                {
                    bool notifyJudge = DateTime.Now.TimeOfDay <= judge.DayEnd.TimeOfDay && DateTime.Now.TimeOfDay >= judge.DayStart.TimeOfDay;
                    args.IsValid = notifyJudge;
                }
            }
        }

        protected void drpAgency_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                drpCounty.Items.Clear();
                BindCountyList(Int32.Parse(drpAgency.SelectedValue));
                drpJudge.SelectedIndex = -1;
            }
            catch (Exception exc)
            {
                Exceptions.ProcessModuleLoadException(this, exc);
                DotNetNuke.UI.Skins.Skin.AddModuleMessage(this, "Error updating agency selection. Please try again or contact support.", DotNetNuke.UI.Skins.Controls.ModuleMessage.ModuleMessageType.RedError);
            }
        }
    }
}