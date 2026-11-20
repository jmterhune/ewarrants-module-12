using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities.Modules;
using DotNetNuke.Entities.Users;
using DotNetNuke.Security;
using DotNetNuke.Services.Exceptions;
using DotNetNuke.Web.Api.Internal;
using System;
using System.IO;
using System.Net;
using System.Web;
using tjc.Modules.eWarrants.Components;

namespace tjc.Modules.eWarrants.Handlers
{
    public class AttachmentHandler : IHttpHandler
    {
        private readonly UserInfo objUser = UserController.Instance.GetCurrentUserInfo();

        public void ProcessRequest(HttpContext context)
        {
            try
            {
                if (objUser == null || objUser.UserID <= 0)
                    throw new UnauthorizedAccessException("User not authenticated. You must be Law Enforcement Personnel.");

                ValidateAntiForgeryToken(context);

                if (context.Request.Files.Count > 0)
                {
                    var file = context.Request.Files[0];
                    var attachmentType = context.Request.Params["attachmentType"];
                    if (string.IsNullOrEmpty(attachmentType))
                        throw new ArgumentException("Attachment type is required.");

                    if (!Int32.TryParse(context.Request.Params["moduleId"], out int moduleId) || moduleId <= 0)
                        throw new ArgumentException("Invalid module ID.");

                    if (!Int32.TryParse(context.Request.Params["tabId"], out int tabId) || tabId <= 0)
                        throw new ArgumentException("Invalid tab ID.");

                    if (string.IsNullOrEmpty(file.FileName) || file.ContentLength == 0)
                        throw new ArgumentException("Invalid or empty file.");

                    var fileId = InsertAttachment(file, file.FileName, attachmentType, objUser.UserID, moduleId, tabId);
                    context.Response.ContentType = "text/plain";
                    context.Response.Write(fileId.ToString());
                }
                else
                {
                    var fileIdParameter = context.Request.Params["fileId"];
                    if (!string.IsNullOrEmpty(fileIdParameter))
                    {
                        if (!Int32.TryParse(fileIdParameter, out int fileId))
                            throw new ArgumentException("Invalid file ID for deletion.");

                        var ctl = new AttachmentController();
                        ctl.DeleteAttachment(fileId);
                        context.Response.ContentType = "text/plain";
                        context.Response.Write("Success");
                    }
                    else
                    {
                        throw new ArgumentException("No file or file ID provided.");
                    }
                }
            }
            catch (Exception ex)
            {
                Exceptions.LogException(ex);
                context.Response.ContentType = "text/plain";
                context.Response.StatusCode = ex is HttpException ? (int)((HttpException)ex).GetHttpCode() : (int)HttpStatusCode.BadRequest;
                context.Response.Write("Error: " + ex.Message);
            }
        }

        private int InsertAttachment(HttpPostedFile file, string filename, string attachmentType, int userID, int moduleId, int tabId)
        {
            if (file == null || string.IsNullOrEmpty(filename) || string.IsNullOrEmpty(attachmentType) || userID <= 0 || moduleId <= 0 || tabId <= 0)
                throw new ArgumentException("Invalid file, filename, attachment type, user ID, module ID, or tab ID.");

            var extension = Path.GetExtension(filename)?.ToLower();
            if (extension != ".pdf")
                throw new ArgumentException("Only PDF files are allowed for attachments.");

            var ctl = new AttachmentController();
            var modCtl = ModuleController.Instance.GetModule(moduleId, tabId, true);
            if (modCtl == null)
                throw new InvalidOperationException("Module not found.");

            var settings = modCtl.TabModuleSettings;
            var attachmentPath = settings.Contains("RPODirectory") ? settings["RPODirectory"].ToString() : @"C:\Sites\Attachments\";
            if (!attachmentPath.EndsWith(@"\"))
                attachmentPath += @"\";

            var attachment = new Attachment
            {
                UserID = userID,
                Description = attachmentType,
                Path = attachmentPath
            };
            ctl.CreateAttachment(attachment);
            var fileId = attachment.FileID;
            if (fileId <= 0)
                throw new InvalidOperationException("Failed to create attachment record.");

            var fname = $"{attachmentPath}{userID}_{fileId}_{attachmentType}.pdf";
            file.SaveAs(fname);
            attachment.FileName = fname;
            ctl.UpdateAttachment(attachment);
            return fileId;
        }

        public bool IsReusable => false;

        private void ValidateAntiForgeryToken(HttpContext context)
        {
            var token = context.Request.Headers["RequestVerificationToken"];
            if (string.IsNullOrEmpty(token))
                throw new HttpException((int)HttpStatusCode.Forbidden, "Missing anti-forgery token.");
            var cookieToken = context.Request.Cookies["__RequestVerificationToken"]?.Value;
            if (string.IsNullOrEmpty(cookieToken))
                throw new HttpException((int)HttpStatusCode.Forbidden, "Missing anti-forgery cookie.");

            try
            {
                AntiForgery.Instance.Validate(cookieToken, token);
            }
            catch
            {
                throw new HttpException((int)HttpStatusCode.Forbidden, "Invalid anti-forgery token.");
            }
        }
    }
}