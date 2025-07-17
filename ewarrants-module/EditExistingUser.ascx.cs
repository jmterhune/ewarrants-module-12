
using System;
using DotNetNuke.Entities.Users;
using tjc.Modules.eWarrants.Components;
using DotNetNuke.Services.Exceptions;

namespace tjc.Modules.eWarrants
{
    public partial class EditExistingUser : ewarrantsModuleBase
    {
        protected void Page_Load(System.Object sender, System.EventArgs e)
        {
            try
            {
                if (Page.IsPostBack == false)
                    lnkCancel.NavigateUrl = EditUrl("aid", AgencyId.ToString(), "adminusers");
            }
            catch (Exception exc)
            {
                Exceptions.ProcessModuleLoadException(this, exc);
            }
        }

        protected void btnSave_Click(object sender, EventArgs e)
        {
            UserInfo objExisting = UserController.GetUserByName(PortalId, txtExistingUser.Text);
            if (objExisting != null)
            {
                var ctl = new AgencyController();
                ctl.CreateAgencyUserXref(new AgencyUser
                {
                    AgencyID = AgencyId,
                    UserID = objExisting.UserID,
                    IsAdmin = false
                });
                txtExistingUser.Text = "";
                Response.Redirect(EditUrl("aid", AgencyId.ToString(), "adminusers"));
            }
            else
                DotNetNuke.UI.Skins.Skin.AddModuleMessage(this, "Username entered does not match an existing user in this portal.", DotNetNuke.UI.Skins.Controls.ModuleMessage.ModuleMessageType.RedError);
        }
    }
}