/*
' Copyright (c) 2021  Joe Terhune
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
using System.Text;
using DotNetNuke.Abstractions;
using Microsoft.Extensions.DependencyInjection;

namespace tjc.Modules.Notifications
{
    /// -----------------------------------------------------------------------------
    /// <summary>   
    /// The Edit class is used to manage content
    /// 
    /// Typically your edit control would be used to create new content, or edit existing content within your module.
    /// The ControlKey for this control is "Edit", and is defined in the manifest (.dnn) file.
    /// 
    /// Because the control inherits from NotificationsModuleBase you have access to any custom properties
    /// defined there, as well as properties from DNN such as PortalId, ModuleId, TabId, UserId and many more.
    /// 
    /// </summary>
    /// -----------------------------------------------------------------------------
    public partial class EditNotification : NotificationsModuleBase
    {
        private NotificationController ctl = new NotificationController();
        private readonly INavigationManager _navigationManager;
        public EditNotification()
        {
            _navigationManager = DependencyProvider.GetRequiredService<INavigationManager>();
        }

        public string JudgeRole
        {
            get
            {
                if (Settings.Contains("JudgeRole"))
                    return Settings["JudgeRole"].ToString();
                else
                    return "";
            }
        }

        public string JARole
        {
            get
            {
                if (Settings.Contains("JARole"))
                    return Settings["JARole"].ToString();
                else
                    return "";
            }
        }

        private bool IsJudge
        {
            get
            {
                JudgeController jCtl = new JudgeController();
                Judge objJudge = jCtl.GetJudge(UserId);
                if (objJudge != null)
                    return true;
                return false;
            }
        }


        public string GetUserInfo(int createdByUserID)
        {
            string returnValue = "";
            UserInfo objUser = UserController.GetUserById(PortalId, createdByUserID);
            if (objUser != null)
            {
                string cell = objUser.Profile.Cell;
                returnValue += string.IsNullOrEmpty(cell) ? "<br />No Cell" : cell;
                returnValue += "<br />";
                returnValue += objUser.Email;
            }
            return returnValue;
        }

        private void BindJudges()
        {
            var judges = DotNetNuke.Security.Roles.RoleController.Instance.GetUsersByRole(PortalId, JudgeRole);
            foreach (UserInfo u in judges)
            {
                drpOOOJudge.Items.Add(new ListItem(u.DisplayName.Replace("&nbsp;", " "), u.UserID.ToString()));
                drpCoverJudge.Items.Add(new ListItem(u.DisplayName.Replace("&nbsp;", " "), u.UserID.ToString()));
            }

            drpCoverJudge.Items.Insert(0, new ListItem("< Select Cover Judge >", ""));
            drpOOOJudge.Items.Insert(0, new ListItem("< Select Judge >", ""));
        }



        /// -----------------------------------------------------------------------------
        ///         ''' <summary>
        ///         ''' Page_Load runs when the control is loaded
        ///         ''' </summary>
        ///         ''' <remarks>
        ///         ''' </remarks>
        ///         ''' <history>
        ///         ''' </history>
        ///         ''' -----------------------------------------------------------------------------
        private void Page_Load(System.Object sender, System.EventArgs e)
        {
            try
            {
                if (Page.IsPostBack == false)
                {
                    if (string.IsNullOrEmpty(JARole) | !UserInfo.IsInRole(JARole))
                        Response.Redirect("/");
                    BindJudges();
                    lnkCancel.NavigateUrl = _navigationManager.NavigateURL();
                }
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
                if (drpCoverJudge.SelectedIndex <= 0 & txtMessage.Text.Trim() == "")
                {
                    DotNetNuke.UI.Skins.Skin.AddModuleMessage(this, "You must select a Judge to cover you or enter a notification message", DotNetNuke.UI.Skins.Controls.ModuleMessage.ModuleMessageType.YellowWarning);
                    return;
                }
                int requestingJudge = DotNetNuke.Common.Utilities.Null.NullInteger;
                Int32.TryParse(drpOOOJudge.SelectedValue, out requestingJudge);

                Notification objNotification = new Notification();
                if (drpCoverJudge.SelectedIndex > 0)
                    objNotification.CoveringJudgeID = System.Convert.ToInt32(drpCoverJudge.SelectedValue);
                else
                    objNotification.CoveringJudgeID = DotNetNuke.Common.Utilities.Null.NullInteger;
                string startDate = txtStartDate.Text + " " + txtStartTime.Text;
                string endDate = txtEndDate.Text + " " + txtEndTime.Text;
                if (!DateTime.TryParse(startDate, out DateTime StartDate) | !DateTime.TryParse(endDate, out DateTime EndDate))
                {
                    ltMessage.Text = "<p class='alert alert-danger'><em class='fa fa-warning'></em>&nbsp;You must enter a valid Start and End Date.  Please check your entries and try again.</p>";
                    return;
                }
                else
                {
                    DateTime sDate = DateTime.Parse(startDate);
                    DateTime eDate = DateTime.Parse(endDate);
                    if (sDate >= eDate)
                    {
                        ltMessage.Text = "<p class='alert alert-danger'><em class='fa fa-warning'></em>&nbsp;The Start date and time must not be equal to or less than the End date and time.</p>";
                        return;
                    }
                    objNotification.StartDateTime = sDate;
                    objNotification.EndDateTime = eDate;
                }

                objNotification.Message = txtMessage.Text;
                objNotification.RequestingJudgeID = requestingJudge;
                IEnumerable<Notification> colNotifications = ctl.GetConflictingNotifications(requestingJudge, objNotification.StartDateTime, objNotification.EndDateTime);
                if (colNotifications.Count() > 0)
                {
                    StringBuilder strMessage = new StringBuilder();
                    strMessage.Append("<strong>The selected dates conflict with other Notifications scheduled for the following dates:<strong><ul>");
                    foreach (Notification n in colNotifications)
                    {
                        strMessage.Append("<li>");
                        strMessage.Append(UserController.GetUserById(PortalId, n.RequestingJudgeID).DisplayName);
                        strMessage.Append(":&nbsp;");
                        strMessage.Append(n.StartDateTime.Value.ToShortDateString());
                        strMessage.Append(" - ");
                        strMessage.Append(n.EndDateTime.Value.ToShortDateString());
                        strMessage.Append("</li>");
                    }
                    strMessage.Append("</ul>");

                    DotNetNuke.UI.Skins.Skin.AddModuleMessage(this, strMessage.ToString(), DotNetNuke.UI.Skins.Controls.ModuleMessage.ModuleMessageType.YellowWarning);
                    return;
                }
                ctl.CreateNotification(objNotification);
                // refresh cache
                DotNetNuke.Entities.Modules.ModuleController.SynchronizeModule(ModuleId);
                // Redirect back to the portal home page
                Response.Redirect(_navigationManager.NavigateURL(), true);
            }
            catch (Exception exc)
            {
                Exceptions.ProcessModuleLoadException(this, exc);
            }

        }
    }
}