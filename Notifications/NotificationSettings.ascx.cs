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
using DotNetNuke;
using DotNetNuke.Entities.Modules;
using DotNetNuke.Services.Exceptions;
using System;
using System.Web.UI.WebControls;

namespace tjc.Modules.Notifications
{
    /// -----------------------------------------------------------------------------
    /// <summary>
    /// The Settings class manages Module Settings
    /// 
    /// Typically your settings control would be used to manage settings for your module.
    /// There are two types of settings, ModuleSettings, and TabModuleSettings.
    /// 
    /// ModuleSettings apply to all "copies" of a module on a site, no matter which page the module is on. 
    /// 
    /// TabModuleSettings apply only to the current module on the current page, if you copy that module to
    /// another page the settings are not transferred.
    /// 
    /// If you happen to save both TabModuleSettings and ModuleSettings, TabModuleSettings overrides ModuleSettings.
    /// 
    /// Below we have some examples of how to access these settings but you will need to uncomment to use.
    /// 
    /// Because the control inherits from NotificationsSettingsBase you have access to any custom properties
    /// defined there, as well as properties from DNN such as PortalId, ModuleId, TabId, UserId and many more.
    /// </summary>
    /// -----------------------------------------------------------------------------
    partial class NotificationSettings : DotNetNuke.Entities.Modules.ModuleSettingsBase
    {
        private void BindList()
        {
            DotNetNuke.Security.Roles.DNNRoleProvider ctl = new DotNetNuke.Security.Roles.DNNRoleProvider();
            var listroles = ctl.GetRoles(PortalId);
            foreach (DotNetNuke.Security.Roles.RoleInfo r in listroles)
            {
                drpJudgeRole.Items.Add(new ListItem(r.RoleName));
                drpJARole.Items.Add(new ListItem(r.RoleName));
            }
            drpJudgeRole.Items.Insert(0, new ListItem("< Select Role >", ""));
            drpJARole.Items.Insert(0, new ListItem("< Select Role >", ""));
        }



        public override void LoadSettings()
        {
            try
            {
                if (!Page.IsPostBack)
                {
                    BindList();
                    if (TabModuleSettings.Contains("JudgeRole"))
                    {
                        string JudgeRole = System.Convert.ToString(TabModuleSettings["JudgeRole"]);
                        drpJudgeRole.SelectedValue = JudgeRole;
                    }
                    if (TabModuleSettings.Contains("JARole"))
                    {
                        string JARole = System.Convert.ToString(TabModuleSettings["JARole"]);
                        drpJARole.SelectedValue = JARole;
                    }
                }
            }
            catch (Exception exc)
            {
                Exceptions.ProcessModuleLoadException(this, exc);
            }
        }

        public override void UpdateSettings()
        {
            try
            {
                DotNetNuke.Entities.Modules.ModuleController objModules = new DotNetNuke.Entities.Modules.ModuleController();
                string JudgeRole = drpJudgeRole.SelectedValue;
                string JARole = drpJARole.SelectedValue;

                if (!string.IsNullOrEmpty(JARole))
                    objModules.UpdateTabModuleSetting(TabModuleId, "JARole", JARole.Trim());
                if (!string.IsNullOrEmpty(JudgeRole))
                    objModules.UpdateTabModuleSetting(TabModuleId, "JudgeRole", JudgeRole.Trim());
            }
            catch (Exception exc)
            {
                Exceptions.ProcessModuleLoadException(this, exc);
            }
        }
    }
}