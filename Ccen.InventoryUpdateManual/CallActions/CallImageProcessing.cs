using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Amazon.Common.Helpers;
using Amazon.Common.Models;
using Amazon.Core.Contracts;
using Amazon.Core.Contracts.Factories;
using Amazon.Core.Entities.Inventory;
using Amazon.Core.Models;
using Amazon.ImageProcessing;
using Amazon.Model.Implementation;
using Amazon.Model.Implementation.Markets;

namespace Amazon.InventoryUpdateManual.CallActions
{
    public class CallImageProcessing
    {
        private IDbFactory _dbFactory;
        private ITime _time;
        private ILogService _log;
        private IHtmlScraperService _htmlScraper;

        public CallImageProcessing(IDbFactory dbFactory,
            ITime time,
            ILogService log)
        {
            _dbFactory = dbFactory;
            _time = time;
            _log = log;
            _htmlScraper = new HtmlScraperService(_log, _time, _dbFactory);
        }

        public void UpdateAllAmazonImageDifferences()
        {
            var imageService = new ImageProcessingService(_dbFactory, _time, _log, AppSettings.WalmartImageDirectory);
            imageService.UpdateDifferenceForAllImages(MarketType.Amazon, MarketplaceKeeper.AmazonComMarketplaceId, null);
            //var styles = imageServie.GetStylesWithDifferentImages(_dbFactory);
            //Console.WriteLine(styles);
            //var importPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Files/OldNewDbImages.csv");
            //imageService.ProcessImageFile(importPath, _dbFactory);

        }

        public void UpdateAllWalmartImageDifferences()
        {
            var imageService = new ImageProcessingService(_dbFactory, _time, _log, AppSettings.WalmartImageDirectory);
            imageService.UpdateDifferenceForAllImages(MarketType.Walmart, "", null);
            //var styles = imageServie.GetStylesWithDifferentImages(_dbFactory);
            //Console.WriteLine(styles);
            //var importPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Files/OldNewDbImages.csv");
            //imageService.ProcessImageFile(importPath, _dbFactory);

        }

        public void CheckGetLargeImage()
        {
            var html = File.ReadAllText(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Files/Amazon/page_B01FCIJX7K.html"));
            var images = new AmazonPageParser().GetLargeImages(html);

            _log.Info(images.ToString());
        }

        public void CreateSwatchImage()
        {
            var imageFile = ImageHelper.BuildSwatchImage(Path.Combine(AppSettings.LabelDirectory, "SwatchImages"),
    "http://ecx.images-amazon.com/images/I/51WWgu8xefL.jpg");//"http://ecx.images-amazon.com/images/I/516mtJup56L.jpg");
            //_log.Info(imageFile);
        }

        public void CreateWalmartImage()
        {
            var imageFile = ImageHelper.BuildWalmartImage(Path.Combine(AppSettings.LabelDirectory, "WalmartImages"),
                "http://ecx.images-amazon.com/images/I/51WWgu8xefL.jpg",
                "51WWgu8xefL");//"http://ecx.images-amazon.com/images/I/516mtJup56L.jpg");
            //_log.Info(imageFile);
        }


        public void GetLargeImage(string asin, MarketType market, string marketplaceId)
        {
            var url = MarketUrlHelper.GetMarketUrl(asin, market, marketplaceId);
            var imageService = new ImageRequestingService(_log, _htmlScraper);
            var webPageParserFactory = new WebPageParserFactory();
            var pageParser = webPageParserFactory.GetPageParser(market);
            long size = 0;
            var image = imageService.GetMainImageFromUrl(url, pageParser, out size);
            Console.WriteLine(image);
        }

        public void UpdateAllLargeImages()
        {
            while (true)
            {
                var imageManager = new ImageManager(_log, _htmlScraper, _dbFactory, _time);

                imageManager.UpdateItemsLargeImages(MarketType.Amazon, MarketplaceKeeper.AmazonComMarketplaceId, null, null);
            }
        }

        public void UpdateItemHiRes(MarketType market, string marketplaceId, string asin)
        {
            var htmlScraper = new HtmlScraperService(_log, _time, _dbFactory);
            var imageManager = new ImageManager(_log, htmlScraper, _dbFactory, _time);
            imageManager.UpdateItemsLargeImages(market, marketplaceId, new List<string>() { asin }, null);
        }

        public void UpdateAllHiRes()
        {
            var htmlScraper = new HtmlScraperService(_log, _time, _dbFactory);
            var imageManager = new ImageManager(_log, htmlScraper, _dbFactory, _time);

            //var itemASINList = new List<string>();
            //using (var db = _dbFactory.GetRWDb())
            //{
            //    itemASINList = db.Items.GetAllViewAsDto(MarketType.Amazon, MarketplaceKeeper.AmazonComMarketplaceId)
            //        .Where(i => i.ParentASIN == parentASIN)
            //        .Select(i => i.ASIN)
            //        .ToList();
            //}

            imageManager.UpdateItemsLargeImages(MarketType.Amazon, MarketplaceKeeper.AmazonComMarketplaceId, null, DateTime.Today.AddDays(-160));
            //imageManager.UpdateParentItemsLargeImages(MarketType.Amazon, 
            //    new List<string>()
            //{
            //   "B01LYP1ADL",
            //   "B01M0YW6HC"
            //}, null);//, DateTime.Today.AddDays(-7));
        }

        public void UpdateStyleImageTypes()
        {
            var htmlScraper = new HtmlScraperService(_log, _time, _dbFactory);
            var imageManager = new ImageManager(_log, htmlScraper, _dbFactory, _time);
            imageManager.UpdateStyleImageTypes();
        }

        public void UpdateStyleHiResImages()
        {
            var htmlScraper = new HtmlScraperService(_log, _time, _dbFactory);
            var imageManager = new ImageManager(_log, htmlScraper, _dbFactory, _time);
            imageManager.UpdateStyleLargeImage();
        }

        public void ReplaceStyleImageToHiRes()
        {
            var htmlScraper = new HtmlScraperService(_log, _time, _dbFactory);
            var imageManager = new ImageManager(_log, htmlScraper, _dbFactory, _time);
            imageManager.ReplaceStyleLargeImage();
        }

        public void ConvertStyleToStyleImages()
        {
            using (var db = _dbFactory.GetRWDb())
            {
                var allStyles = db.Styles.GetAll().ToList();
                var allStyleImages = db.StyleImages.GetAll().ToList();
                foreach (var style in allStyles)
                {
                    var styleImages = allStyleImages.Where(im => im.StyleId == style.Id).ToList();

                    if (!styleImages.Any())
                    {
                        if (!String.IsNullOrEmpty(style.Image))
                        {
                            db.StyleImages.Add(new StyleImage()
                            {
                                StyleId = style.Id,
                                Image = style.Image,
                                Type = (int)StyleImageType.None,
                                IsDefault = true,
                                CreateDate = _time.GetAppNowTime()
                            });
                        }
                        if (!String.IsNullOrEmpty(style.AdditionalImages))
                        {
                            var images = style.AdditionalImages.Split(",;".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                            foreach (var image in images)
                            {
                                db.StyleImages.Add(new StyleImage()
                                {
                                    StyleId = style.Id,
                                    Image = image,
                                    Type = (int)StyleImageType.None,
                                    IsDefault = false,
                                    CreateDate = _time.GetAppNowTime()
                                });
                            }
                        }

                        db.Commit();
                    }
                }
            }
        }
    }
}
