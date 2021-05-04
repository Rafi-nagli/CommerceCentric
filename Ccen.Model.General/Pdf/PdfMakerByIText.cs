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
    public class PdfMakerByIText : IFileMaker
    {
        private readonly iTextSharp.text.Font SearchTextFont;
        private readonly BaseFont SearchTextBaseFont;
        private readonly BaseColor TransparentColor;
        private readonly BaseColor TextColor;

        private const float LabelPaddingX = 60f;
        private const float LabelPaddingY = 45f;
        private const float ScanFormPaddingX = 0f;
        private const float ScanFormPaddingY = 0f;
        private const int FontSize = 12;
        private const int BigFontSize = 24;
        private const int BigNormalFontSize = 20;

        private ILogService _log;

        public PdfMakerByIText(ILogService log)
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
            Document doc = new Document(PageSize.LETTER, 0, 0, 0, 0);

            var fileName = String.Format("label_{0}.pdf", 
                StringHelper.JoinTwo("_", 
                    (batchInfo != null && batchInfo.BatchId > 0) ? batchInfo.BatchId.ToString() : null, 
                    DateHelper.GetAppNowTime().ToString("MM_dd_yyyy_HH_mm_ss")));

            var localPath = outputDirectory + "\\LabelPdf\\" + fileName;
            Directory.CreateDirectory(Path.GetDirectoryName(localPath));

            PdfWriter writer = PdfWriter.GetInstance(doc, new FileStream(localPath, FileMode.Create));
            writer.SetPdfVersion(PdfWriter.PDF_VERSION_1_5);
            writer.CompressionLevel = PdfStream.BEST_COMPRESSION; //TODO: BEST_COMPRESSION

            doc.Open();

            try
            {
                if (batchInfo != null)
                {
                    AddBatchInfo(doc, writer, batchInfo);
                }

                if (scanFormImages != null)
                {
                    foreach (var scanFormImage in scanFormImages)
                    {
                        if (File.Exists(scanFormImage))
                        {
                            AddScanForm(doc, writer, scanFormImage);
                        }
                    }
                }

                var index = 0;
                while (index < labels.Count)
                {
                    if (labels[index].IsPdf 
                        && labels[index].SpecialType != LabelSpecialType.MailPickList
                        && labels[index].ShippingMethodId != ShippingUtils.AmazonPriorityFlatShippingMethodId //NOTE: Temporary exclude Stamps Priority Flat labels, Amazon return it in pdf format
                        && labels[index].ShippingMethodId != ShippingUtils.AmazonFedExHomeDeliveryShippingMethodId
                        && labels[index].ShippingMethodId != ShippingUtils.AmazonFedExHomeDeliveryShippingMethodId
                        && labels[index].ShippingMethodId != ShippingUtils.AmazonFedEx2DayOneRateEnvelopeShippingMethodId
                        && labels[index].ShippingMethodId != ShippingUtils.AmazonFedEx2DayOneRatePakShippingMethodId)
                    {
                        AddPdfLabel(doc, writer, labels[index]);
                        index++;
                    }
                    else
                    {
                        if (labels[index].LabelSize == PrintLabelSizeType.FullPage)
                        {
                            AddPageWithFullLabel(doc, writer, labels[index]);
                            index++;
                        }
                        else
                        {
                            if (index + 1 < labels.Count)
                            {
                                //Checking next label size
                                if (labels[index + 1].LabelSize == PrintLabelSizeType.FullPage 
                                    || (labels[index + 1].IsPdf &&                                         
                                        labels[index + 1].ShippingMethodId != ShippingUtils.AmazonPriorityFlatShippingMethodId
                                        && labels[index].ShippingMethodId != ShippingUtils.AmazonFedExHomeDeliveryShippingMethodId
                                        && labels[index].ShippingMethodId != ShippingUtils.AmazonFedExHomeDeliveryShippingMethodId
                                        && labels[index].ShippingMethodId != ShippingUtils.AmazonFedEx2DayOneRateEnvelopeShippingMethodId
                                        && labels[index].ShippingMethodId != ShippingUtils.AmazonFedEx2DayOneRatePakShippingMethodId
                                        && labels[index + 1].SpecialType != LabelSpecialType.MailPickList))
                                {
                                    AddPageWithTwoHalfLabels(doc, writer, labels[index], null);
                                    index++;
                                }
                                else
                                {
                                    AddPageWithTwoHalfLabels(doc, writer, labels[index], labels[index + 1]);
                                    index += 2;
                                }
                            }
                            else
                            {
                                AddPageWithTwoHalfLabels(doc, writer, labels[index], null);
                                index++;
                            }
                        }
                    }
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

        private PointF AddBatchInfo(Document doc, PdfWriter writer, BatchInfoToPrint batch)
        {
            doc.NewPage();
            //content.SaveGraphicsState();
            ////content.TranslateScaleRotate(8.5 / 2, 11 / (double)2, 1.0, Math.PI / 2);
            try
            {
                PointF basePoint = new PointF(0, 0);
                basePoint = new PointF(doc.PageSize.Width / 2, doc.PageSize.Height * 1 / (float)2);

                var lineHeight = 30;
                var basePointDy = 150;

                DrawText(writer,
                    "Labels",
                    BigFontSize,
                    TextColor,
                    basePoint.X - 200,
                    basePoint.Y + basePointDy + 6 * lineHeight 
                        + 10, //Page Caption
                    false);

                DrawText(writer,
                    "Date: " + DateHelper.ToDateTimeString(batch.Date),
                    BigNormalFontSize,
                    TextColor,
                    basePoint.X - 200,
                    basePoint.Y + basePointDy + 5 * lineHeight,
                    false);

                DrawText(writer,
                    "Batch ID: " + batch.BatchId,
                    BigNormalFontSize,
                    TextColor,
                    basePoint.X - 200,
                    basePoint.Y + basePointDy + 4 * lineHeight,
                    false);

                DrawText(writer,
                    "Batch Name: " + batch.BatchName,
                    BigNormalFontSize,
                    TextColor,
                    basePoint.X - 200,
                    basePoint.Y + basePointDy + 3 * lineHeight,
                    false);
                
                DrawText(writer,
                    "Number of Packages: " + batch.NumberOfPackages,
                    BigNormalFontSize,
                    TextColor,
                    basePoint.X - 200,
                    basePoint.Y + basePointDy + 2 * lineHeight,
                    false);

                DrawText(writer,
                    "FIMS AirBill Number: " + StringHelper.GetFirstNotEmpty(batch.FIMSAirBillNumber, "-"),
                    BigNormalFontSize,
                    TextColor,
                    basePoint.X - 200,
                    basePoint.Y + basePointDy + 1 * lineHeight,
                    false);

                var carriersText = batch.Carriers != null ? String.Join(", ", batch.Carriers.Select(c => c.Key + ": " + c.Value)) : "";
                DrawText(writer,
                    "Carriers: " + carriersText,
                    BigNormalFontSize,
                    TextColor,
                    basePoint.X - 200,
                    basePoint.Y + basePointDy,
                    false);

                var index = 11;
                lineHeight = 20;
                if (batch.StyleChanges != null
                    && batch.StyleChanges.Count > 0)
                {
                    index = index + 2;
                    if (index > 30)
                    {
                        doc.NewPage();
                        index = 0;
                    }

                    DrawText(writer,
                            "Items changed:",
                            FontSize,
                            TextColor,
                            basePoint.X - 200,
                            basePoint.Y + 300 - index * lineHeight,
                            false);

                    foreach (var styleChange in batch.StyleChanges)
                    {
                        index++;
                        if (index > 30)
                        {
                            doc.NewPage();
                            index = 0;
                        }
                        var pointY = basePoint.Y + 300 - index * lineHeight;
                        DrawText(writer,
                            styleChange.SourceStyleString + " - Size: " + styleChange.SourceStyleSize
                                + " => " + styleChange.DestStyleString + " - Size: " + styleChange.DestStyleSize,
                            FontSize,
                            TextColor,
                            basePoint.X - 200,
                            pointY,
                            false);
                    }
                }


                if (batch.OrdersWithPrintError != null
                    && batch.OrdersWithPrintError.Count > 0)
                {
                    index = index + 2;
                    if (index > 30)
                    {
                        doc.NewPage();
                        index = 0;
                    }

                    DrawText(writer,
                            "Orders removed because of errors:",
                            FontSize,
                            TextColor,
                            basePoint.X - 200,
                            basePoint.Y + 300 - index * lineHeight,
                            false);

                    foreach (var order in batch.OrdersWithPrintError)
                    {
                        index++;
                        if (index > 30)
                        {
                            doc.NewPage();
                            index = 0;
                        }
                        var pointY = basePoint.Y + 300 - index * lineHeight;
                        DrawText(writer,
                            "Order " + order.OrderAmazonId + " label #" + order.NumberInBatch,
                            FontSize,
                            TextColor,
                            basePoint.X - 200,
                            pointY,
                            false);

                        for (int i = 0; i < order.Items.Count; i++)
                        {
                            index++;
                            if (index > 30)
                            {
                                doc.NewPage();
                                index = 0;
                            }

                            pointY = basePoint.Y + 300 - index * lineHeight;
                            DrawText(writer,
                                (i + 1) + ". " + order.Items[i].SKU + " - " + order.Items[i].Size + "  " + order.Items[i].Quantity + " pieces  " + order.Items[i].SortIsle + "/" + order.Items[i].SortSection + "/" + order.Items[i].SortShelf,
                                FontSize,
                                TextColor,
                                basePoint.X - 180,
                                pointY,
                                false);
                        }
                    }
                }

                if (batch.OrdersWasManuallyRemoved != null
                    && batch.OrdersWasManuallyRemoved.Count > 0)
                {
                    index = index + 2;
                    if (index > 30)
                    {
                        doc.NewPage();
                        index = 0;
                    }

                    DrawText(writer,
                            "Merchandise removed manually after pick list created",
                            FontSize,
                            TextColor,
                            basePoint.X - 200,
                            basePoint.Y + 300 - index * lineHeight,
                            false);

                    foreach (var order in batch.OrdersWasManuallyRemoved)
                    {
                        index++;
                        if (index > 30)
                        {
                            doc.NewPage();
                            index = 0;
                        }
                        var pointY = basePoint.Y + 300 - index * lineHeight;
                        DrawText(writer,
                            "Order " + order.OrderAmazonId, // + " label #" + order.NumberInBatch,
                            FontSize,
                            TextColor,
                            basePoint.X - 200,
                            pointY,
                            false);

                        for (int i = 0; i < order.Items.Count; i++)
                        {
                            index++;
                            if (index > 30)
                            {
                                doc.NewPage();
                                index = 0;
                            }

                            pointY = basePoint.Y + 300 - index * lineHeight;
                            DrawText(writer,
                                (i + 1) + ". " + order.Items[i].SKU + " - " + order.Items[i].Size + "  " + order.Items[i].Quantity + " pieces  " + order.Items[i].SortIsle + "/" + order.Items[i].SortSection + "/" + order.Items[i].SortShelf,
                                FontSize,
                                TextColor,
                                basePoint.X - 180,
                                pointY,
                                false);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _log.Info("Pdf draw batch info (batch: " + batch.BatchName + " - " + batch.BatchId + ")", ex);
            }
            return new PointF(doc.PageSize.Width - 2 * LabelPaddingX,
                doc.PageSize.Height / 2 - 2 * LabelPaddingY);
        }

        private void AddScanForm(Document doc, PdfWriter writer, string scanFormImage)
        {
            var labelInfo = new PrintLabelInfo()
            {
                Image = scanFormImage,
            };

            if (labelInfo.IsPdf)
            {
                AddPdfLabel(doc, writer, labelInfo);
            }
            else
            {
                doc.NewPage();

                var scanFormImageObj = iTextSharp.text.Image.GetInstance(scanFormImage);
                var width = doc.PageSize.Width - 2*ScanFormPaddingX;
                var height = doc.PageSize.Height - 2*ScanFormPaddingY;

                scanFormImageObj.ScaleToFit(width, height);
                scanFormImageObj.CompressionLevel = PdfStream.BEST_COMPRESSION;

                var dy = (doc.PageSize.Height - scanFormImageObj.ScaledHeight)/2;
                var dx = (doc.PageSize.Width - scanFormImageObj.ScaledWidth)/2;

                scanFormImageObj.SetAbsolutePosition(dx, dy);

                doc.Add(scanFormImageObj);
            }
        }

        private void AddPageWithFullLabel(Document doc, PdfWriter writer, PrintLabelInfo label)
        {
            doc.NewPage();

            var labelSize = new PointF(0, 0);
            if (String.IsNullOrEmpty(label.Image) || !File.Exists(label.Image))
            {
                labelSize = AddLabelDummyText(doc, writer, label, 0);
                PrintAdditionalLabelInfo(doc, writer, label, labelSize, 0);
            }
            else
            {
                if (label.ShippingMethodId == ShippingUtils.AmazonDhlExpressMXShippingMethodId)
                    //NOTE: Temporary Print twice //TODO: create pdf with 2 labels
                {
                    labelSize = AddLabelImage(doc, writer, label.Image, PrintLabelSizeType.HalfPage, label, 0);
                    //PrintAdditionalLabelInfo(doc, writer, label, labelSize, 0);
                    labelSize = AddLabelImage(doc, writer, label.Image, PrintLabelSizeType.HalfPage, label, 1);
                    PrintAdditionalLabelInfo(doc, writer, label, labelSize, 1);
                }
                else
                {
                    labelSize = AddLabelImage(doc, writer, label.Image, label.LabelSize, label, 0);
                    PrintAdditionalLabelInfo(doc, writer, label, labelSize, 0);
                }
            }
        }

        private void AddPageWithTwoHalfLabels(Document doc, PdfWriter writer, PrintLabelInfo label1, PrintLabelInfo label2)
        {
            doc.NewPage();

            AddHalfPageLabel(doc, writer, label1, 0);

            if (label2 != null)
            {
                AddHalfPageLabel(doc, writer, label2, 1);
            }
        }

        private void AddHalfPageLabel(Document doc, PdfWriter writer, PrintLabelInfo label, int number)
        {
            var labelSize = new PointF(0, 0);
            if (String.IsNullOrEmpty(label.Image) || !File.Exists(label.Image))
            {
                labelSize = AddLabelDummyText(doc, writer, label, number);
            }
            else
            {
                if (label.IsPdf) //NOTE: Temporary Amazon return stamps flat priority in pdf format!!!
                {
                    if (label.SpecialType == LabelSpecialType.MailPickList)
                    {
                        labelSize = AddMailPickListPdf(doc, writer, label, number);
                    }
                    else
                    {
                        if (label.ShippingMethodId == ShippingUtils.AmazonFedExHomeDeliveryShippingMethodId
                            || label.ShippingMethodId == ShippingUtils.AmazonFedExHomeDeliveryShippingMethodId
                            || label.ShippingMethodId == ShippingUtils.AmazonFedEx2DayOneRateEnvelopeShippingMethodId
                            || label.ShippingMethodId == ShippingUtils.AmazonFedEx2DayOneRatePakShippingMethodId)
                            labelSize = AddFedexPdf(doc, writer, label, number);
                        else
                            labelSize = AddStampsPdf(doc, writer, label, number);
                    }
                }
                else
                {
                    labelSize = AddLabelImage(doc, writer, label.Image, label.LabelSize, label, number);
                }
            }

            if (label.SpecialType != LabelSpecialType.MailPickList)
            {
                PrintAdditionalLabelInfo(doc, writer, label, labelSize, number);
            }
        }

        private PointF AddLabelDummyText(Document doc,
            PdfWriter writer,
            PrintLabelInfo label,
            int number)
        {
            //content.SaveGraphicsState();
            ////content.TranslateScaleRotate(8.5 / 2, 11 / (double)2, 1.0, Math.PI / 2);
            try
            {
                var validPersonName = String.Join("", label.PersonName.Where(ch => ValidateChar(ch)).ToList());

                var message = String.Format("Label B{0}#{1} has purchase error", label.BatchId, label.Number);
                if (label.PrintResult == LabelPrintStatus.AlreadyMailed)
                {
                    message = String.Format("Label B{0}#{1}. This order was previously processed and mailed", label.BatchId, label.Number);
                }
                if (label.PrintResult == LabelPrintStatus.OnHold)
                {
                    message = String.Format("Label B{0}#{1}. This order was on hold", label.BatchId, label.Number);
                }


                PointF basePoint = new PointF(0, 0);
                if (label.LabelSize == PrintLabelSizeType.HalfPage)
                {
                    if (number == 0)
                    {
                        basePoint = new PointF(doc.PageSize.Width / 2, doc.PageSize.Height * 3 / (float)4);
                    }
                    else
                    {
                        basePoint = new PointF(doc.PageSize.Width / 2, doc.PageSize.Height * 1 / (float)4);
                    }
                }
                else
                {
                    basePoint = new PointF(doc.PageSize.Width / 2, doc.PageSize.Height * 1 / (float)2);
                }

                DrawText(writer,
                    message,
                    BigNormalFontSize,
                    TextColor,
                    basePoint.X - 120,
                    basePoint.Y + 50,
                    false);

                DrawText(writer,
                    label.OrderId,
                    BigFontSize,
                    TextColor,
                    basePoint.X - 120,
                    basePoint.Y,
                    false);

                DrawText(writer,
                    validPersonName,
                    BigFontSize,
                    TextColor,
                    basePoint.X - 120,
                    basePoint.Y - 50,
                    false);
            }
            catch (Exception ex)
            {
                _log.Info("Pdf draw text error (text: " + label.OrderId + " - " + label.PersonName + ")", ex);
            }
            return new PointF(doc.PageSize.Width - 2 * LabelPaddingX,
                doc.PageSize.Height / 2 - 2 * LabelPaddingY);
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

            if (labelSize == PrintLabelSizeType.HalfPage)
            {
                if (imageObj.Height > imageObj.Width)
                {
                    if (labelInfo.RotationAngle.HasValue)
                        imageObj.Rotation = (float)(labelInfo.RotationAngle.Value/(float)360*(2*Math.PI));
                    //if (labelInfo.ShippingMethodId == ShippingUtils.IBCCEPePocketMethodId)
                    //    imageObj.Rotation = -(float) Math.PI/2;
                    else
                        imageObj.Rotation = (float) Math.PI/2;
                    imageObj.Rotate();
                }
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

            if (labelSize == PrintLabelSizeType.HalfPage)
            {
                dy = (doc.PageSize.Height / 2 - imageObj.ScaledHeight) / 2;

                if (number == 0)
                {
                    startPoint = new PointF(dx, dy + doc.PageSize.Height / 2);
                }
                else
                {
                    startPoint = new PointF(dx, dy);
                }
            }
            else
            {
                startPoint = new PointF(dx, dy);
            }

            imageObj.SetAbsolutePosition(startPoint.X, startPoint.Y);

            doc.Add(imageObj);

            return new PointF(imageObj.ScaledWidth, imageObj.ScaledHeight);
        }


        private PointF AddFedexPdf(Document doc, PdfWriter writer, PrintLabelInfo label, int number)
        {
            var pdf = new PdfReader(label.Image);

            float dy = 0;
            var page = 1;
            dy = pdf.GetPageSizeWithRotation(page).Width / 2 + 110;
            if (number == 1)
                dy = pdf.GetPageSizeWithRotation(page).Width / 2 + 110 - pdf.GetPageSizeWithRotation(page).Height / 2;

            var pg = writer.GetImportedPage(pdf, 1);

            writer.DirectContent.AddTemplate(pg, 0, 1f, -1f, 0, pdf.GetPageSizeWithRotation(1).Height + 50, dy);

            var width = doc.PageSize.Width - 2 * LabelPaddingX;
            var height = doc.PageSize.Height / 2 - 2 * LabelPaddingY;

            return new PointF(width, height);
        }

        private static PointF AddStampsPdf(Document doc, PdfWriter writer, PrintLabelInfo label, int number)
        {
            var pdf = new PdfReader(label.Image);

            float dy = 0;

            var page = 1;

            var pg = writer.GetImportedPage(pdf, page);

            dy = pdf.GetPageSizeWithRotation(page).Width / 2 + 80;
            if (number == 1)
                dy = pdf.GetPageSizeWithRotation(page).Width / 2 + 60 - pdf.GetPageSizeWithRotation(page).Height / 2;

            //http://stackoverflow.com/questions/3579058/rotating-pdf-in-c-sharp-using-itextsharp
            writer.DirectContent.AddTemplate(pg, 0, 1.10f, -1.10f, 0, pdf.GetPageSizeWithRotation(page).Height - 175, dy); //270

            var width = doc.PageSize.Width - 2 * LabelPaddingX;
            var height = doc.PageSize.Height / 2 - 2 * LabelPaddingY;

            return new PointF(width, height);
        }

        private static PointF AddMailPickListPdf(Document doc, PdfWriter writer, PrintLabelInfo label, int number)
        {
            var pdf = new PdfReader(label.Image);

            float dy = 0;

            var page = 1;

            var pg = writer.GetImportedPage(pdf, page);

            dy = pdf.GetPageSizeWithRotation(page).Width / 2 + 120;
            if (number == 1)
                dy = pdf.GetPageSizeWithRotation(page).Width / 2 + 140 - pdf.GetPageSizeWithRotation(page).Height / 2;

            //http://stackoverflow.com/questions/3579058/rotating-pdf-in-c-sharp-using-itextsharp
            writer.DirectContent.AddTemplate(pg, 0, 1.0f, -1.0f, 0, pdf.GetPageSizeWithRotation(page).Height, dy); //270

            var width = doc.PageSize.Width - 2 * LabelPaddingX;
            var height = doc.PageSize.Height / 2 - 2 * LabelPaddingY;

            return new PointF(width, height);
        }

        private void AddPdfLabel(Document doc, PdfWriter writer, PrintLabelInfo label)
        {
            //using //if enable it main doc also closing
            var pdf = new PdfReader(label.Image);
            {
                if (label.ShippingMethodId == ShippingUtils.AmazonFedExHomeDeliveryShippingMethodId
                    || label.ShippingMethodId == ShippingUtils.AmazonFedExHomeDeliveryShippingMethodId
                    || label.ShippingMethodId == ShippingUtils.AmazonFedEx2DayOneRateEnvelopeShippingMethodId
                    || label.ShippingMethodId == ShippingUtils.AmazonFedEx2DayOneRatePakShippingMethodId)
                {
                    doc.SetPageSize(pdf.GetPageSizeWithRotation(1));
                    doc.NewPage();
                    var pg = writer.GetImportedPage(pdf, 1);
                    int rotation = 270;// pdf.GetPageRotation(1);
                    if (rotation == 90)
                    {
                        writer.DirectContent.AddTemplate(pg, 0, -1f, 1f, 0, 0, pdf.GetPageSizeWithRotation(1).Height);
                    }
                    else if (rotation == 270)
                    {
                        writer.DirectContent.AddTemplate(pg, 0, 1f, -1f, 0, pdf.GetPageSizeWithRotation(1).Height + 50, pdf.GetPageSizeWithRotation(1).Width / 2 + 110);
                    }
                    else
                    {
                        writer.DirectContent.AddTemplate(pg, 1f, 0, 0, 1f, 0, 0);
                    }
                    
                    PrintAdditionalLabelInfo(doc,
                        writer,
                        label,
                        new PointF(pdf.GetPageSizeWithRotation(1).Width - 150, pdf.GetPageSizeWithRotation(1).Height / 2 - 100),
                        0);
                }
                else
                {
                    for (int page = 0; page < pdf.NumberOfPages; page++)
                    {
                        doc.SetPageSize(pdf.GetPageSizeWithRotation(page + 1));
                        doc.NewPage();
                        var pg = writer.GetImportedPage(pdf, page + 1);
                        int rotation = pdf.GetPageRotation(page + 1);
                        if (rotation == 90 || rotation == 270)
                            writer.DirectContent.AddTemplate(pg, 0, -1f, 1f, 0, 0, pdf.GetPageSizeWithRotation(page + 1).Height);
                        else
                            writer.DirectContent.AddTemplate(pg, 1f, 0, 0, 1f, 0, 0);
                    }
                }
            }
        }

        private void PrintAdditionalLabelInfo(Document doc,
            PdfWriter writer,
            PrintLabelInfo label,
            PointF labelSize,
            int number)
        {
            //Print OrderId and Name
            try
            {
                var validPersonName = String.Join("", label.PersonName.Where(ch => ValidateChar(ch)).ToList());

                Color? color = null;
                bool isDashed = false;
                var packageTypeName = label.PackageNameOnLabel;
                
                switch (label.ServiceType)
                {
                    case ShippingTypeCode.IStandard:
                        if (label.PackageType == PackageTypeCode.Regular)
                            color = Color.Yellow;
                        break;
                    case ShippingTypeCode.Priority:
                    case ShippingTypeCode.IPriority:
                        if (label.PackageType == PackageTypeCode.Flat)
                            color = Color.Red;
                        else
                            color = Color.YellowGreen;
                        break;
                    case ShippingTypeCode.PriorityExpress:
                    case ShippingTypeCode.IPriorityExpress:
                        color = Color.Blue;
                        break;
                    case ShippingTypeCode.SameDay:
                        color = Color.DarkBlue;
                        break;
                }

                if (label.ShippingMethodId == ShippingUtils.AmazonRegionalRateBoxAMethodId
                    || label.ShippingMethodId == ShippingUtils.RegionalRateBoxAMethodId)
                {
                    color = Color.Orange;
                }
                if (label.ShippingMethodId == ShippingUtils.FedexOneRate2DayEnvelope
                    || label.ShippingMethodId == ShippingUtils.AmazonFedEx2DayOneRateEnvelopeShippingMethodId
                    || label.ShippingMethodId == ShippingUtils.FedexPriorityOvernightEnvelope)
                {
                    color = Color.Violet;
                }
                if (label.ShippingMethodId == ShippingUtils.FedexOneRate2DayPak
                    || label.ShippingMethodId == ShippingUtils.AmazonFedEx2DayOneRatePakShippingMethodId
                    || label.ShippingMethodId == ShippingUtils.FedexPriorityOvernightPak)
                {
                    color = Color.FromArgb(155, 135, 12);
                }
                if (label.ShippingMethodId == ShippingUtils.AmazonFedEx2DayShppingMethodId
                    || label.ShippingMethodId == ShippingUtils.AmazonFedEx2DayAMShppingMethodId
                    || label.ShippingMethodId == ShippingUtils.Fedex2DayAMShppingMethodId
                    || label.ShippingMethodId == ShippingUtils.Fedex2DayShppingMethodId)
                {
                    color = Color.FromArgb(155, 135, 12);
                    isDashed = true;
                }

                PointF basePoint;
                float lineHeight = labelSize.Y;// doc.PageSize.Height - 2*LabelPaddingY;
                if (label.LabelSize == PrintLabelSizeType.FullPage)
                {
                    basePoint = new PointF((doc.PageSize.Width - labelSize.X)/2, 
                        (doc.PageSize.Height - labelSize.Y)/2);
                    //lineHeight = doc.PageSize.Height - 2*LabelPaddingY;
                }
                else
                {
                    //lineHeight = doc.PageSize.Height/2 - 2*LabelPaddingY;

                    if (number == 0)
                    {
                        basePoint = new PointF((doc.PageSize.Width - labelSize.X) / 2,
                            (doc.PageSize.Height / 2 - labelSize.Y) / 2 + doc.PageSize.Height / 2);
                    }
                    else
                    {
                        basePoint = new PointF((doc.PageSize.Width - labelSize.X) / 2,
                            (doc.PageSize.Height / 2 - labelSize.Y) / 2);
                    }
                }
                
                DrawText(writer,
                    "B" + label.BatchId + " #" + label.Number.ToString() + " - " + label.OrderId, 
                    FontSize,
                    TransparentColor,
                    LabelPaddingX / 2 + 13,
                    basePoint.Y,
                    true);

                DrawText(writer,
                    validPersonName,
                    FontSize,
                    TransparentColor,
                    LabelPaddingX / 2 + 1,
                    basePoint.Y,
                    true);

                if (!String.IsNullOrEmpty(packageTypeName))
                {
                    DrawText(writer,
                        packageTypeName,
                        FontSize,
                        TextColor,
                        LabelPaddingX / 2 - 13,
                        basePoint.Y,
                        true);
                }

                DrawText(writer,
                    label.Notes,
                    FontSize,
                    TextColor,
                    basePoint.X + labelSize.X + 20,
                    basePoint.Y,
                    true);

                if (label.ShippingMethodId != ShippingUtils.AmazonDhlExpressMXShippingMethodId)
                    //NOTE: Exclude draw line for DHL MX
                {
                    if (color != null)
                    {
                        DrawLine(writer,
                            new PointF(LabelPaddingX/2 + 20, basePoint.Y),
                            new PointF(LabelPaddingX/2 + 20, basePoint.Y + lineHeight),
                            new BaseColor(color.Value.R, color.Value.G, color.Value.B),
                            isDashed);
                    }
                }
            }
            catch (Exception ex)
            {
                _log.Info("Pdf draw text error (text: " + label.OrderId + " - " + label.PersonName + ")", ex);
            }
        }

        private void DrawLine(PdfWriter writer, 
            PointF from,
            PointF to,
            BaseColor color,
            bool isDashed)
        {
            writer.DirectContent.SaveState();
            if (isDashed)
            {
                writer.DirectContent.SetLineDash(10, 5, 7.5);
                //writer.DirectContent.SetLineDash(10);
            }
            writer.DirectContent.SetColorFill(color);
            writer.DirectContent.SetColorStroke(color);
            writer.DirectContent.SetLineWidth(8);
            writer.DirectContent.MoveTo(from.X, from.Y);
            writer.DirectContent.LineTo(to.X, to.Y);
            writer.DirectContent.Stroke();
            writer.DirectContent.SetColorFill(BaseColor.BLACK);
            writer.DirectContent.SetColorStroke(BaseColor.BLACK);
            writer.DirectContent.SetLineWidth(1);
            writer.DirectContent.RestoreState();
        }

        private void DrawText(PdfWriter writer, 
            string text, 
            int fontSize, 
            BaseColor color,
            double x, 
            double y,
            bool vertical)
        {
            AffineTransform textTransform = AffineTransform.GetTranslateInstance(x, y);
            if (vertical)
                textTransform.Rotate(Math.PI / 2);
            //writer.DirectContent.Transform(rotate90);

            PdfContentByte cb = writer.DirectContent;
            cb.BeginText();
            ////BaseFont textFont = BaseFont.CreateFont("Arial", BaseFont.CP1252, true);
            cb.SetColorStroke(color);
            cb.SetColorFill(color);
            cb.SetFontAndSize(SearchTextFont.GetCalculatedBaseFont(false), fontSize);
            cb.SetTextMatrix(textTransform); //(xPos, yPos)

            cb.ShowText(text);
            cb.EndText();
        }


        private bool ValidateChar(char testChar)
        {
            // character is one byte long
            if (testChar <= 255)
            {
                // test for control characters
                if (testChar < ' ' || testChar > '~' && testChar < 160)
                    return false;

                // return the same character
                return true;
            }

            return false;
        }
    }
}