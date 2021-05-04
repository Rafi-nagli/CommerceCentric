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
    public class ItemImageUpdater : BaseFeedUpdater
    {
        private int AttemptNumbers = 10;
        private TimeSpan IntervalBetweenAttempts = TimeSpan.FromHours(3);

        protected override AmazonFeedType Type
        {
            get { return AmazonFeedType.ProductImage; }
        }

        protected override string AmazonFeedName
        {
            get { return "_POST_PRODUCT_IMAGE_DATA_"; }
        }

        public ItemImageUpdater(ILogService log, 
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
            Log.Info("Get listings for data updates");

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
            var toDate = Time.GetAppNowTime().Subtract(IntervalBetweenAttempts);
            IList<ItemDTO> dtoItems;

            if (skuList == null || !skuList.Any())
            {
                var requestInfoes = db.SystemActions.GetAllAsDto()
                    .Where(a => a.Type == (int) SystemActionType.UpdateOnMarketProductImage
                                    && a.Status == (int) SystemActionStatus.None
                                    && (!a.AttemptDate.HasValue || a.AttemptDate < toDate))
                    .OrderByDescending(a => a.CreateDate)
                    .ToList();

                var tags = requestInfoes.Select(i => i.Tag).ToList();
                var itemIds = tags.Select(i => StringHelper.TryGetLong(i)).ToList().Where(i => i.HasValue).Select(i => i.Value).ToList();

                dtoItems = (from i in db.Items.GetAllViewAsDto() 
                            join pi in db.ParentItems.GetAll() on new { i.ParentASIN, i.Market, i.MarketplaceId } equals new { ParentASIN = pi.ASIN, pi.Market, pi.MarketplaceId }
                            where itemIds.Contains(i.Id)
                                && !pi.LockMarketUpdate
                                && (i.PublishedStatus == (int)PublishedStatuses.Published
                                    || i.PublishedStatus == (int)PublishedStatuses.New
                                    || i.PublishedStatus == (int)PublishedStatuses.None
                                    || i.PublishedStatus == (int)PublishedStatuses.PublishedInactive
                                    || i.PublishedStatus == (int)PublishedStatuses.PublishedInProgress
                                    || i.PublishedStatus == (int)PublishedStatuses.PublishingErrors
                                    || i.PublishedStatus == (int)PublishedStatuses.HasChanges)
                                && i.Market == (int)market
                                && (String.IsNullOrEmpty(marketplaceId) || i.MarketplaceId == marketplaceId)
                          select i).ToList();

                foreach (var dtoItem in dtoItems)
                {
                    var requestInfo = requestInfoes.FirstOrDefault(i => i.Tag == dtoItem.Id.ToString());
                    dtoItem.Id = (int) (requestInfo?.Id ?? 0);
                }
            }
            else
            {
                dtoItems = db.Items.GetAllViewAsDto()
                    .Where(i => i.Market == (int) market
                                && (String.IsNullOrEmpty(marketplaceId) || i.MarketplaceId == marketplaceId)
                                && i.StyleItemId.HasValue
                                && i.StyleId.HasValue
                                && skuList.Contains(i.SKU))
                    .ToList();
                dtoItems.ForEach(i => i.Id = 0);
            }

            //TEMP: prevent udpate manually changes of images
            //dtoItems = dtoItems.Where(i => (i.PublishedStatus != (int)PublishedStatuses.Published || i.IsExistOnAmazon == false || i.IsExistOnAmazon == null) && !(i.IsExistOnAmazon == true)).ToList();

            var parentASINs = dtoItems.Select(i => i.ParentASIN).ToList();
            var parentItems = db.ParentItems.GetAllAsDto()
                .Where(pi => parentASINs.Contains(pi.ASIN)
                    && pi.Market == (int)market
                    && pi.MarketplaceId == marketplaceId).ToList();

            var styleIdList = dtoItems.Where(i => i.StyleId.HasValue).Select(i => i.StyleId.Value).ToList();
            var allStyleImageList = db.StyleImages
                .GetAllAsDto()
                .Where(sim => styleIdList.Contains(sim.StyleId) && !sim.IsSystem)
                .ToList()
                .OrderByDescending(im => ImageHelper.GetSortIndex(im.Category))
                .ThenByDescending(im => im.IsDefault)
                .ThenBy(im => im.Id)
                .ToList();
            
            nodesCount = allStyleImageList.Count;

            if (allStyleImageList.Any())
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

                var builder = new ProductImageFeedBuilder();
                var items = builder.Build(
                    dtoItems,
                    parentItems,
                    allStyleImageList);

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
            DefaultActionUpdateEntitiesAfterResponse(feedId, errorList, ItemAdditionFields.UpdateImageError, AttemptNumbers);
            //using (var db = DbFactory.GetRWDb())
            //{
            //    var feedItems = db.FeedItems.GetAllAsDto().Where(fi => fi.FeedId == feedId).ToList();
            //    var systemItemIds = feedItems.Select(f => f.ItemId).ToList();
            //    var systemActions = db.SystemActions.GetAll().Where(i => systemItemIds.Contains(i.Id)).ToList();
            //    var itemIds = systemActions.Select(i => StringHelper.TryGetInt(i.Tag)).Where(i => i.HasValue).Select(i => i.Value).ToList();

            //    //Remove all exist errors
            //    var dbExistErrors = db.ItemAdditions.GetAll().Where(i => itemIds.Contains(i.ItemId)
            //                && i.Field == ItemAdditionFields.UpdateImageError).ToList();
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
            //                            Field = ItemAdditionFields.UpdateImageError,
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
