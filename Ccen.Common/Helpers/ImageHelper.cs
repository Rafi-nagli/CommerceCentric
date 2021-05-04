using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
/*using System.Windows.Media;
using System.Windows.Media.Imaging;*/
using Amazon.Core.Models;
using BarcodeLib;

namespace Amazon.Common.Helpers
{
    public class ImageHelper
    {
        //TODO: Get from config
        public const string NO_IMAGE_URL = "/Images/no-image.jpg";

        public static string GetFirstOrDefaultPicture(string pictures)
        {
            if (String.IsNullOrEmpty(pictures))
                return NO_IMAGE_URL; //noimage
            return pictures.Split(";".ToCharArray(), StringSplitOptions.RemoveEmptyEntries).FirstOrDefault();
        }
        
        public static bool IsAmazonImageUrl(string imageUrl)
        {
            if (string.IsNullOrEmpty(imageUrl))
                return false;

            return imageUrl.Contains("images-amazon");
        }

        public static Rectangle GetFromLayout(ImageLayout layout)
        {
            return new Rectangle(layout.OffsetX, layout.OffsetY, layout.Width, layout.Height);
        }

        public static StyleImageType GetImageTypeBySize(Point size)
        {
            if (size.X == 0)
                return StyleImageType.Unavailable;
            if (size.X <= 100)
                return StyleImageType.Swatch;
            if (size.X <= 1000 && size.Y <= 1000)
                return StyleImageType.LoRes;
            if (Math.Max(size.X, size.Y) >= 2000
                && Math.Min(size.X, size.Y) >= 1500)
                return StyleImageType.HiRes;
            return StyleImageType.MidRes;
        }

        public static string RemoveAmazonImagePostfix(string image)
        {
            if (String.IsNullOrEmpty(image))
                return image;

            return image.Replace("._UL1500_", "").Replace("._UL1200_", "").Replace("._UL1001_", "").Replace("._UL1250_", "").Replace("._UL1080_", "").Replace("._UL1100_", "");
        }



        public static string BuildGrouponImage(string outputDirectory, string image, string destFilenameWithoutExt)
        {
            if (String.IsNullOrEmpty(destFilenameWithoutExt))
                destFilenameWithoutExt = FileHelper.PrepareFileName(Path.GetFileNameWithoutExtension(image));
            var filename = (destFilenameWithoutExt ?? "").Replace("+", "_").Replace(" ", "_").Replace("/", "_").Replace("\\", "_") + "_53_v6.jpg"; //34
            var filepath = Path.Combine(outputDirectory, filename);
            if (File.Exists(filepath))
                return filepath;

            image = (image ?? "").Replace("+", "%20").Replace(" ", "%20");
            using (var imageStream = DownloadRemoteImageFileAsStream(image))
            {
                using (var img = Image.FromStream(imageStream))
                {
                    var destWidth = img.Width;
                    var destHeight = img.Height;
                    var destPaddingX = 0;
                    var destPaddingY = 0;
                    var ration = 3 / (float)5;
                    if (destHeight > ration * destWidth)
                    {
                        destWidth = (int)Math.Round(destHeight / ration);
                        destPaddingX = (destWidth - img.Width) / 2;
                    }
                    if (destHeight < ration * destWidth)
                    {
                        destHeight = (int)Math.Round(destWidth * ration);
                        destPaddingY = (destHeight - img.Height) / 2;
                    }

                    //var maxSizeY = Math.Max(img.Width, img.Height);
                    //var maxSizeX = (int) Math.Round((double)maxSizeY);
                    //var paddingX = (maxSizeX - img.Width)/2;
                    //var paddingY = (maxSizeY - img.Height)/2;

                    using (var copyImage = new Bitmap(destWidth, destHeight, System.Drawing.Imaging.PixelFormat.Format32bppArgb))
                    {
                        using (Graphics gr = Graphics.FromImage(copyImage))
                        {
                            gr.Clear(System.Drawing.Color.White); //NOTE: Color.Transparent = Black

                            // This is said to give best quality when resizing images
                            gr.InterpolationMode = InterpolationMode.HighQualityBicubic;

                            gr.DrawImage(img,
                                new Rectangle(destPaddingX, destPaddingY, img.Width, img.Height),
                                new Rectangle(0, 0, img.Width, img.Height),
                                GraphicsUnit.Pixel);
                        }
                        copyImage.Save(filepath, ImageFormat.Jpeg);
                    }
                }
            }

            return filepath;
        }

        public static string BuildWalmartImage(string outputDirectory, string image, string destFilenameWithoutExt)
        {
            if (String.IsNullOrEmpty(destFilenameWithoutExt))
                destFilenameWithoutExt = FileHelper.PrepareFileName(Path.GetFileNameWithoutExtension(image));
            var filename = destFilenameWithoutExt + "_34.jpg";
            var filepath = Path.Combine(outputDirectory, filename);
            if (File.Exists(filepath))
                return filepath;
            
            using (var imageStream = DownloadRemoteImageFileAsStream(image))
            {
                using (var img = Image.FromStream(imageStream))
                {
                    var destWidth = img.Width;
                    var destHeight = img.Height;
                    var destPaddingX = 0;
                    var destPaddingY = 0;
                    if (destHeight > 1.25*destWidth)
                    {
                        destWidth = (int) Math.Round(destHeight/1.25M);
                        destPaddingX = (destWidth - img.Width)/2;
                    }
                    if (destHeight < 1.25*destWidth)
                    {
                        destHeight = (int) Math.Round(destWidth*1.25M);
                        destPaddingY = (destHeight - img.Height)/2;
                    }
                    
                    //var maxSizeY = Math.Max(img.Width, img.Height);
                    //var maxSizeX = (int) Math.Round((double)maxSizeY);
                    //var paddingX = (maxSizeX - img.Width)/2;
                    //var paddingY = (maxSizeY - img.Height)/2;

                    using (var copyImage = new Bitmap(destWidth, destHeight, PixelFormat.Format32bppArgb))
                    {
                        using (Graphics gr = Graphics.FromImage(copyImage))
                        {
                            gr.Clear(Color.White); //NOTE: Color.Transparent = Black

                            // This is said to give best quality when resizing images
                            gr.InterpolationMode = InterpolationMode.HighQualityBicubic;

                            gr.DrawImage(img,
                                new Rectangle(destPaddingX, destPaddingY, img.Width, img.Height),
                                new Rectangle(0, 0, img.Width, img.Height),
                                GraphicsUnit.Pixel);
                        }
                        copyImage.Save(filepath, ImageFormat.Jpeg);
                    }
                }
            }

            return filepath;
        }


        public static string BuildEbayImage(string outputDirectory, string image)
        {
            //NOTE: save locally, Jet retrieve errors when send images from Amazon domain
            ImageFormat format = ImageFormat.Jpeg;
            if (image.Contains(".png") || image.Contains("_png"))
                format = ImageFormat.Png;
            var filename = FileHelper.PrepareFileName(Path.GetFileNameWithoutExtension(image)) + "." + format.ToString().Replace("Jpeg", "jpg");
            var filepath = Path.Combine(outputDirectory, filename);
            if (File.Exists(filepath))
                return filepath;

            using (var imageStream = DownloadRemoteImageFileAsStream(image))
            {
                using (var img = Image.FromStream(imageStream))
                {
                    img.Save(filepath, format);
                }
            }

            return filepath;
        }

        public static string BuildJetImage(string outputDirectory, string image)
        {
            //NOTE: save locally, Jet retrieve errors when send images from Amazon domain

            var filename = FileHelper.PrepareFileName(Path.GetFileNameWithoutExtension(image)) + ".jpg";
            var filepath = Path.Combine(outputDirectory, filename);
            if (File.Exists(filepath))
                return filepath;
            
            using (var imageStream = DownloadRemoteImageFileAsStream(image))
            {
                using (var img = Image.FromStream(imageStream))
                {
                    img.Save(filepath, ImageFormat.Jpeg);
                }
            }

            return filepath;
        }

        public static string BuildSwatchImage(string outputDirectory, string image)
        {
            float paddingFactor = 0.8f;
            var filename = Utils.MD5Utils.GetMD5HashAsString(image + "_" + paddingFactor) + ".jpg";
            var filepath = Path.Combine(outputDirectory, filename);
            if (File.Exists(filepath))
                return filepath;

            using (var imageStream = DownloadRemoteImageFileAsStream(image))
            {
                using (var img = Image.FromStream(imageStream))
                {
                    var minSize = (int) Math.Round(Math.Min(img.Width, img.Height)*paddingFactor);
                    var paddingX = (img.Width - minSize)/2;
                    var paddingY = (img.Height - minSize)/2;

                    var cropRect = new Rectangle()
                    {
                        X = paddingX, //(int) Math.Round(img.Width*0.25),
                        Y = paddingY, //(int) Math.Round(img.Height*0.25),
                        Width = img.Width - 2*paddingX, // (int) Math.Round(img.Width*0.5),
                        Height = img.Height - 2*paddingY, // (int) Math.Round(img.Height*0.5)
                    };

                    var bmpImage = new Bitmap(img);
                    using (var cropped = bmpImage.Clone(cropRect, bmpImage.PixelFormat))
                    {
                        var thumbnail = cropped.GetThumbnailImage(100, 100, () => false, IntPtr.Zero);
                        var resultStream = thumbnail.ToStream(ImageFormat.Jpeg);

                        using (Stream outputStream = File.Open(filepath, FileMode.Create, FileAccess.ReadWrite))
                        {
                            var buffer = new byte[4096];
                            int bytesRead;
                            do
                            {
                                bytesRead = resultStream.Read(buffer, 0, buffer.Length);
                                outputStream.Write(buffer, 0, bytesRead);
                            } while (bytesRead != 0);
                        }
                    }
                }
            }

            return filepath;
        }

        public static bool? PingImageUrl(string url)
        {
            try
            {
                var request = (HttpWebRequest) WebRequest.Create(url);
                using (var response = (HttpWebResponse)request.GetResponse())
                {
                    if ((response.StatusCode == HttpStatusCode.OK ||
                         response.StatusCode == HttpStatusCode.Moved ||
                         response.StatusCode == HttpStatusCode.Redirect) &&
                        response.ContentType.StartsWith("image", StringComparison.OrdinalIgnoreCase))
                    {
                        return true;
                    }
                }
            }
            catch (Exception ex)
            {
                if (ex.Message.ToLower().Contains("timeout"))
                    return null;
            }
            return false;
        }

        public static Point GetImageSize(string url)
        {
            try
            {
                using (var imageStream = DownloadRemoteImageFileAsStream(url))
                {
                    var img = Image.FromStream(imageStream);
                    return new Point(img.Width, img.Height);
                }
            }
            catch { }
            return new Point(0, 0);
        }

        public static bool IsImageSrgbJpeg(System.Drawing.Image someImage)
        {
           try
            {
                var val = someImage.GetPropertyItem(34675);
                System.Drawing.Imaging.ImageFlags flagValues = (System.Drawing.Imaging.ImageFlags)Enum.Parse(typeof(System.Drawing.Imaging.ImageFlags), someImage.Flags.ToString());
                return flagValues.ToString().Contains("HasRealDpi");
            }
            catch(ArgumentException ae)
            {
                return true;
            }
            catch(Exception er)
            {
                return false;
            }
                     
        }

        public static MemoryStream DownloadRemoteImageFileAsStream(string uri)
        {
            var memoryStream = new MemoryStream();

            var request = (HttpWebRequest)WebRequest.Create(uri);
            if (uri.StartsWith("https"))
            {
                ServicePointManager.ServerCertificateValidationCallback +=
                    (sender, certificate, chain, sslPolicyErrors) => true;
            }

            request.UserAgent = "Mozilla/4.0 (compatible; MSIE 6.0)";// "Mozilla /5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/76.0.3809.100 Safari/537.36");

            try
            {
                using (var response = (HttpWebResponse)request.GetResponse())
                {
                    if ((response.StatusCode == HttpStatusCode.OK ||
                         response.StatusCode == HttpStatusCode.Moved ||
                         response.StatusCode == HttpStatusCode.Redirect) &&
                        response.ContentType.StartsWith("image", StringComparison.OrdinalIgnoreCase))
                    {
                        // if the remote file was found, download oit
                        using (var inputStream = response.GetResponseStream())
                        {
                            if (inputStream != null)
                            {
                                var buffer = new byte[4096];
                                int bytesRead;
                                do
                                {
                                    bytesRead = inputStream.Read(buffer, 0, buffer.Length);
                                    memoryStream.Write(buffer, 0, bytesRead);
                                } while (bytesRead != 0);
                            }
                        }

                        return memoryStream;
                    }
                    else
                    {
                        return null;
                    }
                }
            }
            catch
            {
                return null;
            }
        }

        public static Image GetImageFromStream(Stream stream)
        {
            return Image.FromStream(stream);
        }

        public static bool DownloadRemoteImageFile(string uri, string filePath, ImageLayout layout)
        {
            var request = (HttpWebRequest) WebRequest.Create(uri);
            if (uri.StartsWith("https"))
            {
                ServicePointManager.ServerCertificateValidationCallback +=
                    (sender, certificate, chain, sslPolicyErrors) => true;
            }

            using (var response = (HttpWebResponse) request.GetResponse())
            {
                var dir = Path.GetDirectoryName(filePath);
                if (!Directory.Exists(dir))
                    Directory.CreateDirectory(dir);

                if (File.Exists(filePath))
                    File.Delete(filePath);

                // Check that the remote file was found. The ContentType
                // check is performed since a request for a non-existent
                // image file might be redirected to a 404-page, which would
                // yield the StatusCode "OK", even though the image was not
                // found.
                if ((response.StatusCode == HttpStatusCode.OK ||
                     response.StatusCode == HttpStatusCode.Moved ||
                     response.StatusCode == HttpStatusCode.Redirect) &&
                    (response.ContentType.StartsWith("image", StringComparison.OrdinalIgnoreCase)
                     || response.ContentType.IndexOf("octet-stream", StringComparison.OrdinalIgnoreCase) >= 0))
                {

                    // if the remote file was found, download oit
                    using (var inputStream = response.GetResponseStream())
                    {
                        if (inputStream != null)
                        {
                            var streamToLoad = layout != null && layout.IsCrop
                                ? GetCroppedImageStream(inputStream, GetFromLayout(layout))
                                : inputStream;

                            using (Stream outputStream = File.OpenWrite(filePath))
                            {
                                var buffer = new byte[4096];
                                int bytesRead;
                                do
                                {
                                    bytesRead = streamToLoad.Read(buffer, 0, buffer.Length);
                                    outputStream.Write(buffer, 0, bytesRead);
                                } while (bytesRead != 0);
                            }

                            return true;
                        }
                    }

                }
            }
            return false;
        }

        private static Stream GetCroppedImageStream(Stream inputStream, Rectangle cropRectangle)
        {
            //const int croppedWidth = 1200;
            //const int croppedHeight = 770;
            //const int offX = 200;
            //const int offY = 155;
            var img = Image.FromStream(inputStream);
            return CropImage(img, cropRectangle);
        }

        private static Stream CropImage(Image img, Rectangle cropArea)
        {
            var bmpImage = new Bitmap(img);
            var cropped = bmpImage.Clone(cropArea, bmpImage.PixelFormat);
            return cropped.ToStream(ImageFormat.Jpeg);
        }

        public static int GetSortIndex(int category)
        {
            switch ((StyleImageCategories)category)
            {
                case StyleImageCategories.None:
                    return 50; //NOTE: Default: should be before Size Chart
                case StyleImageCategories.LiveMain:
                    return 100;
                case StyleImageCategories.FlatMain:
                    return 90;
                case StyleImageCategories.Live:
                    return 80;
                case StyleImageCategories.Flat:
                    return 70;
                case StyleImageCategories.SizeChart:
                    return 40;
            }
            return 0;
        }

        public static Image ImageWithBarcode(Stream inputFile, string barCodeString, int width, int height, int x, int y, RotateFlipType rotateFlipType = RotateFlipType.RotateNoneFlipNone)
        {
            var baseImage = Image.FromStream(inputFile);
            var overlayImage = DrawBarCode(barCodeString, width, height);
            overlayImage.RotateFlip(rotateFlipType);
            using (Graphics g = Graphics.FromImage(baseImage))
            {
                g.DrawImage(overlayImage, x, y);
            }
            return baseImage;
        }

        public static Image ImageWithLabel(Stream inputFile, string text, Font font, int width, int height, int x, int y, RotateFlipType rotateFlipType = RotateFlipType.RotateNoneFlipNone)
        {
            var baseImage = Image.FromStream(inputFile);
            Color foreColor = Color.Black;
            Color backColor = Color.Transparent;
            var overlayImage = DrawText(text, font, width, height);
            overlayImage.RotateFlip(rotateFlipType);
            using (Graphics g = Graphics.FromImage(baseImage))
            {
                g.DrawImage(overlayImage, x, y);
            }
            return baseImage;
        }

        static Image DrawBarCode(string barCodeString, int width, int height)
        {
            Image overlayImage;
            var barCode = new Barcode();
            Color foreColor = Color.Black;
            Color backColor = Color.Transparent;
            overlayImage = barCode.Encode(TYPE.CODE128, barCodeString, foreColor, backColor, width, height);
            return overlayImage;
        }


        static Image DrawText(String text, Font font, int width, int height)
        {
            Color textColor = Color.Black;
            Color backColor = Color.Transparent;


            Image img = new Bitmap(width, height);
            {
                using (Graphics drawing = Graphics.FromImage(img))
                {
                    using (Brush textBrush = new SolidBrush(textColor))
                    {
                        drawing.Clear(backColor);
                        drawing.DrawString(text, font, textBrush, 0, 0);
                        drawing.Save();
                        textBrush.Dispose();
                    }
                    drawing.Dispose();
                }
                return img;
            }
        }

        static string GetGroupedString(string text, int itemsIngroup)
        {
            text = text.Replace(" ", "");
            return String.Join(" ", Split(text, itemsIngroup));
        }

        static IEnumerable<string> Split(string str, int itemsIngroup)
        {
            while (str.Length > itemsIngroup)
            {
                yield return new string(str.Take(itemsIngroup).ToArray());
                str = new string(str.Skip(itemsIngroup).ToArray());
            }
            yield return str;
        }
    }
}
