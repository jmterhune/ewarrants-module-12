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
using System.Web.UI.WebControls;
using tjc.Modules.eWarrants.Components;

namespace tjc.Modules.eWarrants
{
    public partial class EditRPO : ewarrantsModuleBase
    {
        private string coverJudgeName = "";
        private bool isOutofOffice = false;
        private UserInfo Judgeinfo = null;
        public string uploadHandler = "";
        public string attachmentHandler = "";
        private readonly INavigationManager _navigationManager;
        public EditRPO()
        {
            _navigationManager = DependencyProvider.GetRequiredService<INavigationManager>();
        }
        private void BindCountyList(int agencyId)
        {
            var ctl = new AgencyController();
            Agency objAgency = ctl.GetAgency(agencyId, ModuleId);
            if (objAgency.Counties.Count() > 0)
            {
                foreach (County c in (objAgency.Counties))
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

        private UserInfo GetCoverJudge(int coverJudgeId)
        {
            return UserController.GetUserById(PortalId, coverJudgeId);
        }

        private void SendEmailResponse(string agencyName, string warrantId)
        {
            string noreplyEmail = "webhelp@jud12.flcourts.org";
            string leoEmail = UserInfo.Email;
            if (Int32.TryParse(hdJudgeId.Value, out int judgeId))
                Judgeinfo = UserController.GetUserById(PortalId, judgeId);
            var sCtl = new SettingController();
            Setting objSettings = sCtl.GetSettings(ModuleId);
            if (objSettings != null)
                noreplyEmail = objSettings.SenderEmail;
            UserInfo CoverJudgeInfo = null;
            string warrantTypeName = "RPO";
            if (Judgeinfo != null)
            {
                string judgeEmail = Judgeinfo.Email;
                var nCtl = new NotificationController();
                IEnumerable<Notification> colNotifications = nCtl.GetCurrentNotificationsByJudge(Judgeinfo.UserID);
                string subject;
                string body;
                string userDisplay;
                string judgeProfileContact1 = Helper.GetProfileValue(Judgeinfo, "Email2");
                string judgeProfileContact2 = Helper.GetProfileValue(Judgeinfo, "Email3");
                if (colNotifications.Count() > 0)
                {
                    Notification objNotification = colNotifications.FirstOrDefault();
                    if (objNotification.CoveringJudgeID > 0)
                    {
                        CoverJudgeInfo = GetCoverJudge(objNotification.CoveringJudgeID);
                        coverJudgeName = CoverJudgeInfo.DisplayName;
                        judgeEmail = CoverJudgeInfo.Email;
                        subject = "New " + warrantTypeName + " Forwarded from  " + Judgeinfo.DisplayName.Replace(" ", " ") + " for Your Review";
                        userDisplay = UserInfo.DisplayName.Replace(" ", " ");
                        body = "A new " + warrantTypeName + " (ID: " + warrantId + ")  has been submitted by " + userDisplay + " from " + agencyName + Environment.NewLine;
                        body += Judgeinfo.DisplayName.Replace(" ", " ") + " is out of the office and has asked that this " + warrantTypeName + " be forwarded to you during their absence.";
                        DotNetNuke.Services.Mail.Mail.SendEmail(leoEmail, noreplyEmail, judgeEmail, subject, body);
                        if (!string.IsNullOrEmpty(judgeProfileContact1) && judgeProfileContact1.Contains("@"))
                            DotNetNuke.Services.Mail.Mail.SendEmail(noreplyEmail, judgeProfileContact1, subject, body);
                        if (!string.IsNullOrEmpty(judgeProfileContact2) && judgeProfileContact2.Contains("@"))
                            Mail.SendEmail(noreplyEmail, judgeProfileContact2, subject, body);
                        if (HasUserOptedForSms(objNotification.CoveringJudgeID))
                            Helper.SendSmsNotifications(CoverJudgeInfo,  subject, body);
                        subject = "Out of Office Notice";
                        body = " The Honorable " + Judgeinfo.DisplayName.Replace(" ", " ") + " is Out of the Office. " + Environment.NewLine;
                        body += "Your " + warrantTypeName + " notification has been forwarded to the Honorable " + coverJudgeName;
                        DotNetNuke.Services.Mail.Mail.SendEmail(noreplyEmail, leoEmail, subject, body);
                    }
                    else
                    {
                        subject = "New " + warrantTypeName + " Submitted for Review";
                        userDisplay = UserInfo.DisplayName.Replace(" ", " ");
                        body = "A new " + warrantTypeName + " has been submitted by " + userDisplay + " from " + agencyName;
                        DotNetNuke.Services.Mail.Mail.SendEmail(leoEmail, noreplyEmail, judgeEmail, subject, body);
                        if (!string.IsNullOrEmpty(judgeProfileContact1) && judgeProfileContact1.Contains("@"))
                            DotNetNuke.Services.Mail.Mail.SendEmail(noreplyEmail, judgeProfileContact1, subject, body);
                        if (!string.IsNullOrEmpty(judgeProfileContact2) && judgeProfileContact2.Contains("@"))
                            DotNetNuke.Services.Mail.Mail.SendEmail(noreplyEmail, judgeProfileContact2, subject, body);
                        if (HasUserOptedForSms(Judgeinfo.UserID))
                            Helper.SendSmsNotifications(Judgeinfo, subject, body);
                        subject = "Out of Office Notice";
                        body = " The Honorable " + Judgeinfo.DisplayName.Replace(" ", " ") + " is Out of the Office. Please review the notice below" + Environment.NewLine;
                        body += objNotification.Message;
                        DotNetNuke.Services.Mail.Mail.SendEmail(noreplyEmail, leoEmail, subject, body);
                    }
                }
                else
                {
                    subject = "New " + warrantTypeName + " Submitted for Review";
                    userDisplay = UserInfo.DisplayName.Replace(" ", " ");
                    body = "A new " + warrantTypeName + " (ID: " + warrantId + ") has been submitted by " + userDisplay + " from " + agencyName;
                    DotNetNuke.Services.Mail.Mail.SendEmail(leoEmail, noreplyEmail, judgeEmail, subject, body);
                    if (!string.IsNullOrEmpty(judgeProfileContact1) && judgeProfileContact1.Contains("@"))
                        DotNetNuke.Services.Mail.Mail.SendEmail(noreplyEmail, judgeProfileContact1, subject, body);
                    if (!string.IsNullOrEmpty(judgeProfileContact2) && judgeProfileContact2.Contains("@"))
                        DotNetNuke.Services.Mail.Mail.SendEmail(noreplyEmail, judgeProfileContact2, subject, body);
                    if (HasUserOptedForSms(Judgeinfo.UserID))
                        Helper.SendSmsNotifications(Judgeinfo,  subject, body);
                }
            }
            else
                DotNetNuke.UI.Skins.Skin.AddModuleMessage(this, "The selected Judge's record could not be found. No email notification can be sent", DotNetNuke.UI.Skins.Controls.ModuleMessage.ModuleMessageType.RedError);
        }

        private byte[] GetImage(int id)
        {
            var ctl = new FileController();
            Components.File objWarrantImage = ctl.GetFile(id);
            return objWarrantImage.Bytes;
        }

        private void SendRPOHearingNotifications(string county, Warrant objWarrant, List<string> attachments)
        {
            string respondent = objWarrant.Defendant;
            var leoUser = UserController.Instance.GetUserById(PortalId, objWarrant.CreatedByUserID);
            string leo = leoUser.DisplayName;
            string agency = objWarrant.Agency.AgencyName;
            string[] EmailString = Array.Empty<string>();
            if (!string.IsNullOrEmpty(RPO_CC_Email))
                EmailString = RPO_CC_Email.Split(new[] { ";" }, StringSplitOptions.RemoveEmptyEntries);
            string warrantTitle = objWarrant.Title;
            PdfImageCollection col = new PdfImageCollection();
            byte[] imageData = GetImage(objWarrant.FileID);
            RegisteredDecoders.Decoders.Add(new Atalasoft.Imaging.Codec.Pdf.PdfDecoder());
            int pages = RegisteredDecoders.GetImageInfo(new MemoryStream(imageData)).FrameCount;
            // Add all pages from a multipage TIFF.
            for (int i = 0; i <= pages - 1; i++)
                col.Add(new PdfImage(new MemoryStream(imageData), i, PdfCompressionType.Auto));

            // Create the PDF.
            PdfEncoder pdf = new PdfEncoder();
            // Set any properties.
            pdf.JpegQuality = 85;
            pdf.Metadata = new PdfMetadata(warrantTitle, "Circuit Court", "RPO File", "", "", "", DateTime.Now, DateTime.Now);
            // Make each image fit into an 8.5 x 11 inch page (612 x 792 @ 72 DPI).
            pdf.SizeMode = PdfPageSizeMode.FitToPage;
            pdf.PageSize = new Size(612, 792);
            byte[] pdfBytes = pdf.ToByteArray(col, null);
            IEnumerable<string> emailList = null;
            switch (county.ToLower())
            {
                case "manatee":
                    {
                        emailList = EmailString.Union(RPO_ManateeEmai.Split(new[] { ";" }, StringSplitOptions.RemoveEmptyEntries));
                        break;
                    }

                case "desoto":
                    {
                        emailList = EmailString.Union(RPO_DeSotoEmail.Split(new[] { ";" }, StringSplitOptions.RemoveEmptyEntries));
                        break;
                    }

                case "sarasota":
                    {
                        emailList = EmailString.Union(RPO_SarasotaEmail.Split(new[] { ";" }, StringSplitOptions.RemoveEmptyEntries));
                        break;
                    }
            }

            List<DotNetNuke.Services.Mail.MailAttachment> emailAttach = new List<DotNetNuke.Services.Mail.MailAttachment>();
            foreach (var a in attachments)
            {
                DotNetNuke.Services.Mail.MailAttachment attach = new DotNetNuke.Services.Mail.MailAttachment(a);
                emailAttach.Add(attach);
            }
            emailAttach.Add(new DotNetNuke.Services.Mail.MailAttachment(string.Format("rpo-{0}.pdf", WarrantId), pdfBytes, "application/pdf"));
            ICollection<DotNetNuke.Services.Mail.MailAttachment> colAttachments = emailAttach;
            string noreplyEmail = "webhelp@jud12.flcourts.org";
            string toAddress = string.Join(",", emailList);
            string subject = "12th Circuit Court RPO Notification";
            string body = "<p><strong>Submitted By:</strong> " + leo + " from " + agency + " </p><p><strong>Respondent: </strong>" + respondent + "</p><p>Submitted RPO Attached</p>";
            SettingController sCtl = new SettingController();
            Setting objSettings = sCtl.GetSettings(ModuleId);
            if (objSettings != null)
                noreplyEmail = objSettings.SenderEmail;
            try
            {
                DotNetNuke.Services.Mail.Mail.SendEmail(noreplyEmail, noreplyEmail, toAddress.Trim(), subject, body, colAttachments);
            }
            catch (Exception exc)
            {
                Exceptions.ProcessModuleLoadException(this, exc);
            }
        }

        protected void Page_Load(System.Object sender, System.EventArgs e)
        {
            try
            {
                uploadHandler = TemplateSourceDirectory + "/Handlers/WarrantHandler.ashx";
                attachmentHandler = TemplateSourceDirectory + "/Handlers/AttachmentHandler.ashx";
                if (Page.IsPostBack == false)
                {
                    if (string.IsNullOrEmpty(JudgeRoleName))
                    {
                        DotNetNuke.UI.Skins.Skin.AddModuleMessage(this, "The Application has not been configured. Please contact the site administrator", DotNetNuke.UI.Skins.Controls.ModuleMessage.ModuleMessageType.RedError);
                        pnlRPO.Visible = false;
                        cmdUpdate.Visible = false;
                        return;
                    }
                    var ctl = new AgencyController();
                    IEnumerable<AgencyUserListItem> colAgencyUser = ctl.GetUserAgencies(UserId, ModuleId).Where(a => a.IsAdmin == false);
                    if (colAgencyUser.Count() > 0 & !IsSiteAdmin)
                    {
                        foreach (AgencyUserListItem item in colAgencyUser)
                        {
                            drpAgency.Items.Add(new ListItem(item.AgencyName, item.AgencyID.ToString()));
                        }
                        if (drpAgency.Items.Count > 1)
                            dvAgency.Visible = true;

                        BindCountyList(Int32.Parse(drpAgency.SelectedValue));
                    }
                    else
                    {
                        DotNetNuke.UI.Skins.Skin.AddModuleMessage(this, "Invalid User Access.  Please contact the site administrator with the error.", DotNetNuke.UI.Skins.Controls.ModuleMessage.ModuleMessageType.RedError);
                        pnlRPO.Visible = false;
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

        protected void cmdUpdate_Click(object sender, EventArgs e)
        {
            try
            {
                var ctl = new WarrantController();
                List<string> fileNames = new List<string>();
                if (chkAffidavit.Checked & chkRPO.Checked & chkPetition.Checked)
                {
                    Warrant objwarrant = new Warrant
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
                        IsJuvenile = false
                    };
                    if (Int32.TryParse(hdFileId.Value, out int fileId))
                    {
                        Warrant existingWarrant = ctl.GetWarrantByFileId(fileId);
                        if (existingWarrant != null)
                        {
                            DotNetNuke.UI.Skins.Skin.AddModuleMessage(this, "Detected duplicate RPO file, Document not added. This usually happens when the submit button is double clicked or the submitted page is refreshed.<br /><br /><strong>Please select the Cancel Button to Reset the Form</strong>", DotNetNuke.UI.Skins.Controls.ModuleMessage.ModuleMessageType.RedError);
                            return;
                        }
                        objwarrant.FileID = fileId;
                    }
                    else
                    {
                        DotNetNuke.UI.Skins.Skin.AddModuleMessage(this, "The file ID could not be found.  Please cancel this process and try again.", DotNetNuke.UI.Skins.Controls.ModuleMessage.ModuleMessageType.RedError);
                        return;
                    }
                    string attachmentPath = @"C:\Websites\eWarrantAttachments\";
                    if (Settings.Contains("RPODirectory"))
                    {
                        attachmentPath = Settings["RPODirectory"].ToString();
                        if (!attachmentPath.EndsWith(@"\"))
                            attachmentPath = attachmentPath + @"\";
                    }
                    if (Int32.TryParse(hdJudgeId.Value, out int judgeId))
                        objwarrant.JudgeUserID = judgeId;
                    ctl.CreateWarrant(objwarrant);

                    if (!string.IsNullOrEmpty(hdAttachmentIds.Value))
                    {
                        string[] fileList = hdAttachmentIds.Value.Split(',');
                        var aCtl = new AttachmentController();
                        foreach (var fileid in fileList)
                        {
                            AttachmentWarrantXref xref = new AttachmentWarrantXref { FileID = Int32.Parse(fileid), WarrantID = objwarrant.WarrantID };
                            aCtl.CreateAttachmentWarrantXref(xref);
                        }
                        fileNames = aCtl.GetAttachmentsByWarrant(objwarrant.WarrantID).Select(a => a.FileName).ToList();
                    }
                    try
                    {
                        var aCtl = new AgencyController();
                        Agency objAgency = aCtl.GetAgency(Int32.Parse(drpAgency.SelectedValue), ModuleId);
                        SendEmailResponse(objAgency.AgencyName, objwarrant.WarrantID.ToString());
                        SendRPOHearingNotifications(drpCounty.SelectedItem.Text.ToLower(), objwarrant, fileNames);
                    }
                    catch (Exception ex)
                    {
                        DotNetNuke.UI.Skins.Skin.AddModuleMessage(this, "Your submission was successfully added, however, there was an error sending the  email.", DotNetNuke.UI.Skins.Controls.ModuleMessage.ModuleMessageType.RedError);
                        Exceptions.ProcessModuleLoadException(this, ex, false);
                    }
                }
                else
                {
                    DotNetNuke.UI.Skins.Skin.AddModuleMessage(this, "The RPO, Affidavit, and Petition must be included.  All checkboxes must be checked.", DotNetNuke.UI.Skins.Controls.ModuleMessage.ModuleMessageType.RedError);
                    return;
                }
                // refresh cache
                DotNetNuke.Entities.Modules.ModuleController.SynchronizeModule(ModuleId);
                // Redirect back to the portal home page
                if (isOutofOffice)
                {
                    string judgeName = Judgeinfo.DisplayName;
                    string notificationMessage = judgeName + " is out of the office. ";
                    if (coverJudgeName != "")
                        notificationMessage += " Your submission has been forwarded to " + coverJudgeName;
                    else
                        notificationMessage += " You will receive an email with further information from the Judge";
                    Response.Redirect(_navigationManager.NavigateURL("", "msg=" + notificationMessage), true);
                }
                else
                    Response.Redirect(_navigationManager.NavigateURL(), true);
            }
            catch (Exception exc)
            {
                Exceptions.ProcessModuleLoadException(this, exc);
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
            var ctl = new JudgeController();
            IEnumerable<Judge> judges = ctl.GetJudges(ModuleId);
            foreach (var j in judges)
                Page.ClientScript.RegisterForEventValidation(drpJudge.UniqueID, j.JudgeID.ToString());

            Page.ClientScript.RegisterForEventValidation(drpJudge.UniqueID, "");
            base.Render(writer);
        }

        protected void valEmergency_ServerValidate(object source, ServerValidateEventArgs args)
        {
            args.IsValid = false;
            Int32.TryParse(hdJudgeId.Value, out int judgeId);
            var ctl = new JudgeController();
            var judge = ctl.GetJudge(judgeId, ModuleId);
            bool NotifyJudge = true;
            if (DateTime.Now.TimeOfDay > judge.DayEnd.TimeOfDay | DateTime.Now.TimeOfDay < judge.DayStart.TimeOfDay)
                NotifyJudge = false;
            if (NotifyJudge)
                args.IsValid = true;
        }

        protected void drpAgency_SelectedIndexChanged(object sender, EventArgs e)
        {
            drpCounty.Items.Clear();
            BindCountyList(Int32.Parse(drpAgency.SelectedValue));
            drpJudge.SelectedIndex = -1;
        }

        protected void SendToAlternateContact(string replyEmail, string contactPoint, string subject, string body)
        {
            if (!string.IsNullOrEmpty(contactPoint) && contactPoint.Contains("@"))
                Mail.SendEmail(replyEmail, contactPoint, subject, body);
        }
    }
}