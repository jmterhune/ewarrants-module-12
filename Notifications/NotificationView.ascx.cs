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
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web.UI;
using System.Web.UI.WebControls;
using tjc.Modules.eWarrants.Components;
using DotNetNuke.Entities.Users;
using DotNetNuke.Services.Exceptions;

namespace tjc.Modules.Notifications
{
    /// -----------------------------------------------------------------------------
    /// <summary>
    /// The View class displays the content
    /// 
    /// Typically your view control would be used to display content or functionality in your module.
    /// 
    /// View may be the only control you have in your project depending on the complexity of your module
    /// 
    /// Because the control inherits from NotificationsModuleBase you have access to any custom properties
    /// defined there, as well as properties from DNN such as PortalId, ModuleId, TabId, UserId and many more.
    /// 
    /// </summary>
    /// -----------------------------------------------------------------------------
    public partial class NotificationView : NotificationsModuleBase
    {
        private NotificationController ctl = new NotificationController();
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

        public string AssistantRole
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


        public string GetJudge(string requestingJudgeId)
        {
            if (Int32.TryParse(requestingJudgeId, out int judgeId))
            {
                UserInfo objJudge = UserController.GetUserById(PortalId, judgeId);
                return objJudge.DisplayName;
            }
            return "";
        }

        public string GetUserInfo(string createdByUserID)
        {
            string returnValue = "";
            if (Int32.TryParse(createdByUserID, out int userId))
            {
                UserInfo objUser = UserController.GetUserById(PortalId, userId);
                if (objUser != null)
                {
                    string cell = objUser.Profile.Cell;
                    returnValue += string.IsNullOrEmpty(cell) ? "<br />No Cell" : cell;
                    returnValue += "<br />";
                    returnValue += objUser.Email;
                }
            }
            return returnValue;
        }

        public string FormatDateTime(string inDate)
        {
            if (DateTime.TryParse(inDate, out DateTime formatDate))
            {
                return string.Format("{0} {1}", formatDate.ToShortDateString(), formatDate.ToShortTimeString());
            }
            return "";
        }

        /// <summary>
        ///         ''' 
        ///         ''' </summary>
        private void BindData()
        {
            IEnumerable<Notification> colNotifications = ctl.GetCurrentNotifications();
            rptNotifications.DataSource = colNotifications;
            rptNotifications.DataBind();
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
                    if (AssistantRole != "")
                    {
                        lnkAddNotification.NavigateUrl = EditUrl();

                        if (!UserInfo.IsInRole(AssistantRole))
                            Response.Redirect("/");
                        BindData();
                    }
                    else
                    {
                        dptUpdate.Visible = false;
                        DotNetNuke.UI.Skins.Skin.AddModuleMessage(this, "Please Configure this Module", DotNetNuke.UI.Skins.Controls.ModuleMessage.ModuleMessageType.RedError);
                    }
                }
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