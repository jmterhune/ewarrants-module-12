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

using DotNetNuke.Entities.Users;
using DotNetNuke.Services.Exceptions;
using System;
using System.Linq;
using System.Reflection;
using System.Web.UI;
using System.Web.UI.WebControls;
using tjc.Modules.eWarrants.Components;

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
    public partial class ManageUsers : ewarrantsModuleBase
    {
        private Agency objAgency = null/* TODO Change to default(_) if this is not a reference type */;
        public string ModuleTitle;
        public string GetImage(bool isadmin)
        {
            if (isadmin)
                return "~/images/checked.gif";
            else
                return "~/images/unchecked.gif";
        }

        private void BindData()
        {
            var ctl = new AgencyController();
            rptUsers.DataSource = ctl.GetAgencyUsers(AgencyId);
            rptUsers.DataBind();
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
                if (!Page.IsPostBack)
                {
                    lnkCancel.NavigateUrl = EditUrl("aid", AgencyId.ToString(), "adminagency");
                    lnkAddUser.NavigateUrl = EditUrl("aid", AgencyId.ToString(), "adduser");
                    var ctl = new AgencyController();
                    objAgency = ctl.GetAgency(AgencyId, ModuleId);
                    if (objAgency != null)
                        ModuleTitle = "Manage users for <strong class=\"text-primary\">" + objAgency.AgencyName + "</strong>";
                    BindData();
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

        protected void btnExistingUser_Click(object sender, EventArgs e)
        {
            Response.Redirect(EditUrl("aid", AgencyId.ToString(), "existuser"));
        }

        protected void rptUsers_ItemCommand(object source, RepeaterCommandEventArgs e)
        {
            int _userId = System.Convert.ToInt32(e.CommandArgument);
            if (e.CommandName.ToLower() == "setadmin")
            {
                var ctl = new AgencyController();
                AgencyUser objAgencyUser = ctl.GetUserAgencyXref(_userId, AgencyId);
                if (objAgencyUser.IsAdmin)
                    objAgencyUser.IsAdmin = false;
                else
                    objAgencyUser.IsAdmin = true;
                ctl.UpdateAgencyUserAdmin(objAgencyUser);
                BindData();
            }

            if (e.CommandName.ToLower() == "delete")
            {
                UserInfo objUser = UserController.GetUserById(PortalId, _userId);
                var ctl = new AgencyController();
                ctl.DeleteAgencyUser(_userId, AgencyId);
                if (ctl.GetUserAgencies(_userId, ModuleId).Count() <= 0)
                {
                    objUser.Membership.Approved = false;
                    UserController.UpdateUser(PortalId, objUser, false);
                }
                BindData();
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
            }

        }
    }
}