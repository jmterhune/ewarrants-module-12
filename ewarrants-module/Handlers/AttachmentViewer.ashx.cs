using tjc.Modules.eWarrants.Components;
using System;
using System.Web;

namespace tjc.Modules.eWarrants.Handlers
{
    /// <summary>
    /// Summary Displays attachments in the browser
    /// </summary>
    public class AttachmentViewer : IHttpHandler
    {
        public void ProcessRequest(HttpContext context)
        {
            try
            {
                if (!Int32.TryParse(context.Request.QueryString["aid"].ToString(), out int attachmentId))
                    throw new ArgumentException("Could Convert AttachmentID Query String to Integer");
                var ctl = new AttachmentController();
                Attachment attachment = ctl.GetAttachment(attachmentId);
                context.Response.Clear();
                context.Response.ContentType = "application/pdf";
                context.Response.AddHeader("content-disposition", "inline; filename=" + attachment.Description + ".pdf");
                context.Response.TransmitFile(attachment.FileName);
                context.Response.End();
                context.Response.Flush();              
            }
            catch (Exception ex)
            {
                context.Response.Write(" <html><body><h1>Error Processing File</h2><p>An error occurred processing the requested file.</p><div style='color:red'>");
                context.Response.Write(ex.Message);
                context.Response.Write("</div></body></html>");
                context.Response.Flush();
                context.Response.End();
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
    }
}