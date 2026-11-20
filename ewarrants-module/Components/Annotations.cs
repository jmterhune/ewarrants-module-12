using System;
using System.IO;
using Atalasoft.Annotate;
using System.Drawing;
using Atalasoft.Imaging;
using Atalasoft.Annotate.UI;
using System.Drawing.Imaging;
using System.Drawing.Drawing2D;
namespace tjc.Modules.eWarrants.Components
{
    public static class Annotations
    {
        public static AnnotationData MyHighlighter()
        {
            return new RectangleData
            {
                Name = "DefaultHighlighter",
                Outline = new AnnotationPen(Color.Transparent, 1),
                Fill = new AnnotationBrush(Color.FromArgb(128, Color.Yellow))
            };
        }

        public static AnnotationData MyFreehand()
        {
            return new FreehandData
            {
                Name = "DefaultFreehand",
                Outline = new AnnotationPen(Color.DarkBlue, 6)
            };
        }

        public static AnnotationData MyRedaction()
        {
            RectangleData redaction = new RectangleData
            {
                Name = "DefaultRedaction",
                Fill = new AnnotationBrush(Color.Black)
            };
            return redaction;
        }

        public static AnnotationData MyText(int imageWidth)
        {
            int fontsize = (int)(imageWidth * 0.017);
            return new TextData
            {
                AutoSize = false,
                Name = "DefaultTextAnnotation",
                Text = "Double-click to edit",
                Font = new AnnotationFont("Times New Roman", (float)fontsize),
                FontBrush = new AnnotationBrush(Color.Black),
                Fill = new AnnotationBrush(Color.Transparent),
                Outline = new AnnotationPen(new AnnotationBrush(Color.Transparent), 1)
            };
        }

        public static AnnotationData MyDate(int isRPO, int imageWidth)
        {
            int fontsize = (int)(imageWidth * 0.017);
            return new EmbeddedImageData(RenderDateGraphic(isRPO, (int)fontsize))
            {
                Name = "DefaultDateStamp",
                CanResize = true
            };
        }

        public static AnnotationData MyPerschedule(string imagePath, int imageWidth)
        {
            decimal widthRatio = (decimal)239 / (decimal)1600;
            decimal heightRatio = (decimal)33 / (decimal)1600;
            int newHeight = Convert.ToInt32(heightRatio * imageWidth);
            int newWidth = Convert.ToInt32(widthRatio * imageWidth);
            return new EmbeddedImageData(new AnnotationImage(imagePath))
            {
                Name = "DefaultPerscheduleStamp",
                CanResize = true,
                Size = new Size(newWidth, newHeight)
            };
        }

        public static AnnotationData MySignatureStamp(AnnotationImage signature, int imageWidth)
        {
            int width = System.Convert.ToInt32(imageWidth / (double)4);
            decimal ratio = (decimal)signature.Height / (decimal)signature.Width;
            int height = (int)(width * ratio);
            return new EmbeddedImageData(signature)
            {
                Size = new Size(width, height),
                Name = "DefaultSignature"
            };
        }

        public static AnnotationData MyInitialStamp(AnnotationImage initial, int imageWidth)
        {
            int width = System.Convert.ToInt32(imageWidth / (double)10);
            decimal ratio = (decimal)initial.Height / (decimal)initial.Width;
            int height = (int)(width * ratio);
            return new EmbeddedImageData(initial)
            {
                Size = new Size(width, height),
                CanResize = true,
                Name = "DefaultInitial"
            };
        }

        public static AnnotationData MyCheckStamp(string imagePath, int imageWidth)
        {
            decimal imageRatio = (decimal)16 / (decimal)800;
            int newWidth = System.Convert.ToInt32(imageRatio * imageWidth);
            AnnotationImage image = new AnnotationImage(imagePath);
            return new EmbeddedImageData(image)
            {
                Name = "DefaultCheckMark",
                CanResize = true,
                Size = new Size(newWidth, newWidth)
            };
        }

        private static AnnotationImage RenderDateGraphic(int isRPO, int fontsize)
        {
            string ptext = DateTime.Today.ToShortDateString();
            if (isRPO == 1)
                ptext = "Loading..."; // loading
            int pSize = fontsize;
            Bitmap b = new Bitmap(1, 1);
            Graphics g = Graphics.FromImage(b);
            Font f = new Font("Times New Roman", pSize);
            int w = (int)g.MeasureString(ptext, f).Width;
            int h = (int)g.MeasureString(ptext, f).Height;
            Color c1 = Color.Black;
            Color c2 = Color.Transparent;
            // Render drawing -------------------------
            b = new Bitmap(w, h);
            g = Graphics.FromImage(b);
            g.Clear(c2);
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.SingleBitPerPixelGridFit;
            g.DrawString(ptext, f, new SolidBrush(c1), 0, 0);
            g.Flush();
            // Save in memory --------------------------
            MemoryStream m = new MemoryStream();
            b.Save(m, ImageFormat.Png);
            return new AnnotationImage(m);
        }

        public static Bitmap AddWatermark(Bitmap bitmap, string watermarkText)
        {
            int phWidth = bitmap.Width;
            int phHeight = bitmap.Height;
            Bitmap bmPhoto = new Bitmap(phWidth, phHeight, System.Drawing.Imaging.PixelFormat.Format32bppPArgb);

            bmPhoto.SetResolution(bitmap.HorizontalResolution, bitmap.VerticalResolution);

            // load the Bitmap into a Graphics object
            Graphics grPhoto = Graphics.FromImage(bmPhoto);
            grPhoto.SmoothingMode = SmoothingMode.AntiAlias;
            grPhoto.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAliasGridFit;
            grPhoto.DrawImage(bitmap, new Rectangle(0, 0, phWidth, phHeight), 0, 0, phWidth, phHeight, GraphicsUnit.Pixel);

            // -------------------------------------------------------
            // to maximize the size of the Date message
            // test multiple Font sizes to determine the largest posible
            // font we can use for the width of the signature
            // -------------------------------------------------------
            int[] sizes = new int[] { 72, 64, 56, 48, 42, 36, 30, 24, 20, 16 };

            // Leave a small margin so the date is not jammed against the edges
            float targetWidth = phWidth * 0.95f;

            Font crFont = null;
            SizeF crSize = new SizeF();

            // Loop through the defined sizes checking the length of the Date string
            // If its length in pixles is less then the image width choose this Font size.
            for (int i = 0; i <= sizes.Length-1; i++)
            {
                crFont = new Font("arial", sizes[i], FontStyle.Bold);
                // Measure the Date string in this Font
                crSize = grPhoto.MeasureString(watermarkText, crFont);

                if (crSize.Width < targetWidth)
                    break;
            }

            // Since all photographs will have varying heights, determine a
            // position 50% from the bottom of the image
            int yPixlesFromBottom = System.Convert.ToInt32(Math.Truncate(phHeight * 0.5));

            // Now that we have a point size use the Dates string height
            // to determine a y-coordinate to draw the string of the photograph
            float yPosFromBottom = ((phHeight - yPixlesFromBottom) - (crSize.Height /2));

            // Determine its x-coordinate by calculating the center of the width of the image
            float xCenterOfImg = (phWidth / 2);

            // Define the text layout by setting the text alignment to centered
            StringFormat StrFormat = new StringFormat
            {
                Alignment = StringAlignment.Center
            };

            PointF textPos = new PointF(xCenterOfImg, yPosFromBottom);

            // Alpha for the date overlay: 128 == 50% transparent. No halo/backing
            // is drawn, so the signature ink shows through the date characters.
            const int overlayAlpha = 128;

            using (SolidBrush textBrush = new SolidBrush(Color.FromArgb(overlayAlpha, 0, 0, 0)))
            {
                grPhoto.DrawString(watermarkText, crFont, textBrush, textPos, StrFormat);
            }

            bitmap = bmPhoto;

            return bitmap;
        }

        public static TextAnnotation CreateStamp(PointF location, string stampText)
        {
            Single textWidth = GetWidthOfString(stampText) + 30;
            SizeF size = new SizeF(textWidth, 30);
            PointF vertlocation = new PointF((-(textWidth / 2)) + 27, (location.Y / 2 - textWidth / 2));
            TextAnnotation stampAnn = new TextAnnotation(stampText)
            {
                Visible = true,
                AutoSize = true,
                Size = size,
                Location = vertlocation,
                Rotation = 270,

            };
            stampAnn.Font.Name = "Times New Roman";
            stampAnn.Font.Size = 20;
            stampAnn.Font.Bold = false;
            return stampAnn;
        }
        private static Single GetWidthOfString(string str)
        {
            Bitmap objBitmap = new Bitmap(500, 200);
            Graphics objGraphics = Graphics.FromImage(objBitmap);
            SizeF stringSize = objGraphics.MeasureString(str, new Font("Times New Roman", 30));

            objBitmap.Dispose();
            objGraphics.Dispose();
            return stringSize.Width;
        }
        public static void BurnAnnotations(AtalaImage image, AnnotationUICollection annoCollection, string path, int frame)
        {

            // References to Atalasoft.DotImage.WinControls.dll and
            // * System.Windows.Forms are necessary to use this method
            // * for burning annotations. 
            bool HasSignature = false;
            using (AnnotateViewer av = new AnnotateViewer())
            {
                AnnotationUICollection colTmp = new AnnotationUICollection();

                foreach (AnnotationUI itype in annoCollection)
                {
                    if (itype is EmbeddedImageAnnotation embeddedImage)
                    {
                        if (embeddedImage.Name == "DefaultSignature" | embeddedImage.Name == "DefaultInitial")
                        {
                            HasSignature = true;
                            AnnotationImage annImage = embeddedImage.Image;
                            Bitmap bmp = (Bitmap)embeddedImage.Image.ImageObject;
                            embeddedImage.Image = new AnnotationImage(AddWatermark(bmp, DateTime.Now.ToString()));
                            colTmp.Add(embeddedImage);
                        }
                        else
                            colTmp.Add(itype);
                    }
                    else
                        colTmp.Add(itype);
                }
                if (HasSignature)
                {
                    if (image.PixelFormat != Atalasoft.Imaging.PixelFormat.Pixel8bppGrayscale)
                        image = image.GetChangedPixelFormat(Atalasoft.Imaging.PixelFormat.Pixel8bppGrayscale);
                }
                av.Burn(colTmp, image);
            }

            ImageProcessing.SaveChanges(image, path, frame);
        }
    }
}