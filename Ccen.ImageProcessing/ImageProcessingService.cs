using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using Amazon.Common.Helpers;
using Amazon.Core;
using Amazon.Core.Contracts;
using Amazon.Core.Contracts.Factories;
using Amazon.Core.Entities;
using Amazon.Core.Models;
using Amazon.DTO;
using Amazon.DTO.Inventory;
using CsvHelper;
using CsvHelper.Configuration;
using ImageMagick;

namespace Amazon.ImageProcessing
{
    public class ImageProcessingService : IImageProcessingService
    {
        private IDbFactory _dbFactory;
        private ITime _time;
        private ILogService _log;
        private string _walmartImageDirectory;

        public ImageProcessingService(IDbFactory dbFactory,
            ITime time,
            ILogService log,
            string walmartImageDirectory)
        {
            _dbFactory = dbFactory;
            _time = time;
            _log = log;
            _walmartImageDirectory = walmartImageDirectory;
        }

        public IList<string> ProcessImageFile(string filePath, IDbFactory dbFactory)
        {
            StreamReader streamReader = new StreamReader(filePath);
            CsvReader reader = new CsvReader(streamReader, new CsvConfiguration
            {
                HasHeaderRecord = true,
                Delimiter = ",",
                TrimFields = true,
            });

            var itemResults = new List<string>();

            //using (var db = dbFactory.GetRWDb())
            {
                while (reader.Read())
                {
                    string id = reader.CurrentRecord[0];
                    string asin = reader.CurrentRecord[1];
                    string image1 = reader.CurrentRecord[2];
                    string image2 = reader.CurrentRecord[3];

                    var diff = GetDifferences(image1, image2, 6, true);

                    if (diff * 100 > 5)
                    {
                        itemResults.Add(id);
                        Console.WriteLine(id + ",");
                    }
                }
            }
            return itemResults;
        }


        private decimal GetPercentageDifference(Bitmap img1, 
            Bitmap img2, 
            int threashold,
            bool onlyColorDistance)
        {
            decimal diff = 0;
            decimal[] image1ColorSum = new decimal[3];
            decimal[] image2ColorSum = new decimal[3];

            var sizeX = 16;
            var sizeY = 16;
            var image1ColorMatrix = new decimal[sizeX, sizeY, 3];
            var image2ColorMatrix = new decimal[sizeX, sizeY, 3];

            var background = Color.White;

            var height = Math.Min(img1.Height, img2.Height);
            var width = Math.Min(img1.Width, img2.Width);

            var dx = width/(float)sizeX;
            var dy = height/(float)sizeY;
            var indexX = 0;
            var indexY = 0;
            var maxX = dx;
            var maxY = dy;
            Color pixel1;
            Color pixel2;

            for (int y = 0; y < height; y++)
            {
                if (y > maxY)
                {
                    indexY ++;
                    maxY += dy;
                    if (indexY > sizeY - 1)
                        indexY = sizeY - 1;
                }
                indexX = 0;
                maxX = dx;

                for (int x = 0; x < width; x++)
                {
                    if (x > maxX)
                    {
                        indexX ++;
                        maxX += dx;

                        if (indexX > sizeX - 1)
                            indexX = sizeX - 1;
                    }

                    pixel1 = img1.GetPixel(x, y);
                    pixel2 = img2.GetPixel(x, y);

                    image1ColorSum[0] += (decimal)Math.Abs(pixel1.R) / 255;
                    image1ColorSum[1] += (decimal)Math.Abs(pixel1.G) / 255;
                    image1ColorSum[2] += (decimal) Math.Abs(pixel1.B)/255;

                    image2ColorSum[0] += (decimal)Math.Abs(pixel2.R) / 255;
                    image2ColorSum[1] += (decimal)Math.Abs(pixel2.G) / 255;
                    image2ColorSum[2] += (decimal)Math.Abs(pixel2.B) / 255;


                    //if (background.R == pixel1.R
                    //    && background.G == pixel1.G
                    //    && background.B == pixel1.B
                    //    && background.R == pixel2.R
                    //    && background.G == pixel2.G
                    //    && background.B == pixel2.B)
                    //    continue;

                    image1ColorMatrix[indexX, indexY, 0] += pixel1.A;
                    image1ColorMatrix[indexX, indexY, 1] += pixel1.G;
                    image1ColorMatrix[indexX, indexY, 2] += pixel1.B;

                    image2ColorMatrix[indexX, indexY, 0] += pixel2.A;
                    image2ColorMatrix[indexX, indexY, 1] += pixel2.G;
                    image2ColorMatrix[indexX, indexY, 2] += pixel2.B;
                }
            }

            var colorRDiff = (Math.Abs(image1ColorSum[0] - image2ColorSum[0])) / ((width * height));
            var colorGDiff = (Math.Abs(image1ColorSum[1] - image2ColorSum[1])) / ((width * height));
            var colorBDiff = (Math.Abs(image1ColorSum[2] - image2ColorSum[2])) / ((width * height));
            var colorDiff = (colorRDiff + colorGDiff + colorBDiff)/(decimal) 3;
            if (colorDiff < threashold / 100M || onlyColorDistance) 
                return colorDiff;

            decimal colorMatrixDiff = 0;
            var cellSize = Math.Ceiling(dx)*Math.Ceiling(dy);
            for (var i = 0; i < sizeX; i++)
            {
                for (var j = 0; j < sizeY; j++)
                {
                    var d = Math.Abs(image1ColorMatrix[i, j, 0] - image2ColorMatrix[i, j, 0]) / ((decimal) cellSize * 255)
                    + Math.Abs(image1ColorMatrix[i, j, 1] - image2ColorMatrix[i, j, 1]) / ((decimal)cellSize * 255)
                    + Math.Abs(image1ColorMatrix[i, j, 2] - image2ColorMatrix[i, j, 2]) / ((decimal)cellSize * 255);

                    colorMatrixDiff += d;
                }
            }

            return colorMatrixDiff / (sizeX * sizeY);
        }

        public decimal GetDifferences(string imageUrl1, 
            string imageUrl2,
            int threshold,
            bool onlyColorDistance)
        {
            using (var image1Stream = ImageHelper.DownloadRemoteImageFileAsStream(imageUrl1))
            {
                using (var image2Stream = ImageHelper.DownloadRemoteImageFileAsStream(imageUrl2))
                {
                    using (MagickImage image1 = new MagickImage(image1Stream))
                    {
                        using (MagickImage image2 = new MagickImage(image2Stream))
                        {
                            //MagickGeometry size = new MagickGeometry(50, 50);
                            //size.FillArea = true;

                            image1.ColorFuzz = new Percentage(4);
                            image1.Trim();
                            //image1.RePage();
                            //image1.Resize(size);
                            //image1.RePage();
                            image1.Scale(400, 400);

                            //image1.Write(Path.GetFileName(imageUrl1));

                            image2.ColorFuzz = new Percentage(4);
                            image2.Trim();
                            //image2.RePage();
                            //image2.Resize(size);
                            //image2.RePage();
                            image2.Scale(400, 400);

                            //image2.Write(Path.GetFileName(imageUrl2));

                            decimal difference = 0;
                            //Using Magick
                            //image1.ColorFuzz = new Percentage(5);
                            //image2.ColorFuzz = new Percentage(5);
                            //var difference = image1.Compare(image2, ErrorMetric.Fuzz);

                            //Using Fun library
                            //var image1Image = (Image) image1.ToBitmap();
                            //var image2Image = (Image) image2.ToBitmap();
                            //var difference = image1Image.PercentageDifference(image2Image, 5);

                            //Own method
                            using (var bitmap1 = image1.ToBitmap())
                            {
                                using (var bitmap2 = image2.ToBitmap())
                                {
                                    difference = GetPercentageDifference(bitmap1,
                                        bitmap2,
                                        threshold,
                                        onlyColorDistance);
                                }
                            }

                            return difference;
                        }
                    }
                }
            }
        }

        public void TestSamples()
        {
            //Images with closest by color pictures (diff layout)
            var difference = GetDifferences("http://ecx.images-amazon.com/images/I/51DMyl-Df6L.jpg",
                "http://ecx.images-amazon.com/images/I/51sTRTmx0jL.jpg", 5, true);
            Console.WriteLine(difference);

            //Equal images
            difference = GetDifferences("http://ecx.images-amazon.com/images/I/41uOlQDRcjL.jpg",
    "http://ecx.images-amazon.com/images/I/512%2BYKsYUUL.jpg", 5, true);
            Console.WriteLine(difference);

            //Different by color and layout
            difference = GetDifferences("http://ecx.images-amazon.com/images/I/51CDlZ5xqXL.jpg",
                "http://ecx.images-amazon.com/images/I/51funtIQAYL.jpg", 5, true);
            Console.WriteLine(difference);
        }

        public void UpdateDifferenceForAllImages(MarketType market,
            string marketplaceId,
            string[] asinList)
        {
            //MagickNET.SetTempDirectory(@"C:\Temp");
            //var lastUpdateTo = _time.GetAppNowTime().AddDays(-3);

            using (var db = _dbFactory.GetRWDb())
            {
                var itemImagesQuery = from im in db.ItemImages.GetAll() 
                                join i in db.Items.GetAll() on im.ItemId equals i.Id
                                join s in db.Styles.GetAll() on i.StyleId equals s.Id
                                where i.Market == (int)market
                                    && (i.PrimaryImage != im.Image
                                        || i.PrimaryImage != s.Image
                                        || !im.DiffWithLocalImageValue.HasValue
                                        || !im.DiffWithStyleImageValue.HasValue)
                                select new
                                {
                                    Id = im.Id,
                                    Market = i.Market,
                                    MarketplaceId = i.MarketplaceId,
                                    ASIN = i.ASIN,
                                    ParentASIN = i.ParentASIN,

                                    StyleString = s.StyleID,
                                    StyleId = s.Id,
                                    LargeImage = im.Image,

                                    LocalImage = i.PrimaryImage,
                                    ComparedLocalImage = im.ComparedLocalImage,
                                    DiffWithLocalImageValue = im.DiffWithLocalImageValue,

                                    StyleImage = s.Image,
                                    ComparedStyleImage = im.ComparedStyleImage,
                                    DiffWithStyleImageValue = im.DiffWithStyleImageValue,
                                };

                if (!String.IsNullOrEmpty(marketplaceId))
                    itemImagesQuery = itemImagesQuery.Where(i => i.MarketplaceId == marketplaceId);
                
                if (asinList != null && asinList.Any())
                    itemImagesQuery = itemImagesQuery.Where(i => asinList.Contains(i.ASIN));

                var itemImages = itemImagesQuery.ToList();
                
                var dbImages = db.ItemImages.GetAll().ToList();
                var index = 0;
                foreach (var itemImage in itemImages)
                {
                    var dbImage = dbImages.FirstOrDefault(im => im.Id == itemImage.Id);
                    try
                    {
                        var diffWithLocal = 0M;
                        var diffWithStyle = 0M;

                        if (market == MarketType.Amazon || market == MarketType.AmazonEU || market == MarketType.AmazonAU)
                        {
                            var localImageUrl = ConvertToSL(itemImage.LocalImage);
                            var largeImageUrl = ConvertToSL(itemImage.LargeImage);
                            var styleImageUrl = ConvertToSL(itemImage.StyleImage);

                            diffWithLocal = GetDifferences(localImageUrl, largeImageUrl, 6, true);
                            diffWithStyle = GetDifferences(styleImageUrl, largeImageUrl, 6, true);
                        }
                        if (market == MarketType.Walmart
                            || market == MarketType.WalmartCA
                            || market == MarketType.Shopify
                            || market == MarketType.Jet)
                        {
                            var styleImageUrl = ImageHelper.BuildWalmartImage(_walmartImageDirectory, 
                                itemImage.StyleImage,
                                itemImage.StyleString + "_" + itemImage.StyleId + "_" + itemImage.Id);
                            var largeImageUrl = ConvertToSL(itemImage.LargeImage);

                            diffWithLocal = GetDifferences(styleImageUrl, largeImageUrl, 6, true);
                            diffWithStyle = diffWithLocal;
                        }
                        
                        _log.Info("Market=" + market 
                            + ", ASIN=" + itemImage.ASIN 
                            + ", diffWithLocal=" + diffWithLocal 
                            + ", diffWithStyle=" + diffWithStyle);
                        
                        if (dbImage != null)
                        {
                            dbImage.DiffWithStyleImageValue = diffWithStyle;
                            dbImage.DiffWithStyleImageUpdateDate = _time.GetAppNowTime();
                            dbImage.ComparedStyleImage = itemImage.StyleImage;

                            dbImage.DiffWithLocalImageValue = diffWithLocal;
                            dbImage.DiffWithLocalImageUpdateDate = _time.GetAppNowTime();
                            dbImage.ComparedLocalImage = itemImage.LocalImage;

                            db.Commit();
                        }
                    }
                    catch (Exception ex)
                    {
                        dbImage.DiffWithStyleImageValue = null;
                        dbImage.DiffWithStyleImageUpdateDate = null;
                        dbImage.ComparedStyleImage = null;

                        dbImage.DiffWithLocalImageValue = null;
                        dbImage.DiffWithLocalImageUpdateDate = null;
                        dbImage.ComparedLocalImage = null;

                        db.Commit();

                        _log.Info("Error processing ASIN=" + itemImage.ASIN + ", message=" + ex.Message);
                    }

                    index ++;
                    if (index % 100 == 0)
                        db.Commit();
                }
                db.Commit();
            }
        }

        private const string DestSize = "_UL400_";

        private static string ConvertToSL(string image)
        {
            if (!String.IsNullOrEmpty(image))
            {
                if (image.Contains("images-amazon.com"))
                {
                    if (image.Contains("_UL1500"))
                        return image.Replace("_UL1500_", DestSize);
                    if (!image.Contains("_.") && !image.Contains("._"))
                        return image.Replace(".jpg", "." + DestSize + ".jpg");
                }
            }
            return image;
        }
    }
}
