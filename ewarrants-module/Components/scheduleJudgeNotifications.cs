using DotNetNuke.Common.Utilities;
using DotNetNuke.Services.Scheduling;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace tjc.Modules.eWarrants.Components
{
    public class ScheduleJudgeNotifications : SchedulerClient
    {
        public ScheduleJudgeNotifications(ScheduleHistoryItem oItem) : base()
        {
            this.ScheduleHistoryItem = oItem;
        }

        public override void DoWork()
        {
            try
            {
                string notificationEmail = "jterhune@jud12.flcourts.org";
                int signedThreshold = 3;

                // Perform required items for logging
                this.Progressing();
                var ctl = new SettingController();
                Setting objSettings = ctl.GetSetting();
                if (objSettings != null)
                {
                    notificationEmail = objSettings.NotificationEmail;
                    signedThreshold = objSettings.SignedThreshold != Null.NullInteger ? 3 : objSettings.SignedThreshold;
                }
                var wCtl = new WarrantController();
                IEnumerable<WarrantListItem> colWarrants = wCtl.GetUnsignedWarrants(DateTime.Now.AddDays(-signedThreshold));
                if (colWarrants != null && colWarrants.Count() > 0)
                    EmailWarrantAlerts(colWarrants, notificationEmail, "The following warrants have been waiting for review for over " + signedThreshold.ToString() + " days.");

                // To log note
                this.ScheduleHistoryItem.AddLogNote("Notifications Processed Successfully");
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

        public static void EmailWarrantAlerts(IEnumerable<WarrantListItem> colWarrants, string email, string body)
        {
            StringBuilder sbWarrantList = new StringBuilder();
            sbWarrantList.Append(Environment.NewLine);
            foreach (WarrantListItem w in colWarrants)
            {
                string createdBy = "";
                string title = "";
                string createdDate = "";
                string JudgeName = "";
                if (w.JudgeUserID >0)
                    JudgeName = w.JudgeName;
                if (w.CreatedByUserID >0)
                    createdBy = w.CreatedByName;
                if (w.Title != null)
                    title = w.Title;
                if (w.CreatedDate == Null.NullDate)
                    createdDate = w.CreatedDate.ToShortDateString();
                sbWarrantList.Append(Environment.NewLine);
                sbWarrantList.Append("Warrant ID: ");
                sbWarrantList.Append(w.WarrantID);
                sbWarrantList.Append(Environment.NewLine);
                sbWarrantList.Append(" Title:");
                sbWarrantList.Append(title);
                sbWarrantList.Append(Environment.NewLine);
                sbWarrantList.Append(" Submitted on: ");
                sbWarrantList.Append(createdDate);
                sbWarrantList.Append(Environment.NewLine);
                sbWarrantList.Append(" Submitted by: ");
                sbWarrantList.Append(createdBy);
                sbWarrantList.Append(Environment.NewLine);
                sbWarrantList.Append(" Assigned Judge: ");
                sbWarrantList.Append(JudgeName);
                sbWarrantList.Append(Environment.NewLine);
            }
            string subject = "eWarrant Alert";

            body += sbWarrantList.ToString();

            DotNetNuke.Services.Mail.Mail.SendEmail("webhelp@jud12.flcourts.org", email, subject, body);
        }
    }
}