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
    public partial class ManageAgencies : ewarrantsModuleBase
    {
        private readonly INavigationManager _navigationManager;
        public ManageAgencies()
        {
            _navigationManager = DependencyProvider.GetRequiredService<INavigationManager>();
        }

        #region Methods
        private void BindData()
        {
            var ctl = new AgencyController();
            rptAgencies.DataSource = ctl.GetAgencies(ModuleId);
            rptAgencies.DataBind();
        }

        #endregion

        #region Event Handlers

        protected void Page_Load(System.Object sender, System.EventArgs e)
        {
            try
            {
                if (!Page.IsPostBack)
                {
                    lnkCancel.NavigateUrl = _navigationManager.NavigateURL();
                    lnkAddAgency.NavigateUrl = EditUrl("editAgency");
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
        protected void rptAgencies_ItemCommand(object source, RepeaterCommandEventArgs e)
        {
            int _agencyId = System.Convert.ToInt32(e.CommandArgument);
            if (e.CommandName.ToLower() == "delete")
            {
                var ctl = new AgencyController();
                IEnumerable<AgencyUserListItem> listUsers = ctl.GetAgencyUsers(_agencyId);

                foreach (var lu in listUsers)
                {
                    UserInfo objuser = UserController.GetUserById(PortalId, lu.UserID);
                    objuser.Membership.Approved = false;
                    UserController.UpdateUser(PortalId, objuser);
                }
                ctl.DeleteAgency(_agencyId, ModuleId);
                BindData();
            }

        }

        protected void rptAgencies_ItemCreated(object sender, RepeaterItemEventArgs e)
        {
            if (e.Item.ItemType == ListItemType.Item || e.Item.ItemType == ListItemType.AlternatingItem)
            {
                RepeaterItem item = e.Item;
                var sm = ScriptManager.GetCurrent(Page);
                LinkButton cmdDelete = (LinkButton)item.FindControl("cmdDelete");
                sm.RegisterAsyncPostBackControl(cmdDelete);
            }

        }

        protected void rptAgencies_ItemDataBound(object sender, RepeaterItemEventArgs e)
        {
            if (e.Item.ItemType == ListItemType.Item || e.Item.ItemType == ListItemType.AlternatingItem)
            {
                RepeaterItem item = e.Item;
                Agency objAgency = (Agency)e.Item.DataItem;
                HyperLink lnkEdit = (HyperLink)item.FindControl("lnkEdit");
                HyperLink lnkUsers = (HyperLink)item.FindControl("lnkUsers");

                lnkEdit.NavigateUrl = EditUrl("aid", objAgency.AgencyID.ToString(), "editAgency");
                lnkUsers.NavigateUrl = EditUrl("aid", objAgency.AgencyID.ToString(), "adminusers");
            }

        }

        #endregion

        protected void cmdEncrptFiles_Click(object sender, EventArgs e)
        {
            var ctl = new FileController();
            var encryptor = new tjc.Modules.eWarrants.Components.Encryptor();
            IEnumerable<File> files = ctl.GetFiles();
            foreach (File file in files)
            {
                ctl.UpdateFileBytes(file.FileID, encryptor.EncryptStream(file.OldBytes));
            }
        }
    }
}