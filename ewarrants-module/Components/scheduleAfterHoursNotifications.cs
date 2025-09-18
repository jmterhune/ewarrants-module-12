using DotNetNuke.Entities.Users;
using DotNetNuke.Services.Mail;
using DotNetNuke.Services.Scheduling;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using tjc.Modules.eWarrants.Components;

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
                this.Progressing();
                if (DateTime.Now.DayOfWeek == DayOfWeek.Saturday || DateTime.Now.DayOfWeek == DayOfWeek.Sunday)
                {
                    this.ScheduleHistoryItem.AddLogNote("Weekend notifications skipped.");
                    this.ScheduleHistoryItem.Succeeded = true;
                    return;
                }

                var ctl = new WarrantController();
                var colWarrants = ctl.GetNotificationsNotSent() ?? new List<WarrantListItem>();
                if (colWarrants.Any())
                    SendEmailResponse(colWarrants);

                this.ScheduleHistoryItem.AddLogNote("Queued warrant notifications sent successfully.");
                this.ScheduleHistoryItem.Succeeded = true;
            }
            catch (Exception ex)
            {
                this.ScheduleHistoryItem.Succeeded = false;
                this.ScheduleHistoryItem.AddLogNote("Exception: " + ex.Message);
                this.Errored(ref ex);
                DotNetNuke.Services.Exceptions.Exceptions.LogException(ex);
            }
        }

        private UserInfo GetCoverJudge(int coverJudgeId)
        {
            return UserController.GetUserById(PortalId, coverJudgeId);
        }

        private void SendEmailResponse(IEnumerable<WarrantListItem> colWarrants)
        {
            try
            {
                var ctl = new WarrantController();
                string fromAddress = "webhelp@jud12.flcourts.org";
                string senderEmail = "webhelp@jud12.flcourts.org";

                var sCtl = new SettingController();
                var setting = sCtl.GetSetting();
                if (setting != null && !string.IsNullOrEmpty(setting.SenderEmail))
                    fromAddress = setting.SenderEmail;

                foreach (var w in colWarrants)
                {
                    bool notificationSent = false;

                    var sender = UserController.GetUserById(PortalId, w.CreatedByUserID);
                    if (sender == null)
                    {
                        this.ScheduleHistoryItem.AddLogNote($"Failed to send notification for Warrant ID {w.WarrantID}: Sender user not found.");
                        continue;
                    }
                    senderEmail = sender.Email;

                    var judge = UserController.GetUserById(PortalId, w.JudgeUserID);
                    if (judge == null)
                    {
                        this.ScheduleHistoryItem.AddLogNote($"Failed to send notification for Warrant ID {w.WarrantID}: Judge user not found.");
                        continue;
                    }

                    string toAddress = judge.Email;
                    string address2 = Helper.GetProfileValue(judge, "Email2");
                    string address3 = Helper.GetProfileValue(judge, "Email3");
                    string subject = "";
                    string userDisplay = sender.DisplayName;
                    string body = "";
                    string coverJudgeName = "";
                    UserInfo coverJudgeInfo = null;

                    var nCtl = new NotificationController();
                    var colNotifications = nCtl.GetCurrentNotificationsByJudge(judge.UserID) ?? new List<Notification>();
                    var jCtl = new JudgeController();

                    if (colNotifications.Any())
                    {
                        var objNotification = colNotifications.FirstOrDefault();
                        if (objNotification.CoveringJudgeID > 0)
                        {
                            var jInfo = jCtl.GetJudge(objNotification.CoveringJudgeID);
                            if (jInfo == null)
                            {
                                this.ScheduleHistoryItem.AddLogNote($"Failed to send notification for Warrant ID {w.WarrantID}: Covering judge not found.");
                                continue;
                            }

                            if (DateTime.Now.TimeOfDay >= jInfo.DayStart.TimeOfDay && DateTime.Now.TimeOfDay <= jInfo.DayEnd.TimeOfDay)
                            {
                                coverJudgeInfo = GetCoverJudge(objNotification.CoveringJudgeID);
                                if (coverJudgeInfo == null)
                                {
                                    this.ScheduleHistoryItem.AddLogNote($"Failed to send notification for Warrant ID {w.WarrantID}: Covering judge user not found.");
                                    continue;
                                }

                                coverJudgeName = coverJudgeInfo.DisplayName;
                                toAddress = coverJudgeInfo.Email;
                                address2 = Helper.GetProfileValue(coverJudgeInfo, "Email2");
                                address3 = Helper.GetProfileValue(coverJudgeInfo, "Email3");
                                subject = $"New Warrant Forwarded from {judge.DisplayName} for Your Review";
                                body = $"A new warrant (ID: {w.WarrantID}) has been submitted by {userDisplay} from {w.AgencyName}{Environment.NewLine}{Environment.NewLine}";
                                body += $"{judge.DisplayName} is out of the office and has asked that warrants be forwarded to you during their absence.";

                                Mail.SendEmail(senderEmail, fromAddress, toAddress, subject, body);
                                Helper.SendToAlternateContact(fromAddress, address2, subject, body);
                                Helper.SendToAlternateContact(fromAddress, address3, subject, body);
                                if (HasUserOptedForSms(coverJudgeInfo))
                                {
                                    Task.Run(() => Helper.SendSmsNotifications(coverJudgeInfo,  subject, body)).GetAwaiter().GetResult();
                                }
                                notificationSent = true;
                            }
                        }
                        else
                        {
                            var jInfo = jCtl.GetJudge(w.JudgeUserID);
                            if (jInfo == null)
                            {
                                this.ScheduleHistoryItem.AddLogNote($"Failed to send notification for Warrant ID {w.WarrantID}: Judge info not found.");
                                continue;
                            }

                            if (DateTime.Now.TimeOfDay >= jInfo.DayStart.TimeOfDay && DateTime.Now.TimeOfDay <= jInfo.DayEnd.TimeOfDay)
                            {
                                subject = "New Warrant Submitted for Review";
                                body = $"A new warrant has been submitted by {userDisplay} from {w.AgencyName}";

                                Mail.SendEmail(senderEmail, fromAddress, toAddress, subject, body);
                                Helper.SendToAlternateContact(fromAddress, address2, subject, body);
                                Helper.SendToAlternateContact(fromAddress, address3, subject, body);
                                if (HasUserOptedForSms(judge))
                                {
                                    Task.Run(() => Helper.SendSmsNotifications(judge,  subject, body)).GetAwaiter().GetResult();
                                }
                                notificationSent = true;
                            }
                        }
                    }
                    else
                    {
                        var jInfo = jCtl.GetJudge(w.JudgeUserID);
                        if (jInfo == null)
                        {
                            this.ScheduleHistoryItem.AddLogNote($"Failed to send notification for Warrant ID {w.WarrantID}: Judge info not found.");
                            continue;
                        }

                        if (DateTime.Now.TimeOfDay >= jInfo.DayStart.TimeOfDay && DateTime.Now.TimeOfDay <= jInfo.DayEnd.TimeOfDay)
                        {
                            subject = $"New Warrant Submitted for Review";
                            body = $"A new warrant (ID: {w.WarrantID}) has been submitted by {userDisplay} from {w.AgencyName}";

                            Mail.SendEmail(senderEmail, fromAddress, toAddress, subject, body);
                            Helper.SendToAlternateContact(fromAddress, address2, subject, body);
                            Helper.SendToAlternateContact(fromAddress, address3, subject, body);
                            if (HasUserOptedForSms(judge))
                            {
                                Task.Run(() => Helper.SendSmsNotifications(judge,  subject, body)).GetAwaiter().GetResult();
                            }
                            notificationSent = true;
                        }
                    }

                    if (notificationSent)
                        ctl.UpdateNotificationSent(w.WarrantID);
                }
            }
            catch (Exception ex)
            {
                this.ScheduleHistoryItem.AddLogNote($"Failed to send notifications: {ex.Message}");
                DotNetNuke.Services.Exceptions.Exceptions.LogException(ex);
            }
        }

        private bool HasUserOptedForSms(UserInfo user)
        {
            if (user == null)
                return false;

            var smsConsent = Helper.GetProfileValue(user, "SmsConsent");
            return !string.IsNullOrEmpty(smsConsent) && smsConsent.ToLower() == "true";
        }
    }
}