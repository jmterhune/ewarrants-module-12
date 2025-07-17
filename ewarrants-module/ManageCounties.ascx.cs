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
using tjc.Modules.eWarrants.Components;
using DotNetNuke.Services.Exceptions;
using System.Web.UI.WebControls;
using System.Linq;
using System.Reflection;
using System.Web.UI;
using DotNetNuke.Abstractions;
using Microsoft.Extensions.DependencyInjection;

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
    public partial class ManageCounties : ewarrantsModuleBase
    {
        private readonly INavigationManager _navigationManager;
        public ManageCounties()
        {
            _navigationManager = DependencyProvider.GetRequiredService<INavigationManager>();

        }
        private void BindCounties()
        {
            var ctl = new CountyController();
            rptCounties.DataSource = ctl.GetCounties(ModuleId);
            rptCounties.DataBind();
        }
        private void BindJudgeTypes()
        {
            var ctl = new JudgeTypeController();
            rptTypes.DataSource = ctl.GetJudgeTypes(ModuleId);
            rptTypes.DataBind();
        }
        private string BuildMessage(string message, string type, string icon)
        {
            string container = "<div class='alert alert-{0}'><em class='fa fa-{1}'></em>&nbsp;{2}</div>";
            return string.Format(container, type, icon, message);
        }
        private void Page_Load(System.Object sender, System.EventArgs e)
        {
            try
            {
                if (!IsPostBack)
                {
                    lnkCancel.NavigateUrl = _navigationManager.NavigateURL();
                    lnkCancelCounty.NavigateUrl = EditUrl("admincounty");
                    lnkCancelDivisionType.NavigateUrl = EditUrl("admincounty");
                    BindCounties();
                    BindJudgeTypes();
                }
                ltMessage.Text = "";
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
        protected void cmdSaveCounty_Click(object sender, EventArgs e)
        {
            try
            {
                var ctl = new CountyController();
                County objCounty = new County()
                {
                    CountyName = txtCounty.Text.Trim(),
                    ModuleID = ModuleId
                };

                if (hdCountyId.Value != "")
                {
                    objCounty.CountyID = Convert.ToInt32(hdCountyId.Value);
                    ctl.UpdateCounty(objCounty);
                    hdCountyId.Value = "";
                    cmdSaveCounty.Text = "Add County";
                    lnkCancelCounty.Visible = false;
                }
                else
                    ctl.CreateCounty(objCounty);
                txtCounty.Text = "";
                BindCounties();
            }
            catch (Exception ex)
            {
                ltMessage.Text = BuildMessage(ex.Message, "danger", "warning");
                Exceptions.ProcessModuleLoadException(this, ex);
            }
        }
        protected void cmdAddDivisionType_Click(object sender, EventArgs e)
        {
            try
            {
                var ctl = new JudgeTypeController();
                JudgeType objJudgeType = new JudgeType
                {
                    JudgeTypeName = txtDivisionType.Text.Trim(),
                    ModuleID = ModuleId
                };
                if (!string.IsNullOrEmpty(hdDivisionTypeId.Value))
                {
                    objJudgeType.JudgeTypeID = Convert.ToInt32(hdDivisionTypeId.Value);
                    ctl.UpdateJudgeType(objJudgeType);
                    hdDivisionTypeId.Value = "";
                    cmdAddDivisionType.Text = "Add Division Type";
                    lnkCancelDivisionType.Visible = false;
                }
                else
                    ctl.CreateJudgeType(objJudgeType);
                txtDivisionType.Text = "";
                BindJudgeTypes();
            }
            catch (Exception ex)
            {
                ltMessage.Text = BuildMessage(ex.Message, "danger", "warning");
                Exceptions.ProcessModuleLoadException(this, ex);
            }
        }
        protected void rptCounties_ItemCommand(object source, RepeaterCommandEventArgs e)
        {
            var ctl = new CountyController();
            int countyId = Convert.ToInt32(e.CommandArgument);
            if (e.CommandName.ToLower() == "edit")
            {
                County objCounty = ctl.GetCounty(countyId, ModuleId);
                hdCountyId.Value = countyId.ToString();
                lnkCancelCounty.Visible = true;
                cmdSaveCounty.Text = "Update County";
                txtCounty.Text = objCounty.CountyName;
            }
            if (e.CommandName.ToLower() == "delete")
            {
                ctl.DeleteCounty(countyId, ModuleId);
                BindCounties();
            }
        }
        protected void rptCounties_ItemCreated(object sender, RepeaterItemEventArgs e)
        {
            if (e.Item.ItemType == ListItemType.Item || e.Item.ItemType == ListItemType.AlternatingItem)
            {
                RepeaterItem item = e.Item;
                var sm = ScriptManager.GetCurrent(Page);
                LinkButton cmdDelete = (LinkButton)item.FindControl("cmdDelete");
                LinkButton cmdEdit = (LinkButton)item.FindControl("cmdEdit");
                sm.RegisterAsyncPostBackControl(cmdDelete);
                sm.RegisterAsyncPostBackControl(cmdEdit);
            }
        }
        protected void rptTypes_ItemCommand(object source, RepeaterCommandEventArgs e)
        {
            var ctl = new JudgeTypeController();
            int judgeTypeId = Convert.ToInt32(e.CommandArgument);
            if (e.CommandName.ToLower() == "edit")
            {
                JudgeType objJudgeType = ctl.GetJudgeType(judgeTypeId, ModuleId);
                hdDivisionTypeId.Value = judgeTypeId.ToString();
                lnkCancelDivisionType.Visible = true;
                cmdAddDivisionType.Text = "Update Division Type";
                txtDivisionType.Text = objJudgeType.JudgeTypeName;
            }
            if (e.CommandName.ToLower() == "delete")
            {
                ctl.DeleteJudgeType(judgeTypeId, ModuleId);
                BindJudgeTypes();
            }
        }
        protected void rptTypes_ItemCreated(object sender, RepeaterItemEventArgs e)
        {
            if (e.Item.ItemType == ListItemType.Item || e.Item.ItemType == ListItemType.AlternatingItem)
            {
                RepeaterItem item = e.Item;
                var sm = ScriptManager.GetCurrent(Page);
                LinkButton cmdDelete = (LinkButton)item.FindControl("cmdDelete");
                LinkButton cmdEdit = (LinkButton)item.FindControl("cmdEdit");
                sm.RegisterAsyncPostBackControl(cmdDelete);
                sm.RegisterAsyncPostBackControl(cmdEdit);
            }
        }
    }
}