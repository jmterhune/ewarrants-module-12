using tjc.Modules.eWarrants.Components;
using DotNetNuke.Entities.Users;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;

namespace tjc.Modules.eWarrants.Handlers
{
    /// <summary>
    /// Summary description for SignatureHandler
    /// </summary>
    public class SignatureHandler : IHttpHandler
    {
        private readonly Encryptor encryptor = new Encryptor();
        private readonly FileController ctl = new FileController();
        private readonly UserInfo objuser = UserController.Instance.GetCurrentUserInfo();
        public void ProcessRequest(HttpContext context)
        {
            try
            {
                if (!string.IsNullOrEmpty(context.Request.Params["actionType"]))
                {
                    var actionType = context.Request.Params["actionType"];
                    if (actionType == "delete")
                    {
                        if (!string.IsNullOrEmpty(context.Request.Params["fileId"]))
                        {
                            if(Int32.TryParse(context.Request.Params["fileId"].ToString(),out int fileId))
                            {
                                var ctl = new FileController();
                                ctl.DeleteFile(fileId);
                            }
                        }
                    }
                    else // (actionType == "insert")
                    {
                        if (context.Request.Files.Count > 0)
                        {
                            HttpFileCollection files = context.Request.Files;
                            HttpPostedFile file = files[0];
                            string filename = file.FileName;
                            int fileId = 0;
                            try
                            {
                                fileId = InsertSignatureFile(file,  objuser.UserID, true);
                            }
                            catch (Exception ex)
                            {
                                DotNetNuke.Services.Exceptions.Exceptions.LogException(ex);
                            }
                            context.Response.ContentType = "text/plain";
                            context.Response.Write(fileId.ToString());
                        }
                    }
                }
                else
                {
                    try
                    {
                        string encryptedValue = "";
                        var qs = context.Request.QueryString["enc"];
                        if (qs != null)
                            encryptedValue = qs.ToString();
                        if (string.IsNullOrEmpty(encryptedValue))
                            throw new ArgumentException("Invalid encryption string");
                        if (!Int32.TryParse(encryptor.QueryStringDecode(encryptedValue, objuser.Username), out int fileId))
                            throw new ArgumentException("Could Convert Decrypted FileID to Integer");
                        byte[] imageData = GetImage(fileId);
                        context.Response.ContentType = "image/png";
                        context.Response.BinaryWrite(imageData);
                        context.Response.Flush();
                    }
                    catch (Exception ex)
                    {
                        context.Response.End();
                        DotNetNuke.Services.Exceptions.Exceptions.LogException(ex);
                    }
                }
            }
            catch (Exception ex)
            {
                DotNetNuke.Services.Exceptions.Exceptions.LogException(ex);
            }
        }

        public bool IsReusable
        {
            get
            {
                return false;
            }
        }
        private byte[] GetImage(int id)
        {

            Components.File objWarrantImage = ctl.GetFile(id);

            byte[] imageData = encryptor.DecryptStream(objWarrantImage.Bytes);
            return imageData;
        }
        private int InsertSignatureFile(HttpPostedFile file,  int userID, bool persist)
        {
            var ctl = new FileController();
            FileType fileType=FileType.nonwarrant;
            byte[] imageData = GetImageBytes(file.InputStream);
            Components.File objFile = new Components.File { Bytes = imageData, UploadedBy = userID, Persist = persist, FileType = fileType, UploadedDate = DateTime.Now };
            ctl.CreateFile(objFile);
            return objFile.FileID;
        }

        private byte[] GetImageBytes(Stream stream)
        {
            byte[] buffer;
            Encryptor encrypt = new Encryptor();
            long filesize = stream.Length;
            buffer = new byte[stream.Length - 1 + 1];
            stream.Read(buffer, 0, (int)filesize);
            return encrypt.EncryptStream(buffer);
        }

    }
}