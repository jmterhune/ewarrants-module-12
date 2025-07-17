
using Atalasoft.Imaging.Codec;
using Atalasoft.Imaging.Codec.Tiff;
using Atalasoft.PdfDoc;
using Atalasoft.Imaging;
using Atalasoft.Imaging.Codec.Pdf;
using System.IO;
using System.Drawing;
using DotNetNuke.Entities.Users;
using DotNetNuke.Entities.Portals;
using System;

namespace tjc.Modules.eWarrants.Components
{
    public static class PageManipulation
    {
       
        public static void InsertPageAndSave(string path, int frame)
        {
            AtalaImage newImage = new AtalaImage(2550, 3263, PixelFormat.Pixel1bppIndexed)
            {
                Resolution = new Dpi(200, 200, ResolutionUnit.DotsPerInch)
            };

            using (FileStream fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                // We're assuming only two file types are being used in 
                // * this application, TIF and PDF. 
                var details = RegisteredDecoders.GetDecoder(fs);

                if (details != null && details.GetType() == typeof(PdfDecoder))
                {
                    PdfDocument pDoc = new PdfDocument(fs);
                    MemoryStream ms = new MemoryStream();
                    newImage.Save(ms, new PdfEncoder(), null);
                    PdfDocument newDoc = new PdfDocument(ms);

                    if (pDoc.Pages.Count == frame)
                        pDoc.Pages.Add(newDoc.Pages[0]);
                    else
                        pDoc.Pages.Insert(frame, newDoc.Pages[0]);
                    pDoc.Save(path + "_tmp");
                }
                else
                {
                    TiffDocument tDoc = new TiffDocument(fs);
                    if (tDoc.Pages.Count == frame)
                        tDoc.Pages.Add(new TiffPage(newImage));
                    else
                        tDoc.Pages.Insert(frame, new TiffPage(newImage));
                    tDoc.Save(path + "_tmp");
                }
            }
            System.IO.File.Delete(path);
            System.IO.File.Move(path + "_tmp", path);
        }

        public static void RemovePageAndSave(string path, int frame)
        {
            using (FileStream fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                // We're assuming only two file types are being used in 
                // * this application, TIF and PDF.  
                fs.Position = 0;
                var details = RegisteredDecoders.GetDecoder(fs);
                if (details != null && details.GetType() == typeof(PdfDecoder))
                {
                    PdfDocument pDoc = new PdfDocument(fs);
                    pDoc.Pages.RemoveAt(frame);
                    pDoc.Save(path + "_tmp");
                } 
                else
                {
                    TiffDocument tDoc = new TiffDocument(fs);
                    tDoc.Pages.RemoveAt(frame);
                    tDoc.Save(path + "_tmp");
                }
            }
            System.IO.File.Delete(path);
            System.IO.File.Move(path + "_tmp", path);
        }

        public static Stream GetPDFAttachment(string title, string createdByName, DateTime createdDate, DateTime modifiedDate, byte[] imageData)
        {
            PdfImageCollection col = new PdfImageCollection();
            int pages = RegisteredDecoders.GetImageInfo(new MemoryStream(imageData)).FrameCount;
            // Add all pages from a multipage TIFF.
            for (int i = 0; i <= pages - 1; i++)
                col.Add(new PdfImage(new MemoryStream(imageData), i, PdfCompressionType.Auto));
            // Create the PDF.
            PdfEncoder pdf = new PdfEncoder
            {
                JpegQuality = 85,
                Metadata = new PdfMetadata(title, "Circuit Court", "RPO File", "", createdByName, "", createdDate, modifiedDate),
                SizeMode = PdfPageSizeMode.FitToPage,
                PageSize = new Size(612, 792)
            };
            Stream pdfStream = new MemoryStream();
            pdf.Save(pdfStream, col, null/* TODO Change to default(_) if this is not a reference type */);
            pdfStream.Seek(0, SeekOrigin.Begin);

            return pdfStream;
        }
        public static Stream GetPDFEmailAttachment(string title, string createdByName, DateTime createdDate, DateTime modifiedDate, byte[] imageData)
        {
            PdfImageCollection col = new PdfImageCollection();
            int pages = RegisteredDecoders.GetImageInfo(new MemoryStream(imageData)).FrameCount;
            // Add all pages from a multipage TIFF.
            for (int i = 0; i <= pages - 2; i++)
                col.Add(new PdfImage(new MemoryStream(imageData), i, PdfCompressionType.Auto));
            // Create the PDF.
            PdfEncoder pdf = new PdfEncoder
            {
                JpegQuality = 85,
                Metadata = new PdfMetadata(title, "Circuit Court", "RPO File", "", createdByName, "", createdDate, modifiedDate),
                SizeMode = PdfPageSizeMode.FitToPage,
                PageSize = new Size(612, 792)
            };
            Stream pdfStream = new MemoryStream();
            pdf.Save(pdfStream, col, null/* TODO Change to default(_) if this is not a reference type */);
            pdfStream.Seek(0, SeekOrigin.Begin);

            return pdfStream;
        }
        //Thubnail Reordering
        public static void Reorder(string path, int DragIndex, int DropIndex)
        {
            using (Stream fs = new FileStream(path, FileMode.Open, FileAccess.Read))
            {
                RegisteredDecoders.Decoders.Add(new Atalasoft.Imaging.Codec.Pdf.PdfDecoder());

                ImageInfo info = RegisteredDecoders.GetImageInfo(fs);
                fs.Position = 0;

                SwapTiff(fs, path, DragIndex, DropIndex);
            }

            System.IO.File.Delete(path);
            System.IO.File.Move(path + "_tmp", path);
        }

        private static void SwapTiff(Stream fs, string path, int DragIndex, int DropIndex)
        {
            TiffDocument tDoc = new TiffDocument(fs);
            TiffPage tPage = tDoc.Pages[DragIndex];
            tDoc.Pages.RemoveAt(DragIndex);
            tDoc.Pages.Insert(DropIndex, tPage);
            tDoc.Save(path + "_tmp");
        }
    }
}