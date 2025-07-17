using System;
using System.Web;
using System.IO;
using Atalasoft.Imaging.Codec.Pdf;
using Atalasoft.Imaging.Codec;
using DotNetNuke.Entities.Users;
using tjc.Modules.eWarrants.Components;
using File = tjc.Modules.eWarrants.Components.File;
using System.Drawing;

namespace tjc.Modules.eWarrants.Handlers
{
    /// <summary>
    /// Summary used to present image as PDF
    /// </summary>
    public class PdfHandler : IHttpHandler
    {
        private readonly UserInfo objuser = UserController.Instance.GetCurrentUserInfo();
        public void ProcessRequest(HttpContext context)
        {
            try
            {
                var ctl = new WarrantController();
                Helper.AddPdfDecoder();
                string encryptedValue = "";
                var qs = context.Request.QueryString["enc"];
                if (qs != null)
                    encryptedValue = qs.ToString();
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
                                EventDescription = "Viewed PDF Read Only Mode",
                                WarrantID = objwarrant.WarrantID,
                                IP = GetIp(context)
                            };
                            lCtl.CreateLog(objLog);
                            if (objwarrant.Status == WarrantStatus.Rejected | objwarrant.Status == WarrantStatus.Signed)
                            {
                                if (!IsJudge(objuser.UserID, objwarrant.ModuleID))
                                {
                                    objwarrant.Status = WarrantStatus.Complete;
                                    objwarrant.ReviewedByAgencyUserID = objuser.UserID;
                                    objwarrant.ReviewedByAgencyDate = DateTime.Now;
                                    ctl.UpdateWarrant(objwarrant);
                                }
                            }
                        }
                        else
                            throw new Exception("Unassociated warrant file");

                        context.Response.Clear();
                        context.Response.ContentType = "application/pdf";
                        context.Response.AddHeader("content-disposition", "inline; filename=Warrant.pdf");
                        PdfImageCollection col = new PdfImageCollection();

                        // get number of pages
                        var fCtl = new FileController();
                        File objWarrantImage = fCtl.GetFile(fileId);
                        int pages = RegisteredDecoders.GetImageInfo(new MemoryStream(objWarrantImage.Bytes)).FrameCount;
                        // Add all pages from a multipage TIFF.
                        for (int i = 0; i <= pages - 1; i++)
                        {
                            col.Add(new PdfImage(new MemoryStream(objWarrantImage.Bytes), i, PdfCompressionType.Auto));
                        }
                        // Create the PDF.
                        DateTime modifiedDate = objwarrant.CreatedDate;
                        if (objwarrant.ReviewedDate.HasValue)
                            modifiedDate = objwarrant.ReviewedDate.Value;
                        PdfEncoder pdf = new PdfEncoder
                        {
                            JpegQuality = 85,

                            Metadata = new PdfMetadata(warrantTitle, "Circuit Court", "Warrant File", "", "", "", objwarrant.CreatedDate, modifiedDate),
                            // Make each image fit into an 8.5 x 11 inch page (612 x 792 @ 72 DPI).
                            SizeMode = PdfPageSizeMode.FitToPage,
                            PageSize = new Size(612, 792)
                        };
                        Stream pdfStream = new MemoryStream();
                        pdf.Save(pdfStream, col, null);
                        pdfStream.Seek(0, SeekOrigin.Begin);
                        byte[] pdfBytes = new byte[pdfStream.Length - 1 + 1];
                        pdfStream.Read(pdfBytes, 0, System.Convert.ToInt32(pdfStream.Length));
                        context.Response.BinaryWrite(pdfBytes);
                    }
                    else
                    {
                        throw new ArgumentException("Couldn't Convert Decrypted FileId to Integer");
                    }
                }
                else
                {
                    context.Response.Write("<html><body><h1>Error Processing File</h2><p>An error occurred processing the requested file.</p><div style='color:red'>");
                    context.Response.Write("<p>Your Session has expired. Please <a href='/'>return to the home page</a> and login</p>");
                    context.Response.Write("</div></body></html>");
                }
            }
            catch (Exception ex)
            {
                context.Response.Write("<html><body><h1>Error Processing File</h2><p>An error occurred processing the requested file.</p><div style='color:red'>");
                context.Response.Write(ex.Message);
                context.Response.Write("</div></body></html>");
                DotNetNuke.Services.Exceptions.Exceptions.LogException(ex);
            }

            finally
            {
                context.Response.Flush();
                context.Response.End();
            }
        }

        #region Methods
        public bool IsReusable
        {
            get
            {
                return false;
            }
        }
        private string GetIp(HttpContext context)
        {
            string clientIp = context.Request.ServerVariables["HTTP_X_FORWARDED_FOR"];
            if (!string.IsNullOrEmpty(clientIp))
            {
                string[] forwardedIps = clientIp.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                clientIp = forwardedIps[forwardedIps.Length - 1];
            }
            else
                clientIp = context.Request.ServerVariables["REMOTE_ADDR"];
            return clientIp;
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

        #endregion
    }
}