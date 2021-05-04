using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Amazon.Core.Contracts;
using Amazon.Core.Contracts.Factories;
using Amazon.Core.Entities;
using Amazon.Core.Models;
using Amazon.Core.Models.Calls;
using Amazon.DTO;
using Amazon.DTO.DropShippers;
using Amazon.DTO.Inventory;

namespace Amazon.Model.General
{
    public class PublicationRunner
    {
        public enum PublishingMode
        {
            PerItem = 0,
            Report = 1,
            ReportAll = 2,
        }

        protected ILogService _log;
        protected ITime _time;
        protected IDbFactory _dbFactory;
        private PublishingMode _mode;

        public PublicationRunner(ILogService log,
            ITime time,
            IDbFactory dbFactory,
            PublishingMode mode)
        {
            _log = log;
            _time = time;
            _dbFactory = dbFactory;
            _mode = mode;
        }


        public void SendItemUpdates(Action<StyleImageDTO> prepareStyleImageCallback,
            Func<ParentItemDTO, bool, IList<ItemExDTO>, IList<StyleEntireDto>, IList<StyleImageDTO>, IList<StyleFeatureValueDTO>, IList<DropShipperDTO>, CallResult<ParentItemDTO>> itemPublishCallback,
            MarketType market,
            string marketplaceId,
            IList<long> styleIds)
        {
            _log.Info("Begin ItemUpdates");

            using (var db = _dbFactory.GetRWDb())
            {
                IList<Item> allDbItems;
                if (styleIds == null)
                {
                    var toDate = _time.GetAppNowTime().AddHours(-30);
                    allDbItems = (from i in db.Items.GetAll()
                                      join l in db.Listings.GetAll() on i.Id equals l.ItemId
                                      join st in db.Styles.GetAll() on i.StyleId equals st.Id
                                      where !st.Deleted
                                         && !l.IsRemoved
                                         && (i.ItemPublishedStatus == (int)PublishedStatuses.None
                                             || i.ItemPublishedStatus == (int)PublishedStatuses.New
                                             || i.ItemPublishedStatus == (int)PublishedStatuses.PublishingErrors
                                             || i.ItemPublishedStatus == (int)PublishedStatuses.HasChanges
                                             || i.ItemPublishedStatus == (int)PublishedStatuses.HasChangesWithProductId
                                             || i.ItemPublishedStatus == (int)PublishedStatuses.HasChangesWithSKU
                                             || ((i.ItemPublishedStatus == (int)PublishedStatuses.PublishedInProgress
                                                 || i.ItemPublishedStatus == (int)PublishedStatuses.ChangesSubmited)
                                                 && i.ItemPublishedStatusDate < toDate)
                                             || _mode == PublishingMode.ReportAll) //NOTE: Added in-progress statuses if items in it more then one day
                                         && i.Market == (int)market
                                         && (i.MarketplaceId == marketplaceId || String.IsNullOrEmpty(marketplaceId))
                                         && i.StyleItemId.HasValue
                                      select i).ToList();
                }
                else
                {
                    allDbItems = (from i in db.Items.GetAll()
                                  join l in db.Listings.GetAll() on i.Id equals l.ItemId
                                  join st in db.Styles.GetAll() on i.StyleId equals st.Id
                                  where !st.Deleted
                                     && !l.IsRemoved
                                     && styleIds.Contains(st.Id)
                                     && i.Market == (int)market
                                     && (i.MarketplaceId == marketplaceId || String.IsNullOrEmpty(marketplaceId))
                                     && i.StyleItemId.HasValue
                                  select i).ToList();
                }

                //NOTE: Need to submit items with group, otherwise we have incorrect color variations calculation, and sometimes image calculation
                var parentASINList = allDbItems.Select(i => i.ParentASIN).Distinct().ToList();
                _log.Info("Parent ASIN Count to submit=" + parentASINList.Count + ", item count=" + allDbItems.Count);

                var allItemList = db.Items.GetAllActualExAsDto()
                      .Where(i => parentASINList.Contains(i.ParentASIN)
                        && i.Market == (int)market
                        && (i.MarketplaceId == marketplaceId || String.IsNullOrEmpty(marketplaceId))).ToList();

                //Refresh parentASIN list, exclude asins with not actual items
                parentASINList = allItemList.Select(i => i.ParentASIN).Distinct().ToList();


                var allParentItemList = db.ParentItems.GetAllAsDto().Where(p => parentASINList.Contains(p.ASIN)
                                                                         && p.Market == (int)market
                                                                         && (p.MarketplaceId == marketplaceId || String.IsNullOrEmpty(marketplaceId)))
                    .ToList();

                var allStyleIdList = allItemList.Where(i => i.StyleId.HasValue).Select(i => i.StyleId.Value).ToList();
                var allStyleList = db.Styles.GetAllAsDtoEx().Where(s => allStyleIdList.Contains(s.Id)).ToList();
                var allStyleImageList = db.StyleImages.GetAllAsDto().Where(sim => allStyleIdList.Contains(sim.StyleId)).ToList();
                var allFeatures = db.StyleFeatureValues.GetAllFeatureValuesByStyleIdAsDto(allStyleIdList).ToList();
                var allDropShipperList = db.DropShippers.GetAllAsDto().ToList();
                allFeatures.AddRange(db.StyleFeatureTextValues.GetAllFeatureTextValuesByStyleIdAsDto(allStyleIdList));
                allFeatures = allFeatures.OrderBy(fv => fv.SortOrder).ToList();

                foreach (var item in allItemList)
                {
                    var parent = allParentItemList.FirstOrDefault(p => p.ASIN == item.ParentASIN);
                    if (parent != null)
                        item.OnHold = parent.OnHold;
                }

                //Exclude OnHold (after ParentItem onHold was applied)
                allItemList = allItemList.Where(i => !i.OnHold).ToList();

                foreach (var styleImage in allStyleImageList)
                {
                    try
                    {
                        prepareStyleImageCallback(styleImage);
                    }
                    catch (Exception ex)
                    {
                        _log.Info("PrepareStyleImage error, image=" + styleImage.Image, ex);
                    }
                }

                if (allItemList.Any())
                {
                    _log.Info("Items to update=" + String.Join(", ", allItemList.Select(i => i.SKU).ToList()));

                    foreach (var parentItem in allParentItemList)
                    {
                        var groupItems = allItemList.Where(i => i.ParentASIN == parentItem.ASIN).ToList();
                        var groupStyleIdList =
                            groupItems.Where(i => i.StyleId.HasValue).Select(i => i.StyleId).Distinct().ToList();
                        var styles = allStyleList.Where(s => groupStyleIdList.Contains(s.Id)).ToList();
                        var styleFeatures = allFeatures.Where(f => groupStyleIdList.Contains(f.StyleId)).ToList();

                        var enableColorVariation = (parentItem != null && parentItem.ForceEnableColorVariations)
                                                   || groupItems.GroupBy(i => i.Size)
                                                       .Select(g => new
                                                       {
                                                           Count = g.Count(),
                                                           Size = g.Key,
                                                       })
                                                       .Count(g => g.Count > 1) > 0;

                        var styleImages = allStyleImageList
                            .Where(im => groupStyleIdList.Contains(im.StyleId) && !im.IsSystem)
                            .OrderBy(im => im.StyleId)
                            .ThenByDescending(im => im.IsDefault)
                            .ThenBy(im => im.Id)
                            .ToList();

                        if (_mode == PublishingMode.Report
                            || _mode == PublishingMode.ReportAll)
                        {
                            itemPublishCallback(parentItem,
                                enableColorVariation,
                                groupItems,                                
                                styles,
                                styleImages,
                                styleFeatures,
                                allDropShipperList);
                        }

                        if (_mode == PublishingMode.PerItem)
                        {
                            //SEND
                            var result = itemPublishCallback(parentItem,
                                enableColorVariation,
                                groupItems,
                                styles,
                                styleImages,
                                styleFeatures,
                                allDropShipperList);

                            var itemIdList = groupItems.Select(i => i.Id).ToList();
                            var dbItems = db.Items.GetAll().Where(i => itemIdList.Contains(i.Id)).OrderBy(i => i.StyleId).ToList();
                            var dbParentItem = db.ParentItems.GetAll().FirstOrDefault(pi => pi.Id == parentItem.Id);
                            var dbListings = db.Listings.GetAll().Where(l => itemIdList.Contains(l.ItemId)).ToList();

                            if (result.IsSuccess)
                            {
                                foreach (var dbItem in dbItems)
                                {
                                    var dbListing = dbListings.FirstOrDefault(l => l.ItemId == dbItem.Id);

                                    var variation = result?.Data?.Variations?.FirstOrDefault(v => v.SKU == dbItem.ASIN
                                                                                                  || v.ASIN == dbItem.ASIN
                                                                                                  || v.SKU == dbListing.SKU);
                                    //For new items by SKU, for exists by ASIN
                                    var marketId = variation?.SourceMarketId;
                                    var marketUrl = variation?.SourceMarketUrl;

                                    dbItem.SourceMarketId = marketId;
                                    dbItem.SourceMarketUrl = marketUrl;
                                    dbItem.ItemPublishedStatus = (int) PublishedStatuses.Published;
                                    dbItem.ItemPublishedStatusDate = _time.GetAppNowTime();
                                }
                                if (dbParentItem != null)
                                {
                                    dbParentItem.SourceMarketId = result?.Data?.SourceMarketId;
                                    dbParentItem.SourceMarketUrl = result?.Data?.SourceMarketUrl;
                                }
                            }
                            else
                            {
                                foreach (var dbItem in dbItems)
                                {
                                    dbItem.ItemPublishedStatus = (int) PublishedStatuses.PublishingErrors;
                                    dbItem.ItemPublishedStatusDate = _time.GetAppNowTime();
                                }
                            }

                            db.Commit();
                        }
                    }
                }
                _log.Info("End ItemUpdates");
            }
        }
    }
}
