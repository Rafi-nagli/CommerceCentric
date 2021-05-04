using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
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
using Amazon.Core.Models.Enums;
using Amazon.DTO;
using Amazon.DTO.Feeds;
using Amazon.Utils;

namespace Amazon.Model.Implementation.Markets.Amazon.Feeds
{
    public class ItemRelationshipUpdater : BaseFeedUpdater
    {
        protected override AmazonFeedType Type
        {
            get { return AmazonFeedType.Relationship; }
        }

        protected override string AmazonFeedName
        {
            get { return "_POST_PRODUCT_RELATIONSHIP_DATA_"; }
        }

        public ItemRelationshipUpdater(ILogService log,
            ITime time,
            IDbFactory dbFactory) : base(log, time, dbFactory)
        {

        }

        protected IList<Item> Items { get; set; }

        protected override DocumentInfo ComposeDocument(IUnitOfWork db,
            long companyId,
            MarketType market,
            string marketplaceId,
            IList<string> asinList)
        {
            Log.Info("Get listings for data updates");

            var nodesCount = 0;
            IList<FeedItemDTO> feedItems;

            var merchant = db.Companies.Get(companyId).AmazonFeedMerchantIdentifier;
            var document = ComposeDocument(db,
                market,
                marketplaceId,
                asinList,
                merchant,
                Type.ToString(),
                out nodesCount,
                out feedItems);

            return new DocumentInfo
            {
                XmlDocument = document,
                FeedItems = feedItems,
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
            out IList<FeedItemDTO> feedItems)
        {
            var toDate = Time.GetAppNowTime().AddHours(-12);
            IList<ItemDTO> dtoItems;
            IList<ParentItemDTO> dtoParentItems;

            if (skuList == null || !skuList.Any())
            {
                var requestInfoes = db.SystemActions.GetAllAsDto()
                    .Where(a => a.Type == (int)SystemActionType.UpdateOnMarketProductRelationship
                                    && a.Status != (int)SystemActionStatus.Done
                                    && (!a.AttemptDate.HasValue || a.AttemptDate < toDate))
                    .ToList();

                var tags = requestInfoes.Select(i => i.Tag).ToList();
                var itemIds = tags.Select(i => StringHelper.TryGetLong(i)).ToList().Where(i => i.HasValue).Select(i => i.Value).ToList();

                var parentASINs = db.Items.GetAllViewAsDto()
                    .Where(i => itemIds.Contains(i.Id)
                        && (i.PublishedStatus != (int)PublishedStatuses.New
                            && i.PublishedStatus != (int)PublishedStatuses.Unpublished
                            && i.PublishedStatus != (int)PublishedStatuses.HasUnpublishRequest)
                        && i.Market == (int) market
                        && (String.IsNullOrEmpty(marketplaceId) || i.MarketplaceId == marketplaceId))
                    .Select(i => i.ParentASIN)
                    .ToList();

                dtoItems = (from i in db.Items.GetAllViewAsDto()
                            join si in db.StyleItemCaches.GetAll() on i.StyleItemId equals si.Id
                            where parentASINs.Contains(i.ParentASIN)
                                && i.Market == (int)market
                                && (String.IsNullOrEmpty(marketplaceId) || i.MarketplaceId == marketplaceId)
                                && si.RemainingQuantity > 0
                                && (i.PublishedStatus != (int)PublishedStatuses.New
                                    && i.PublishedStatus != (int)PublishedStatuses.Unpublished
                                    && i.PublishedStatus != (int)PublishedStatuses.HasUnpublishRequest)
                            select i)
                            .ToList();
                
                foreach (var dtoItem in dtoItems)
                {
                    var requestInfo = requestInfoes.FirstOrDefault(i => i.Tag == dtoItem.Id.ToString());
                    dtoItem.Id = (int)(requestInfo?.Id ?? 0);
                }
            }
            else
            {
                var parentASINs = db.Items.GetAllViewAsDto()
                    .Where(i => i.Market == (int)market
                                && (String.IsNullOrEmpty(marketplaceId) || i.MarketplaceId == marketplaceId)
                                //&& !String.IsNullOrEmpty(i.Barcode)
                                && i.StyleItemId.HasValue
                                && i.StyleId.HasValue
                                && skuList.Contains(i.SKU))
                    .Select(i => i.ParentASIN)
                    .ToList();

                dtoItems = (from i in db.Items.GetAllViewAsDto()
                            join si in db.StyleItemCaches.GetAll() on i.StyleItemId equals si.Id
                            where parentASINs.Contains(i.ParentASIN)
                                && i.Market == (int)market
                                && si.RemainingQuantity > 0
                                && (i.PublishedStatus != (int)PublishedStatuses.New
                                && i.PublishedStatus != (int)PublishedStatuses.Unpublished
                                && i.PublishedStatus != (int)PublishedStatuses.HasUnpublishRequest)
                                && (String.IsNullOrEmpty(marketplaceId) || i.MarketplaceId == marketplaceId)
                            select i)
                            .ToList();

                dtoItems.ForEach(i => i.Id = 0);
            }

            var parentASINsToUpdate = dtoItems.Select(i => i.ParentASIN).ToList();
            dtoParentItems = db.ParentItems.GetAll()
                    .Where(pi => parentASINsToUpdate.Contains(pi.ASIN)
                        && pi.Market == (int)market
                        && (String.IsNullOrEmpty(marketplaceId) || pi.MarketplaceId == marketplaceId))
                    .Select(pi => new ParentItemDTO()
                    {
                        Id = pi.Id,
                        SKU = pi.SKU,
                        ASIN = pi.ASIN,
                    }).ToList();
            
            nodesCount = dtoItems.Count;

            if (dtoItems.Any())
            {
                Log.Info("Items to submit=" + String.Join(", ", dtoItems.Select(i => i.SKU).ToList()));

                var newFeed = new Feed()
                {
                    Market = (int)market,
                    MarketplaceId = marketplaceId,
                    Type = (int)Type,
                    Status = (int)FeedStatus.Submitted,
                    SubmitDate = Time.GetAppNowTime()
                };
                db.Feeds.Add(newFeed);
                db.Commit();

                Log.Info("Feed id=" + newFeed.Id);

                var builder = new ProductRelationshipFeedBuilder();
                var items = builder.Build(dtoParentItems,
                    dtoItems);

                feedItems = items
                    .SelectMany(i => i.ItemIds.Select(id => new FeedItemDTO()
                    {
                        FeedId = newFeed.Id,
                        MessageId = i.MessageID,
                        ItemId = id
                    })).ToList();

                var xml = builder.ToXmlFeed(items,
                    merchantId,
                    Type.ToString());

                var doc = new XmlDocument();
                doc.LoadXml(xml);
                return doc;
            }

            feedItems = new List<FeedItemDTO>();
            return null;
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
            DefaultActionUpdateEntitiesAfterResponse(feedId, errorList, ItemAdditionFields.UpdateRelationshipError, 3);

            //using (var db = DbFactory.GetRWDb())
            //{
            //    var feedItems = db.FeedItems.GetAllAsDto().Where(fi => fi.FeedId == feedId).ToList();
            //    var systemActionIds = feedItems.Select(f => f.ItemId).ToList();
            //    var systemActions = db.SystemActions.GetAll().Where(i => systemActionIds.Contains(i.Id)).ToList();
            //    var itemIds = systemActions.Select(a => StringHelper.TryGetInt(a.Tag))
            //        .Where(a => a.HasValue)
            //        .Select(a => a.Value)
            //        .ToList();

            //    //Remove all exist errors
            //    var dbExistErrors = db.ItemAdditions.GetAll().Where(i => itemIds.Contains(i.ItemId)
            //                && i.Field == ItemAdditionFields.UpdateRelationshipError).ToList();
            //    foreach (var dbExistError in dbExistErrors)
            //    {
            //        db.ItemAdditions.Remove(dbExistError);
            //    }

            //    foreach (var action in systemActions)
            //    {
            //        var feedItem = feedItems.FirstOrDefault(fi => fi.ItemId == action.Id);
            //        if (feedItem != null)
            //        {
            //            var itemErrors = errorList.Where(e => e.MessageId == feedItem.MessageId).ToList();
            //            if (itemErrors.Any())
            //            {
            //                action.AttemptNumber++;
            //                action.AttemptDate = Time.GetAppNowTime();                            
            //                if (action.AttemptNumber > 10)
            //                    action.Status = (int)SystemActionStatus.Fail;

            //                var itemId = StringHelper.TryGetLong(action.Tag);

            //                if (itemId.HasValue)
            //                {
            //                    foreach (var itemError in itemErrors)
            //                    {
            //                        db.ItemAdditions.Add(new Core.Entities.Listings.ItemAddition()
            //                        {
            //                            ItemId = (int)itemId.Value,
            //                            Field = ItemAdditionFields.UpdateRelationshipError,
            //                            Value = itemError.Message,
            //                            CreateDate = Time.GetAppNowTime(),
            //                        });
            //                    }
            //                }
            //            }
            //            else
            //            {
            //                action.Status = (int)SystemActionStatus.Done;
            //            }
            //            Log.Info("Update action status, actionId=" + action.Id + ", status=" + action.Status);
            //        }
            //    }

            //    db.Commit();
            //}
        }
    }
}
