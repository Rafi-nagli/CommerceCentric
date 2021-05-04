using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Amazon.Common.Helpers;
using Amazon.Core;
using Amazon.Core.Contracts;
using Amazon.Core.Models;
using Amazon.Core.Models.Calls;
using Amazon.Web.Models;

namespace Amazon.Web.ViewModels.Products
{
    public class MarketImageViewModel
    {
        public long Id { get; set; }
        public string ASIN { get; set; }
        public string SourceMarketId { get; set; }

        public string Name { get; set; }

        public int Market { get; set; }
        public string MarketplaceId { get; set; }

        public string LocalImage { get; set; }

        public string StyleImage { get; set; }
        public int? StyleImageType { get; set; }

        public string MarketImage { get; set; }

        public bool StyleImageFromAmazon
        {
            get { return String.IsNullOrEmpty(StyleImage) || ImageHelper.IsAmazonImageUrl(StyleImage); }
        }

        public decimal? DiffWithLocalImageValue { get; set; }
        public decimal? DiffWithStyleImageValue { get; set; }
        public bool ImagesIgnored { get; set; }

        public string StyleString { get; set; }
        public long? StyleId { get; set; }
        public string StyleSize { get; set; }

        public bool IsHiResStyleImage 
        {
            get { return StyleImageType == (int) Core.Models.StyleImageType.HiRes; }
        }

        public string StyleUrl
        {
            get { return UrlHelper.GetStyleUrl(StyleString); }
        }


        public bool HasLocalImage
        {
            get { return !String.IsNullOrEmpty(LocalImage) && LocalImage != "#"; }
        }

        public bool HasStyleImage
        {
            get { return !String.IsNullOrEmpty(StyleImage) && StyleImage != "#"; }
        }

        public bool HasMarketImage
        {
            get { return !String.IsNullOrEmpty(MarketImage) && MarketImage != "#"; }
        }

        public string LocalImageThumbnail
        {
            get
            {
                return UrlHelper.GetThumbnailUrl(LocalImage, 0, 75, false, ImageHelper.NO_IMAGE_URL, convertInDomainUrlToThumbnail: true);
            }
        }

        public string StyleImageThumbnail
        {
            get
            {
                return UrlHelper.GetThumbnailUrl(StyleImage, 0, 75, false, ImageHelper.NO_IMAGE_URL, convertInDomainUrlToThumbnail: true);
            }
        }


        public string MarketImageThumbnail
        {
            get
            {
                return UrlHelper.GetThumbnailUrl(MarketImage, 0, 75, false, ImageHelper.NO_IMAGE_URL, convertInDomainUrlToThumbnail: true);
            }
        }

        public string ProductUrl
        {
            get
            {
                return UrlHelper.GetProductUrl(ASIN, (MarketType)Market, MarketplaceId);
            }
        }

        public string MarketUrl
        {
            get
            {
                return UrlHelper.GetMarketUrl(ASIN, SourceMarketId, (MarketType)Market, MarketplaceId);
            }
        }


        public static IQueryable<MarketImageViewModel> GetAll(IUnitOfWork db, 
            MarketType market,
            string marketplaceId,
            string keywords,
            bool onlyIgnored)
        {
            var styleDefaultImg = from im in db.StyleImages.GetAll()
                where im.IsDefault
                select im;

            var baseQuery = from i in db.Items.GetAll()
                            join s in db.Styles.GetAll() on i.StyleId equals s.Id

                            join sImg in styleDefaultImg on s.Id equals sImg.StyleId into withStyleImage
                            from sImg in withStyleImage.DefaultIfEmpty()

                            join im in db.ItemImages.GetAll() on i.Id equals im.ItemId into withImage
                            from im in withImage.DefaultIfEmpty()

                            where i.Market == (int)market
                            select new { im, i, s, sImg };

            if (!String.IsNullOrEmpty(marketplaceId))
                baseQuery = baseQuery.Where(m => m.i.MarketplaceId == marketplaceId);

            if (!String.IsNullOrEmpty(keywords))
                baseQuery = baseQuery.Where(m => m.i.ASIN.Contains(keywords)
                                                 || m.s.StyleID.Contains(keywords));

            if (onlyIgnored)
                baseQuery = baseQuery.Where(m => m.i.ImagesIgnored);

            var query = baseQuery.Select(i => new MarketImageViewModel()
                {
                    Id = i.i.Id,
                    ASIN = i.i.ASIN,
                    SourceMarketId = i.i.SourceMarketId,
                    Market = i.i.Market,
                    MarketplaceId = i.i.MarketplaceId,
                    LocalImage = i.i.PrimaryImage,
                    MarketImage = i.im.Image,
                    StyleImage = i.sImg.Image,
                    StyleImageType = i.sImg.Type,
                    Name = i.s.Name,
                    DiffWithLocalImageValue =i.im.DiffWithLocalImageValue,
                    DiffWithStyleImageValue = i.im.DiffWithStyleImageValue,
                    ImagesIgnored = i.i.ImagesIgnored,

                    StyleId = i.i.StyleId,
                    StyleString = i.s.StyleID,
                    StyleSize = i.i.Size,
                });

            return query;
        }

        public static CallResult<string> ReplaceStyleImage(IUnitOfWork db,
            ILogService log,
            long itemImageId,
            DateTime when,
            long? by)
        {
            var images = db.ItemImages.GetAll()
                            .Where(im => im.Id == itemImageId
                                && im.ImageType == (int)ProductImageType.Large)
                            .ToList();
            if (images.Any())
            {
                var itemId = images[0].ItemId;
                var item = db.Items.GetAll().FirstOrDefault(i => i.Id == itemId);
                if (item != null
                    && item.StyleId.HasValue)
                {
                    var style = db.Styles.Get(item.StyleId.Value);
                    var styleDefaultImage = db.StyleImages.GetAll().FirstOrDefault(im => im.StyleId == style.Id && im.IsDefault);
                    var newImage = ImageHelper.RemoveAmazonImagePostfix(images[0].Image);
                    style.Image = newImage;
                    if (styleDefaultImage != null)
                    {
                        log.Info("Style image was replaced, from=" + styleDefaultImage.Image + ", to=" + images[0].Image);
                        styleDefaultImage.Image = newImage;
                        styleDefaultImage.Type = (int) Core.Models.StyleImageType.HiRes;
                        styleDefaultImage.Tag = "UI replaced";
                        styleDefaultImage.UpdateDate = when;
                        styleDefaultImage.UpdatedBy = by;
                    }

                    db.Commit();

                    var items = db.Items.GetAll().Where(i => i.StyleId == style.Id
                                                             && (i.ItemPublishedStatus == (int) PublishedStatuses.ChangesSubmited
                                                              || i.ItemPublishedStatus == (int) PublishedStatuses.Published
                                                              || i.ItemPublishedStatus == (int) PublishedStatuses.PublishedInProgress
                                                              || i.ItemPublishedStatus == (int) PublishedStatuses.PublishingErrors
                                                              || i.ItemPublishedStatus == (int) PublishedStatuses.PublishedInactive)).ToList();
                    log.Info("Listings to resubmit, count=" + items.Count());
                    items.ForEach(i => i.ItemPublishedStatus = (int) PublishedStatuses.HasChanges);
                    db.Commit();

                    return CallResult<string>.Success(images[0].Image);
                }
            }

            return CallResult<string>.Fail("Style listings do not have a suitable Hi-res images", null);
        }

        public static void ResetItemLargeImage(IUnitOfWork db, 
            ILogService log,
            long itemId,
            bool includeParent)
        {
            var images = db.ItemImages.GetAll().Where(im => im.ItemId == itemId).ToList();
            foreach (var image in images)
            {
                db.ItemImages.Remove(image);
                log.Info("Removed image: " + image.Image + ", itemId=" + itemId);
            }
            db.Commit();

            if (includeParent)
            {
                var item = db.Items.GetAll().FirstOrDefault(i => i.Id == itemId);
                if (item != null)
                {
                    var parent = db.ParentItems.GetAllAsDto().FirstOrDefault(p => p.ASIN == item.ParentASIN
                                                                                  && p.Market == item.Market
                                                                                  && (p.MarketplaceId == item.MarketplaceId || String.IsNullOrEmpty(item.MarketplaceId)));
                    if (parent != null)
                    {
                        var parentImages = db.ParentItemImages.GetAll().Where(im => im.ParentItemId == parent.Id).ToList();
                        foreach (var parentImage in parentImages)
                        {
                            db.ParentItemImages.Remove(parentImage);
                            log.Info("Remove parent image: " + parentImage.Image + ", parentASIN=" + parent.ASIN);
                        }

                        var dbItems = db.Items.GetAll().Where(i => i.ParentASIN == parent.ASIN
                                                                   && i.Market == parent.Market
                                                                   && (i.MarketplaceId == parent.MarketplaceId ||
                                                                    String.IsNullOrEmpty(parent.MarketplaceId))).ToList();

                        var itemIdList = dbItems.Select(i => (long)i.Id).ToList();
                        var dbImages = db.ItemImages.GetAll().Where(im => itemIdList.Contains(im.ItemId)).ToList();
                        foreach (var dbImage in dbImages)
                        {
                            db.ItemImages.Remove(dbImage);
                            log.Info("Removed image: " + dbImage.Image + ", itemId=" + dbImage.ItemId);
                        }

                        db.Commit();
                    }
                }
            }
        }

        public static void ResetItemImageDiff(IUnitOfWork db,
            ILogService log,
            long id)
        {
            var itemImage = db.ItemImages.GetAll().FirstOrDefault(i => i.Id == id);
            if (itemImage != null)
            {
                log.Info("Reset item image diff, local: " + itemImage.DiffWithLocalImageValue + ", style: " + itemImage.DiffWithStyleImageValue);
                itemImage.DiffWithLocalImageValue = null;
                itemImage.DiffWithLocalImageUpdateDate = null;

                itemImage.DiffWithStyleImageValue = null;
                itemImage.DiffWithStyleImageUpdateDate = null;

                db.Commit();
            }
        }

        public static void ToggleIgnoreItemImage(IUnitOfWork db,
            ILogService log,
            long itemId,
            bool isIgnore)
        {
            var item = db.Items.GetAll().FirstOrDefault(i => i.Id == itemId);
            if (item != null)
            {
                log.Info("Set is ignore=" + isIgnore + ", itemId=" + item.Id);
                item.ImagesIgnored = isIgnore;
                db.Commit();
            }
        }
    }
}