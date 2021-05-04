using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using Amazon.Common.Helpers;
using Amazon.Common.Models;
using Amazon.Core;
using Amazon.Core.Contracts;
using Amazon.Core.Contracts.Factories;
using Amazon.Core.Entities;
using Amazon.Core.Entities.Inventory;
using Amazon.Core.Entities.Listings;
using Amazon.Core.Models;
using Amazon.Core.Models.Calls;
using Amazon.Core.Views;
using Amazon.DTO;
using Amazon.DTO.Listings;
using Amazon.Model.Implementation.Markets;
using Amazon.Utils;

namespace Amazon.Model.Implementation
{
    public class ImageManager
    {
        private ILogService _log;
        private ITime _time;
        private IHtmlScraperService _htmlScraper;
        private IDbFactory _dbFactory;

        public ImageManager(ILogService log,
            IHtmlScraperService htmlScraper,
            IDbFactory dbFactory,
            ITime time)
        {
            _log = log;
            _time = time;
            _htmlScraper = htmlScraper;
            _dbFactory = dbFactory;
        }

        public void UpdateStyleImageTypes()
        {
            using (var db = _dbFactory.GetRWDb())
            {
                var styleImages = db.StyleImages.GetAll().Where(si => si.Type == (int)StyleImageType.None).ToList();
                _log.Info("Images to detect type: " + styleImages.Count);
                foreach (var styleImage in styleImages)
                {
                    if (styleImage.Type == (int) StyleImageType.None)
                    {
                        var size = ImageHelper.GetImageSize(styleImage.Image);

                        var styleImageType = ImageHelper.GetImageTypeBySize(size);
                        styleImage.Type = (int) styleImageType;
                    }
                }
                db.Commit();
            }
        }

        public void UpdateStyleLargeImage()
        {
            using (var db = _dbFactory.GetRWDb())
            {
                db.DisableValidation();

                var allStyleImages = db.StyleImages.GetAll().ToList();
                var allItemImageQuery = from im in db.ItemImages.GetAllAsDto()
                    join i in db.Items.GetAll() on im.ItemId equals i.Id
                    join pi in db.ParentItems.GetAllAsDto() on new {ASIN = i.ParentASIN, i.Market, i.MarketplaceId}
                        equals new {pi.ASIN, pi.Market, pi.MarketplaceId} into withPi
                    from pi in withPi.DefaultIfEmpty()
                    where i.MarketplaceId == MarketplaceKeeper.AmazonComMarketplaceId
                          && !String.IsNullOrEmpty(im.Image)
                          && im.Image != "#"
                    select new
                    {
                        StyleId = i.StyleId,
                        Image = im.Image,
                        Rank = pi != null ? pi.Rank : null,
                        CreateDate = i.CreateDate
                    };

                var allItemImages = allItemImageQuery.ToList();
                var styles = db.Styles.GetAllAsDto().ToList();

                foreach (var style in styles)
                {
                    var styleImages = allStyleImages.Where(si => si.StyleId == style.Id).ToList();
                    if (styleImages.Any(si => si.Type == (int) StyleImageType.HiRes && !si.IsSystem))
                    {
                        //Remove hi-res isSystem if exist
                        var systemStyleImages =
                            styleImages.Where(si => si.Type == (int) StyleImageType.HiRes && si.IsSystem).ToList();
                        foreach (var image in systemStyleImages)
                        {
                            db.StyleImages.Remove(image);
                        }
                    }
                    else
                    {
                        //var styleItemIds = allItemImages.Where(i => i.StyleId == style.Id).Select(i => (long) i.Id).ToList();
                        var styleItemImage = allItemImages
                            .OrderBy(im => im.Rank ?? 0)
                            .ThenByDescending(im => im.CreateDate)
                            .FirstOrDefault(im => im.StyleId == style.Id);

                        var existSystemStyleImages =
                            styleImages.Where(si => si.Type == (int) StyleImageType.HiRes && si.IsSystem).ToList();
                        if (existSystemStyleImages.Any())
                        {
                            if (styleItemImage != null)
                            {
                                var styleImage = ImageHelper.RemoveAmazonImagePostfix(styleItemImage.Image);
                                if (existSystemStyleImages[0].Image != styleImage)
                                {
                                    _log.Info("Update style image, styleId=" + style.StyleID + ", image=" + styleImage);

                                    existSystemStyleImages[0].Image = styleImage;
                                    existSystemStyleImages[0].UpdateDate = _time.GetAppNowTime();
                                }
                            }
                        }
                        else
                        {
                            if (styleItemImage != null)
                            {
                                var styleImage = ImageHelper.RemoveAmazonImagePostfix(styleItemImage.Image); 
                                _log.Info("Add style image, styleId=" + style.StyleID + ", image=" + styleImage);
                                db.StyleImages.Add(new StyleImage()
                                {
                                    StyleId = style.Id,
                                    Image = styleImage,
                                    Type = (int) StyleImageType.HiRes,
                                    IsSystem = true,
                                    CreateDate = _time.GetAppNowTime(),
                                });
                            }
                        }
                    }

                    db.Commit();
                }
            }
        }

        public void ReplaceStyleLargeImage()
        {
            using (var db = _dbFactory.GetRWDb())
            {
                db.DisableValidation();

                var allStyleImages = db.StyleImages.GetAll().ToList();
                var allItemImageQuery = from im in db.ItemImages.GetAll()
                                        join i in db.Items.GetAll() on im.ItemId equals i.Id
                                        join st in db.Styles.GetAll() on i.StyleId equals st.Id
                                        join pi in db.ParentItems.GetAllAsDto() on new { ASIN = i.ParentASIN, i.Market, i.MarketplaceId }
                                            equals new { pi.ASIN, pi.Market, pi.MarketplaceId } into withPi
                                        from pi in withPi.DefaultIfEmpty()
                                        where i.MarketplaceId == MarketplaceKeeper.AmazonComMarketplaceId
                                              && !String.IsNullOrEmpty(im.Image)
                                              && im.Image != "#"
                                              && im.ComparedStyleImage == st.Image
                                              && im.DiffWithStyleImageValue < 0.01M
                                        select new
                                        {
                                            StyleId = i.StyleId,
                                            Image = im.Image,
                                            Rank = pi != null ? pi.Rank : null,
                                            CreateDate = i.CreateDate
                                        };

                var allItemImages = allItemImageQuery.ToList();
                var styleIdList = allItemImages.Select(i => i.StyleId).ToList();
                var allStyles = db.Styles.GetAll().ToList();
                var activeStyles = allStyles.Where(s => styleIdList.Contains(s.Id)).ToList();

                var index = 0;
                foreach (var style in activeStyles)
                {
                    _log.Info(index + " - StyleId: " + style.StyleID);
                    var styleImages = allStyleImages.Where(si => si.StyleId == style.Id).ToList();
                    var defaultStyleImage = styleImages.OrderByDescending(im => im.IsDefault).FirstOrDefault();

                    var imageUpdated = false;

                    if (defaultStyleImage != null
                        && defaultStyleImage.Type != (int) StyleImageType.HiRes
                        && ImageHelper.IsAmazonImageUrl(defaultStyleImage.Image))
                    {
                        //var styleItemIds = allItemImages.Where(i => i.StyleId == style.Id).Select(i => (long) i.Id).ToList();
                        var newStyleImage = allItemImages
                            .OrderBy(im => im.Rank ?? RankHelper.DefaultRank)
                            .ThenByDescending(im => im.CreateDate)
                            .FirstOrDefault(im => im.StyleId == style.Id);

                        if (newStyleImage != null)
                        {
                            var newImage = ImageHelper.RemoveAmazonImagePostfix(newStyleImage.Image);
                            defaultStyleImage.Image = newImage;
                            defaultStyleImage.Type = (int) StyleImageType.HiRes;
                            defaultStyleImage.Tag = "replaced to hi-res";
                            defaultStyleImage.UpdateDate = _time.GetAppNowTime();
                            defaultStyleImage.UpdatedBy = null;

                            _log.Info("Style=" + style.StyleID + ", id=" + style.Id + ", original image=" + style.Image +
                                      ", new image=" + newImage);
                            style.Image = newImage;

                            db.Commit();

                            imageUpdated = true;
                        }
                        else
                        {
                            _log.Info("No new image");
                        }
                    }
                    else
                    {
                        _log.Info("Skipped");
                    }

                    if (imageUpdated)
                    {
                        var items = db.Items.GetAll().Where(i => i.StyleId == style.Id
                            && (i.ItemPublishedStatus == (int)PublishedStatuses.ChangesSubmited
                                || i.ItemPublishedStatus == (int)PublishedStatuses.Published
                                || i.ItemPublishedStatus == (int)PublishedStatuses.PublishedInProgress
                                || i.ItemPublishedStatus == (int)PublishedStatuses.PublishingErrors
                                || i.ItemPublishedStatus == (int)PublishedStatuses.PublishedInactive)).ToList();
                        _log.Info("Listings to resubmit, count=" + items.Count());
                        items.ForEach(i => i.ItemPublishedStatus = (int)PublishedStatuses.HasChanges);
                        db.Commit();
                    }

                    index++;
                }
            }
        }

        public void UpdateItemsLargeImages(MarketType market, string marketplaceId, IList<string> asinList, DateTime? expiredFrom)
        {
            using (var db = _dbFactory.GetRWDb())
            {
                db.DisableValidation();
                
                //Items
                var itemQuery = from i in db.Items.GetAllViewAsDto()
                                join im in db.ItemImages.GetAll() on i.Id equals im.ItemId into withImage
                                from im in withImage.DefaultIfEmpty()
                                where !String.IsNullOrEmpty(i.ASIN)
                                      && (i.Market == (int)market)
                                      && (String.IsNullOrEmpty(marketplaceId) || i.MarketplaceId == marketplaceId)
                                orderby i.CreateDate descending 
                                select new { i, im };

                if (asinList != null && asinList.Any())
                {
                    itemQuery = itemQuery.Where(p => asinList.Contains(p.i.ASIN));
                    if (expiredFrom.HasValue)
                        itemQuery = itemQuery.Where(p => p.im == null || (!p.im.UpdateDate.HasValue || p.im.UpdateDate < expiredFrom));// && p.im.CreateDate < expiredFrom));
                }
                else
                {
                    if (expiredFrom.HasValue)
                        itemQuery = itemQuery.Where(p => p.im == null || (!p.im.UpdateDate.HasValue || p.im.UpdateDate < expiredFrom));// && p.im.CreateDate < expiredFrom));
                    else
                        itemQuery = itemQuery.Where(p => p.im == null || String.IsNullOrEmpty(p.im.Image));
                }

                var items = itemQuery.Select(i => i.i).ToList();

                var index = 0;
                var step = 10;
                while (index < items.Count)
                {
                    var stopWatch = Stopwatch.StartNew();
                    var itemsToUpdate = items.Skip(index).Take(step).ToList();

                    _log.Info("Request updating for items, count=" + itemsToUpdate.Count);

                    var images = UpdateForItems(itemsToUpdate);
                    foreach (var item in itemsToUpdate)
                    {
                        var itemImages = images.Where(i => i.Tag == item.Id).ToList();
                        var image = itemImages.Count > 1 ? itemImages.FirstOrDefault(im => im.Color == item.Color) : itemImages.FirstOrDefault();
                        if (image != null)
                        {
                            db.ItemImages.Update(image, _time.GetAppNowTime());
                            db.Commit(); //NOTE: Prevent duplicates when item repeats
                        }
                    }

                    _log.Info("Changes have been commited, time=" + stopWatch.ElapsedMilliseconds);
                    stopWatch.Stop();

                    index += step;
                }
            }
        }

        public void UpdateParentItemsLargeImages(MarketType market, string marketplaceId, IList<string> asinList, DateTime? expiredFrom)
        {
            using (var db = _dbFactory.GetRWDb())
            {
                db.DisableValidation();
                
                //Parents
                var parentQuery = from pi in db.ParentItems.GetAllAsDto()
                    join im in db.ParentItemImages.GetAll() on pi.Id equals im.ParentItemId into withImage
                    from im in withImage.DefaultIfEmpty()
                    where !String.IsNullOrEmpty(pi.ASIN)
                          && (pi.Market == (int)market
                          && (String.IsNullOrEmpty(marketplaceId) || pi.MarketplaceId == marketplaceId))
                    select new {pi, im};

                if (asinList != null && asinList.Any())
                {
                    parentQuery = parentQuery.Where(p => asinList.Contains(p.pi.ASIN));
                    if (expiredFrom.HasValue)
                        parentQuery = parentQuery.Where(p => p.im == null || ((!p.im.UpdateDate.HasValue || p.im.UpdateDate < expiredFrom) && p.im.CreateDate < expiredFrom));
                }
                else
                {
                    if (expiredFrom.HasValue)
                        parentQuery = parentQuery.Where(p => p.im == null || ((!p.im.UpdateDate.HasValue || p.im.UpdateDate < expiredFrom) && p.im.CreateDate < expiredFrom));
                    else
                        parentQuery = parentQuery.Where(p => p.im == null || String.IsNullOrEmpty(p.im.Image));
                }

                var parents = parentQuery.Select(p => p.pi).ToList();
                var parentASINList = parents.Select(pi => pi.ASIN).ToList();
                var allItems = db.Items.GetAllViewAsDto()
                        .Where(i => (i.Market == (int) MarketType.Amazon || i.Market == (int) MarketType.AmazonEU || i.Market == (int)MarketType.AmazonAU)
                                    && parentASINList.Contains(i.ParentASIN)).ToList();

                var index = 0;
                var step = 10;
                while (index < parents.Count)
                {
                    var stopWatch = Stopwatch.StartNew();
                    var parentsToUpdate = parents.Skip(index).Take(step).ToList();
                    
                    _log.Info("Request updating for parentItems, count=" + parentsToUpdate.Count);

                    var images = UpdateForItems(parentsToUpdate.Select(pi => new ItemDTO()
                    {
                        Id = pi.Id,
                        ASIN = pi.ASIN,
                        Market = pi.Market,
                        MarketplaceId = pi.MarketplaceId
                    }).ToList());
                    foreach (var parent in parentsToUpdate)
                    {
                        var parentImages = images.Where(i => i.Tag == parent.Id).ToList();
                        db.ParentItemImages.Update(parentImages.FirstOrDefault(), _time.GetAppNowTime());

                        var childItems = allItems.Where(i => i.ParentASIN == parent.ASIN
                                                           && i.Market == parent.Market
                                                           && i.MarketplaceId == parent.MarketplaceId).ToList();
                        foreach (var item in childItems)
                        {
                            var image = parentImages.Count > 1 ? parentImages.FirstOrDefault(pi => pi.Color == item.Color) : parentImages.FirstOrDefault();
                            if (image != null)
                            {
                                image.Tag = item.Id;
                                db.ItemImages.Update(image, _time.GetAppNowTime());
                                db.Commit(); //NOTE: Prevent duplicates when item repeats
                            }
                        }
                    }

                    _log.Info("Changes have been commited, time=" + stopWatch.ElapsedMilliseconds);
                    stopWatch.Stop();

                    index += step;
                }

            }
        }

        private IList<ImageInfo> UpdateForItems(IList<ItemDTO> items)
        {
            var imageService = new ImageRequestingService(_log, _htmlScraper);
            var resultList = new List<ImageInfo>();
            foreach (var item in items)
            {
                var url = String.Empty;
                if (item.Market == (int)MarketType.Amazon || item.Market == (int)MarketType.AmazonEU || item.Market == (int)MarketType.AmazonAU)
                    url = MarketUrlHelper.GetMarketUrl(item.ASIN, (MarketType)item.Market, item.MarketplaceId);
                if (item.Market == (int)MarketType.Walmart
                    || item.Market == (int)MarketType.WalmartCA
                    || item.Market == (int)MarketType.Jet
                    || item.Market == (int)MarketType.Shopify
                    || item.Market == (int)MarketType.eBay)
                    url = MarketUrlHelper.GetMarketUrl(item.SourceMarketId, (MarketType)item.Market, item.MarketplaceId);

                long downloadedSize = 0;
                CallResult<IList<ImageInfo>> result = null;
                var pageParser = new WebPageParserFactory().GetPageParser((MarketType)item.Market);
                result = imageService.GetMainImageFromUrl(url, pageParser, out downloadedSize);
                
                var imageInfoList = result?.Data ?? new List<ImageInfo>();
                imageInfoList.ForEach(i =>
                {
                    i.Tag = item.Id;
                    i.ImageType = (int)ProductImageType.Large;
                });
                
                if (imageInfoList.Any())
                {
                    _log.Info("Image updated, page size=" + downloadedSize + " asin=" + item.ASIN + ", market=" + item.Market + ", marketplaceId=" +
                              item.MarketplaceId + ", image=" + String.Join("; ", imageInfoList.Select(i => i.Image).ToList()));
                }
                else
                {
                    var image = new ImageInfo();
                    image.Tag = item.Id;

                    if (result.Exception != null
                        && result.Exception.Message.Contains("404")
                        && result.Exception.Message.IndexOf("Not Found", StringComparison.OrdinalIgnoreCase) > 0)
                    {
                        image.UpdateFailAttempts = 100;
                    }
                    else
                    {
                        image.UpdateFailAttempts++;
                    }
                    _log.Info("Image update failed, page size=" + downloadedSize + ", asin=" + item.ASIN + ", market=" + item.Market + ", marketplaceId=" + item.MarketplaceId);

                    imageInfoList.Add(image);
                }

                resultList.AddRange(imageInfoList);


                Thread.Sleep(300);
            }

            return resultList;
        }
    }
}
