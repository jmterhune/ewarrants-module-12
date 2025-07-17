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
using DotNetNuke.Security.Roles;
using System.Web.UI.WebControls;
using System.Collections.Generic;
using System.Linq;
using DotNetNuke.Services.Mail;
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
    public partial class EditJudge : ewarrantsModuleBase
    {
        private readonly INavigationManager _navigationManager;
        public EditJudge()
        {
            _navigationManager = DependencyProvider.GetRequiredService<INavigationManager>();
        }

        public string fileHandler = "";
        private void BindJudges()
        {
            var usersInJudgeRole = RoleController.Instance.GetUsersByRole(PortalId, JudgeRoleName);
            var ctl = new JudgeController();
            var judges = ctl.GetJudges(ModuleId).Select(j => j.JudgeID);
            foreach (UserInfo u in usersInJudgeRole)
                if (!judges.Contains(u.UserID))
                {
                    drpJudge.Items.Add(new ListItem(u.DisplayName.Replace("&nbsp;", " "), u.UserID.ToString()));
                }
            drpJudge.Items.Insert(0, new ListItem("< Select Judge >", ""));
        }

        private void BindCounties(IEnumerable<County> counties, IEnumerable<JudgeType> coljudgeType)
        {
            var ctl = new CountyController();
            var jCtl = new JudgeTypeController();
            var lstCounties = ctl.GetCounties(ModuleId);
            cklCounties.DataTextField = "CountyName";
            cklCounties.DataValueField = "CountyId";
            cklCounties.DataSource = lstCounties;
            cklCounties.DataBind();
            foreach (ListItem c in cklCounties.Items)
            {
                if (counties.Select(cn => cn.CountyID.ToString()).Contains(c.Value))
                    c.Selected = true;
            }

            cklOffenseType.DataSource = jCtl.GetJudgeTypes(ModuleId);
            cklOffenseType.DataTextField = "JudgeTypeName";
            cklOffenseType.DataValueField = "JudgeTypeID";
            cklOffenseType.DataBind();

            if (coljudgeType.Count() > 0)
            {
                foreach (ListItem i in cklOffenseType.Items)
                {
                    foreach (JudgeType jtype in coljudgeType)
                    {
                        if (i.Value == jtype.JudgeTypeID.ToString())
                            i.Selected = true;
                    }
                }
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
        protected void Page_Load(System.Object sender, System.EventArgs e)
        {
            try
            {

                if (AdminJudgeRole == "")
                {
                    DotNetNuke.UI.Skins.Skin.AddModuleMessage(this, "You must define the Admin Judge through Module Settings.", DotNetNuke.UI.Skins.Controls.ModuleMessage.ModuleMessageType.RedError);
                    cmdUpdate.Visible = false;
                    return;
                }
                // The upload configuration will be available in the handler
                if (Page.IsPostBack == false)
                {
                    fileHandler = TemplateSourceDirectory + "/Handlers/SignatureHandler.ashx";
                    var imageHandler = "~" + fileHandler + "?enc=";
                    if (JudgeId > 0)
                    {
                        var ctl = new JudgeController();
                        Judge objJudge = ctl.GetJudge(JudgeId, ModuleId);
                        drpJudge.Items.Add(new ListItem(objJudge.JudgeName, objJudge.JudgeID.ToString()));
                        pnlSignatures.Visible = true;
                        drpJudge.SelectedValue = JudgeId.ToString();
                        drpJudge.Enabled = false;
                        IEnumerable<JudgeType> colJudgeTypes = ctl.GetJudgeTypesByJudge(JudgeId);
                        BindCounties(objJudge.Counties, colJudgeTypes);
                        txtDayStart.Text = objJudge.DayStart.TimeOfDay.ToString();
                        txtDayEnd.Text = objJudge.DayEnd.TimeOfDay.ToString();
                        Encryptor encypt = new Encryptor();
                        imgSignature.ImageUrl = imageHandler + encypt.QueryStringEncode(objJudge.SignatureFileID.ToString(), UserInfo.Username);
                        imgInitial.ImageUrl = imageHandler + encypt.QueryStringEncode(objJudge.InitialFileID.ToString(), UserInfo.Username);
                    }
                    else
                    {
                        BindJudges();
                        BindCounties(new List<County>(), new List<JudgeType>());
                    }
                   
                }
            }
            catch (Exception exc)
            {
                Exceptions.ProcessModuleLoadException(this, exc);
            }
        }

        /// -----------------------------------------------------------------------------
        ///         ''' <summary>
        ///         ''' cmdCancel_Click runs when the cancel button is clicked
        ///         ''' </summary>
        ///         ''' <remarks>
        ///         ''' </remarks>
        ///         ''' <history>
        ///         ''' </history>
        ///         ''' -----------------------------------------------------------------------------
        protected void cmdCancel_Click(object sender, EventArgs e)
        {
            try
            {
                var ctl = new FileController();
                string[] fileList = hdSignatureIds.Value.Split(',');

                if (fileList.Length > 0 && hdSignatureIds.Value.Length > 0)
                {
                    foreach (var valueSet in fileList)
                    {
                        int fileId = System.Convert.ToInt32(valueSet.Split(':')[1]);
                        ctl.DeleteFile(fileId);
                    }
                }
                Response.Redirect(EditUrl("JudgeSign"), true);
            }
            catch (Exception exc)
            {
                Exceptions.ProcessModuleLoadException(this, exc);
            }
        }

        /// -----------------------------------------------------------------------------
        ///         ''' <summary>
        ///         ''' cmdUpdate_Click runs when the update button is clicked
        ///         ''' </summary>
        ///         ''' <remarks>
        ///         ''' </remarks>
        ///         ''' <history>
        ///         ''' </history>
        ///         ''' -----------------------------------------------------------------------------
        protected void cmdUpdate_Click(object sender, EventArgs e)
        {
            try
            {
                var ctl = new JudgeController();
                Judge objJudge = new Judge();
                if (JudgeId > 0)
                    objJudge = ctl.GetJudge(JudgeId, ModuleId);
                if (hdSignatureIds.Value != "")
                {
                    string[] fileList = hdSignatureIds.Value.Split(',');
                    if (fileList.Length > 0)
                    {
                        foreach (var valueSet in fileList)
                        {
                            string imageType = valueSet.Split(':')[0];
                            int fileId = System.Convert.ToInt32(valueSet.Split(':')[1]);
                            switch (imageType.ToLower())
                            {
                                case "i":
                                    {
                                        objJudge.InitialFileID = fileId;
                                        break;
                                    }

                                case "s":
                                    {
                                        objJudge.SignatureFileID = fileId;
                                        break;
                                    }
                            }
                        }
                    }
                }
                if (JudgeId <= 0)
                {
                    if (txtDayStart.Text != "")
                    {
                        DateTime.TryParse(DateTime.Today.ToShortDateString() + " " + txtDayStart.Text, out DateTime day);
                        objJudge.DayStart = day;
                    }
                    if (txtDayEnd.Text != "")
                    {
                        DateTime.TryParse(DateTime.Today.ToShortDateString() + " " + txtDayEnd.Text, out DateTime day);
                        objJudge.DayEnd = day;
                    }
                    if (Int32.TryParse(drpJudge.SelectedValue, out int judgeId))
                        objJudge.JudgeID = judgeId;
                    objJudge.ModuleID = ModuleId;
                    ctl.CreateJudge(objJudge);
                    ctl.AddJudgeCounties(objJudge.JudgeID, cklCounties.Items);
                    ctl.AddJudgeJudgeTypes(objJudge.JudgeID, cklOffenseType.Items);
                    try // Send email to Chief Judges
                    {
                        var adminJudges = RoleController.Instance.GetUsersByRole(PortalId, AdminJudgeRole);
                        string fromaddress = UserInfo.Email;
                        string judgeAddress = "";
                        string subject = "";
                        string body = "";
                        foreach (var j in adminJudges)
                        {
                            judgeAddress = j.Email;
                            subject = "New Judge Added to eWarrants Application";
                            body = "Please log into the eWarrants site and access the Judge List to configure the new judge.";
                            Mail.SendEmail(fromaddress, judgeAddress, subject, body);
                        }

                        string toaddress = UserController.GetUserById(PortalId, judgeId).Email;
                        body = "The site administrator has added your account to the Judge list on the eWarrants application.  Once the request has been approved you will be able to view warrants on the eWarrants site.";
                        Mail.SendEmail(fromaddress, toaddress, subject, body);
                    }
                    catch
                    {
                        DotNetNuke.UI.Skins.Skin.AddModuleMessage(this, "The judge was successfully updated, however, there was an error sending the notification email.", DotNetNuke.UI.Skins.Controls.ModuleMessage.ModuleMessageType.RedError);
                    }
                }
                else
                {
                    if (DateTime.TryParse(DateTime.Today.ToShortDateString() + " " + txtDayStart.Text, out DateTime sday))
                        objJudge.DayStart = sday;
                    if (DateTime.TryParse(DateTime.Today.ToShortDateString() + " " + txtDayEnd.Text, out DateTime eday))
                        objJudge.DayEnd = eday;
                    ctl.UpdateJudge(objJudge);
                    ctl.AddJudgeCounties(JudgeId, cklCounties.Items);
                    ctl.AddJudgeJudgeTypes(JudgeId, cklOffenseType.Items);
                }
                // refresh cache
                DotNetNuke.Entities.Modules.ModuleController.SynchronizeModule(ModuleId);
                // Redirect back to the portal home page
                Response.Redirect(EditUrl("JudgeSign"), true);
            }
            catch (Exception exc)
            {
                Exceptions.ProcessModuleLoadException(this, exc);
            }
        }
    }
}