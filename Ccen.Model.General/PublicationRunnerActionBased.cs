using Amazon.Common.Helpers;
using Amazon.Core.Contracts;
using Amazon.Core.Contracts.Factories;
using Amazon.Core.Models;
using Amazon.Core.Models.Calls;
using Amazon.DTO;
using Amazon.DTO.DropShippers;
using Amazon.DTO.Inventory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ccen.Model.General
{
    public class PublicationRunnerActionBased
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
        protected ISystemActionService _actionService;
        private PublishingMode _mode;

        public PublicationRunnerActionBased(ILogService log,
            ITime time,
            IDbFactory dbFactory,
            ISystemActionService actionService,
            PublishingMode mode)
        {
            _log = log;
            _time = time;
            _dbFactory = dbFactory;
            _actionService = actionService;
            _mode = mode;
        }


        public void SendItemUpdates(Action<StyleImageDTO> prepareStyleImageCallback,
            Func<ParentItemDTO, bool, IList<ItemExDTO>, IList<StyleEntireDto>, IList<StyleImageDTO>, IList<StyleFeatureValueDTO>, IList<DropShipperDTO>, CallResult<ParentItemDTO>> itemPublishCallback,
            MarketType market,
            string marketplaceId,
            IList<long> styleIds)
        {
            _log.Info("Begin ItemUpdates");

            IList<SystemActionDTO> updateActions = null;

            using (var db = _dbFactory.GetRWDb())
            {
                IList<string> parentASINList;

                if (styleIds == null)
                {
                    DateTime? maxLastAttemptDate = _time.GetUtcTime().AddHours(-4);

                    updateActions = _actionService.GetUnprocessedByType(db,
                        SystemActionType.UpdateOnMarketProductData,
                        maxLastAttemptDate,
                        null);

                    var parentItemIds = updateActions.Select(i => StringHelper.TryGetInt(i.Tag))
                        .Where(i => i.HasValue)
                        .ToList();

                    parentASINList = (from pi in db.ParentItems.GetAll()
                                      where parentItemIds.Contains(pi.Id)
                                        && pi.Market == (int)market
                                        && pi.MarketplaceId == marketplaceId
                                      select pi.ASIN).Distinct().ToList();
                }
                else
                {
                    parentASINList = (from i in db.Items.GetAll()
                                      join l in db.Listings.GetAll() on i.Id equals l.ItemId
                                      join st in db.Styles.GetAll() on i.StyleId equals st.Id
                                      where !st.Deleted
                                         && !l.IsRemoved
                                         && styleIds.Contains(st.Id)
                                         && i.Market == (int)market
                                         && (i.MarketplaceId == marketplaceId || String.IsNullOrEmpty(marketplaceId))
                                         && i.StyleItemId.HasValue
                                      select i.ParentASIN).Distinct().ToList();
                }

                //NOTE: Need to submit items with group, otherwise we have incorrect color variations calculation, and sometimes image calculation
                var allItemList = db.Items.GetAllActualExAsDto()
                      .Where(i => parentASINList.Contains(i.ParentASIN)
                        && i.Market == (int)market
                        && (i.MarketplaceId == marketplaceId || String.IsNullOrEmpty(marketplaceId))).ToList();

                _log.Info("Parent ASIN Count to submit=" + parentASINList.Count + ", item count=" + allItemList.Count);

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

                            var parentItemIdStr = parentItem.Id.ToString();
                            var dbSystemActions = db.SystemActions.GetAll()
                                .Where(a => a.Tag == parentItemIdStr
                                    && a.Type == (int)SystemActionType.UpdateOnMarketProductData)
                                .ToList();

                            if (result.IsSuccess)
                            {
                                var dbParentItem = db.ParentItems.GetAll().FirstOrDefault(pi => parentItem.Id == pi.Id);
                                dbParentItem.SourceMarketId = result.Data.SourceMarketId;
                                dbParentItem.SourceMarketUrl = result.Data.SourceMarketUrl;

                                var itemIds = groupItems.Select(i => i.Id).ToList();
                                var dbItems = db.Items.GetAll().Where(i => itemIds.Contains(i.Id)).ToList();
                                foreach (var groupItem in groupItems)
                                {
                                    var resultItem = result.Data.Variations?.FirstOrDefault(v => v.SKU == groupItem.SKU);
                                    var dbItem = dbItems.FirstOrDefault(i => i.Id == groupItem.Id);
                                    if (dbItem != null && resultItem != null)
                                    {
                                        dbItem.SourceMarketId = resultItem.SourceMarketId;
                                        dbItem.SourceMarketUrl = resultItem.SourceMarketUrl;
                                        dbItem.ItemPublishedStatus = (int)PublishedStatuses.Published;
                                        dbItem.ItemPublishedStatusDate = _time.GetAppNowTime();
                                    }
                                }

                                foreach (var dbSystemAction in dbSystemActions)
                                {
                                    dbSystemAction.Status = (int)SystemActionStatus.Done;
                                }
                            }
                            else
                            {
                                foreach (var dbSystemAction in dbSystemActions)
                                {
                                    dbSystemAction.AttemptDate = _time.GetAppNowTime();
                                    dbSystemAction.AttemptNumber++;
                                    if (dbSystemAction.AttemptNumber > 3)
                                    {
                                        dbSystemAction.Status = (int)SystemActionStatus.Fail;
                                    }
                                }
                            }

                            db.Commit();
                        }
                    }
                }
                else
                {
                    if (updateActions != null && updateActions.Any())
                    {
                        foreach (var updateAction in updateActions)
                        {
                            _actionService.SetResult(db, updateAction.Id, SystemActionStatus.Fail, null);
                        }
                    }
                }
                _log.Info("End ItemUpdates");
            }
        }
    }
}
