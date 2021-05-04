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
    public class DeleteItemUpdaterByItemStatus : BaseFeedUpdater
    {
        protected override AmazonFeedType Type
        {
            get { return AmazonFeedType.ProductDelete; }
        }

        protected override string AmazonFeedName
        {
            get { return "_POST_PRODUCT_DATA_"; }
        }

        public DeleteItemUpdaterByItemStatus(ILogService log, 
            ITime time,
            IDbFactory dbFactory) : base(log, time, dbFactory)
        {
            
        }

        protected IList<Item> Items { get; set; }

        protected override DocumentInfo ComposeDocument(IUnitOfWork db,
            long companyId,
            MarketType market,
            string marketplaceId,
            IList<string> skuList)
        {
            Log.Info("Get listings for delete items");

            var nodesCount = 0;
            IList<FeedItemDTO> feedItems;

            var merchant = db.Companies.Get(companyId).AmazonFeedMerchantIdentifier;
            var document = ComposeDocument(db,
                market,
                marketplaceId,
                skuList,
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
            var toDate = Time.GetAppNowTime().AddHours(-30);
            IList<ItemDTO> dtoItems;

            if (skuList == null || !skuList.Any())
            {
                dtoItems = (from i in db.Items.GetAll()
                           join l in db.Listings.GetAll() on i.Id equals l.ItemId
                           where i.ItemPublishedStatus == (int)PublishedStatuses.HasUnpublishRequest
                                && i.Market == (int)market
                                && (i.MarketplaceId == marketplaceId || String.IsNullOrEmpty(marketplaceId))
                                && i.IsExistOnAmazon == true
                                && !l.IsRemoved
                           select new ItemDTO()
                           {
                               Id = i.Id,
                               SKU = l.SKU
                           }).ToList();
            }
            else
            {
                dtoItems = skuList.Select(i => new ItemDTO() { SKU = i }).ToList();
                //dtoItems = db.Items.GetAllViewAsDto()
                //    .Where(i => i.Market == (int) market
                //                && (String.IsNullOrEmpty(marketplaceId) || i.MarketplaceId == marketplaceId)
                //                && skuList.Contains(i.SKU))
                //    .ToList();
                //dtoItems.ForEach(i => i.Id = 0);
            }

#if DEBUG
            dtoItems = dtoItems.Where(i => i.SKU.Contains("-FBP")).ToList();
#endif

            nodesCount = dtoItems.Count;

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

                var builder = new ProductDeleteFeedBuilder();
                var items = builder.Build(
                    dtoItems);

                feedItems = items
                    .Select(i => new FeedItemDTO()
                {
                    FeedId = newFeed.Id,
                    MessageId = i.MessageID,
                    ItemId = i.Id ?? 0
                }).ToList();

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
            using (var db = DbFactory.GetRWDb())
            {
                var feedItems = db.FeedItems.GetAllAsDto().Where(fi => fi.FeedId == feedId).ToList();
                var itemIds = feedItems.Select(f => f.ItemId).ToList();
                var items = db.Items.GetAll().Where(i => itemIds.Contains(i.Id)).ToList();

                //Remove all exist errors
                var dbExistErrors = db.ItemAdditions.GetAll().Where(i => itemIds.Contains(i.ItemId)
                            && (i.Field == ItemAdditionFields.PublishError
                                || i.Field == ItemAdditionFields.PrePublishError)).ToList();
                foreach (var dbExistError in dbExistErrors)
                {
                    db.ItemAdditions.Remove(dbExistError);
                }

                foreach (var item in items)
                {
                    var feedItem = feedItems.FirstOrDefault(fi => fi.ItemId == item.Id);
                    if (feedItem != null)
                    {
                        var itemErrors = errorList.Where(e => e.MessageId == feedItem.MessageId).ToList();
                        if (itemErrors.Any())
                        {
                            item.ItemPublishedStatusDate = Time.GetAppNowTime();

                            foreach (var itemError in itemErrors)
                            {
                                db.ItemAdditions.Add(new Core.Entities.Listings.ItemAddition()
                                {
                                    ItemId = item.Id,
                                    Field = ItemAdditionFields.PublishError,
                                    Value = itemError.Message,
                                    CreateDate = Time.GetAppNowTime(),
                                });
                            }
                        }
                        else
                        {
                            if (item.ItemPublishedStatus != (int)PublishedStatuses.Unpublished)
                            {
                                item.ItemPublishedStatus = (int)PublishedStatuses.Unpublished;
                                item.ItemPublishedStatusDate = Time.GetAppNowTime();
                            }
                        }
                        Log.Info("Update item status, itemId=" + item.Id + ", status=" + item.ItemPublishedStatus);
                    }
                }

                db.Commit();
            }
        }
    }
}
