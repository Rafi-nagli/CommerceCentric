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
    public class DeleteItemUpdater : BaseFeedUpdater
    {
        protected override AmazonFeedType Type
        {
            get { return AmazonFeedType.ProductDelete; }
        }

        protected override string AmazonFeedName
        {
            get { return "_POST_PRODUCT_DATA_"; }
        }

        public DeleteItemUpdater(ILogService log, 
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
                var requestInfoes = db.SystemActions.GetAllAsDto()
                    .Where(a => a.Type == (int) SystemActionType.DeleteOnMarketProduct
                                    && a.Status != (int) SystemActionStatus.Done
                                    && a.InputData.Contains("\"MarketplaceId\": \"" + marketplaceId))
                    .ToList();

                var requestedSKUs = requestInfoes.Select(i => i.Tag).ToList();


                dtoItems = (from i in db.Items.GetAll() 
                            join l in db.Listings.GetAll() on i.Id equals l.ItemId
                            where requestedSKUs.Contains(l.SKU)
                                && !l.IsRemoved
                                && i.IsExistOnAmazon == true
                                && i.Market == (int)market
                                && (String.IsNullOrEmpty(marketplaceId) || i.MarketplaceId == marketplaceId)
                          select new ItemDTO
                          {
                              Id = i.Id,
                              SKU = l.SKU,
                          }).ToList();

                foreach (var dtoItem in dtoItems)
                {
                    var requestInfo = requestInfoes.FirstOrDefault(i => i.Tag == dtoItem.SKU);
                    dtoItem.Id = (int) (requestInfo?.Id ?? 0);
                }
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
                var systemActions = db.SystemActions.GetAll().Where(i => itemIds.Contains(i.Id)).ToList();
                
                foreach (var action in systemActions)
                {
                    var feedItem = feedItems.FirstOrDefault(fi => fi.ItemId == action.Id);
                    if (feedItem != null)
                    {
                        if (errorList.Any(e => e.MessageId == feedItem.MessageId))
                        {
                            action.Status = (int) SystemActionStatus.Fail;
                        }
                        else
                        {
                            action.Status = (int)SystemActionStatus.Done;
                        }
                        Log.Info("Update action status, actionId=" + action.Id + ", status=" + action.Status);
                    }
                }

                db.Commit();
            }
        }
    }
}
