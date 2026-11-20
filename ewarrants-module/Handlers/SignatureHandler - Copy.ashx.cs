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
    public class SignatureHandler : IHttpHandler
    {
        private readonly Encryptor encryptor = new Encryptor();
        private readonly FileController ctl = new FileController();
        private readonly UserInfo objUser = UserController.Instance.GetCurrentUserInfo();

        public void ProcessRequest(HttpContext context)
        {
            try
            {
                if (objUser == null || objUser.UserID <= 0)
                    throw new UnauthorizedAccessException("User not authenticated.");

                var actionType = context.Request.Params["actionType"];
                if (!string.IsNullOrEmpty(actionType))
                {
                    if (actionType == "delete")
                    {
                        var fileIdParam = context.Request.Params["fileId"];
                        if (string.IsNullOrEmpty(fileIdParam) || !Int32.TryParse(fileIdParam, out int fileId))
                            throw new ArgumentException("Invalid file ID for deletion.");

                        ctl.DeleteFile(fileId);
                        context.Response.ContentType = "text/plain";
                        context.Response.Write("Success");
                    }
                    else if (actionType == "insert")
                    {
                        if (context.Request.Files.Count == 0)
                            throw new ArgumentException("No file provided for upload.");

                        var file = context.Request.Files[0];
                        if (string.IsNullOrEmpty(file.FileName) || file.ContentLength == 0)
                            throw new ArgumentException("Invalid or empty file.");

                        var fileId = InsertSignatureFile(file, objUser.UserID, true);
                        context.Response.ContentType = "text/plain";
                        context.Response.Write(fileId.ToString());
                    }
                    else
                    {
                        throw new ArgumentException("Invalid action type.");
                    }
                }
                else
                {
                    var encryptedValue = context.Request.QueryString["enc"];
                    if (string.IsNullOrEmpty(encryptedValue))
                        throw new ArgumentException("Invalid encryption string.");

                    if (!Int32.TryParse(encryptor.QueryStringDecode(encryptedValue, objUser.Username), out int fileId))
                        throw new ArgumentException("Could not convert decrypted FileID to integer.");

                    var imageData = GetImage(fileId);
                    context.Response.ContentType = "image/png";
                    context.Response.BinaryWrite(imageData);
                    context.Response.Flush();
                }
            }
            catch (Exception ex)
            {
                Exceptions.LogException(ex);
                context.Response.ContentType = "text/plain";
                context.Response.StatusCode = ex is HttpException ? (int)((HttpException)ex).GetHttpCode() : (int)HttpStatusCode.BadRequest;
                context.Response.Write("Error: " + ex.Message);
                context.Response.End();
            }
        }

        public bool IsReusable => false;

        private byte[] GetImage(int id)
        {
            if (id <= 0)
                throw new ArgumentException("Invalid file ID.");

            var objWarrantImage = ctl.GetFile(id) ?? throw new InvalidOperationException("Signature file not found.");
            return encryptor.DecryptStream(objWarrantImage.Bytes);
        }

        private int InsertSignatureFile(HttpPostedFile file, int userID, bool persist)
        {
            if (file == null || userID <= 0)
                throw new ArgumentException("Invalid file or user ID.");

            var extension = Path.GetExtension(file.FileName)?.ToLower();
            if (extension != ".png")
                throw new ArgumentException("Only PNG files are allowed for signatures.");

            var imageData = GetImageBytes(file.InputStream);
            var objFile = new Components.File
            {
                Bytes = imageData,
                UploadedBy = userID,
                Persist = persist,
                FileType = FileType.nonwarrant,
                UploadedDate = DateTime.Now
            };
            ctl.CreateFile(objFile);
            return objFile.FileID;
        }

        private byte[] GetImageBytes(Stream stream)
        {
            if (stream == null || stream.Length == 0)
                throw new ArgumentException("Empty file stream.");

            var buffer = new byte[stream.Length];
            stream.Read(buffer, 0, (int)stream.Length);
            return encryptor.EncryptStream(buffer);
        }

        private void ValidateAntiForgeryToken(HttpContext context)
        {
            var token = context.Request.Headers["RequestVerificationToken"];
            if (string.IsNullOrEmpty(token))
                throw new HttpException((int)HttpStatusCode.Forbidden, "Missing anti-forgery token.");

            try
            {
                AntiForgery.Instance.Validate(null, token);
            }
            catch
            {
                throw new HttpException((int)HttpStatusCode.Forbidden, "Invalid anti-forgery token.");
            }
        }
    }
}