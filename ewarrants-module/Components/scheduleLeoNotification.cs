using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities.Users;
using DotNetNuke.Services.Scheduling;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace tjc.Modules.eWarrants.Components
{
    public class ScheduleLeoNotification : SchedulerClient
    {
        private string notificationEmail = "jterhune@jud12.flcourts.org";
        private int deleteThreshold = 10;
        private int claimedThreshold = 3;

        public ScheduleLeoNotification(ScheduleHistoryItem oItem) : base()
        {
            this.ScheduleHistoryItem = oItem;
        }


        public override void DoWork()
        {
            try
            {
                string body = "";
                // Perform required items for logging
                this.Progressing();
                var ctl = new SettingController();
                Setting objSettings = ctl.GetSetting();
                if (objSettings != null)
                {
                    notificationEmail = objSettings.NotificationEmail;
                    claimedThreshold = objSettings.ClaimedThreshold != Null.NullInteger ? objSettings.ClaimedThreshold : 3;
                    deleteThreshold = objSettings.DeleteThreshold != Null.NullInteger ? objSettings.DeleteThreshold : 3;
                }
                var wCtl = new WarrantController();
                wCtl.DeleteOldWarrants(objSettings.ModuleID, DateTime.Now.AddDays(-deleteThreshold));

                IEnumerable<WarrantListItem> colWarrants = wCtl.GetUnclaimedWarrants(DateTime.Now.AddDays(-claimedThreshold));
                var colAgencies = colWarrants.Select(w => w.AgencyID).Distinct();
                body = "The following warrants have been signed but have not been reveiwed by your agency for over " + claimedThreshold.ToString() + " days.";
                if (colWarrants != null)
                {
                    foreach (int a in colAgencies)
                    {
                        try
                        {
                            var aCtl = new AgencyController();
                            Agency objagency = aCtl.GetAgencyById(a);
                            var colAgencyWarrants = colWarrants.Where(aw => aw.AgencyID == a).ToList();
                            EmailWarrantAlerts(colAgencyWarrants, objagency.EmailAddress, body + objagency.AgencyName);
                        }
                        catch
                        {
                        }
                    }
                }


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

        public void EmailWarrantAlerts(List<WarrantListItem> colWarrants, string email, string body)
        {
            StringBuilder sbWarrantList = new StringBuilder();
            sbWarrantList.Append(Environment.NewLine);

            foreach (WarrantListItem w in colWarrants)
            {
                string createdBy = "";
                string title = "";
                string createdDate = "";
                string judgeName = "";
                if (w.CreatedByName != null)
                    createdBy = w.CreatedByName;
                if (w.Title != null)
                    title = w.Title;
                if (w.JudgeName != null)
                    judgeName = w.JudgeName;
                if (w.CreatedDate != Null.NullDate)
                    createdDate = w.CreatedDate.ToShortDateString();
                EmailOfficer(w);
                sbWarrantList.Append(Environment.NewLine);
                sbWarrantList.Append("Warrant ID: ");
                sbWarrantList.Append(w.WarrantID.ToString());
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
                sbWarrantList.Append(judgeName);
                sbWarrantList.Append(Environment.NewLine);
            }
            string subject = "eWarrant Alert";

            body += sbWarrantList.ToString();

            DotNetNuke.Services.Mail.Mail.SendEmail("webhelp@jud12.flcourts.org", email, subject, body);
        }

        public void EmailOfficer(WarrantListItem w)
        {
            UserInfo objUser = UserController.GetUserById(0, w.CreatedByUserID);
            if (objUser != null)
            {
                StringBuilder sbWarrantList = new StringBuilder();
                sbWarrantList.Append("Warrant ID: ");
                sbWarrantList.Append(w.WarrantID.ToString());
                sbWarrantList.Append(Environment.NewLine);
                sbWarrantList.Append(" Title:");
                sbWarrantList.Append(w.Title);
                sbWarrantList.Append(Environment.NewLine);
                sbWarrantList.Append(" Submitted on: ");
                sbWarrantList.Append(w.CreatedDate.ToShortDateString());
                sbWarrantList.Append(Environment.NewLine);
                sbWarrantList.Append(" Submitted by: ");
                sbWarrantList.Append(w.CreatedByName);
                sbWarrantList.Append(Environment.NewLine);

                string subject = "eWarrant Alert";
                string body = "The Warrant" + w.Title + "has been signed but has not been reveiwed by your agency for over " + claimedThreshold.ToString() + " days.";
                body += sbWarrantList.ToString();

                DotNetNuke.Services.Mail.Mail.SendEmail("webhelp@jud12.flcourts.org", objUser.Email, subject, body);
            }
        }
    }

}