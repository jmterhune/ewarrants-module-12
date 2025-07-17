/*
' Copyright (c) 2019  12th Judicial Circuit Court
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
using DotNetNuke.Entities.Modules;
using System.Web.Configuration;
using System.Configuration;
using DotNetNuke.Services.Exceptions;

namespace tjc.Modules.Utilities
{
    /// -----------------------------------------------------------------------------
    /// <summary>
    /// The View class displays the content
    /// 
    /// Typically your view control would be used to display content or functionality in your module.
    /// 
    /// View may be the only control you have in your project depending on the complexity of your module
    /// 
    /// Because the control inherits from UserAccountModuleBase you have access to any custom properties
    /// defined there, as well as properties from DNN such as PortalId, ModuleId, TabId, UserId and many more.
    /// 
    /// </summary>
    /// -----------------------------------------------------------------------------
    public partial class ConfigEncryption : PortalModuleBase
    {
        private void ProtectSection(string sectionName, string provider)
        {
            System.Configuration.Configuration config = WebConfigurationManager.OpenWebConfiguration(Request.ApplicationPath);

            ConfigurationSection section = config.GetSection(sectionName);

            if (section != null && (!section.SectionInformation.IsProtected))
            {
                section.SectionInformation.ProtectSection(provider);
                config.Save();
            }
        }

        private void UnProtectSection(string sectionName)
        {
            System.Configuration.Configuration config = WebConfigurationManager.OpenWebConfiguration(Request.ApplicationPath);

            ConfigurationSection section = config.GetSection(sectionName);

            if (section != null && section.SectionInformation.IsProtected)
            {
                section.SectionInformation.UnprotectSection();
                config.Save();
            }
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
            }
            catch (Exception exc)
            {
                Exceptions.ProcessModuleLoadException(this, exc);
            }
        }

        protected void cmdProtect_Click(object sender, System.EventArgs e)
        {
            ProtectSection("appSettings", "DataProtectionConfigurationProvider");
            ProtectSection("connectionStrings", "DataProtectionConfigurationProvider");
        }

        protected void cmdUnProtect_Click(object sender, System.EventArgs e)
        {
            UnProtectSection("appSettings");
            UnProtectSection("connectionStrings");
        }
    }
}