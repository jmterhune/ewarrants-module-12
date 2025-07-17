using DotNetNuke.Abstractions.Portals;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Users;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Contexts;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using tjc.Modules.eWarrants.Components;

namespace tjc.Modules.eWarrants
{
    public partial class WarrantReview : System.Web.UI.Page
    {
        private readonly UserInfo objuser = UserController.Instance.GetCurrentUserInfo();
        private IPortalSettings portalSettings = PortalController.Instance.GetCurrentSettings();

        public string EncodedURL
        {
            get
            {
                return Request.QueryString["enc"];
            }
        }
        protected void Page_Load(object sender, EventArgs e)
        {
            ifDocumentViewer.Attributes["src"] = "/desktopmodules/tjc.modules/ewarrants/handlers/pdfhandler.ashx?enc=" + EncodedURL;
        }

        protected void cmdComplete_Click(object sender, EventArgs e)
        {
            try
            {
                var ctl = new WarrantController();
                Helper.AddPdfDecoder();
                string encryptedValue = EncodedURL;
                if (string.IsNullOrEmpty(encryptedValue))
                    throw new ArgumentException("Invalid encryption string");
                if (objuser != null && objuser.UserID > 0)
                {
                    Encryptor encrypt = new Encryptor();
                    if (Int32.TryParse(encrypt.QueryStringDecode(encryptedValue, objuser.Username), out int fileId))
                    {
                        Warrant objwarrant = ctl.GetWarrantByFileId(fileId);
                        string warrantTitle = objwarrant.Title;
                        if (objwarrant != null)
                        {
                            var lCtl = new LogController();
                            Log objLog = new Log
                            {
                                UserID = objuser.UserID,
                                EventTime = DateTime.Now,
                                EventDescription = "Leo Accepted Warrant",
                                WarrantID = objwarrant.WarrantID,
                                IP = GetIp()
                            };
                            lCtl.CreateLog(objLog);
                            objwarrant.Status = WarrantStatus.Complete;
                            objwarrant.ReviewedByAgencyUserID = objuser.UserID;
                            objwarrant.ReviewedByAgencyDate = DateTime.Now;
                            ctl.UpdateWarrant(objwarrant);
                        }
                        else
                            throw new Exception("Unable to retreive Warrant File");
                    }
                    else
                        throw new Exception("Invalid Warrant ID Provided");
                }
                else
                    throw new Exception("User Session has Expired");
                ltMessage.Text = "<div class='alert alert-success'><i class='fa fa-thumbs-up'></i> The Warrant status has been updated to Completed and will be deleted in 24 hrs. The Document will be forwarded to the ePortal for processing by the clerk. You can safely close this window and return to the warrant list.</div>";
                ifDocumentViewer.Visible = false;
            }
            catch (Exception ex)
            {
                ltMessage.Text =string.Format("<div class='alert alert-danger'><i class='fa fa-warning'></i> {0}</div>", ex.Message);
                ifDocumentViewer.Visible = false;
                DotNetNuke.Services.Exceptions.Exceptions.LogException(ex);
            }
        }

        protected void cmdReject_Click(object sender, EventArgs e)
        {
            try
            {
                var ctl = new WarrantController();
                Helper.AddPdfDecoder();
                string encryptedValue = EncodedURL;
                if (string.IsNullOrEmpty(encryptedValue))
                    throw new ArgumentException("Invalid encryption string");
                if (objuser != null && objuser.UserID > 0)
                {
                    Encryptor encrypt = new Encryptor();
                    if (Int32.TryParse(encrypt.QueryStringDecode(encryptedValue, objuser.Username), out int fileId))
                    {
                        Warrant objwarrant = ctl.GetWarrantByFileId(fileId);
                        string warrantTitle = objwarrant.Title;
                        if (objwarrant != null)
                        {
                            var lCtl = new LogController();
                            Log objLog = new Log
                            {
                                UserID = objuser.UserID,
                                EventTime = DateTime.Now,
                                EventDescription = "Leo Rejected Warrant",
                                WarrantID = objwarrant.WarrantID,
                                IP = GetIp()
                            };
                            lCtl.CreateLog(objLog);
                            objwarrant.Status = WarrantStatus.New;
                            objwarrant.ReviewedByAgencyUserID = -1;
                            objwarrant.ReviewedByAgencyDate = null;
                            UserInfo judgeInfo = UserController.Instance.GetUserById(portalSettings.PortalId, objwarrant.ReviewedByJudgeUserID);
                                objwarrant.ReviewedByJudgeUserID = -1;
                            ctl.UpdateWarrant(objwarrant);
                            if (judgeInfo == null)
                            {
                                SendRejectionEmail(judgeInfo.Email,objwarrant.WarrantID);
                            }
                        }
                        else
                            throw new Exception("Unable to retreive Warrant File");
                    }
                    else
                        throw new Exception("Invalid Warrant ID Provided");
                }
                else
                    throw new Exception("User Session has Expired");
                ltMessage.Text = "<div class='alert alert-warning'><i class='fa fa-warning'></i> The Warrant status has been updated to New. An email has been sent to the Judge informing them that the Warrant was not signed. You can safely close this window and return to the warrant list.</div>";
                ifDocumentViewer.Visible = false;
            }
            catch (Exception ex)
            {
                ltMessage.Text = string.Format("<div class='alert alert-danger'><i class='fa fa-warning'></i> {0}</div>", ex.Message);
                ifDocumentViewer.Visible = false;
                DotNetNuke.Services.Exceptions.Exceptions.LogException(ex);
            }

        }
        private string GetIp()
        {
            string clientIp = Request.ServerVariables["HTTP_X_FORWARDED_FOR"];
            if (!string.IsNullOrEmpty(clientIp))
            {
                string[] forwardedIps = clientIp.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                clientIp = forwardedIps[forwardedIps.Length - 1];
            }
            else
                clientIp = Request.ServerVariables["REMOTE_ADDR"];
            return clientIp;
        }
        private void SendRejectionEmail(string toEmail, int warrantId)
        {
            string subject = string.Format("Warrant #{0} Received Unsigned", warrantId);
                string body = string.Format("The signature did not get applied to Warrant #{0}. The Warrant has been reset so that you can attempt signing it again.", warrantId);
                DotNetNuke.Services.Mail.Mail.SendEmail("noreply.eWarrants@jud12.flcourts.org", "webhelp@jud12.flcourts.org", toEmail, subject, body);
        }
        private bool IsJudge(int userId, int moduleId)
        {
            var ctl = new JudgeController();
            Judge objJudge = ctl.GetJudge(userId, moduleId);
            if (objJudge == null)
                return false;
            else
                return true;
        }
    }
}