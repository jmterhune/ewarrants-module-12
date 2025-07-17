using Atalasoft.Imaging.Codec;
using Atalasoft.Imaging.Codec.Pdf;
using Atalasoft.Imaging.WebControls;
using System;
using System.Collections.Concurrent;
using System.IO;
using System.Web;

namespace tjc.Modules.eWarrants.Handlers
{
    /// <summary>
    /// Summary description for WebDocumentViewerHandler
    /// </summary>
    public class WebDocumentViewerHandler : WebDocumentRequestHandler
    {
        // This is a special storage we will be using to map client files back to file names
        private readonly ConcurrentDictionary<string, string> _renamedFilesMap = new ConcurrentDictionary<string, string>();

        static WebDocumentViewerHandler()
        {
            Atalasoft.Imaging.Codec.RegisteredDecoders.Decoders.Add(new Atalasoft.Imaging.Codec.Pdf.PdfDecoder() { Resolution = 200, RenderSettings = new RenderSettings() { AnnotationSettings = AnnotationRenderSettings.None } });
        }

        public WebDocumentViewerHandler()
        {
        }
    }
}