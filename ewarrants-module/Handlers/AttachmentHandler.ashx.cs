using tjc.Modules.eWarrants.Components;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities.Modules;
using DotNetNuke.Entities.Users;
using System;
using System.Collections;
using System.IO;
using System.Web;

namespace tjc.Modules.eWarrants.Handlers
{
    /// <summary>
    /// Summary description for Handler1
    /// </summary>
    public class AttachmentHandler : IHttpHandler
    {
        private readonly UserInfo objuser = UserController.Instance.GetCurrentUserInfo();
        public void ProcessRequest(HttpContext context)
        {
            if (context.Request.Files.Count > 0)
            {
                if (objuser != null && objuser.UserID > 0)
                {
                    HttpFileCollection files = context.Request.Files;
                    HttpPostedFile file = files[0];
                    string filename = file.FileName;
                    int fileId = 0;
                    string attachmentType = context.Request.Params["attachmentType"];
                    int moduleId = System.Convert.ToInt32(context.Request.Params["moduleId"]);
                    int tabId = System.Convert.ToInt32(context.Request.Params["tabId"]);
                    try
                    {
                        fileId = InsertAttachment(file, filename, attachmentType, objuser.UserID, moduleId, tabId);
                    }
                    catch (Exception ex)
                    {
                        DotNetNuke.Services.Exceptions.Exceptions.LogException(ex);
                    }
                    context.Response.ContentType = "text/plain";
                    context.Response.Write(fileId.ToString());
                }
                else
                {
                    context.Response.ContentType = "text/plain";
                    context.Response.Write("File Rejected. You must be Law Enforcement Personnel");
                }
            }
            else
            {
                string fileIdParameter = context.Request.Params["fileId"];
                if (!string.IsNullOrEmpty(fileIdParameter))
                {
                    try
                    {
                        int fileId = System.Convert.ToInt32(fileIdParameter);
                        var ctl = new AttachmentController();
                        ctl.DeleteAttachment(fileId);
                        context.Response.ContentType = "text/plain";
                        context.Response.Write("");
                    }
                    catch (Exception ex)
                    {
                        context.Response.ContentType = "text/plain";
                        context.Response.Write("Can't Delete");
                        DotNetNuke.Services.Exceptions.Exceptions.LogException(ex);
                    }
                }
            }
        }

        private int InsertAttachment(HttpPostedFile file, string filename, string attachmentType, int userID, int moduleId, int tabid)
        {
            var ctl = new AttachmentController();
            string extension = Path.GetExtension(filename);
            if (extension.ToLower() != ".pdf")
                return 0;

            ModuleInfo modCtl = ModuleController.Instance.GetModule(moduleId, tabid, true);
            Hashtable setting = modCtl.TabModuleSettings;
            string attachmentPath = @"C:\Sites\Attachments\";
            if (setting.Contains("RPODirectory"))
            {
                attachmentPath = setting["RPODirectory"].ToString();
                if (!attachmentPath.EndsWith(@"\"))
                    attachmentPath += @"\";
            }
            Attachment attachment = new Attachment { UserID = userID, Description = attachmentType, Path = attachmentPath };
            ctl.CreateAttachment(attachment);
            int fileId = attachment.FileID;
            if (fileId >= 0)
            {
                string fname = attachmentPath + userID.ToString() + "_" + fileId.ToString() + "_" + attachmentType + ".pdf";
                file.SaveAs(fname);
                attachment.FileName = fname;
                ctl.UpdateAttachment(attachment);
            }
            else
                return 0;
            return fileId;
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