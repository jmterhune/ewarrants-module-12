
using System;
using DotNetNuke.Entities.Users;
using tjc.Modules.eWarrants.Components;
using DotNetNuke.Services.Exceptions;
using System.Collections.Generic;
using System.Linq;
using DotNetNuke.Services.Mail;
using DotNetNuke.Security.Membership;
using DotNetNuke.Services.Localization;
using DotNetNuke.Security.Permissions;
using DotNetNuke.Abstractions;
using Microsoft.Extensions.DependencyInjection;

namespace tjc.Modules.eWarrants
{
    public partial class EditUser : ewarrantsModuleBase
    {
        private readonly INavigationManager _navigationManager;
        public EditUser()
        {
            _navigationManager = DependencyProvider.GetRequiredService<INavigationManager>();
        }

        //------Properties and Methods---------------------
        #region Properties and Methods
        private void ClearForm()
        {
            txtEmail.Text = "";
            txtFirstName.Text = "";
            txtLastName.Text = "";
            txtUserName.Text = "";
            chkAdmin.Checked = false;
        }
        #endregion

        //------Event Handlers---------------------
        #region Event Handlers
        protected void Page_Load(System.Object sender, System.EventArgs e)
        {
            try
            {
              
                if (!IsPostBack)
                {
                    
                    if ( !ModulePermissionController.CanEditModuleContent(this.ModuleConfiguration))
                        lnkCancel.NavigateUrl = EditUrl("users");
                    else
                        lnkCancel.NavigateUrl = EditUrl("aid", AgencyId.ToString(), "adminusers");

                }
                bool IsAgencyAdmin = false;
                var ctl = new AgencyController();
                IEnumerable<AgencyUserListItem> agencyUsers = ctl.GetAgencyUsers(AgencyId);
                IEnumerable<AgencyUserListItem> agencyAdmins = agencyUsers.Where(au => au.IsAdmin == true);
                foreach (var a in agencyAdmins)
                {
                    if (a.UserID == UserInfo.UserID)
                        IsAgencyAdmin = true;
                }
                if (IsAgencyAdmin | UserInfo.IsAdmin)
                {
                    int admins = agencyAdmins.Count();
                    if (admins >= 3)
                        chkAdmin.Visible = false;
                }
                else
                    // Not an admin
                    Response.Redirect(_navigationManager.NavigateURL());

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
                    UserInfo objUser = new UserInfo
                    {
                        LastIPAddress = "",
                        UserID = -1,
                        Username = this.txtUserName.Text,
                        FirstName = this.txtFirstName.Text,
                        LastName = this.txtLastName.Text,
                        Email = this.txtEmail.Text,
                        DisplayName = this.txtFirstName.Text + " " + this.txtLastName.Text,
                        PortalID=PortalId
                    };
                    objUser.Membership.Password = UserController.GeneratePassword(8);
                    objUser.Membership.UpdatePassword = true;
                    objUser.Membership.Approved = true;

                    UserCreateStatus createStatus = UserController.CreateUser(ref objUser);

                    if (createStatus == UserCreateStatus.Success)
                    {
                        if (chkNotify.Checked)
                            Mail.SendMail(objUser, MessageType.UserRegistrationPublic, PortalSettings);
                        var ctl = new AgencyController();

                        objUser = UserController.GetUserByName(PortalSettings.PortalId, objUser.Username);
                        string body = Localization.GetString("", Localization.SharedResourceFile);
                        DotNetNuke.UI.Skins.Skin.AddModuleMessage(this, "The User was added Successfully.", DotNetNuke.UI.Skins.Controls.ModuleMessage.ModuleMessageType.GreenSuccess);
                        AgencyUser objAgencyUser = new AgencyUser
                        {
                            AgencyID = AgencyId,
                            UserID = objUser.UserID,
                            IsAdmin = chkAdmin.Checked
                        };

                        if (IsSiteAdmin)
                        {
                            IEnumerable<AgencyUserListItem> agencyAdmins = ctl.GetAgencyUsers(AgencyId).Where(au => au.IsAdmin == true);
                            foreach (var a in agencyAdmins)
                            {
                                UserInfo obju = UserController.GetUserById(PortalId, a.UserID);
                                Mail.SendEmail(PortalSettings.Email, obju.Email, "User added by site administrator", "The administrator at ewarrants.jud12.org has added " + objUser.DisplayName + " as a new user to your agency.");
                            }
                        }
                        ctl.CreateAgencyUserXref(objAgencyUser);
                        ClearForm();
                    }
                    else
                    {
                        string message = "";
                        switch (createStatus)
                        {
                            case UserCreateStatus.DuplicateUserName:
                                {
                                    message = "The Username is already in Use.  Please choose a different Username.";
                                    break;
                                }

                            case UserCreateStatus.InvalidPassword:
                                {
                                    message = "Invalid Password.  The Password does not meet the minium password strength requirements. The password must be a minimum of 7 characters, must include at least one lower case letter and one upper case letter, and at least one number.";
                                    break;
                                }

                            case UserCreateStatus.InvalidEmail:
                                {
                                    message = "The Email address is not in the correct format.  User was not created.";
                                    break;
                                }

                            case UserCreateStatus.InvalidUserName:
                                {
                                    message = "The username must be a minimum of four characters. User was not created.";
                                    break;
                                }

                            case UserCreateStatus.PasswordMismatch:
                                {
                                    message = "The Password and the Confirmation Password fields do not match.  Please re-enter the password.";
                                    break;
                                }

                            case UserCreateStatus.UserAlreadyRegistered:
                                {
                                    message = "The Username is already in Use.  Please choose a different Username.";
                                    break;
                                }

                            case UserCreateStatus.UsernameAlreadyExists:
                                {
                                    message = "The Username is already in Use.  Please choose a different Username.";
                                    break;
                                }
                        }
                        DotNetNuke.UI.Skins.Skin.AddModuleMessage(this, message, DotNetNuke.UI.Skins.Controls.ModuleMessage.ModuleMessageType.RedError);
                    }
                }
                else
                {
                    string message = "Please enter a first and last name and a valid e-mail address.";
                    DotNetNuke.UI.Skins.Skin.AddModuleMessage(this, message, DotNetNuke.UI.Skins.Controls.ModuleMessage.ModuleMessageType.BlueInfo);
                }
            }
            catch (Exception exc)
            {
                Exceptions.ProcessModuleLoadException(this, exc);
            }

        }
        #endregion
    }
}