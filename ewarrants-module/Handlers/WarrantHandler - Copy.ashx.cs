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
    /// Summary description for WarrantHandler
    /// </summary>
    public class WarrantHandler : IHttpHandler
    {
        private readonly UserInfo objuser = UserController.Instance.GetCurrentUserInfo();
        public void ProcessRequest(HttpContext context)
        {
            if (context.Request.Files.Count > 0)
            {

                HttpFileCollection files = context.Request.Files;
                HttpPostedFile file = files[0];
                string filename = file.FileName;
                int fileId = 0;
                try
                {
                    fileId = InsertFile(file, filename, objuser.UserID, false);
                }
                catch (Exception ex)
                {
                    DotNetNuke.Services.Exceptions.Exceptions.LogException(ex);
                }
                context.Response.ContentType = "text/plain";
                context.Response.Write(fileId.ToString());
            }
        }

        public bool IsReusable
        {
            get
            {
                return false;
            }
        }

        private int InsertFile(HttpPostedFile file, string filename, int userID, bool persist)
        {
            var ctl = new FileController();
            FileType fileType;
            byte[] imageData = GetImageBytes(file.InputStream);
            string extension = Path.GetExtension(filename);
            switch (extension.ToLower())
            {
                case ".pdf":
                    {
                        fileType = FileType.pdf;
                        
                        break;
                    }

                case ".tiff":
                    {
                        fileType = FileType.tiff;
                        break;
                    }

                case ".tif":
                    {
                        fileType = FileType.tiff;
                        break;
                    }
                default:
                    {
                        fileType = FileType.nonwarrant;
                        break;
                    }
            }
            if (fileType == FileType.nonwarrant)
                return 0;
            
            Components.File objFile = new Components.File { Bytes = imageData, UploadedBy = userID, Persist = persist, FileType = fileType, UploadedDate = DateTime.Now };
            ctl.CreateFile(objFile);
            return objFile.FileID;
        }

        private byte[] GetImageBytes(Stream stream)
        {
            byte[] buffer;
            long filesize = stream.Length;
            buffer = new byte[stream.Length - 1 + 1];
            stream.Read(buffer, 0, (int)filesize);
            return buffer;
        }
    }
}