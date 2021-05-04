using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using Amazon.Common.Helpers;
using Amazon.Core.Contracts;
using Amazon.Core.Entities.Enums;
using Amazon.Core.Models;
using Amazon.Core.Models.Stamps;
using iTextSharp.awt.geom;
using iTextSharp.text;
using iTextSharp.text.pdf;
using Image = iTextSharp.text.Image;

namespace Amazon.Model.Implementation.Pdf
{
    public class Pdf4x6MakerByIText : IFileMaker
    {
        private readonly iTextSharp.text.Font SearchTextFont;
        private readonly BaseFont SearchTextBaseFont;
        private readonly BaseColor TransparentColor;
        private readonly BaseColor TextColor;

        private const float LabelPaddingX = 5f;
        private const float LabelPaddingY = 5f;
        private const float ScanFormPaddingX = 0f;
        private const float ScanFormPaddingY = 0f;
        private const int FontSize = 12;
        private const int BigFontSize = 24;
        private const int BigNormalFontSize = 20;

        private ILogService _log;

        public Pdf4x6MakerByIText(ILogService log)
        {
            _log = log;

            TextColor = BaseColor.BLACK;
            TransparentColor = new BaseColor(255, 255, 255, 255);
            SearchTextFont = FontFactory.GetFont(FontFactory.HELVETICA);
            SearchTextBaseFont = SearchTextFont.GetCalculatedBaseFont(false);
            //SearchTextFont.Color = new BaseColor(255, 255, 255, 255); //Transparent
        }

        public string CreateFileWithLabels(IList<PrintLabelInfo> labels, 
            IList<string> scanFormImages, 
            BatchInfoToPrint batchInfo,
            string outputDirectory, 
            string name = null)
        {
            var docSize = new iTextSharp.text.Rectangle(288, 432);
            Document doc = new Document(docSize, 0, 0, 0, 0);

            var fileName = String.Format("label_{0}.pdf", 
                StringHelper.JoinTwo("_", 
                    (batchInfo != null && batchInfo.BatchId > 0) ? batchInfo.BatchId.ToString() : null, 
                    DateHelper.GetAppNowTime().ToString("MM_dd_yyyy_HH_mm_ss")));

            var localPath = outputDirectory + "\\LabelPdf\\" + fileName;
            Directory.CreateDirectory(Path.GetDirectoryName(localPath));

            PdfWriter writer = PdfWriter.GetInstance(doc, new FileStream(localPath, FileMode.Create));
            writer.SetPdfVersion(PdfWriter.PDF_VERSION_1_5);
            writer.CompressionLevel = PdfStream.BEST_COMPRESSION; //TODO: BEST_COMPRESSION

            labels = labels.Where(l => !String.IsNullOrEmpty(l.Image)).ToList();

            doc.Open();

            try
            {
                var index = 0;
                while (index < labels.Count)
                {
                    AddPageWithFullLabel(doc, writer, labels[index]);
                    index++;
                }
            }
            catch (Exception ex)
            {
                _log.Error("AddPage", ex);
            }
            finally
            {
                doc.Close();
            }

            return "~\\LabelPdf\\" + fileName;
        }
        
        private void AddPageWithFullLabel(Document doc, PdfWriter writer, PrintLabelInfo label)
        {
            doc.NewPage();

            var labelSize = new PointF(0, 0);
            if (String.IsNullOrEmpty(label.Image) || !File.Exists(label.Image))
            {
                
            }
            else
            {

                labelSize = AddLabelImage(doc, writer, label.Image, label.LabelSize, label, 0);
                PrintAdditionalLabelInfo(doc, writer, label, labelSize, 0);

            }
        }
        
        private PointF AddLabelImage(Document doc,
            PdfWriter writer,
            string image,
            PrintLabelSizeType labelSize,
            PrintLabelInfo labelInfo,
            int number)
        {
            var imageObj = Image.GetInstance(image);
            imageObj.CompressionLevel = PdfStream.BEST_COMPRESSION;

            if (imageObj.Height < imageObj.Width)
            {
                if (labelInfo.RotationAngle.HasValue)
                    imageObj.Rotation = (float)(labelInfo.RotationAngle.Value/(float)360*(2*Math.PI));
                //if (labelInfo.ShippingMethodId == ShippingUtils.IBCCEPePocketMethodId)
                //    imageObj.Rotation = -(float) Math.PI/2;
                else
                    imageObj.Rotation = -(float) Math.PI/2;
                imageObj.Rotate();
            }

            //image1.SetImageQuality(80); //before 50%

            var width = doc.PageSize.Width - 2*LabelPaddingX;
            var height = doc.PageSize.Height / 2 - 2 * LabelPaddingY;
            if (labelSize == PrintLabelSizeType.FullPage)
                height = doc.PageSize.Height - 2 * LabelPaddingY;

            imageObj.ScaleToFit(width, height);

            PointF startPoint;
            var dy = (doc.PageSize.Height - imageObj.ScaledHeight) / 2;
            var dx = (doc.PageSize.Width - imageObj.ScaledWidth) / 2;

            startPoint = new PointF(dx, dy);

            imageObj.SetAbsolutePosition(startPoint.X, startPoint.Y);

            doc.Add(imageObj);

            return new PointF(imageObj.ScaledWidth, imageObj.ScaledHeight);
        }

        private void PrintAdditionalLabelInfo(Document doc,
            PdfWriter writer,
            PrintLabelInfo label,
            PointF labelSize,
            int number)
        {
           
        }
    }
}