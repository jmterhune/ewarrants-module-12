
using System;
using DotNetNuke.Entities.Users;
using tjc.Modules.eWarrants.Components;
using DotNetNuke.Services.Exceptions;
using System.Web.UI.WebControls;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.UI;
using System.Reflection;
using DotNetNuke.Abstractions;
using Microsoft.Extensions.DependencyInjection;

namespace tjc.Modules.eWarrants
{
    public partial class EditSchedule : ewarrantsModuleBase
    {
        private readonly INavigationManager _navigationManager;
        public EditSchedule()
        {
            _navigationManager = DependencyProvider.GetRequiredService<INavigationManager>();

        }
        private void BindJudges()
        {
            var judges = DotNetNuke.Security.Roles.RoleController.Instance.GetUsersByRole(PortalId, JudgeRoleName);
            foreach (UserInfo u in judges)
                drpJudge.Items.Add(new ListItem(u.DisplayName.Replace("&nbsp;", " "), u.UserID.ToString()));
            drpJudge.Items.Insert(0, new ListItem("< Select Judge >", ""));
        }
        public string FormatDateTime(string inDate)
        {
            DateTime.TryParse(inDate, out DateTime outDate);
            if (outDate == DotNetNuke.Common.Utilities.Null.NullDate)
                return "";
            else
                return outDate.ToShortDateString() + " " + outDate.ToShortTimeString();
        }
        private void BindData()
        {
            var ctl = new NotificationController();
            IEnumerable<Notification> colNotifications = ctl.GetNotificationsByJudge(UserId,false,false,true);
            rptNotifications.DataSource = colNotifications;
            rptNotifications.DataBind();
        }
        protected void Page_Load(System.Object sender, System.EventArgs e)
        {
            try
            {
                if (Page.IsPostBack == false)
                {
                    if (!HasJudgeRole)
                        Response.Redirect(_navigationManager.NavigateURL());
                    BindJudges();
                    BindData();
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
                Response.Redirect(_navigationManager.NavigateURL(), true);
            }
            catch (Exception exc)
            {
                Exceptions.ProcessModuleLoadException(this, exc);
            }
        }
        protected void cmdReset_Click(object sender, EventArgs e)
        {
            try
            {
                Response.Redirect(EditUrl("cover"), true);
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
                Notification objNotification = new Notification();
                {
                    var withBlock = objNotification;
                    if (drpJudge.SelectedIndex > 0)
                        withBlock.CoveringJudgeID = System.Convert.ToInt32(drpJudge.SelectedValue);
                    else
                        withBlock.CoveringJudgeID = -1;
                    string startDate = txtStartDate.Text + " " + txtStartTime.Text;
                    string endDate = txtEndDate.Text + " " + txtEndTime.Text;
                    bool isStartDate = DateTime.TryParse(startDate, out DateTime sDate);
                    bool isEndDate = DateTime.TryParse(endDate, out DateTime eDate);
                    if (!isStartDate | !isEndDate)
                    {
                        ltMessage.Text = "<p class='alert alert-danger'><em class='fa fa-warning'></em>&nbsp;You must enter a valid Start and End Date.  Please check your entries and try again.</p>";
                        return;
                    }
                    else
                    {
                        if (sDate >= eDate)
                        {
                            ltMessage.Text = "<p class='alert alert-danger'><em class='fa fa-warning'></em>&nbsp;The Start date and time must not be equal to or greater than the End date and time.</p>";
                            return;
                        }
                        withBlock.StartDateTime = sDate;
                        withBlock.EndDateTime = eDate;
                    }
                    if (drpJudge.SelectedIndex <= 0 & txtMessage.Text.Trim() == "")
                    {
                        ltMessage.Text = "<p class='alert alert-danger'><em class='fa fa-warning'></em>&nbsp;You must select a Judge to cover you or enter a notification message</p>";
                        return;
                    }

                    withBlock.Message = txtMessage.Text;
                    withBlock.RequestingJudgeID = UserId;
                }
                var ctl = new NotificationController();
                IEnumerable<Notification> colNotifications = ctl.GetConflictingNotifications(UserId, objNotification.StartDateTime.Value, objNotification.EndDateTime.Value);
                if (colNotifications.Count() > 0)
                {
                    StringBuilder strMessage = new StringBuilder();
                    strMessage.Append("<div class='alert alert-danger'><em class='fa fa-warning'></em><strong>The selected dates conflict with other Notifications scheduled for the following dates:<strong><ul>");
                    foreach (Notification n in colNotifications)
                    {
                        strMessage.Append("<li>");
                        strMessage.Append(n.StartDateTime.Value.ToShortDateString());
                        strMessage.Append(" - ");
                        strMessage.Append(n.EndDateTime.Value.ToShortDateString());
                        strMessage.Append("</li>");
                    }
                    strMessage.Append("</ul></div>");
                    ltMessage.Text = strMessage.ToString();
                    return;
                }
                ctl.CreateNotification(objNotification);
                // refresh cache
                DotNetNuke.Entities.Modules.ModuleController.SynchronizeModule(ModuleId);
                // Redirect back to the portal home page
                Response.Redirect(EditUrl("cover"), true);
            }
            catch (Exception exc)
            {
                Exceptions.ProcessModuleLoadException(this, exc);
            }
        }
        protected void rptNotifications_ItemCommand(object source, RepeaterCommandEventArgs e)
        {
            int ScheduleId = System.Convert.ToInt32(e.CommandArgument);
            if (e.CommandName.ToLower() == "delete")
            {
                var ctl = new NotificationController();
                ctl.DeleteNotification(ScheduleId);
                BindData();
            }
        }
        protected void rptNotifications_ItemCreated(object sender, RepeaterItemEventArgs e)
        {
            if (e.Item.ItemType == ListItemType.Item || e.Item.ItemType == ListItemType.AlternatingItem)
            {
                RepeaterItem item = e.Item;
                var sm = ScriptManager.GetCurrent(Page);
                LinkButton cmdDelete = (LinkButton)item.FindControl("cmdDelete");
                sm.RegisterAsyncPostBackControl(cmdDelete);
            }
        }
        protected void UpdatePanel_Unload(object sender, EventArgs e)
        {
            MethodInfo methodInfo = typeof(ScriptManager).GetMethods(BindingFlags.NonPublic | BindingFlags.Instance).Where(i => i.Name.Equals("System.Web.UI.IScriptManagerInternal.RegisterUpdatePanel")).First();
            methodInfo.Invoke(ScriptManager.GetCurrent(Page), new object[] { sender as UpdatePanel });
        }
    }
}