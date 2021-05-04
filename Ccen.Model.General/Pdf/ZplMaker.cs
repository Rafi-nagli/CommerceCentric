using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using Amazon.Common.Helpers;
using Amazon.Core.Contracts;
using Amazon.Core.Entities.Enums;
using Amazon.Core.Models;
using Amazon.Core.Models.Stamps;

namespace Amazon.Model.Implementation.Pdf
{
    public class ZplMaker : IFileMaker
    {
        private const float LabelPaddingX = 60f;
        private const float LabelPaddingY = 45f;
        private const float ScanFormPaddingX = 0f;
        private const float ScanFormPaddingY = 0f;

        private ILogService _log;

        public ZplMaker(ILogService log)
        {
            _log = log;
        }

        public string CreateFileWithLabels(IList<PrintLabelInfo> labels, 
            IList<string> scanFormImages, 
            BatchInfoToPrint batchInfo,
            string outputDirectory, 
            string name = null)
        {
            var fileName = String.Format("label_{0}.zpl", DateHelper.GetAppNowTime().ToString("MM_dd_yyyy_HH_mm_ss"));

            var localPath = outputDirectory + "\\LabelPdf\\" + fileName;
            Directory.CreateDirectory(Path.GetDirectoryName(localPath));
            
            try
            {
                using (var stream = File.Create(localPath))
                {
                    using (var writer = new StreamWriter(stream))
                    {
                        writer.Write("");

                        //if (scanFormImages != null)
                        //{
                        //    foreach (var scanFormImage in scanFormImages)
                        //    {
                        //        if (File.Exists(scanFormImage))
                        //        {
                        //            AddScanForm(writer, scanFormImage);
                        //        }
                        //    }
                        //}

                        var index = 0;
                        while (index < labels.Count)
                        {
                            if (labels[index].IsPdf
                                && labels[index].SpecialType != LabelSpecialType.MailPickList
                                && labels[index].ShippingMethodId != ShippingUtils.AmazonPriorityFlatShippingMethodId)
                                //NOTE: Temporary exclude Stamps Priority Flat labels, Amazon return it in pdf format
                            {
                                AddPdfLabel(writer, labels[index]);
                                index++;
                            }
                            else
                            {
                                AddImageLabel(writer, labels[index]);
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

            return "~\\LabelPdf\\" + fileName;
        }

        private void AddScanForm(StreamWriter writer, string scanFormImage)
        {
            var labelInfo = new PrintLabelInfo()
            {
                Image = scanFormImage,
            };

            AddImage(writer, scanFormImage);
        }

        private void AddPdfLabel(StreamWriter writer, PrintLabelInfo label)
        {
            //Nothing
            throw new NotSupportedException("Unable to mix pdf and zpl label formats");
        }

        private void AddImageLabel(StreamWriter writer, PrintLabelInfo label)
        {
            if (label.IsZpl)
                AddZpl(writer, label.Image);
            else
                AddImage(writer, label.Image);
        }

        private void AddZpl(StreamWriter writer, string zplFilename)
        {
            var text = File.ReadAllText(zplFilename);
            writer.Write(text);
            writer.Write(Environment.NewLine);
            writer.Flush();
        }

        private void AddImage(StreamWriter writer, string imageFilename)
        {
            throw new NotSupportedException("Unable to mix Png/Jpg with zpl label formats");


            //Command descriptions
            //https://stackoverflow.com/questions/33349262/png-zebra-printer-commands-what-do-they-mean   
            //https://www.zebra.com/content/dam/zebra/manuals/en-us/software/zpl-zbi2-pm-en.pdf
            using (MemoryStream memoryStream = new MemoryStream())
            {
                var imageName = Path.GetFileName(imageFilename);
                var image = Image.FromFile(imageFilename);

                Bitmap bmp = null;
                BitmapData bmpData = null;
                try
                {
                    bmp = new Bitmap(image);
                    bmpData = bmp.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height), ImageLockMode.ReadWrite,
                        bmp.PixelFormat);

                    int width = (bmp.Width + 7)/8;
                    int height = bmp.Height;
                    //int bitsPerPixel = 1; // Monochrome image required!
                    //double widthInBytes = Math.Ceiling(width / 8.0);

                    var sb = new StringBuilder(width*bmp.Height*2);
                    var ptr = bmpData.Scan0;
                    var pixels = new byte[width];
                    for (var y = 0; y < height - 1; y++)
                    {
                        Marshal.Copy(ptr, pixels, 0, width);
                        for (var x = 0; x < width - 1; x++)
                        {
                            sb.Append(String.Format("{0:X2}", (byte) (pixels[x] ^ 0xFF)));
                        }
                        sb.Append(Environment.NewLine);
                        ptr = (IntPtr) (ptr.ToInt64() + bmpData.Stride);
                    }

                    writer.Write(String.Format("~DG{0},{1},{2},", imageName, width*(height - 1), width) + sb.ToString());

                    /*//image.Save(memoryStream, ImageFormat.Bmp);
                    var bitmapFileData = memoryStream.GetBuffer();

                    string bitmapFilePath = imageFilename; // file is attached to this support article
                    //byte[] bitmapFileData = System.IO.File.ReadAllBytes(bitmapFilePath);
                    int fileSize = bitmapFileData.Length;

                    int width = image.Width;
                    int height = image.Height;
                    int bitsPerPixel = 1; // Monochrome image required!
                    int bitmapDataLength = bitmapFileData.Length;
                    double widthInBytes = Math.Ceiling(width/8.0);
                                        
                    // Copy over the actual bitmap data from the bitmap file.
                    // This represents the bitmap data without the header information.
                    byte[] bitmap = new byte[bitmapDataLength];
                    Buffer.BlockCopy(bitmapFileData, 0, bitmap, 0, bitmapDataLength);

                    // Invert bitmap colors
                    for (int i = 0; i < bitmapDataLength; i++)
                    {
                        bitmap[i] ^= 0xFF;
                    }

                    // Create ASCII ZPL string of hexadecimal bitmap data
                    string ZPLImageDataString = BitConverter.ToString(bitmap);
                    ZPLImageDataString = ZPLImageDataString.Replace("-", string.Empty);

                    writer.Write(
                        String.Format("^XA^FO20,20^GFA, " + bitmapDataLength + "," + bitmapDataLength + "," +
                                      widthInBytes + "," + ZPLImageDataString));
                    */
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
                finally
                {
                    if (bmp != null)
                    {
                        if (bmpData != null)
                        {
                            bmp.UnlockBits(bmpData);
                        }
                        bmp.Dispose();
                    }
                }

                /*
                //var image = Image.FromFile(imageFilename);
                //image.Save(memoryStream, ImageFormat.Png);
                //byte[] bitmapBytes = memoryStream.GetBuffer();
                var bitmapBytes = File.ReadAllBytes(imageFilename);
                foreach (Byte b in bitmapBytes)
                {
                    string hexRep = String.Format("{0:X}", b);
                    if (hexRep.Length == 1)
                        hexRep = "0" + hexRep;
                    zipImageString.Append(hexRep);
                }
                //var bitmapString = Convert.ToBase64String(bitmapBytes, Base64FormattingOptions.None);
                writer.Write(String.Format("^XA^FO0,0~DYE:{0},P,P,{1},,{2}^XZ", Path.GetFileNameWithoutExtension(imageFilename), memoryStream.Length, zipImageString));
                */
            }
        }
    }
}