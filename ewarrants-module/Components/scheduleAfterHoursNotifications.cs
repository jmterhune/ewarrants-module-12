using DotNetNuke.Entities.Users;
using DotNetNuke.Services.Mail;
using DotNetNuke.Services.Scheduling;
using System;
using System.Collections.Generic;
using System.Linq;

namespace tjc.Modules.eWarrants.Components
{
    public class ScheduleAfterHoursNotifications : SchedulerClient
    {
        private int PortalId = 0;
        public ScheduleAfterHoursNotifications(ScheduleHistoryItem oItem) : base()
        {
            this.ScheduleHistoryItem = oItem;
        }

        public override void DoWork()
        {
            try
            {
                // Perform required items for logging
                this.Progressing();
                if (DateTime.Now.DayOfWeek == DayOfWeek.Saturday | DateTime.Now.DayOfWeek == DayOfWeek.Sunday)
                {
                    // To log note
                    this.ScheduleHistoryItem.AddLogNote("Weekend Notifications skipped");
                    // Show success
                    this.ScheduleHistoryItem.Succeeded = true;
                    return;
                }
                var ctl = new WarrantController();
                IEnumerable<WarrantListItem> colWarrants = ctl.GetNotificationsNotSent();
                if (colWarrants != null && colWarrants.Count() > 0)
                    SendEmailResponse(colWarrants);

                // To log note
                this.ScheduleHistoryItem.AddLogNote(" Queued Warrant Notifications Sent Successfully");
                // Show success
                this.ScheduleHistoryItem.Succeeded = true;
            }

            catch (Exception ex)
            {
                this.ScheduleHistoryItem.Succeeded = false;
                this.ScheduleHistoryItem.AddLogNote("Exception= " + ex.ToString());
                this.Errored(ref ex);
                DotNetNuke.Services.Exceptions.Exceptions.LogException(ex);
            }
        }

        private UserInfo GetCoverJudge(int coverJudgeId)
        {
            return UserController.GetUserById(0, coverJudgeId);
        }
        private void SendEmailResponse(IEnumerable<WarrantListItem> colWarrants)
        {
            var ctl = new WarrantController();
            string fromaddress = "webhelp@jud12.flcourts.org";
            string senderEmail = "webhelp@jud12.flcourts.org";
            bool notificationSent;
            foreach (WarrantListItem w in colWarrants)
            {
                notificationSent = false;

                var sCtl = new SettingController();
                Setting setting = sCtl.GetSetting();
                if (setting != null)
                {
                    if(!string.IsNullOrEmpty(setting.SenderEmail))
                     fromaddress = setting.SenderEmail;
                }
                UserInfo sender = UserController.GetUserById(PortalId, w.CreatedByUserID);
                if (sender != null)
                    senderEmail = sender.Email;
                UserInfo judge = UserController.GetUserById(PortalId, w.JudgeUserID);
                UserInfo CoverJudgeInfo = null;
                string coverJudgeName = "";
                string subject = "";
                string userDisplay = "";
                string body = "";

                if (judge != null)
                {
                    string toAddress = judge.Email;
                    string address2 = "";
                    string address3 = "";
                    IEnumerable<Notification> colNotifications;
                    var nCtl = new NotificationController();
                    colNotifications = nCtl.GetCurrentNotificationsByJudge(judge.UserID);
                    var jCtl = new JudgeController();
                    if (colNotifications != null && colNotifications.Count() > 0)
                    {
                        Notification objNotification = colNotifications.FirstOrDefault();
                        if (objNotification.CoveringJudgeID > 0)
                        {
                            Judge jInfo = jCtl.GetJudge(objNotification.CoveringJudgeID);
                            if (DateTime.Now.TimeOfDay >= jInfo.DayStart.TimeOfDay & DateTime.Now.TimeOfDay <= jInfo.DayEnd.TimeOfDay)
                            {
                                CoverJudgeInfo = GetCoverJudge(objNotification.CoveringJudgeID);
                                coverJudgeName = CoverJudgeInfo.DisplayName;
                                toAddress = CoverJudgeInfo.Email;
                                if (CoverJudgeInfo.Profile.ProfileProperties["Email2"] != null)
                                    address2 = CoverJudgeInfo.Profile.ProfileProperties["Email2"].PropertyValue;
                                if (CoverJudgeInfo.Profile.ProfileProperties["Email3"] != null)
                                    address3 = CoverJudgeInfo.Profile.ProfileProperties["Email3"].PropertyValue;
                                subject = "New Warrant Forwarded from  " + judge.DisplayName.Replace("&nbsp;", " ") + " for Your Review";
                                userDisplay = sender.DisplayName.Replace("&nbsp;", " ");
                                body = "A new warrant (ID: " + w.WarrantID + ")  has been submitted by " + userDisplay + " from " + w.AgencyName + Environment.NewLine + Environment.NewLine;
                                body += judge.DisplayName.Replace("&nbsp;", " ") + " is out of the office and has asked that warrants be forwarded to you during their absence.";
                                DotNetNuke.Services.Mail.Mail.SendEmail(senderEmail,fromaddress, toAddress, subject, body);
                                if (!string.IsNullOrEmpty(address2))
                                    SendToAlternateContact(setting.TwilioAccountSid, setting.TelnyxApiKey, setting.TwilioPhoneNumber, fromaddress, address2, subject, body);
                                if (!string.IsNullOrEmpty(address3))
                                    SendToAlternateContact(setting.TwilioAccountSid, setting.TelnyxApiKey, setting.TwilioPhoneNumber, fromaddress, address3, subject, body);
                                notificationSent = true;
                            }
                        }
                        else
                        {
                            Judge jInfo = jCtl.GetJudge(w.JudgeUserID);
                            if (DateTime.Now.TimeOfDay >= jInfo.DayStart.TimeOfDay & DateTime.Now.TimeOfDay <= jInfo.DayEnd.TimeOfDay)
                            {
                                subject = "New Warrant Submitted for Review";
                                userDisplay = sender.DisplayName.Replace("&nbsp;", " ");
                                body = "A new warrant has been submitted by " + userDisplay + " from " + w.AgencyName;
                                if (judge.Profile.ProfileProperties["Email2"] != null)
                                    address2 = judge.Profile.ProfileProperties["Email2"].PropertyValue;
                                if (judge.Profile.ProfileProperties["Email3"] != null)
                                    address3 = judge.Profile.ProfileProperties["Email3"].PropertyValue;

                                DotNetNuke.Services.Mail.Mail.SendEmail(senderEmail,fromaddress, toAddress, subject, body);

                                if (!string.IsNullOrEmpty(address2))
                                    SendToAlternateContact(setting.TwilioAccountSid, setting.TelnyxApiKey, setting.TwilioPhoneNumber, fromaddress, address2, subject, body);

                                if (!string.IsNullOrEmpty(address3))
                                    SendToAlternateContact(setting.TwilioAccountSid, setting.TelnyxApiKey, setting.TwilioPhoneNumber, fromaddress, address3, subject, body);
                                notificationSent = true;
                            }
                        }
                    }
                    else
                    {
                        Judge jInfo = jCtl.GetJudge(w.JudgeUserID);
                        if (DateTime.Now.TimeOfDay >= jInfo.DayStart.TimeOfDay & DateTime.Now.TimeOfDay <= jInfo.DayEnd.TimeOfDay)
                        {
                            subject = "New Warrant Submitted for Review";
                            userDisplay = sender.DisplayName.Replace("&nbsp;", " ");
                            body = "A new warrant (ID: " + w.WarrantID + ") has been submitted by " + userDisplay + " from " + w.AgencyName;
                            if (judge.Profile.ProfileProperties["Email2"] != null)
                                address2 = judge.Profile.ProfileProperties["Email2"].PropertyValue;
                            if (judge.Profile.ProfileProperties["Email3"] != null)
                                address3 = judge.Profile.ProfileProperties["Email3"].PropertyValue;
                            DotNetNuke.Services.Mail.Mail.SendEmail(senderEmail,fromaddress, toAddress, subject, body);
                            if (!string.IsNullOrEmpty(address2))
                                SendToAlternateContact(setting.TwilioAccountSid, setting.TelnyxApiKey, setting.TwilioPhoneNumber, fromaddress, address2, subject, body);
                            if (!string.IsNullOrEmpty(address3))
                                SendToAlternateContact(setting.TwilioAccountSid, setting.TelnyxApiKey, setting.TwilioPhoneNumber, fromaddress, address3, subject, body);
                            notificationSent = true;
                        }
                    }
                }
                if (notificationSent)
                    ctl.UpdateNotificationSent(w.WarrantID);
            }
        }
        private async void SendToAlternateContact(string accountSid, string authToken, string fromPhoneNumber, string replyEmail, string contactPoint, string subject, string body)
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
    }
}