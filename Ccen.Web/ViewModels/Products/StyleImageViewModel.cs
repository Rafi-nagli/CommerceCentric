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
    public class StyleImageViewModel
    {
        public long? Id { get; set; }

        public string StyleImage { get; set; }
        public int? StyleImageType { get; set; }
        public decimal? DiffWithStyleImage { get; set; }

        public string MarketImage { get; set; }

        public bool ImagesIgnored { get; set; }

        public string StyleString { get; set; }
        public long StyleId { get; set; }

        public int? RemainingQuantity { get; set; }

        public bool IsStyleImageFromAmazon
        {
            get { return String.IsNullOrEmpty(StyleImage) || ImageHelper.IsAmazonImageUrl(StyleImage); }
        }

        public bool IsHiResStyleImage 
        {
            get { return StyleImageType == (int) Core.Models.StyleImageType.HiRes; }
        }

        public string StyleUrl
        {
            get { return UrlHelper.GetStyleUrl(StyleString); }
        }

        public bool HasStyleImage
        {
            get { return !String.IsNullOrEmpty(StyleImage) && StyleImage != "#"; }
        }

        public bool HasMarketImage
        {
            get { return !String.IsNullOrEmpty(MarketImage) && MarketImage != "#"; }
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
        
        public static IQueryable<StyleImageViewModel> GetAll(IUnitOfWork db, 
            string keywords,
            bool styleWithLoRes,
            bool withMarketHiRes)
        {
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
                                    select new
                                    {
                                        Id = im.Id,
                                        StyleId = i.StyleId,
                                        Image = im.Image,
                                        Rank = pi.Rank ?? RankHelper.DefaultRank,
                                        DiffWithStyleImage = im.DiffWithStyleImageValue,
                                        CreateDate = i.CreateDate
                                    };

            var allItemImage = allItemImageQuery.ToList();

            var groupedItemImageByStyle = (from i in allItemImage
                group i by i.StyleId
                into byStyle
                select new
                {
                    StyleId = byStyle.Key,
                    ImageObj = byStyle.OrderBy(sk => sk.Rank).ThenBy(sk => sk.Image).FirstOrDefault(),
                }).ToList();

            var styleDefaultImg = from im in db.StyleImages.GetAll()
                where im.IsDefault
                select im;

            var styleQtyQuery = from si in db.StyleItemCaches.GetAll()
                group si by si.StyleId
                into byStyle
                select new
                {
                    StyleId = byStyle.Key,
                    Quantity = byStyle.Sum(i => i.RemainingQuantity)
                };

            var styleQuery = from s in db.Styles.GetAll() 
                             join sQty in styleQtyQuery on s.Id equals sQty.StyleId

                             join sImg in styleDefaultImg on s.Id equals sImg.StyleId into withStyleImage
                             from sImg in withStyleImage.DefaultIfEmpty()


                            select new { s, sImg, sQty };
            
            if (!String.IsNullOrEmpty(keywords))
                styleQuery = styleQuery.Where(m => m.s.StyleID.Contains(keywords));

            if (styleWithLoRes)
                styleQuery = styleQuery.Where(m => m.sImg.Type != (int) Core.Models.StyleImageType.HiRes
                    && m.sImg.Image.Contains("amazon.com")
                    && m.sQty.Quantity > 0);


            var baseItems = styleQuery.ToList();

            var resultQuery = from s in baseItems
                join im in groupedItemImageByStyle on s.s.Id equals im.StyleId into withItemImage
                from im in withItemImage.DefaultIfEmpty()
                select new {s, im};

            var query = resultQuery.Select(i => new StyleImageViewModel()
                {
                    Id = i.im?.ImageObj?.Id,
                    MarketImage = i.im?.ImageObj?.Image,
                    StyleImage = i.s.sImg?.Image,
                    StyleImageType = i.s.sImg?.Type,
                    DiffWithStyleImage = i.im?.ImageObj?.DiffWithStyleImage,

                    StyleId = i.s.s.Id,
                    StyleString = i.s.s.StyleID,
                    RemainingQuantity = i.s.sQty?.Quantity,
                }).ToList();


            if (withMarketHiRes)
                query = query.Where(m => !String.IsNullOrEmpty(m.MarketImage)).ToList();

            return query.AsQueryable();
        }

        public static CallResult<string> ReplaceStyleImage(IUnitOfWork db,
            ILogService log,
            long itemId,
            DateTime when,
            long? by)
        {
            var images = db.ItemImages.GetAll()
                            .Where(im => im.ItemId == itemId 
                                && im.ImageType == (int)ProductImageType.Large)
                            .ToList();

            var item = db.Items.GetAll().FirstOrDefault(i => i.Id == itemId);
            if (item != null 
                && item.StyleId.HasValue
                && images.Any())
            {
                var style = db.Styles.Get(item.StyleId.Value);
                var styleDefaultImage = db.StyleImages.GetAll().FirstOrDefault(im => im.StyleId == style.Id && im.IsDefault);
                style.Image = images[0].Image;
                if (styleDefaultImage != null)
                {
                    log.Info("Style image was replaced, from=" + styleDefaultImage.Image + ", to=" + images[0].Image);
                    styleDefaultImage.Image = images[0].Image;
                    styleDefaultImage.Type = (int)Core.Models.StyleImageType.HiRes;
                    styleDefaultImage.Tag = "UI replaced";
                    styleDefaultImage.UpdateDate = when;
                    styleDefaultImage.UpdatedBy = by;
                }

                db.Commit();

                var items = db.Items.GetAll().Where(i => i.StyleId == style.Id
                            && (i.ItemPublishedStatus == (int)PublishedStatuses.ChangesSubmited
                                || i.ItemPublishedStatus == (int)PublishedStatuses.Published
                                || i.ItemPublishedStatus == (int)PublishedStatuses.PublishedInProgress
                                || i.ItemPublishedStatus == (int)PublishedStatuses.PublishingErrors
                                || i.ItemPublishedStatus == (int)PublishedStatuses.PublishedInactive)).ToList();
                log.Info("Listings to resubmit, count=" + items.Count());
                items.ForEach(i => i.ItemPublishedStatus = (int)PublishedStatuses.HasChanges);
                db.Commit();

                return CallResult<string>.Success(images[0].Image);
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