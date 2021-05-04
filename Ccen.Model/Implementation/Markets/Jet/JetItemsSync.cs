using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Amazon.Common.Helpers;
using Amazon.Core;
using Amazon.Core.Contracts;
using Amazon.Core.Contracts.Factories;
using Amazon.Core.Entities;
using Amazon.Core.Models;
using Amazon.Core.Models.Enums;
using Amazon.DAL;
using Amazon.DTO;
using Amazon.DTO.Inventory;
using Amazon.DTO.Listings;
using Amazon.Model.Implementation.Sync;
using Jet.Api;
using Walmart.Api;

namespace Amazon.Model.Implementation.Markets.Walmart.Feeds
{
    public class JetItemsSync : IItemsSync
    {
        private string _feedBaseDirectory;
        private string _swatchImageDirectory;
        private string _swatchImageBaseUrl;
        private string _jetImageDirectory;
        private string _jetImageBaseUrl;

        private ILogService _log;
        private ITime _time;
        private JetApi _api;
        private IDbFactory _dbFactory;

        public JetItemsSync(ILogService log,
            ITime time,
            JetApi api,
            IDbFactory dbFactory,
            string jetImageDirectory,
            string jetImageBaseUrl)
        {
            _log = log;
            _time = time;
            _api = api;
            _dbFactory = dbFactory;
            
            _jetImageDirectory = jetImageDirectory;
            _jetImageBaseUrl = jetImageBaseUrl;
        }
        
        public void SendItemUpdates()
        {
            _log.Info("Begin ItemUpdates");
            using (var db = _dbFactory.GetRWDb())
            {
                var toDate = _time.GetAppNowTime().AddHours(-30);
                var dbItems = db.Items.GetAll()
                    .Where(i => (i.ItemPublishedStatus == (int) PublishedStatuses.None
                        || i.ItemPublishedStatus == (int) PublishedStatuses.New
                        || i.ItemPublishedStatus == (int) PublishedStatuses.PublishingErrors
                        || i.ItemPublishedStatus == (int) PublishedStatuses.HasChanges
                        || i.ItemPublishedStatus == (int) PublishedStatuses.HasChangesWithProductId
                        || i.ItemPublishedStatus == (int) PublishedStatuses.HasChangesWithSKU
                        || ((i.ItemPublishedStatus == (int) PublishedStatuses.PublishedInProgress
                            || i.ItemPublishedStatus == (int) PublishedStatuses.ChangesSubmited)
                            && i.ItemPublishedStatusDate < toDate)) //NOTE: Added in-progress statuses if items in it more then one day
                            && i.Market == (int) _api.Market
                            && !String.IsNullOrEmpty(i.Barcode)
                            && i.StyleItemId.HasValue)
                        .ToList();

                //NOTE: Need to submit items with group, otherwise we have incorrect color variations calculation, and sometimes image calculation
                var parentASINList = dbItems.Select(i => i.ParentASIN).Distinct().ToList();
                _log.Info("Parent ASIN Count to submit=" + parentASINList.Count + ", item count=" + dbItems.Count);

                var allItemList = db.Items.GetAllActualExAsDto()
                      .Where(i => parentASINList.Contains(i.ParentASIN)
                        && i.Market == (int)_api.Market
                        && !String.IsNullOrEmpty(i.Barcode)).ToList();

                //var parentASINList = dbItems.Select(i => i.ParentASIN).Distinct().ToList();
                var allParentItemList = db.ParentItems.GetAllAsDto().Where(p => parentASINList.Contains(p.ASIN)
                                                                         && p.Market == (int) _api.Market)
                    .ToList();

                var styleIdList = allItemList.Where(i => i.StyleId.HasValue).Select(i => i.StyleId.Value).ToList();
                var allStyleList = db.Styles.GetAllAsDtoEx().Where(s => styleIdList.Contains(s.Id)).ToList();
                var allStyleImageList = db.StyleImages.GetAllAsDto().Where(sim => styleIdList.Contains(sim.StyleId)).ToList();
                var allFeatures = db.FeatureValues.GetValuesByStyleIds(styleIdList);

                foreach (var item in allItemList)
                {
                    var parent = allParentItemList.FirstOrDefault(p => p.ASIN == item.ParentASIN);
                    if (parent != null)
                        item.OnHold = parent.OnHold;

                    var itemStyle = allStyleList.FirstOrDefault(s => s.Id == item.StyleId);

                    if (String.IsNullOrEmpty(itemStyle.BulletPoint1))
                    {
                        var features = allFeatures.Where(f => f.StyleId == item.StyleId).ToList();

                        var subLiscense =
                            StyleFeatureHelper.GetFeatureValue(
                                features.FirstOrDefault(f => f.FeatureId == StyleFeatureHelper.SUB_LICENSE1));
                        var mainLicense =
                            StyleFeatureHelper.PrepareMainLicense(
                                StyleFeatureHelper.GetFeatureValue(
                                    features.FirstOrDefault(f => f.FeatureId == StyleFeatureHelper.MAIN_LICENSE)),
                                subLiscense);
                        var material =
                            StyleFeatureHelper.GetFeatureValue(
                                features.FirstOrDefault(f => f.FeatureId == StyleFeatureHelper.MATERIAL));
                        var gender =
                            StyleFeatureHelper.GetFeatureValue(
                                features.FirstOrDefault(f => f.FeatureId == StyleFeatureHelper.GENDER));
                        var bulletPoints = ExportDataHelper.BuildKeyFeatures(mainLicense,
                            subLiscense,
                            material,
                            gender);

                        item.Features = String.Join(";", bulletPoints);
                    }
                    else
                    {
                        item.Features = String.Join(";", new string[]
                        {
                            itemStyle.BulletPoint1,
                            itemStyle.BulletPoint2,
                            itemStyle.BulletPoint3,
                            itemStyle.BulletPoint4,
                            itemStyle.BulletPoint5,
                        });
                    }
                }

                //Exclude OnHold (after ParentItem onHold was applied)
                allItemList = allItemList.Where(i => !i.OnHold).ToList();

                foreach (var styleImage in allStyleImageList)
                {
                    try
                    {
                        var filepath = ImageHelper.BuildJetImage(_jetImageDirectory, styleImage.Image);
                        var filename = Path.GetFileName(filepath);
                        styleImage.Image = _jetImageBaseUrl + filename;
                    }
                    catch (Exception ex)
                    {
                        _log.Info("BuildWalmartImage error, image=" + styleImage.Image, ex);
                    }
                }

                //foreach (var style in allStyleList)
                //{
                //    try
                //    {
                //        var filepath = ImageHelper.BuildSwatchImage(_swatchImageDirectory, style.Image);
                //        var filename = Path.GetFileName(filepath);
                //        style.SwatchImage = _swatchImageBaseUrl + filename;
                //    }
                //    catch (Exception ex)
                //    {
                //        _log.Info("BuildSwatchImage error, image=" + style.Image, ex);
                //    }
                //}

                if (allItemList.Any())
                {
                    _log.Info("Items to update=" + String.Join(", ", allItemList.Select(i => i.SKU).ToList()));

                    foreach (var item in allItemList)
                    {
                        var style = allStyleList.FirstOrDefault(s => s.Id == item.StyleId);
                        var styleImages = new List<StyleImageDTO>();

                        var styleFeatures = allFeatures.Where(f => f.StyleId == item.StyleId).ToList();
                        var parentItem = allParentItemList.FirstOrDefault(p => p.ASIN == item.ASIN);
                        var groupItems = allItemList.Where(i => i.ParentASIN == item.ParentASIN).ToList();

                        var enableColorVariation = (parentItem != null && parentItem.ForceEnableColorVariations)
                            || groupItems.GroupBy(i => i.Size)
                            .Select(g => new
                            {
                                Count = g.Count(),
                                Size = g.Key,
                            })
                            .Count(g => g.Count > 1) > 0;

                        if (!enableColorVariation)
                        {
                            //NOTE: get images from all styles from group
                            var groupStyleIds = groupItems.Select(i => i.StyleId).Distinct().ToList();
                            styleImages = allStyleImageList
                               .Where(im => groupStyleIds.Contains(im.StyleId) && !im.IsSystem)
                               .OrderByDescending(im => im.IsDefault)
                               .ThenBy(im => im.Id)
                               .ToList();
                        }
                        else
                        {
                            //NOTE: when color variation, get images only from current style, other images were present other color variations
                            styleImages = allStyleImageList
                               .Where(im => im.StyleId == item.StyleId && !im.IsSystem)
                               .OrderByDescending(im => im.IsDefault)
                               .ThenBy(im => im.Id)
                               .ToList();
                        }

                        var result = _api.SendProduct(item,
                            style,
                            styleImages.Select(im => im.Image).ToList(),
                            styleFeatures);

                        var dbItem = db.Items.GetAll().FirstOrDefault(i => i.Id == item.Id);
                        if (dbItem != null)
                        {
                            if (result.IsSuccess)
                            {
                                dbItem.ItemPublishedStatus = (int) PublishedStatuses.Published;
                                dbItem.ItemPublishedStatusDate = _time.GetAppNowTime();
                            }
                            else
                            {
                                dbItem.ItemPublishedStatus = (int) PublishedStatuses.PublishingErrors;
                                dbItem.ItemPublishedStatusDate = _time.GetAppNowTime();
                            }
                        }
                        else
                        {
                            _log.Error("Can't find Item record in DB to update, id=" + item.Id + ", SKU=" + item.SKU);
                        }
                    }
                    db.Commit();

                    foreach (var parentItem in allParentItemList)
                    {
                        var childItems = allItemList.Where(i => i.ParentASIN == parentItem.ASIN).ToList();

                        var enableColorVariation = (parentItem != null && parentItem.ForceEnableColorVariations)
                            || childItems.GroupBy(i => i.Size)
                            .Select(g => new
                            {
                                Count = g.Count(),
                                Size = g.Key,
                            })
                            .Count(g => g.Count > 1) > 0;
                        
                        _api.SendVariations(childItems[0].SKU,
                            childItems.Skip(1).Select(i => i.SKU).ToArray(),
                            enableColorVariation);
                    }
                }
                _log.Info("End ItemUpdates");
            }
        }

        public void ReadItems(DateTime? lastDate)
        {
            //TODO: get current market price/qty
        }

        public void SendInventoryUpdates()
        {
            _log.Info("Begin InventoryUpdates");
            using (var db = _dbFactory.GetRWDb())
            {
                var beforeDate = _time.GetAppNowTime().AddMinutes(-180); //NOTE: update not often, every 3 hours

                var listingQuery = from l in db.Listings.GetAll()
                                   join i in db.Items.GetAll() on l.ItemId equals i.Id
                                   where l.QuantityUpdateRequested
                                       && (i.ItemPublishedStatus == (int)PublishedStatuses.Published
                                       || i.ItemPublishedStatus == (int)PublishedStatuses.HasChanges)
                                       && (!l.LastQuantityUpdatedOnMarket.HasValue
                                        || l.LastQuantityUpdatedOnMarket < beforeDate)
                                       && i.Market == (int)_api.Market
                                   select l;

                var dbListings = listingQuery.ToList();
                
                if (dbListings.Any())
                {
                    _log.Info("Listings to sync=" + String.Join(", ", dbListings.Select(i => i.SKU).ToList()));

                    var listings = dbListings.ToList();

                    foreach (var listing in listings)
                    {
                        var dbListing = dbListings.FirstOrDefault(i => i.Id == listing.Id);
                        if (dbListing == null)
                        {
                            _log.Info("Item haven't listing, item=" + listing.Id);
                            continue;
                        }

                        _log.Info(String.Format("Send SKU={0}, quantity={1}",
                            listing.SKU,
                            listing.RealQuantity));

                        var result = _api.SendInventory(listing.SKU, listing.RealQuantity);
                        if (result.IsSuccess)
                        {
                            dbListing.QuantityUpdateRequested = false;
                        }
                        else
                        {
                            _log.Info("Can't update quantity");
                        }

                        dbListing.LastQuantityUpdatedOnMarket = _time.GetAppNowTime();
                        db.Commit();
                    }
                }
            }
            _log.Info("End ItemUpdates");
        }


        public void SendPriceUpdates()
        {
            _log.Info("Begin PriceUpdates");
            var today = _time.GetAppNowTime().Date;
            var beforeDate = _time.GetAppNowTime().AddMinutes(-180); //NOTE: update not often, every 3 hours

            using (var db = _dbFactory.GetRWDb())
            {
                var itemQuery = from l in db.Listings.GetAll()
                                   join i in db.Items.GetAll() on l.ItemId equals i.Id
                                   join s in db.Styles.GetAll() on i.StyleId equals s.Id
                                   join sale in db.StyleItemSaleToListings.GetAllListingSaleAsDTO() on l.Id equals sale.ListingId into withSale
                                   from sale in withSale.DefaultIfEmpty()
                                   where l.PriceUpdateRequested
                                       && (i.ItemPublishedStatus == (int)PublishedStatuses.Published
                                        || i.ItemPublishedStatus == (int)PublishedStatuses.HasChanges)
                                       && (!l.LastPriceUpdatedOnMarket.HasValue || l.LastPriceUpdatedOnMarket < beforeDate)
                                       && i.Market == (int)_api.Market
                                       && !l.IsRemoved
                                   select new ItemDTO()
                                   {
                                       ListingEntityId = l.Id,
                                       SKU = l.SKU,
                                       StyleId = i.StyleId,
                                       CurrentPrice = l.CurrentPrice,
                                       ListPrice = s.MSRP,
                                       SalePrice = sale != null && sale.SaleStartDate <= today ? sale.SalePrice : null,
                                   };

                var items = itemQuery.ToList();

                if (items.Any())
                { 
                    _log.Info("Listings to sync=" + String.Join(", ", items.Select(i => i.SKU).ToList()));

                    foreach (var item in items)
                    {
                        var dbListing = db.Listings.GetAll().FirstOrDefault(i => i.Id == item.ListingEntityId);

                        if (dbListing == null)
                        {
                            _log.Info("Item haven't listing, item=" + item.Id);
                            continue;
                        }

                        if (item.StyleId.HasValue)
                        {
                            var itemStyle = db.StyleFeatureValues.GetFeatureValueByStyleIdByFeatureId(
                                item.StyleId.Value,
                                StyleFeatureHelper.ITEMSTYLE);

                            if (!item.ListPrice.HasValue && itemStyle != null)
                            {
                                item.ListPrice = PriceHelper.GetDefaultMSRP(itemStyle.Value);
                            }
                        }

                        _log.Info(String.Format("Send SKU={0}, price={1}, msrp={2}, salePrice={3}",
                            item.SKU,
                            item.CurrentPrice,
                            item.ListPrice,
                            item.SalePrice));

                        var result = _api.SendPrice(item.SKU, item.SalePrice ?? item.CurrentPrice);
                        if (result.IsSuccess)
                        {
                            dbListing.PriceUpdateRequested = false;
                        }
                        else
                        {
                            _log.Info("Can't update price");
                        }
                        dbListing.LastPriceUpdatedOnMarket = _time.GetAppNowTime();
                        db.Commit();
                    }
                }
                _log.Info("End PriceUpdates");
            }
        }
    }
}
