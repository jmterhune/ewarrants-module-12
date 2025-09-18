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
using System.Web.UI.WebControls;
using tjc.Modules.eWarrants.Components;
using DotNetNuke.Entities.Modules;
using DotNetNuke.Entities.Tabs;
using DotNetNuke.Entities.Users;
using DotNetNuke.Services.Exceptions;

namespace tjc.Modules.eWarrants
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
    /// Because the control inherits from ewarrantsSettingsBase you have access to any custom properties
    /// defined there, as well as properties from DNN such as PortalId, ModuleId, TabId, UserId and many more.
    /// </summary>
    /// -----------------------------------------------------------------------------
    public partial class Settings : ewarrantsModuleSettingsBase
    {
        #region Methods
        private void BindList()
        {
            DotNetNuke.Security.Roles.RoleController ctl = new DotNetNuke.Security.Roles.RoleController();
            var listroles = ctl.GetRoles(PortalId);
            foreach (DotNetNuke.Security.Roles.RoleInfo r in listroles)
            {
                drpJudgeRole.Items.Add(new ListItem(r.RoleName));
                drpSergeantRole.Items.Add(new ListItem(r.RoleName));
                drpJARole.Items.Add(new ListItem(r.RoleName));
                drpAdminJudge.Items.Add(new ListItem(r.RoleName));
            }
            drpJudgeRole.Items.Insert(0, new ListItem("< Select Role >", ""));
            drpSergeantRole.Items.Insert(0, new ListItem("< Select Role >", ""));
            drpJARole.Items.Insert(0, new ListItem("< Select Role >", ""));
            drpAdminJudge.Items.Insert(0, new ListItem("< Select Role >", ""));
        }
        private void BindJudges(string JudgeRole)
        {
            DotNetNuke.Security.Roles.RoleController rolectl = new DotNetNuke.Security.Roles.RoleController();
            var judges = DotNetNuke.Security.Roles.RoleController.Instance.GetUsersByRole(PortalId, JudgeRole);
            foreach (UserInfo u in judges)
            {
                drpDesotoAOJ.Items.Add(new ListItem(u.DisplayName.Replace("&nbsp;", " "), u.UserID.ToString()));
                drpManateeAOJ.Items.Add(new ListItem(u.DisplayName.Replace("&nbsp;", " "), u.UserID.ToString()));
                drpSarasotaAOJ.Items.Add(new ListItem(u.DisplayName.Replace("&nbsp;", " "), u.UserID.ToString()));
            }
            drpDesotoAOJ.Items.Insert(0, new ListItem("< Select Judge >", ""));
            drpManateeAOJ.Items.Insert(0, new ListItem("< Select Judge >", ""));
            drpSarasotaAOJ.Items.Insert(0, new ListItem("< Select Judge >", ""));
            var ctl = new JudgeTypeController();
            var judgeTypes = ctl.GetJudgeTypes(ModuleId);
            drpJudgeTypes.DataValueField = "JudgeTypeID";
            drpJudgeTypes.DataTextField = "JudgeTypeName";
            drpJudgeTypes.DataSource = ctl.GetJudgeTypes(ModuleId);
            drpJudgeTypes.DataBind();
            drpJudgeTypes.Items.Insert(0, new ListItem("< Select RPO Type >", ""));

        }
        private void BindTabs()
        {
            cboJAPage.DataSource = TabController.GetPortalTabs(PortalSettings.PortalId, PortalSettings.ActiveTab.TabID, true, "< Select Page >", true, false, false, false, true);
            cboJAPage.DataBind();
            cboContactList.DataSource = TabController.GetPortalTabs(PortalSettings.PortalId, PortalSettings.ActiveTab.TabID, true, "< Select Page >", true, false, false, false, true);
            cboContactList.DataBind();
        }
        #endregion

        #region Base Method Implementations

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// LoadSettings loads the settings from the Database and displays them
        /// </summary>
        /// -----------------------------------------------------------------------------
        public override void LoadSettings()
        {
            try
            {
                if (!Page.IsPostBack)
                {
                    BindList();
                    BindTabs();
                    if (TabModuleSettings.Contains("RpoJudgeTypeID"))
                        drpJudgeTypes.SelectedValue = Convert.ToString(TabModuleSettings["RpoJudgeTypeID"]);
                    if (TabModuleSettings.Contains("JudgeRole"))
                    {
                        drpJudgeRole.SelectedValue = Convert.ToString(TabModuleSettings["JudgeRole"]);
                        BindJudges(drpJudgeRole.SelectedValue);
                    }
                    if (TabModuleSettings.Contains("JaRole"))
                        drpJARole.SelectedValue = Convert.ToString(TabModuleSettings["JaRole"]);
                    if (TabModuleSettings.Contains("SergeantRole"))
                        drpSergeantRole.SelectedValue = Convert.ToString(TabModuleSettings["SergeantRole"]);
                    if (TabModuleSettings.Contains("JaPage"))
                        cboJAPage.SelectedValue = Convert.ToString(TabModuleSettings["JaPage"]);
                    if (TabModuleSettings.Contains("ContactPage"))
                        cboContactList.SelectedValue = Convert.ToString(TabModuleSettings["ContactPage"]);
                    if (TabModuleSettings.Contains("AdminJudge"))
                        drpAdminJudge.SelectedValue = Convert.ToString(TabModuleSettings["AdminJudge"]);
                    if (TabModuleSettings.Contains("dAOJ"))
                        drpDesotoAOJ.SelectedValue = Convert.ToString(TabModuleSettings["dAOJ"]);
                    if (TabModuleSettings.Contains("mAOJ"))
                        drpManateeAOJ.SelectedValue = Convert.ToString(TabModuleSettings["mAOJ"]);
                    if (TabModuleSettings.Contains("sAOJ"))
                        drpSarasotaAOJ.SelectedValue = Convert.ToString(TabModuleSettings["sAOJ"]);
                    if (TabModuleSettings.Contains("sAOJ"))
                        txtCompletedThreshold.Text = Convert.ToString(TabModuleSettings["sAOJ"]);
                    if (TabModuleSettings.Contains("ccRPO"))
                        txtCC.Text = Convert.ToString(TabModuleSettings["ccRPO"]);
                    if (TabModuleSettings.Contains("dRPO"))
                        txtRPODeSoto.Text = Convert.ToString(TabModuleSettings["dRPO"]);
                    if (TabModuleSettings.Contains("mRPO"))
                        txtRPOManatee.Text = Convert.ToString(TabModuleSettings["mRPO"]);
                    if (TabModuleSettings.Contains("sRPO"))
                        txtRPOSarasota.Text = Convert.ToString(TabModuleSettings["sRPO"]);
                    if (TabModuleSettings.Contains("RPODirectory"))
                        txtAttachemntDirectory.Text = Convert.ToString(TabModuleSettings["RPODirectory"]);
                    if (TabModuleSettings.Contains("DemoMode"))
                    {
                        if (bool.TryParse(TabModuleSettings["DemoMode"].ToString(), out bool demoMode))
                            chkDemoMode.Checked = demoMode;
                    }
                    if (TabModuleSettings.Contains("DeSotoClerk"))
                        txtDeSotoClerk.Text = Convert.ToString(TabModuleSettings["DeSotoClerk"]);
                    if (TabModuleSettings.Contains("ManateeClerk"))
                        txtManateeClerk.Text = Convert.ToString(TabModuleSettings["ManateeClerk"]);
                    if (TabModuleSettings.Contains("SarasotaClerk"))
                        txtSarasotaClerk.Text = Convert.ToString(TabModuleSettings["SarasotaClerk"]);
                    var ctl = new SettingController();
                    Setting objSettings = ctl.GetSettings(ModuleId);
                    if (objSettings != null)
                    {
                        txtClaimedThreshold.Text = objSettings.ClaimedThreshold.ToString();
                        txtDeleteThreshold.Text = objSettings.DeleteThreshold.ToString();
                        txtNotificationEmail.Text = objSettings.NotificationEmail;
                        txtSignedThreshold.Text = objSettings.SignedThreshold.ToString();
                        chkDemoMode.Checked = objSettings.DemoMode;
                        txtCompletedThreshold.Text = objSettings.Hours.ToString();
                        txtSenderEmail.Text = objSettings.SenderEmail;
                    }
                }
            }
            catch (Exception exc)
            {
                Exceptions.ProcessModuleLoadException(this, exc);
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// UpdateSettings saves the modified settings to the Database
        /// </summary>
        /// -----------------------------------------------------------------------------
        public override void UpdateSettings()
        {
            try
            {
                ModuleController objModules = new ModuleController();
                string JudgeRole = drpJudgeRole.SelectedValue;
                string JaRole = drpJARole.SelectedValue;
                string AdminJudge = drpAdminJudge.SelectedValue;
                string SergeantRole = drpSergeantRole.SelectedValue;
                bool DemoMode = chkDemoMode.Checked;
                string JaPage = cboJAPage.SelectedValue;
                string dAOJ = drpDesotoAOJ.SelectedValue;
                string mAOJ = drpManateeAOJ.SelectedValue;
                string sAOJ = drpSarasotaAOJ.SelectedValue;
                string ContactPage = cboContactList.SelectedValue;
                bool canConvert = long.TryParse(txtCompletedThreshold.Text, out long number1);
                int hours = canConvert ? Int32.Parse(txtCompletedThreshold.Text) : 0;

                if (!string.IsNullOrEmpty(SergeantRole.Trim()))
                    objModules.UpdateTabModuleSetting(TabModuleId, "SergeantRole", SergeantRole.Trim());
                if (!string.IsNullOrEmpty(JudgeRole.Trim()))
                    objModules.UpdateTabModuleSetting(TabModuleId, "JudgeRole", JudgeRole.Trim());
                if (!string.IsNullOrEmpty(JaRole.Trim()))
                    objModules.UpdateTabModuleSetting(TabModuleId, "JaRole", JaRole.Trim());
                if (!string.IsNullOrEmpty(AdminJudge.Trim()))
                    objModules.UpdateTabModuleSetting(TabModuleId, "AdminJudge", AdminJudge.Trim());
                if (!string.IsNullOrEmpty(JaPage.Trim()))
                    objModules.UpdateTabModuleSetting(TabModuleId, "JaPage", JaPage.Trim());
                if (!string.IsNullOrEmpty(dAOJ))
                    objModules.UpdateTabModuleSetting(TabModuleId, "dAOJ", dAOJ);
                if (!string.IsNullOrEmpty(mAOJ))
                    objModules.UpdateTabModuleSetting(TabModuleId, "mAOJ", mAOJ);
                if (!string.IsNullOrEmpty(sAOJ))
                    objModules.UpdateTabModuleSetting(TabModuleId, "sAOJ", sAOJ);
                if (!string.IsNullOrEmpty(ContactPage.Trim()))
                    objModules.UpdateTabModuleSetting(TabModuleId, "ContactPage", ContactPage.Trim());

                objModules.UpdateTabModuleSetting(TabModuleId, "ccRPO", txtCC.Text);
                objModules.UpdateTabModuleSetting(TabModuleId, "dRPO", txtRPODeSoto.Text);
                objModules.UpdateTabModuleSetting(TabModuleId, "mRPO", txtRPOManatee.Text);
                objModules.UpdateTabModuleSetting(TabModuleId, "sRPO", txtRPOSarasota.Text);
                objModules.UpdateTabModuleSetting(TabModuleId, "DeSotoClerk", txtDeSotoClerk.Text);
                objModules.UpdateTabModuleSetting(TabModuleId, "ManateeClerk", txtManateeClerk.Text);
                objModules.UpdateTabModuleSetting(TabModuleId, "SarasotaClerk", txtSarasotaClerk.Text);
                objModules.UpdateTabModuleSetting(TabModuleId, "RPODirectory", txtAttachemntDirectory.Text);
                objModules.UpdateTabModuleSetting(TabModuleId, "hours", hours.ToString());
                objModules.UpdateTabModuleSetting(TabModuleId, "RpoJudgeTypeID", drpJudgeTypes.SelectedValue);

                Int32.TryParse(txtClaimedThreshold.Text, out int claimedThreshold);
                Int32.TryParse(txtDeleteThreshold.Text, out int deleteThreshold);
                Int32.TryParse(txtSignedThreshold.Text, out int signedThreshold);
                Int32.TryParse(txtCompletedThreshold.Text, out int completedThreshold);
                var ctl = new SettingController();
                Setting objSettings = ctl.GetSettings(ModuleId);
                if (objSettings == null)
                {
                    objSettings = new Setting();
                }
                objSettings.ClaimedThreshold = claimedThreshold;
                objSettings.DeleteThreshold = deleteThreshold;
                objSettings.NotificationEmail = txtNotificationEmail.Text;
                objSettings.DemoMode = chkDemoMode.Checked;
                objSettings.Hours = completedThreshold;
                objSettings.SignedThreshold = signedThreshold;
                objSettings.SenderEmail = txtSenderEmail.Text;
                if (objSettings.ModuleID > 0)
                {
                    ctl.UpdateSettings(objSettings);
                }
                else
                {
                    objSettings.ModuleID = ModuleId;
                    ctl.CreateSetting(objSettings);
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