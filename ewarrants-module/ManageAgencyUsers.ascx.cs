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
using System.Reflection;
using System.Web.UI;
using DotNetNuke.Abstractions;
using Microsoft.Extensions.DependencyInjection;

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
    public partial class ManageAgencyUsers : ewarrantsModuleBase
    {
        private Agency objAgency = new Agency();
        private AgencyUserListItem objAgencyUser = new AgencyUserListItem();
        public string ModuleTitle;
        private readonly INavigationManager _navigationManager;
        public ManageAgencyUsers()
        {
            _navigationManager = DependencyProvider.GetRequiredService<INavigationManager>();
        }
        public int agencyId
        {
            get
            {
                if (ViewState["AgencyId"] != null)
                    return Int32.Parse(ViewState["AgencyId"].ToString());
                else
                    return -1;
            }
            set
            {
                ViewState["AgencyId"] = value;
            }
        }
        private void BindData()
        {
            var ctl = new AgencyController();
            rptUsers.DataSource = ctl.GetAgencyUsers(agencyId);
            rptUsers.DataBind();
        }
        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);
            var ctl = new AgencyController();
            objAgencyUser = ctl.GetAgencyUser(UserId, ModuleId);
            if (objAgencyUser != null)
            {
                agencyId = objAgencyUser.AgencyID;
                objAgency = ctl.GetAgency(agencyId, ModuleId);
                ModuleTitle = "Users for <strong class=\"text-primary\">" + objAgency.AgencyName + "</strong>";
            }
        }
        private void Page_Load(System.Object sender, System.EventArgs e)
        {
            try
            {
                if (!Page.IsPostBack)
                {
                    lnkCancel.NavigateUrl = _navigationManager.NavigateURL();
                    if (objAgencyUser != null)
                    {
                        if (objAgencyUser.IsAdmin)
                        {
                            lnkAddUser.NavigateUrl = EditUrl("aid", agencyId.ToString(), "adduser");
                            BindData();

                            if (objAgency == null)
                                DotNetNuke.UI.Skins.Skin.AddModuleMessage(this, "Unable to determine assigned agency.", DotNetNuke.UI.Skins.Controls.ModuleMessage.ModuleMessageType.RedError);
                        }
                        else
                            // not admin
                            Response.Redirect(_navigationManager.NavigateURL());
                    }
                    else
                        // notuser
                        Response.Redirect(_navigationManager.NavigateURL());
                }
            }
            catch (Exception exc)
            {
                Exceptions.ProcessModuleLoadException(this, exc);
            }
        }
        protected void UpdatePanel_Unload(object sender, EventArgs e)
        {
            MethodInfo methodInfo = typeof(ScriptManager).GetMethods(BindingFlags.NonPublic | BindingFlags.Instance).Where(i => i.Name.Equals("System.Web.UI.IScriptManagerInternal.RegisterUpdatePanel")).First();
            methodInfo.Invoke(ScriptManager.GetCurrent(Page), new object[] { sender as UpdatePanel });
        }
        protected void rptUsers_ItemCommand(object source, RepeaterCommandEventArgs e)
        {
            Int32.TryParse(e.CommandArgument.ToString(), out int userId);
            if (e.CommandName.ToLower() == "setadmin")
            {
                var ctl = new AgencyController();
                AgencyUser objAgencyUser = ctl.GetUserAgencyXref(userId, agencyId);
                if (objAgencyUser.IsAdmin)
                    objAgencyUser.IsAdmin = false;
                else
                {
                    IEnumerable<AgencyUserListItem> agencyUsers = ctl.GetAgencyUsers(agencyId);
                    int admins = agencyUsers.Where(au => au.IsAdmin == true).Count();
                    if (admins >= 3)
                    {
                        DotNetNuke.UI.Skins.Skin.AddModuleMessage(this, "You cannot assign more than three admins.", DotNetNuke.UI.Skins.Controls.ModuleMessage.ModuleMessageType.RedError);
                        return;
                    }
                    else
                        objAgencyUser.IsAdmin = true;
                }
                ctl.UpdateAgencyUserAdmin(objAgencyUser);
                BindData();
            }
            if (e.CommandName == "reset")
            {
                UserInfo objUser = UserController.GetUserById(PortalId, userId);
                if (objUser != null)
                {
                    DotNetNuke.Entities.Users.UserController.ResetPasswordToken(objUser);

                    DotNetNuke.Services.Mail.Mail.SendMail(objUser, DotNetNuke.Services.Mail.MessageType.PasswordReminder, PortalSettings);
                    string password = UserController.ResetPassword(objUser, "");
                    DotNetNuke.UI.Skins.Skin.AddModuleMessage(this, "A password reset link was sent to " + objUser.DisplayName, DotNetNuke.UI.Skins.Controls.ModuleMessage.ModuleMessageType.BlueInfo);
                }
                else
                    DotNetNuke.UI.Skins.Skin.AddModuleMessage(this, "Unable to retrieve user information", DotNetNuke.UI.Skins.Controls.ModuleMessage.ModuleMessageType.RedError);
            }
            if (e.CommandName == "email")
            {
                string url = EditUrl("aid", agencyId.ToString(), "EditEmail", "userid=" + userId);
                Response.Redirect(url, true);
            }

            if (e.CommandName.ToLower() == "delete")
            {
                if (userId == UserId)
                {
                    DotNetNuke.UI.Skins.Skin.AddModuleMessage(this, "You cannot delete yourself.", DotNetNuke.UI.Skins.Controls.ModuleMessage.ModuleMessageType.YellowWarning);
                    return;
                }
                UserInfo objUser = UserController.GetUserById(PortalId, userId);
                var ctl = new AgencyController();

                ctl.DeleteAgencyUser(userId, agencyId);
                UserController.DeleteUser(ref objUser, false, true);
            }

        }
        protected void rptUsers_ItemCreated(object sender, RepeaterItemEventArgs e)
        {
            if (e.Item.ItemType == ListItemType.Item || e.Item.ItemType == ListItemType.AlternatingItem)
            {
                RepeaterItem item = e.Item;
                var sm = ScriptManager.GetCurrent(Page);
                LinkButton cmdDelete = (LinkButton)item.FindControl("cmdDelete");
                LinkButton cmdSetAdmin = (LinkButton)item.FindControl("cmdSetAdmin");
                sm.RegisterAsyncPostBackControl(cmdDelete);
                sm.RegisterAsyncPostBackControl(cmdSetAdmin);
            }

        }
        protected void rptUsers_ItemDataBound(object sender, RepeaterItemEventArgs e)
        {
            if (e.Item.ItemType == ListItemType.Item || e.Item.ItemType == ListItemType.AlternatingItem)
            {
                RepeaterItem item = e.Item;
                AgencyUserListItem objUser = (AgencyUserListItem)e.Item.DataItem;
                LinkButton cmdDelete = (LinkButton)item.FindControl("cmdDelete");
                LinkButton cmdPasswordReset = (LinkButton)item.FindControl("cmdPasswordReset");
                LinkButton cmdEmailEdit = (LinkButton)item.FindControl("cmdEmailEdit");
                LinkButton cmdSetAdmin = (LinkButton)item.FindControl("cmdSetAdmin");
                if (objUser.IsAdmin)
                {
                    cmdSetAdmin.Text = "<em class='fa fa-check-square'>&nbsp;</em>";
                    if (objUser.UserID == UserId)
                    {
                        cmdSetAdmin.Enabled = false;
                        cmdSetAdmin.Text = "<em class='text-danger fa fa-check-square'>&nbsp;</em>";
                    }
                }
                else
                    cmdSetAdmin.Text = "<em class='fa fa-square'>&nbsp;</em>";

                if (objUser.UserID == UserId)
                {
                    cmdPasswordReset.Visible = false;
                    cmdDelete.Visible = false;
                    cmdEmailEdit.Visible = false;
                }
            }

        }
    }
}