using System;
using System.Web.UI;
using DotNetNuke.Entities.Modules;
using DotNetNuke.Common;


namespace tjc.Modules.Profile
{
    /// -----------------------------------------------------------------------------
    /// <summary>
    /// The View class displays the content
    /// 
    /// Typically your view control would be used to display content or functionality in your module.
    /// 
    /// View may be the only control you have in your project depending on the complexity of your module
    /// 
    /// Because the control inherits from ProfileModuleBase you have access to any custom properties
    /// defined there, as well as properties from DNN such as PortalId, ModuleId, TabId, UserId and many more.
    /// 
    /// </summary>
    /// -----------------------------------------------------------------------------
    public partial class UserProfile : ProfileModuleBase
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            Page.Response.Redirect(Globals.NavigateURL(ModuleContext.PortalSettings.ActiveTab.TabID, "Profile", "userId=" + UserId, "pageno=1"));
        }
    }
}