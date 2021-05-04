 using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Amazon.Api;
using Amazon.Api.Exports;
using Amazon.Api.Feeds;
using Amazon.Common.ExcelExport;
using Amazon.Common.Helpers;
using Amazon.Core;
using Amazon.Core.Contracts;
using Amazon.Core.Contracts.Factories;
using Amazon.Core.Entities;
using Amazon.Core.Helpers;
using Amazon.Core.Models;
using Amazon.Core.Models.Calls;
using Amazon.Core.Models.Enums;
using Amazon.DTO;
using Amazon.DTO.Exports;
using Amazon.DTO.Feeds;

namespace Amazon.Model.Implementation.Markets.Amazon.Feeds
{
    public class ItemDataUpdater : BaseFeedUpdater
    {
        private int AttemptNumbers = 10;
        private TimeSpan IntervalBetweenAttempts = TimeSpan.FromHours(3);

        private IMarketCategoryService _categoryService;

        protected override AmazonFeedType Type
        {
            get { return AmazonFeedType.Product; }
        }

        protected override string AmazonFeedName
        {
            get { return "_POST_PRODUCT_DATA_"; }
        }

        public ItemDataUpdater(ILogService log, 
            ITime time,
            IDbFactory dbFactory,
            IMarketCategoryService categoryService) : base(log, time, dbFactory)
        {
            _categoryService = categoryService;
        }

        protected IList<Item> Items { get; set; }

        protected override DocumentInfo ComposeDocument(IUnitOfWork db,
            long companyId,
            MarketType market,
            string marketplaceId,
            IList<string> skuList)
        {
            Log.Info("Get listings for data updates");

            var nodesCount = 0;
            IList<FeedItemDTO> feedItems;
            IList<FeedMessageDTO> messages;

            var merchant = db.Companies.Get(companyId).AmazonFeedMerchantIdentifier;
            var document = ComposeDocument(db,
                market,
                marketplaceId,
                skuList,
                merchant,
                Type.ToString(),
                out nodesCount,
                out feedItems,
                out messages);

            return new DocumentInfo
            {
                XmlDocument = document,
                FeedItems = feedItems,
                Messages = messages,
                NodesCount = nodesCount
            };
        }

        private XmlDocument ComposeDocument(IUnitOfWork db,
            MarketType market,
            string marketplaceId,
            IList<string> skuList,
            string merchantId,
            string type,
            out int nodesCount,
            out IList<FeedItemDTO> feedItems,
            out IList<FeedMessageDTO> messages)
        {
            var toDate = Time.GetAppNowTime().Subtract(IntervalBetweenAttempts);

            IList<int> itemIds;
            IList<SystemActionDTO> requestInfoes = null;
            if (skuList == null || !skuList.Any())
            {
                requestInfoes = db.SystemActions.GetAllAsDto()
                    .Where(a => a.Type == (int)SystemActionType.UpdateOnMarketProductData
                                    && a.Status == (int)SystemActionStatus.None
                                    && (!a.AttemptDate.HasValue || a.AttemptDate < toDate))
                    .OrderByDescending(a => a.CreateDate)
                    .ToList();

                var tags = requestInfoes.Select(i => i.Tag).ToList();
                itemIds = tags.Select(i => StringHelper.TryGetInt(i)).ToList().Where(i => i.HasValue).Select(i => i.Value).ToList();
            }
            else
            {
                itemIds = (from i in db.Items.GetAll()
                            join l in db.Listings.GetAll() on i.Id equals l.ItemId
                            where i.Market == (int)market
                                    && (i.MarketplaceId == marketplaceId || String.IsNullOrEmpty(marketplaceId))
                                    && (!String.IsNullOrEmpty(i.Barcode)
                                        || i.IsExistOnAmazon == true)
                                    && i.StyleItemId.HasValue
                                    && skuList.Contains(l.SKU)
                                    && !l.IsFBA
                            select i.Id).ToList();
            }

            //NOTE: No need to sumbit all childs
            Log.Info("Items to submit=" + itemIds.Count);

            var dtoItems = db.Items.GetAllActualExAsDto()
                .Where(i => itemIds.Contains(i.Id)
                            && i.Market == (int) market
                            && (i.MarketplaceId == marketplaceId || String.IsNullOrEmpty(marketplaceId))
                            && (!String.IsNullOrEmpty(i.Barcode)
                                || i.IsExistOnAmazon == true)                            
                            && !i.IsFBA) //NOTE: Exclude FBA listings
                .ToList();

            if (skuList == null)
            {
                dtoItems.ForEach(i => i.Id = 0);
            }
            else
            {
                foreach (var dtoItem in dtoItems)
                {
                    var requestInfo = requestInfoes.FirstOrDefault(i => i.Tag == dtoItem.Id.ToString());
                    dtoItem.Id = (int)(requestInfo?.Id ?? 0);
                }
            }

            var parentASINList = dtoItems.Select(i => i.ParentASIN).Distinct().ToList();

            //TEMP: Exclude updates for already published items
            //itemDtoList = itemDtoList.Where(i => i.PublishedStatus != (int)PublishedStatuses.Published).ToList();

            //var parentASINList = dbItems.Select(i => i.ParentASIN).Distinct().ToList();
            var parentItemDtoList = db.ParentItems.GetAllAsDto().Where(p => parentASINList.Contains(p.ASIN)
                                                                            && p.Market == (int) market
                                                                            && p.MarketplaceId == marketplaceId)
                .ToList();

            #region Detect Variation Type
            var allChildItems = db.Items.GetAll().Where(i => parentASINList.Contains(i.ParentASIN))
                .Select(i => new ItemDTO()
                {
                    ParentASIN = i.ParentASIN,
                    Size = i.Size,
                    Color = i.Color,
                }).ToList();
            var colorVariations = allChildItems.GroupBy(i => i.ParentASIN).Select(i => new ParentItemDTO()
            {
                ASIN = i.Key,
                ForceEnableColorVariations = i.Select(c => c.Color).Distinct().Count() > 1,
            }).ToList();

            foreach (var parentItem in parentItemDtoList)
            {
                var colorVariation = colorVariations.FirstOrDefault(c => c.ASIN == parentItem.ASIN);
                if (colorVariation != null)
                {
                    parentItem.ForceEnableColorVariations = parentItem.ForceEnableColorVariations || colorVariation.ForceEnableColorVariations;
                }
            }
            #endregion

            //var lockedParentASINs = parentItemDtoList
            //    .Where(pi => pi.LockMarketUpdate)
            //    .Select(pi => pi.ASIN)
            //    .ToList();

            //parentItemDtoList = parentItemDtoList.Where(pi => !pi.LockMarketUpdate).ToList();
            //itemDtoList = itemDtoList.Where(i => !lockedParentASINs.Contains(i.ParentASIN)).ToList();

            var styleIdList = dtoItems.Where(i => i.StyleId.HasValue).Select(i => i.StyleId.Value).ToList();
            var styleList = db.Styles.GetAllAsDtoEx().Where(s => styleIdList.Contains(s.Id)).ToList();
            var allStyleImageList =
                db.StyleImages.GetAllAsDto().Where(sim => styleIdList.Contains(sim.StyleId)).ToList();
            var allFeatures = db.FeatureValues.GetValuesByStyleIds(styleIdList);

            nodesCount = dtoItems.Count;

            foreach (var item in dtoItems)
            {
                var parent = parentItemDtoList.FirstOrDefault(p => p.ASIN == item.ParentASIN);
                if (parent != null)
                    item.OnHold = parent.OnHold;

                var itemStyle = styleList.FirstOrDefault(s => s.Id == item.StyleId);

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
            //itemDtoList = itemDtoList.Where(i => !i.OnHold).ToList();

            if (dtoItems.Any())
            {
                Log.Info("Items to submit=" + String.Join(", ", dtoItems.Select(i => i.SKU).ToList()));

                var newFeed = new Feed()
                {
                    Market = (int) market,
                    MarketplaceId = marketplaceId,
                    Type = (int) Type,
                    Status = (int) FeedStatus.Submitted,
                    SubmitDate = Time.GetAppNowTime()
                };
                db.Feeds.Add(newFeed);
                db.Commit();

                Log.Info("Feed id=" + newFeed.Id);

                var builder = new ProductFeedBuilder(Log, Time, _categoryService, ChooseTemplate);
                IList<WithMessages<AmazonProductExportDto>> items = new List<WithMessages<AmazonProductExportDto>>();
                items = builder.Build(
                        dtoItems,
                        parentItemDtoList,
                        styleList,
                        allStyleImageList,
                        allFeatures,
                        UseStyleImageModes.StyleImage,
                        market,
                        marketplaceId);

                feedItems = items
                    .Where(i => i.Value.Parentage != ExcelHelper.ParentageParent.ToLower() //TODO: why Perentage != ParentageParent ????
                        && i.IsSucess)
                    .Select(i => new FeedItemDTO()
                {
                    FeedId = newFeed.Id,
                    MessageId = i.Value.MessageID,
                    ItemId = i.Value.Id ?? 0
                }).ToList();

                messages = items.SelectMany(i => i.Messages.Select(m => new FeedMessageDTO()
                {
                    Id = i.Value?.Id ?? 0,
                    Message = m.Message
                }).ToList())
                .ToList();

                UpdatePrePublishErrors(db, itemIds, messages, ItemAdditionFields.PublishError);

                var xml = builder.ToXmlFeed(items.Select(i => i.Value).ToList(),
                    merchantId,
                    Type.ToString());

                var doc = new XmlDocument();
                doc.LoadXml(xml);
                return doc;
            }

            feedItems = new List<FeedItemDTO>();
            messages = new List<FeedMessageDTO>();
            return null;
        }

        private string ChooseTemplate(bool isPrime, double? weight)
        {            
            return isPrime ? AmazonTemplateHelper.PrimeTemplate : (weight > 16 ? AmazonTemplateHelper.OversizeTemplate : null);
        }


        protected override void UpdateEntitiesBeforeSubmitFeed(IUnitOfWork db, long feedId)
        {
            //foreach (var item in Items)
            //{
            //}
        }

        protected override void UpdateEntitiesAfterResponse(long feedId,
            IList<FeedResultMessage> errorList)
        {
            DefaultActionUpdateEntitiesAfterResponse(feedId, errorList, ItemAdditionFields.PublishError, AttemptNumbers);


            //using (var db = DbFactory.GetRWDb())
            //{
            //    var feedItems = db.FeedItems.GetAllAsDto().Where(fi => fi.FeedId == feedId).ToList();
            //    var itemIds = feedItems.Select(f => f.ItemId).ToList();
            //    var items = db.Items.GetAll().Where(i => itemIds.Contains(i.Id)).ToList();
                
            //    //Remove all exist errors
            //    var dbExistErrors = db.ItemAdditions.GetAll().Where(i => itemIds.Contains(i.ItemId)
            //                && (i.Field == ItemAdditionFields.PublishError
            //                    || i.Field == ItemAdditionFields.PrePublishError)).ToList();
            //    foreach (var dbExistError in dbExistErrors)
            //    {
            //        db.ItemAdditions.Remove(dbExistError);
            //    }

            //    foreach (var item in items)
            //    {
            //        var feedItem = feedItems.FirstOrDefault(fi => fi.ItemId == item.Id);
            //        if (feedItem != null)
            //        {
            //            var itemErrors = errorList.Where(e => e.MessageId == feedItem.MessageId).ToList();
            //            if (itemErrors.Any())
            //            {
            //                item.ItemPublishedStatus = (int)PublishedStatuses.PublishingErrors;
            //                item.ItemPublishedStatusDate = Time.GetAppNowTime();

            //                foreach (var itemError in itemErrors)
            //                {
            //                    db.ItemAdditions.Add(new Core.Entities.Listings.ItemAddition()
            //                    {
            //                        ItemId = item.Id,
            //                        Field = ItemAdditionFields.PublishError,
            //                        Value = itemError.Message,
            //                        CreateDate = Time.GetAppNowTime(),
            //                    });
            //                }
            //            }
            //            else
            //            {
            //                if (item.ItemPublishedStatus != (int)PublishedStatuses.Published)
            //                {
            //                    item.ItemPublishedStatus = (int)PublishedStatuses.PublishedInProgress;
            //                    item.ItemPublishedStatusDate = Time.GetAppNowTime();
            //                }
            //            }
            //            Log.Info("Update item status, itemId=" + item.Id + ", status=" + item.ItemPublishedStatus);
            //        }
            //    }

            //    db.Commit();

            //    if (_useSystemActionMode 
            //        && items.Any())
            //    {
            //        var parentASINs = items.Select(i => i.ParentASIN).ToList();
            //        var market = items.FirstOrDefault().Market;
            //        var marketplaceId = items.FirstOrDefault().MarketplaceId;
            //        var parentIds = db.ParentItems.GetAll().Where(pi => pi.Market == market
            //            && pi.MarketplaceId == marketplaceId
            //            && parentASINs.Contains(pi.ASIN))
            //            .Select(pi => pi.Id)
            //            .ToList();
            //        var dbParentItems = db.ParentItems.GetAll()
            //            .Where(pi => parentIds.Contains(pi.Id))
            //            .ToList();
            //        var parentIdsStr = dbParentItems.Select(pi => pi.Id.ToString()).ToList();
            //        var allSystemActions = db.SystemActions.GetAll().Where(a => parentIdsStr.Contains(a.Tag)).ToList();
            //        allSystemActions.ForEach(a => a.Status = (int)SystemActionStatus.Done);
            //        db.Commit();
            //    }
            //}            
        }
    }
}
