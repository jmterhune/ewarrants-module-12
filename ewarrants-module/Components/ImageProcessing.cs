using System;
using System.IO;
using Atalasoft.Imaging.ImageProcessing;
using Atalasoft.Imaging;
using Atalasoft.Imaging.Codec;
using Atalasoft.PdfDoc;
using Atalasoft.Imaging.Codec.Tiff;
using Atalasoft.Imaging.Codec.Pdf;
using DotNetNuke.Services.Exceptions;

namespace tjc.Modules.eWarrants.Components
{
    public static class ImageProcessing
    {
        public static void ApplyCommand(ImageCommand command, AtalaImage temp, string path, int frame)
        {
            if (command != null)
            {
                if (command.InPlaceProcessing)
                    command.Apply(temp);
                else
                    temp = command.Apply(temp).Image;

                SaveChanges(temp, path, frame);
            }
        }

        public static void SaveChanges(AtalaImage image, string path, int frame)
        {

            using (FileStream fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                // We're assuming only two file types are being used in 
                // * this application, TIF and PDF. 
                fs.Position = 0;
                var details = RegisteredDecoders.GetDecoder(fs);
                if (details != null && details.GetType() == typeof(PdfDecoder))
                {
                    EditPDF(image, fs, frame, path);
                }
                else
                {
                    EditTiff(image, fs, frame, path);
                }
            }

            System.IO.File.Delete(path);
            System.IO.File.Move(path + "_tmp", path);
        }

        public static void EditTiff(AtalaImage image, Stream fs, int frame, string path)
        {
            TiffDocument tDoc = new TiffDocument(fs, true);
            TiffPage tPage = new TiffPage(image);
            tDoc.Pages.RemoveAt(frame);
            tDoc.Pages.Insert(frame, tPage);
            tDoc.Save(path + "_tmp");
        }

        public static void EditPDF(AtalaImage image, FileStream fs, int frame, string path)
        {
            MemoryStream ms = new MemoryStream();
            image.Save(ms, new PdfEncoder(), null);
            Atalasoft.PdfDoc.PdfDocument newDoc = new PdfDocument(ms);
            PdfDocument pDoc = null;
            try
            {
                //pDoc = new PdfDocument(null, null, fs, null, new Atalasoft.PdfDoc.Repair.RepairOptions())
                //{
                //    AllowSavingOfPreviouslySignedDocuments = true
                //};
                pDoc = new PdfDocument(fs, null, null, null, new Atalasoft.PdfDoc.Repair.RepairOptions())
                {
                    AllowSavingOfPreviouslySignedDocuments = true
                };
            }
            catch (PdfException)
            {
                try
                {
                    Atalasoft.PdfDoc.Repair.RepairOptions opts = new Atalasoft.PdfDoc.Repair.RepairOptions();
                    opts.StructureOptions.ForceRebuildCrossReferenceTable = true;
                    opts.StructureOptions.RestoreOrphanedPages = false;
                    pDoc = new PdfDocument(fs, null, null, null, opts);
                }
                catch (PdfException ex2)
                {
                    Exceptions.ProcessPageLoadException(ex2);
                }
            }
            pDoc.Pages.RemoveAt(frame);
            pDoc.Pages.Insert(frame, newDoc.Pages[0]);
            pDoc.Save(path + "_tmp");
        }
        public static void AddPdfDecoder()
        {
            bool pdfRegistered = false;

            foreach (ImageDecoder decoder in RegisteredDecoders.Decoders)
            {
                var type = decoder.GetType();
                if (type.Name.ToLower() == "pdfdecoder")
                    pdfRegistered = true;
            }
            if (!pdfRegistered)
                RegisteredDecoders.Decoders.Add(new PdfDecoder() { Resolution = 125 });
        }
        public static byte[] ConvertTifToPdf(byte[] tifBytes)
        {
            RegisteredDecoders.Decoders.Add(new PdfDecoder());
            using (MemoryStream pdfStream = new MemoryStream())
            {
                PdfEncoder pdfEncoder = new PdfEncoder();
                PdfImageCollection col = new PdfImageCollection();
                using (MemoryStream tiffStream = new MemoryStream(tifBytes))
                {
                    TiffDecoder tiffDecoder = new TiffDecoder();
                    int frameCount = tiffDecoder.GetFrameCount(tiffStream);
                    for (int i = 0; i < frameCount; i++)
                    {
                        AtalaImage image = tiffDecoder.Read(tiffStream, i, null);
                        col.Add(new PdfImage(image, PdfCompressionType.Auto));
                    }
                    pdfEncoder.Save(pdfStream, col, null);
                    return pdfStream.ToArray();
                }
            }
        }
        static void PdfEnc_SetEncoderCompression(object sender, EncoderCompressionEventArgs e)
        {
            if (e.Image.PixelFormat == PixelFormat.Pixel1bppIndexed)
            {
                e.Compression = new PdfCodecCompression(PdfCompressionType.CcittGroup4);
            }
            else if (e.Image.PixelFormat == PixelFormat.Pixel8bppGrayscale || e.Image.PixelFormat == PixelFormat.Pixel24bppBgr)
            {
                e.Compression = new PdfCodecCompression(PdfCompressionType.Jpeg);
            }
            else
            {
                e.Compression = new PdfCodecCompression(PdfCompressionType.Deflate);
            }
        }

    }
}