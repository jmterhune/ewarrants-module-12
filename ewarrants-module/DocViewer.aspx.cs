using DotNetNuke.Common.Utilities;
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
using System.Web.UI;
using tjc.Modules.eWarrants.Components;
namespace tjc.Modules.eWarrants
{
    public partial class DocViewer : System.Web.UI.Page
    {
        #region members
        private int _warrantId = 0;
        private readonly UserInfo _objUser = UserController.Instance.GetCurrentUserInfo();
        private readonly Components.Encryptor _encryptor = new Components.Encryptor();
        private readonly PortalSettings _portalSettings = PortalSettings.Current;
        private Setting _warrantSettings = null;
        #endregion
        #region Properties
        public int ModuleId
        {
            get
            {
                if (Request.QueryString["ModuleId"] != null)
                    return Int32.Parse(Request.QueryString["ModuleId"]);
                return Null.NullInteger;

            }
        }
        public string NavigateUrl
        {
            get
            {
                return string.Format("/Warrants/ctl/annotations/mid/{0}", ModuleId);
            }
        }
        public string UrlReferrer
        {
            get
            {
                if (ViewState["UrlReferrer"] != null)
                    return ViewState["UrlReferrer"].ToString();
                else
                    return "";
            }
            set
            {
                ViewState["UrlReferrer"] = value;
            }
        }
        public string Resolution
        {
            get
            {
                if (ViewState["Resolution"] != null)
                    return ViewState["Resolution"].ToString();
                else
                    return "";
            }
            set
            {
                ViewState["Resolution"] = value;
            }
        }
        public int OriginalFileSize
        {
            get
            {
                if (Session["OriginalFileSize"] != null)
                    return Int32.Parse(Session["OriginalFileSize"].ToString());
                else
                    return 0;
            }
            set
            {
                Session["OriginalFileSize"] = value;
            }
        }
        public string ImageCache
        {
            get
            {
                return System.Web.Configuration.WebConfigurationManager.AppSettings["AtalasoftWebControls_Cache"];
            }
        }
        public static byte[] GetBytsForFile(string filePath)
        {
            byte[] fileByte;
            using (FileStream fileStream = System.IO.File.OpenRead(filePath))
            {
                fileByte = new byte[fileStream.Length - 1 + 1];
                fileStream.Read(fileByte, 0, (int)fileStream.Length);
            }

            return fileByte;
        }
        private string GetIp()
        {
            string clientIp = Page.Request.ServerVariables["HTTP_X_FORWARDED_FOR"];
            if (!string.IsNullOrEmpty(clientIp))
            {
                string[] forwardedIps = clientIp.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                clientIp = forwardedIps[forwardedIps.Length - 1];
            }
            else
                clientIp = Page.Request.ServerVariables["REMOTE_ADDR"];
            return clientIp;
        }
        private string AdminJudge { get; set; }
        private string SarasotaClerkEmail { get; set; }
        private string DeSotoClerkEmail { get; set; }
        private string ManateeClerkEmail { get; set; }
        private string RPO_CC_Email { get; set; }
        private string RPO_DeSotoEmail { get; set; }
        private string RPO_ManateeEmail { get; set; }
        private string RPO_SarasotaEmail { get; set; }
        private string RPO_Directory { get; set; }
        #endregion

        #region Methods
        private void InitializeProperties()
        {
            DotNetNuke.Entities.Modules.ModuleController objModuleController = new DotNetNuke.Entities.Modules.ModuleController();
            DotNetNuke.Entities.Modules.ModuleInfo objModule = objModuleController.GetModule(ModuleId);
            Hashtable tempValue = objModule.TabModuleSettings;
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

        }

        private void CancelCheckout(bool redirect)
        {
            try
            {
                var ctl = new WarrantController();
                Warrant objwarrant = ctl.GetWarrant(_warrantId);
                if (objwarrant.Status == WarrantStatus.UnderReview)
                    objwarrant.Status = WarrantStatus.New;
                ctl.UpdateWarrant(objwarrant);
                if (redirect) { Response.Redirect("/"); }
            }
            catch (Exception ex)
            {
                Exceptions.LogException(ex);
                Response.Redirect("/");
            }
        }
        #endregion

        #region Events
        protected void Page_Load(object sender, EventArgs e)
        {
            try
            {
                HttpContext.Current.Response.Cache.SetAllowResponseInBrowserHistory(false);
                HttpContext.Current.Response.Cache.SetNoStore();
                Response.Cache.SetExpires(DateTime.Now);
                Response.Cache.SetValidUntilExpires(true);
                Response.Cache.AppendCacheExtension("no-cache");
                Response.Expires = 0;
                string encryptedValue = "";
                if (Request.QueryString["enc"] != null)
                {
                    encryptedValue = Request.QueryString["enc"];
                    Int32.TryParse(_encryptor.QueryStringDecode(encryptedValue, _objUser.Username), out _warrantId);
                }
                else
                    throw new ArgumentException("The Warrant File Could not be Retrieved from the Database");
                InitializeProperties();
                var sessionid = Session.SessionID;
                // Put user code to initialize the page here
                if (!IsPostBack)
                {
                    var jCtl = new JudgeController();
                    Judge judge = jCtl.GetJudge(_objUser.UserID, ModuleId);
                    var sCtl = new SettingController();
                    _warrantSettings = sCtl.GetSettings(ModuleId);
                    hdModuleId.Value = ModuleId.ToString();
                    UrlReferrer = "/";
                    if (Request.UrlReferrer != null)
                    {
                        UrlReferrer = Request.UrlReferrer.AbsoluteUri;
                    }
                    Int32.TryParse(_encryptor.QueryStringDecode(judge.ApprovalKey, AdminJudge), out int judgeId);
                    if (_objUser.UserID != judgeId)
                        Response.Redirect(UrlReferrer);
                    if (_warrantId > 0)
                    {
                        hdWarrantId.Value = _warrantId.ToString();
                        var ctl = new WarrantController();
                        Warrant objwarrant = ctl.GetWarrant(_warrantId);
                        if (objwarrant != null)
                        {
                            if (objwarrant.WarrantType == WarrantType.RPO)
                            {
                                var aCtl = new AttachmentController();
                                attachments.Visible = true;
                                //dateNormal.Visible = false;
                                //dateRPO.Visible = true;
                                rptAttachments.DataSource = aCtl.GetAttachmentsByWarrant(_warrantId);
                                rptAttachments.DataBind();
                                ltAffidavitInfo.Visible = false;
                            }
                            else
                            {
                                string affidavitInfo = string.Format("<h5 class='affidavit-header'>Affidavit Information:</h5><ul class='affidavit'><li><strong>Name:</strong> {0}</li><li><strong>Agency:</strong> {1}</li><li><strong>ID:</strong> {2}</li></ul>", objwarrant.AdministeredOathName, objwarrant.AdministeredOathAgency, objwarrant.AdministeredOathId);
                                ltAffidavitInfo.Text = affidavitInfo;
                            }
                            ltHeader.Text = string.Format(ltHeader.Text, objwarrant.Title, objwarrant.Defendant, objwarrant.Agency.AgencyName);
                            if (objwarrant.Status == WarrantStatus.Rejected)
                            {
                                ViewContainer.Visible = false;
                                phMessage.Controls.Add(DotNetNuke.UI.Skins.Skin.GetModuleMessageControl("Notice", "The requested warrant has already been reviewed and rejected.  <a href='/'>Return to the warrants list</a> and click the refresh link to update the warrants available for signing", DotNetNuke.UI.Skins.Controls.ModuleMessage.ModuleMessageType.BlueInfo));
                            }
                            if (objwarrant.Status == WarrantStatus.Signed)
                            {
                                ViewContainer.Visible = false;
                                phMessage.Controls.Add(DotNetNuke.UI.Skins.Skin.GetModuleMessageControl("Notice", "The requested warrant has already been reviewed and signed.  <a href='/'>Return to the warrants list</a> and click the refresh link to update the warrants available for signing", DotNetNuke.UI.Skins.Controls.ModuleMessage.ModuleMessageType.BlueInfo));
                            }
                            var lCtl = new LogController();
                            Log objLog = new Log
                            {
                                UserID = _objUser.UserID,
                                EventTime = DateTime.Now,
                                EventDescription = "Viewed in Edit Mode",
                                WarrantID = objwarrant.WarrantID,
                                IP = GetIp()
                            };
                            lCtl.CreateLog(objLog);
                            ImageProcessing.AddPdfDecoder();
                            var fCtl = new FileController();
                            Components.File objwarrantFile = fCtl.GetFile(objwarrant.FileID);
                            if (objwarrantFile != null)
                            {
                                string filename = string.Format("{0}{1}.{2}", Session.SessionID.ToString(), DateTime.Now.ToString("mmddyyyyhhss"), objwarrantFile.Extension);
                                string directoryPath = Path.Combine(ImageCache, "TempSession");
                                DirectoryInfo dir = new DirectoryInfo(Server.MapPath(directoryPath));
                                if (!dir.Exists)
                                    dir.Create();
                                string fullPath = Path.Combine(dir.FullName, filename);
                                OriginalFileSize = objwarrantFile.Bytes.Length;
                                System.IO.File.WriteAllBytes(fullPath, objwarrantFile.Bytes);
                                hdFilePath.Value = string.Format("~/ImageCache/TempSession/{0}", filename);
                                hdSignature.Value = SaveSignatureFiles(judge.SignatureFileID, string.Format("{0}-{1}-s", Session.SessionID.ToString(), _objUser.UserID),"s");
                                hdInitial.Value = SaveSignatureFiles(judge.InitialFileID, string.Format("{0}-{1}-i", Session.SessionID.ToString(), _objUser.UserID),"i");
                            }
                            else
                                throw new ArgumentException("The Warrant File Could not be Retrieved from the Database");
                        }
                        else
                            throw new ArgumentException("Warrant Information Invalid");
                    }
                    else
                        throw new ArgumentException("Warrant Information Invalid");
                }
            }
            catch (Exception ex)
            {
                phMessage.Controls.Add(DotNetNuke.UI.Skins.Skin.GetModuleMessageControl("Error", "An error has occurred processing the file.<br /> Refresh the Page and try again", DotNetNuke.UI.Skins.Controls.ModuleMessage.ModuleMessageType.RedError));
                Exceptions.LogException(ex);
                CancelCheckout(false);
            }
        }
        private string SaveSignatureFiles(int id, string file,string sigType)
        {
            var ctl = new FileController();
            Components.File objWarrantImage = ctl.GetFile(id);
            byte[] imageData = _encryptor.DecryptStream(objWarrantImage.Bytes);
            string fullFilename = string.Format("{0}.{1}", file, objWarrantImage.Extension);
            string relativePath = string.Format("{0}/{1}/{2}", ImageCache, "TempSession", fullFilename);
            string physicalPath = Server.MapPath(Path.Combine(ImageCache, "TempSession", fullFilename));
            System.IO.File.WriteAllBytes(physicalPath, imageData);
            System.Drawing.Image img = System.Drawing.Image.FromFile(physicalPath);
            if (sigType == "s")
            {
                hdSignatureHeight.Value = img.Height.ToString();
                hdSignatureWidth.Value = img.Width.ToString();
            }
            else
            {
                hdInitialHeight.Value = img.Height.ToString();
                hdInitialWidth.Value = img.Width.ToString();
            }
            return relativePath;
        }
        protected void cmdReject_Click(object sender, EventArgs e)
        {
            try
            {
                var ctl = new WarrantController();
                Warrant objwarrant = ctl.GetWarrant(_warrantId);
                var sCtl = new SettingController();
                _warrantSettings = sCtl.GetSettings(ModuleId);
                string warrantTypeName = "Warrant";
                if (objwarrant != null)
                {
                    int portalId = 0;
                    if (_portalSettings != null)
                        portalId = _portalSettings.PortalId;
                    string warrantTitle = objwarrant.Title;
                    objwarrant.ReviewedByJudgeUserID = _objUser.UserID;
                    objwarrant.Status = WarrantStatus.Rejected;
                    objwarrant.ReviewedDate = DateTime.Now;
                    ctl.UpdateWarrant(objwarrant);
                    try
                    {
                        string toaddress = UserController.GetUserById(portalId, objwarrant.CreatedByUserID).Email;
                        string subject = warrantTypeName + " Submission Rejected";
                        string body = string.Format("The {0} submission [{1}] was rejected for the following reason: {2}", warrantTypeName, warrantTitle, txtReason.Text);
                        DotNetNuke.Services.Mail.Mail.SendEmail(_warrantSettings.SenderEmail, toaddress, subject, body);
                    }
                    catch (Exception ex)
                    {
                        Exceptions.LogException(ex);
                    }
                }
            }
            catch (Exception ex)
            {
                Exceptions.LogException(ex);
            }
        }
        protected void cmdCancel_Click(object sender, EventArgs e)
        {
            CancelCheckout(true);
        }
        protected void cmdSave_Click(object sender, EventArgs e)
        {
            var ctl = new WarrantController();
            Warrant objwarrant = ctl.GetWarrant(_warrantId);
            var sCtl = new SettingController();
            _warrantSettings = sCtl.GetSettings(ModuleId);
            string warrantTypeName = "Warrant";
            if (objwarrant != null)
            {
                objwarrant.Status = WarrantStatus.Signed;
                objwarrant.ReviewedByJudgeUserID = _objUser.UserID;
                string warrantTitle = objwarrant.Title;
                objwarrant.ReviewedDate = DateTime.Now;
                string physicalDocumentPath = MapPath(hdFilePath.Value);
                byte[] unencryptedBytes = GetBytsForFile(physicalDocumentPath);

                byte[] encryptedBytes = _encryptor.EncryptStream(unencryptedBytes);
                var fCtl = new FileController();
                Components.File file = fCtl.GetFile(objwarrant.FileID);
                file.Bytes = encryptedBytes;
                UserInfo leoUser = UserController.Instance.GetUserById(_portalSettings.PortalId, objwarrant.CreatedByUserID);
                ctl.UpdateWarrant(objwarrant);
                fCtl.UpdateFile(file);
                try
                {
                    FileInfo oldImage = new FileInfo(physicalDocumentPath);
                    DirectoryInfo tempSessionDirectory = new DirectoryInfo(oldImage.Directory.FullName);
                    var sessionFiles = tempSessionDirectory.GetFiles(string.Format("{0}*", Session.SessionID.ToString()), SearchOption.AllDirectories);
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
                    int portalId = 0;
                    if (_portalSettings != null)
                        portalId = _portalSettings.PortalId;
                    string toaddress = UserController.GetUserById(portalId, objwarrant.CreatedByUserID).Email;
                    string subject = warrantTypeName + " Submission Accepted";
                    string body = "The " + warrantTypeName + " submission [" + warrantTitle + "] has been signed.";
                    DotNetNuke.Services.Mail.Mail.SendEmail(_warrantSettings.SenderEmail, toaddress, subject, body);
                }
                catch (Exception ex)
                {
                    Exceptions.LogException(ex);
                }
            }
        }

        #endregion
    }
}
