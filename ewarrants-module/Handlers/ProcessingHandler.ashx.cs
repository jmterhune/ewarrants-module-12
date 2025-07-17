using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Users;
using DotNetNuke.Services.Exceptions;
using DotNetNuke.Services.Mail;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Script.Serialization;
using tjc.Modules.eWarrants.Components;
namespace tjc.Modules.eWarrants.Handlers
{
    public class ProcessingHandler : IHttpHandler
    {
        public void ProcessRequest(HttpContext context)
        {
            Dictionary<string, object> returnDict = new Dictionary<string, object>();
            string errors = string.Empty;
            string warrantIdString = context.Request["warrantid"];
            string action = context.Request["action"];
            string moduleIdString = context.Request["moduleid"];
            string rejectReason = context.Request["rejectReason"];            
            string userIdString = context.Request["userid"];
            string savePath = context.Request["savepath"];
            string fileName = context.Request["document"];
            string tempPath = context.Request["temppath"];
            string RPO_DeSotoEmail = string.Empty;
            string RPO_ManateeEmail = string.Empty;
            string RPO_Directory = string.Empty;
            string RPO_SarasotaEmail = string.Empty;
            string RPO_CC_Email = string.Empty;
            string DeSotoClerkEmail = string.Empty;
            string ManateeClerkEmail = string.Empty;
            string SarasotaClerkEmail = string.Empty;
            string AdminJudge = string.Empty;
            string warrantTitle = string.Empty;
            string leoEmail = string.Empty;
            string warrantTypeName = "Warrant";
            try
            {
                var ctl = new WarrantController();
                PortalSettings portalSettings = PortalSettings.Current;
                Int32.TryParse(warrantIdString, out int warrantId);
                Int32.TryParse(moduleIdString, out int moduleId);
                Int32.TryParse(userIdString, out int userId);
                Warrant objwarrant = ctl.GetWarrant(warrantId);
                var sCtl = new SettingController();
                Components.Setting warrantSettings = sCtl.GetSettings(moduleId);
                if (objwarrant != null)
                {
                    DotNetNuke.Entities.Modules.ModuleController objModuleController = new DotNetNuke.Entities.Modules.ModuleController();
                    DotNetNuke.Entities.Modules.ModuleInfo objModule = objModuleController.GetModule(moduleId);
                    Hashtable tempValue = objModule.TabModuleSettings;
                    if (tempValue["AdminJudge"] != null)
                        AdminJudge = tempValue["AdminJudge"].ToString();
                    if (tempValue["dRPO"] != null)
                        RPO_DeSotoEmail = tempValue["dRPO"].ToString();
                    if (tempValue["RPODirectory"] != null)
                        RPO_Directory = tempValue["RPODirectory"].ToString();
                    if (tempValue["mRPO"] != null)
                        RPO_ManateeEmail = tempValue["mRPO"].ToString();
                    if (tempValue["sRPO"] != null)
                        RPO_SarasotaEmail = tempValue["sRPO"].ToString();
                    if (tempValue["ccRPO"] != null)
                        RPO_CC_Email = tempValue["ccRPO"].ToString();
                    if (tempValue["DeSotoClerk"] != null)
                        DeSotoClerkEmail = tempValue["DeSotoClerk"].ToString();
                    if (tempValue["ManateeClerk"] != null)
                        ManateeClerkEmail = tempValue["ManateeClerk"].ToString();
                    if (tempValue["SarasotaClerk"] != null)
                        SarasotaClerkEmail = tempValue["SarasotaClerk"].ToString();
                    if (action == "approve")
                    {
                        try
                        {
                            objwarrant.Status = WarrantStatus.Signed;
                            objwarrant.ReviewedByJudgeUserID = userId;
                            warrantTitle = objwarrant.Title;
                            objwarrant.ReviewedDate = DateTime.Now;
                            UserInfo leoUser = UserController.Instance.GetUserById(portalSettings.PortalId, objwarrant.CreatedByUserID);
                            if (leoUser != null)
                                leoEmail = leoUser.Email;
                            string physicalDocumentPath = Path.Combine(context.Server.MapPath(savePath), Path.GetFileName(fileName));
                            var fCtl = new FileController();
                            Components.File file = fCtl.GetFile(objwarrant.FileID);
                            byte[] burnedBytes = System.IO.File.ReadAllBytes(physicalDocumentPath);
                            byte[] pdfBytes = ImageProcessing.ConvertTifToPdf(burnedBytes);
                            file.FileType = FileType.pdf;
                            file.Bytes = pdfBytes;
                            fCtl.UpdateFile(file);
                            if (objwarrant.WarrantType == WarrantType.RPO)
                            {
                                try
                                {
                                    warrantTypeName = "RPO";
                                    string attachmentPath = @"C:\Websites\eWarrantAttachments\";
                                    if (RPO_Directory != null)
                                    {
                                        attachmentPath = RPO_Directory;
                                        if (!attachmentPath.EndsWith(@"\"))
                                            attachmentPath += @"\";
                                    }
                                    string attchmentfileName = attachmentPath + objwarrant.WarrantID.ToString() + "_rpo.pdf";
                                    using (Stream fileStream = PageManipulation.GetPDFAttachment(objwarrant.Title, leoUser.DisplayName, file.UploadedDate, DateTime.Now, file.Bytes))
                                    {
                                        using (FileStream fs = new FileStream(attchmentfileName, FileMode.Create))
                                        {
                                            fileStream.CopyTo(fs);
                                            fileStream.Close();
                                            fs.Close();
                                        }
                                    }
                                    SendRPOHearingNotifications(objwarrant, fileName, RPO_DeSotoEmail, RPO_ManateeEmail, RPO_SarasotaEmail, RPO_CC_Email);
                                }
                                catch (Exception exc)
                                {
                                    string message = "Failed to send RPO Notifications for Record ID: " + warrantId.ToString();
                                    Mail.SendEmail("webhelp@jud12.flcourts.org", "webhelp@jud12.flcourts.org", "RPO Notification Error", message);
                                    Exceptions.LogException(exc);
                                }
                            }
                            else
                            {
                                switch (objwarrant.WarrantType)
                                {
                                    case WarrantType.Search:
                                        warrantTypeName = "Search Warrant";
                                        break;
                                    case WarrantType.Arrest:
                                        warrantTypeName = "Arrest Warrant";
                                        break;
                                    case WarrantType.PickupOrder:
                                        warrantTypeName = "Juvenile Pickup Order";
                                        break;
                                    case WarrantType.RPO:
                                        warrantTypeName = "Risk Protection Order";
                                        break;
                                    case WarrantType.CBS:
                                        warrantTypeName = "Compel Blood Screening";
                                        break;
                                    case WarrantType.CivilForfeiture:
                                        warrantTypeName = "Civil Forfeiture";
                                        break;
                                    default:
                                        break;
                                }
                            }
                        }
                        catch (Exception exc)
                        {
                            Exceptions.LogException(exc);
                            errors = string.Format("Error: {0} Stack Trace:{1}", exc.Message, exc.StackTrace.ToString());
                        }
                    }
                    else if (action == "reject")
                    {
                        try
                        {
                            warrantTitle = objwarrant.Title;
                            objwarrant.ReviewedByJudgeUserID = userId;
                            objwarrant.Status = WarrantStatus.Rejected;
                            objwarrant.ReviewedDate = DateTime.Now;
                            ctl.UpdateWarrant(objwarrant);
                            try
                            {
                                if (objwarrant.WarrantType == WarrantType.RPO)
                                {
                                    SendRPORejectionNotifications(objwarrant, rejectReason,leoEmail, RPO_DeSotoEmail, RPO_ManateeEmail, RPO_SarasotaEmail, RPO_CC_Email);
                                }
                                string fromaddress = "webhelp@jud12.flcourts.org";
                                UserInfo leoUser = UserController.Instance.GetUserById(portalSettings.PortalId, objwarrant.CreatedByUserID);
                                UserInfo currentUser = UserController.Instance.GetUserById(portalSettings.PortalId, userId);
                                leoEmail = leoUser.Email;
                                string subject = warrantTypeName + " Submission Rejected";
                                string body = string.Format("The {0} submission [{1}] was rejected for the following reason: {2}", warrantTypeName, warrantTitle,rejectReason);
                                Mail.SendEmail(fromaddress, currentUser.Email, subject, body);
                                Mail.SendEmail(fromaddress, leoEmail, subject, body);
                            }
                            catch (Exception exc)
                            {
                                string message = string.Format("Failed to send RPO Notifications for Record ID: {0}", warrantId);
                                Mail.SendEmail("webhelp@jud12.flcourts.org", "webhelp@jud12.flcourts.org", "RPO Notification Error", message);
                                Exceptions.LogException(exc);
                            }
                        }
                        catch (Exception exc)
                        {

                            Exceptions.LogException(exc);
                            errors = string.Format("Error: {0} Stack Trace:{1}", exc.Message, exc.StackTrace.ToString());
                        }
                    }
                    if (errors.Length > 0)
                    {
                        returnDict.Clear();
                        returnDict.Add("success", "false");
                        returnDict.Add("error", errors);
                    }
                    else
                    {
                        ctl.UpdateWarrant(objwarrant);
                        returnDict.Clear();
                        returnDict.Add("success", "true");

                        try
                        {
                            int portalId = 0;
                            if (portalSettings != null)
                                portalId = portalSettings.PortalId;
                            string subject = warrantTypeName + " Submission Accepted";
                            string body = "The " + warrantTypeName + " submission [" + warrantTitle + "] has been signed.";
                            Mail.SendEmail(warrantSettings.SenderEmail, leoEmail, subject, body);
                        }
                        catch (Exception ex)
                        {
                            string message = "Failed to send RPO Notifications for Record ID: " + warrantId.ToString();
                            Mail.SendEmail("webhelp@jud12.flcourts.org", "webhelp@jud12.flcourts.org", "RPO Notification Error", message);
                            Exceptions.LogException(ex);
                        }
                        try
                        {
                            string sessionid = Path.GetFileNameWithoutExtension(tempPath);
                            DirectoryInfo tempSessionDirectory = new DirectoryInfo(context.Server.MapPath(Path.GetDirectoryName(tempPath)));
                            if (tempSessionDirectory.Exists)
                            {
                                var sessionFiles = tempSessionDirectory.GetFiles(string.Format("{0}*", sessionid), SearchOption.AllDirectories);
                                foreach (FileInfo sessionFile in sessionFiles)
                                {
                                    try
                                    {
                                        sessionFile.Delete();
                                    }
                                    catch (Exception ex)
                                    {
                                        Exceptions.LogException(ex);
                                    }
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            Exceptions.LogException(ex);
                        }
                    }
                }
                else
                {
                    returnDict.Clear();
                    returnDict.Add("success", "false");
                    returnDict.Add("error", string.Format("The {0} could not be found. Please try again.", warrantTypeName));
                }
            }
            catch (Exception ex)
            {
                returnDict.Clear();
                returnDict.Add("success", "false");
                returnDict.Add("error", ex.ToString());
            }
            context.Response.Cache.SetCacheability(HttpCacheability.NoCache);
            context.Response.Cache.SetNoStore();
            var serializer = new JavaScriptSerializer();
            context.Response.Write(serializer.Serialize(returnDict));
        }
        private void SendRPOHearingNotifications(Warrant objWarrant, string fileName, string rpoDeSotoEmail, string rpoManateeEmail, string rpoSarasota, string rpoCcEmail)
        {
            string respondent = objWarrant.Defendant;
            UserInfo leoUser = UserController.Instance.GetUserById(PortalSettings.Current.PortalId, objWarrant.CreatedByUserID);
            string leo = leoUser.DisplayName;
            string agency = objWarrant.Agency.AgencyName;
            IEnumerable<string> emailList = null;
            string[] EmailString = Array.Empty<string>();
            if (!string.IsNullOrEmpty(rpoCcEmail))
                EmailString = rpoCcEmail.Split(new[] { ";" }, StringSplitOptions.RemoveEmptyEntries);
            switch (objWarrant.CountyName.ToLower())
            {
                case "manatee":
                    {
                        emailList = EmailString.Union(rpoManateeEmail.Split(new[] { ";" }, StringSplitOptions.RemoveEmptyEntries));
                        break;
                    }
                case "desoto":
                    {
                        emailList = EmailString.Union(rpoDeSotoEmail.Split(new[] { ";" }, StringSplitOptions.RemoveEmptyEntries));
                        break;
                    }
                case "sarasota":
                    {
                        emailList = EmailString.Union(rpoSarasota.Split(new[] { ";" }, StringSplitOptions.RemoveEmptyEntries));
                        break;
                    }
            }
            List<MailAttachment> emailAttach = new List<MailAttachment>();
            MailAttachment attach = new MailAttachment(fileName);
            emailAttach.Add(attach);
            string noreplyEmail = "webhelp@jud12.flcourts.org";
            string subject = "12th Circuit Court RPO Notification";
            string body = "<p><strong>Submitted By:</strong> " + leo + " from " + agency + " </p><p><strong>Respondent: </strong>" + respondent + Environment.NewLine + "</p><p>Approved RPO Attached</p>";
            try
            {
                string toAddress = string.Join(",", emailList);
                Mail.SendEmail(noreplyEmail, noreplyEmail, toAddress.Trim(), subject, body, emailAttach);
            }
            catch (Exception exc)
            {
                Exceptions.LogException(exc);
            }
        }
        private void SendRPORejectionNotifications(Warrant objWarrant, string reasonstring, string userEmail, string rpoDeSotoEmail, string rpoManateeEmail, string rpoSarasota, string rpoCcEmail)
        {
            string respondent = objWarrant.Defendant;
            UserInfo leoUser = UserController.Instance.GetUserById(PortalSettings.Current.PortalId, objWarrant.CreatedByUserID);
            string leo = leoUser.DisplayName;
            string agency = objWarrant.Agency.AgencyName;
            IEnumerable<string> emailList = null;
            string[] EmailString = Array.Empty<string>();
            if (!string.IsNullOrEmpty(rpoCcEmail))
                EmailString = rpoCcEmail.Split(new[] { ";" }, StringSplitOptions.RemoveEmptyEntries);
            switch (objWarrant.CountyName.ToLower())
            {
                case "manatee":
                    {
                        emailList = EmailString.Union(rpoManateeEmail.Split(new[] { ";" }, StringSplitOptions.RemoveEmptyEntries));
                        break;
                    }
                case "desoto":
                    {
                        emailList = EmailString.Union(rpoDeSotoEmail.Split(new[] { ";" }, StringSplitOptions.RemoveEmptyEntries));
                        break;
                    }
                case "sarasota":
                    {
                        emailList = EmailString.Union(rpoSarasota.Split(new[] { ";" }, StringSplitOptions.RemoveEmptyEntries));
                        break;
                    }
            }
            List<MailAttachment> emailAttach = new List<MailAttachment>();
            string noreplyEmail = "webhelp@jud12.flcourts.org";
            string subject = "12th Circuit Court RPO Rejection Notification";
            string body = string.Format("<p><strong>The Proposed Order Submitted By:</strong> {0} from {1} reqarding <strong>Respondent: </strong> {2}{3}</p><p>Proposed Order was Rejected by Judge </p><p>Reason: {4}</p>", leo, agency, respondent, Environment.NewLine, reasonstring);
            try
            {
                string toAddress = string.Join(",", emailList);
                Mail.SendEmail(noreplyEmail, noreplyEmail, toAddress.Trim(), subject, body, emailAttach);
                Mail.SendEmail(noreplyEmail, noreplyEmail, userEmail, subject, body);
            }
            catch (Exception exc)
            {
                Exceptions.LogException(exc);
            }
        }

        public bool IsReusable
        {
            get
            {
                return false;
            }
        }
    }
}