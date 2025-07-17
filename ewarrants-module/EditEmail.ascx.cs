
using System;
using DotNetNuke.Entities.Users;
using tjc.Modules.eWarrants.Components;
using DotNetNuke.Services.Exceptions;
using System.Collections.Generic;
using System.Linq;
using DotNetNuke.Services.Mail;
using DotNetNuke.Abstractions;
using Microsoft.Extensions.DependencyInjection;

namespace tjc.Modules.eWarrants
{
    public partial class EditEmail : ewarrantsModuleBase
    {

        private readonly INavigationManager _navigationManager;
        public EditEmail()
        {
            _navigationManager = DependencyProvider.GetRequiredService<INavigationManager>();
        }
        protected void Page_Load(System.Object sender, System.EventArgs e)
        {
            try
            {
                if (Page.IsPostBack == false)
                {
                    if (!IsEditable)
                        lnkCancel.NavigateUrl = EditUrl("users");
                    else
                        lnkCancel.NavigateUrl = EditUrl("aid", AgencyId.ToString(), "adminusers");

                    bool IsAgencyAdmin = false;
                    var ctl = new AgencyController();
                    IEnumerable<AgencyUserListItem> agencyUsers = ctl.GetAgencyUsers(AgencyId);
                    IEnumerable<AgencyUserListItem> agencyAdmins = agencyUsers.Where(au => au.IsAdmin == true);
                    foreach (var a in agencyAdmins)
                    {
                        if (a.UserID == UserInfo.UserID)
                            IsAgencyAdmin = true;
                    }
                    if (IsAgencyAdmin)
                    {
                        UserInfo objAgencyUser = UserController.GetUserById(PortalId, AgencyUserId);
                        if (objAgencyUser != null)
                        {
                            lblEmailAddress.Text = "Email for " + objAgencyUser.DisplayName;
                            txtEmail.Text = objAgencyUser.Email;
                        }
                    }
                    else
                        // Not an admin
                        Response.Redirect(_navigationManager.NavigateURL());
                }
            }
            catch (Exception exc)
            {
               Exceptions.ProcessModuleLoadException(this, exc);
            }
        }


        protected void btnSave_Click(object sender, EventArgs e)
        {
            try
            {
                if (Page.IsValid)
                {
                    UserInfo objAgencyUser = UserController.GetUserById(PortalId, AgencyUserId);
                    objAgencyUser.Email = txtEmail.Text.ToLower().Trim();
                    UserController.UpdateUser(PortalId, objAgencyUser);

                    Mail.SendEmail(UserInfo.Email, objAgencyUser.Email, "Email Updated", "Your Agency Administrator for ewarrants.jud12.org has modified your email address.");
                    Response.Redirect(EditUrl("users"));
                }
                else
                {
                    string message = "Please enter a valid e-mail address.";
                    DotNetNuke.UI.Skins.Skin.AddModuleMessage(this, message, DotNetNuke.UI.Skins.Controls.ModuleMessage.ModuleMessageType.BlueInfo);
                }
            }
            catch (Exception exc)
            {
               Exceptions.ProcessModuleLoadException(this, exc);
            }
        }
    }
}