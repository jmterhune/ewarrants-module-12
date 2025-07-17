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
    public partial class ManageJudges : ewarrantsModuleBase
    {
        private readonly INavigationManager _navigationManager;
        public ManageJudges()
        {
            _navigationManager = DependencyProvider.GetRequiredService<INavigationManager>();
        }
        private bool IsApproved(string encryptedText, int judgeId)
        {
            bool approved = false;
            UserInfo judgeInfo = UserController.GetUserById(PortalId, judgeId);
            if (judgeInfo != null)
            {
                int tempJudgeId = 0;
                Encryptor encrypt = new Encryptor();
                try
                {
                    Int32.TryParse(encrypt.QueryStringDecode(encryptedText, AdminJudgeRole), out tempJudgeId);
                }
                catch
                {
                }
                if (tempJudgeId == judgeId)
                    approved = true;
            }
            return approved;
        }
        private void BindData()
        {
            var ctl = new JudgeController();
            rptJudges.DataSource = ctl.GetJudges(ModuleId);
            rptJudges.DataBind();
        }
        private void BindAdminData()
        {
            var ctl = new JudgeController();

            rptAdminJudges.DataSource = ctl.GetJudges(ModuleId);
            rptAdminJudges.DataBind();
        }
        protected void Page_Load(System.Object sender, System.EventArgs e)
        {
            try
            {
                if (Page.IsPostBack == false)
                {
                    lnkCancel.NavigateUrl = _navigationManager.NavigateURL();
                    if (IsAdminJudge)
                    {
                        rptAdminJudges.Visible = true;
                        rptJudges.Visible = false;
                        lnkAddJudge.NavigateUrl = EditUrl("EditJudge");
                        if (UserInfo.IsInRole("Administrator"))
                        {
                            lnkAddJudge.NavigateUrl = EditUrl("EditJudge");
                            lnkAddJudge.Visible = true;
                            rptAdminJudges.Visible = false;
                            rptJudges.Visible = true;
                            BindData();
                        }
                        else
                            BindAdminData();
                    }
                    else
                        // Security Redirect Not Chief Judge or Admin
                        Response.Redirect(_navigationManager.NavigateURL());
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
        protected void rptAdminJudges_ItemCreated(object sender, RepeaterItemEventArgs e)
        {
            if (e.Item.ItemType == ListItemType.Item || e.Item.ItemType == ListItemType.AlternatingItem)
            {
                RepeaterItem item = e.Item;
                var sm = ScriptManager.GetCurrent(Page);
                LinkButton cmdDelete = (LinkButton)item.FindControl("cmdDelete");
                LinkButton cmdApprove = (LinkButton)item.FindControl("cmdApprove");
                sm.RegisterAsyncPostBackControl(cmdDelete);
                sm.RegisterAsyncPostBackControl(cmdApprove);
            }
        }
        protected void rptAdminJudges_ItemDataBound(object sender, RepeaterItemEventArgs e)
        {
            if (e.Item.ItemType == ListItemType.Item || e.Item.ItemType == ListItemType.AlternatingItem)
            {
                RepeaterItem item = e.Item;
                Judge objJudge = (Judge)e.Item.DataItem;
                Label lblSignature = (Label)item.FindControl("lblSignature");
                Label lblInitial = (Label)item.FindControl("lblInitial");
                LinkButton cmdApprove = (LinkButton)item.FindControl("cmdApprove");

                cmdApprove.Text = "<em class='fa fa-square'>&nbsp;</em>";
                cmdApprove.CommandName = "approve";
                if (!string.IsNullOrEmpty(objJudge.ApprovalKey))
                {
                    if (IsApproved(objJudge.ApprovalKey, objJudge.JudgeID))
                    {
                        cmdApprove.Text = "<em class='fa fa-check-square'>&nbsp;</em>";
                        cmdApprove.CommandName = "deny";
                    }
                }
                lblSignature.Text = "<em class='fa fa-square'>&nbsp;</em>";
                lblSignature.Enabled = false;
                lblInitial.Text = "<em class='fa fa-square'>&nbsp;</em>";
                lblInitial.Enabled = false;
                if (objJudge.HasSignature)
                    lblSignature.Text = "<em class='fa fa-check-square'>&nbsp;</em>";
                if (objJudge.HasInitial)
                    lblInitial.Text = "<em class='fa fa-check-square'>&nbsp;</em>";
            }
        }
        protected void rptAdminJudges_ItemCommand(object source, RepeaterCommandEventArgs e)
        {
            int _judgeId = System.Convert.ToInt32(e.CommandArgument);

            if (e.CommandName.ToLower() == "approve")
            {
                Encryptor encrypt = new Encryptor();
                string approvalCode = encrypt.StringEncode(_judgeId.ToString(), AdminJudgeRole);
                var ctl = new JudgeController();
                ctl.UpdateJudgeApproval(_judgeId, approvalCode);
                BindAdminData();
            }

            if (e.CommandName.ToLower() == "deny")
            {
                var ctl = new JudgeController();
                ctl.UpdateJudgeApproval(_judgeId, "");
                BindAdminData();
            }

            if (e.CommandName.ToLower() == "delete")
            {
                var ctl = new JudgeController();
                ctl.DeleteJudge(_judgeId, ModuleId);
                BindAdminData();
            }
        }
        protected void rptJudges_ItemDataBound(object sender, RepeaterItemEventArgs e)
        {
            if (e.Item.ItemType == ListItemType.Item || e.Item.ItemType == ListItemType.AlternatingItem)
            {
                RepeaterItem item = e.Item;
                Judge objJudge = (Judge)e.Item.DataItem;
                HyperLink lnkEdit = (HyperLink)item.FindControl("lnkEdit");
                Label lblSignature = (Label)item.FindControl("lblSignature");
                Label lblInitial = (Label)item.FindControl("lblInitial");
                lblSignature.Text = "<em class='fa fa-square'>&nbsp;</em>";
                lblSignature.Enabled = false;
                lblInitial.Text = "<em class='fa fa-square'>&nbsp;</em>";
                lblInitial.Enabled = false;
                if (objJudge.HasSignature)
                    lblSignature.Text = "<em class='fa fa-check-square'>&nbsp;</em>";
                if (objJudge.HasInitial)
                    lblInitial.Text = "<em class='fa fa-check-square'>&nbsp;</em>";
                lnkEdit.NavigateUrl = EditUrl("jid", objJudge.JudgeID.ToString(), "EditJudge");
            }
        }
    }
}