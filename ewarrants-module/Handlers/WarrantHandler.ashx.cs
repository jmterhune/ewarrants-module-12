using DotNetNuke.Entities.Users;
using DotNetNuke.Services.Exceptions;
using DotNetNuke.Web.Api.Internal;
using System;
using System.IO;
using System.Net;
using System.Web;
using tjc.Modules.eWarrants.Components;

namespace tjc.Modules.eWarrants.Handlers
{
    public class WarrantHandler : IHttpHandler
    {
        private readonly UserInfo objUser = UserController.Instance.GetCurrentUserInfo();

        public void ProcessRequest(HttpContext context)
        {
            try
            {
                if (objUser == null || objUser.UserID <= 0)
                    throw new UnauthorizedAccessException("User not authenticated.");

                if (context.Request.Files.Count == 0)
                    throw new ArgumentException("No file provided for upload.");

                ValidateAntiForgeryToken(context);
                var file = context.Request.Files[0];
                if (string.IsNullOrEmpty(file.FileName) || file.ContentLength == 0)
                    throw new ArgumentException("Invalid or empty file.");

                var fileId = InsertFile(file, file.FileName, objUser.UserID, false);
                context.Response.ContentType = "text/plain";
                context.Response.Write(fileId.ToString());
            }
            catch (Exception ex)
            {
                Exceptions.LogException(ex);
                context.Response.ContentType = "text/plain";
                context.Response.StatusCode = ex is HttpException ? (int)((HttpException)ex).GetHttpCode() : (int)HttpStatusCode.BadRequest;
                context.Response.Write("Error: " + ex.Message);
            }
        }

        public bool IsReusable => false;

        private int InsertFile(HttpPostedFile file, string filename, int userID, bool persist)
        {
            if (file == null || string.IsNullOrEmpty(filename) || userID <= 0)
                throw new ArgumentException("Invalid file, filename, or user ID.");

            var extension = Path.GetExtension(filename)?.ToLower();
            FileType fileType;
            switch (extension)
            {
                case ".pdf":
                    fileType = FileType.pdf;
                    break;
                case ".tiff":
                case ".tif":
                    fileType = FileType.tiff;
                    break;
                default:
                    throw new ArgumentException("Invalid file type. Only PDF or TIFF files are allowed.");
            }

            var imageData = GetImageBytes(file.InputStream);
            var objFile = new Components.File
            {
                Bytes = imageData,
                UploadedBy = userID,
                Persist = persist,
                FileType = fileType,
                UploadedDate = DateTime.Now
            };
            var ctl = new FileController();
            ctl.CreateFile(objFile);
            return objFile.FileID;
        }

        private byte[] GetImageBytes(Stream stream)
        {
            if (stream == null || stream.Length == 0)
                throw new ArgumentException("Empty file stream.");

            var buffer = new byte[stream.Length];
            stream.Read(buffer, 0, (int)stream.Length);
            return buffer;
        }

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