using System;
using tjc.Modules.eWarrants.Components;
using DotNetNuke.Entities.Tabs;

namespace tjc.Modules.eWarrants.controls
{
    public partial class ToolBar : ViewStateBase
    {
        #region Properties
        public string JudgeRoleName { get; set; }
        public string AdminJudge { get; set; }
        public string SergeantRoleName { get; set; }
        public bool IsJudge { get; set; }
        public bool IsSiteAdmin { get; set; }
        public bool IsAdminJudge { get; set; }
        public bool HasAgencyUser { get; set; }
        public bool AgencyUserIsAdmin { get; set; }
        public string NavJudgeSign { get; set; }
        public string NavAdminAgency { get; set; }
        public string NavSms { get; set; }
        public string NavClerkFTP { get; set; }
        public string NavAdminCounty { get; set; }
        public string NavManageUsers { get; set; }
        public string NavAddWarrant { get; set; }
        public string NavAddRPO { get; set; }
        public string NavAnnotations { get; set; }
        public string NavStatus { get; set; }
        public string NavOutofOffice { get; set; }
        public string HiddenMenu { get; set; }
        public string ContactListUrl { get; set; }
        #endregion

        //------------------Event Handlers--------------------
        #region Event  Handlers
        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);
            if (Settings.Contains("AdminJudge"))
                AdminJudge = Settings["AdminJudge"].ToString().Trim();
        }
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                lnkSms.NavigateUrl = NavSms;
                if (JudgeRoleName == "" || AdminJudge == "" || SergeantRoleName == "")
                {
                    tbGroup.Visible = false;
                    return;
                }
                if (ContactListUrl != "")
                {
                    Int32.TryParse(ContactListUrl, out int contactTabId);
                    if (contactTabId > 0)
                    {
                        TabController ctlTab = new TabController();
                        DotNetNuke.Entities.Tabs.TabInfo tab = ctlTab.GetTab(contactTabId, PortalId, true);
                        lnkContactInfo.NavigateUrl = "/" + tab.TabPath.Trim('/');
                        lnkContactInfo.Visible = true;
                    }
                    else
                        lnkContactInfo.Visible = false;
                }
                else
                    lnkContactInfo.Visible = false;
                if (HiddenMenu != "")
                    tbGroup.Disabled = true;
                if (IsAdminJudge | IsSiteAdmin)
                {
                    tbGroup.Visible = true;
                    lnkWarrant.Visible = false;
                    lnkRPO.Visible = false;
                    lnkUsers.Visible = false;
                    lnkJudges.Visible = true;
                    lnkJudges.NavigateUrl = NavJudgeSign;
                    if (IsSiteAdmin)
                    {
                        lnkChangeStatus.Visible = true;
                        lnkAgencies.Visible = true;
                        lnkAgencies.NavigateUrl = NavAdminAgency;
                        lnkLists.Visible = true;
                        lnkLists.NavigateUrl = NavAdminCounty;
                        lnkChangeStatus.Visible = true;
                        lnkChangeStatus.NavigateUrl = NavStatus;
                    }
                }
                else if (!HasAgencyUser & !IsJudge)
                    tbGroup.Visible = false;
                else
                {
                    if (HasAgencyUser && AgencyUserIsAdmin)
                    {
                        lnkWarrant.Visible = false;
                        lnkRPO.Visible = false;
                        lnkContactInfo.Visible = false;
                        lnkUsers.Visible = true;
                        lnkUsers.NavigateUrl = NavManageUsers;
                    }

                    lnkWarrant.NavigateUrl = NavAddWarrant;
                    lnkRPO.NavigateUrl = NavAddRPO;
                }
                if (IsJudge)
                {
                    lnkWarrant.Visible = false;
                    lnkSms.Visible = true;
                    lnkRPO.Visible = false;
                    lnkUsers.Visible = false;
                    lnkOutofOffice.Visible = true;
                    lnkCustomText.Visible = true;
                    lnkOutofOffice.NavigateUrl = NavOutofOffice;
                    lnkCustomText.NavigateUrl = NavAnnotations;
                }
            }
        }

        #endregion
    }
}