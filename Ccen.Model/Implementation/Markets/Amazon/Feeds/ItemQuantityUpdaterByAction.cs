using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using Amazon.Api.Feeds;
using Amazon.Core;
using Amazon.Core.Contracts;
using Amazon.Core.Contracts.Factories;
using Amazon.Core.Entities;
using Amazon.Core.Models;
using Amazon.Core.Models.Enums;
using Amazon.Core.Models.SystemActions.SystemActionDatas;
using Amazon.Model.Models;

namespace Amazon.Model.Implementation.Markets.Amazon.Feeds
{
    public class ItemQuantityUpdaterByAction : BaseFeedUpdater
    {
        protected override AmazonFeedType Type
        {
            get { return AmazonFeedType.Inventory; }
        }

        protected override string AmazonFeedName
        {
            get { return "_POST_INVENTORY_AVAILABILITY_DATA_"; }
        }

        private long _fulfillmentLatency;

        public ItemQuantityUpdaterByAction(ILogService log, 
            ITime time,
            IDbFactory dbFactory,
            long fulfillmentLatency) : base(log, time, dbFactory)
        {
            _fulfillmentLatency = fulfillmentLatency;
        }        

        protected override DocumentInfo ComposeDocument(IUnitOfWork db, 
            long companyId, 
            MarketType market,
            string marketplaceId,
            IList<string> asinList)
        {
            Dictionary<string, int> _quantityToSend = new Dictionary<string, int>();

            Log.Info("Get listings for quantity update");
            var requestInfoes = db.SystemActions.GetAllAsDto()
                    .Where(a => a.Type == (int)SystemActionType.UpdateOnMarketProductQuantity
                                    && a.Status != (int)SystemActionStatus.Done)
                    .ToList();

            var requestedSKUs = requestInfoes.Select(i => i.Tag).ToList();
            var dtoItems = (from i in db.Items.GetAllViewAsDto()
                        where requestedSKUs.Contains(i.SKU)
                            && i.PublishedStatus == (int)PublishedStatuses.Published
                            && i.Market == (int)market
                            && (String.IsNullOrEmpty(marketplaceId) || i.MarketplaceId == marketplaceId)
                        select i).ToList();

            foreach (var dtoItem in dtoItems)
            {
                var requestInfo = requestInfoes.FirstOrDefault(i => i.Tag == dtoItem.SKU);
                var info = SystemActionHelper.FromStr<UpdateQtyInput>(requestInfo.InputData);
                dtoItem.Id = (int)(requestInfo?.Id ?? 0);
                dtoItem.RealQuantity = info.NewQty;
            }
            
            _quantityToSend = new Dictionary<string, int>();

            if (dtoItems.Any())
            {
                var quantMessages = new List<XmlElement>();
                var index = 0;

                foreach (var listing in dtoItems)
                {
                    //Skip processing duplicate SKU
                    if (_quantityToSend.ContainsKey(listing.SKU))
                        continue;
                    
                    index++;
                    var quantity = listing.RealQuantity;

                    Log.Info("add listing " + index
                                + ", listingId=" + listing.Id
                                + ", SKU=" + listing.SKU
                                + ", quantity=" + quantity);

                    quantMessages.Add(FeedHelper.ComposeInventoryMessage(index, 
                        listing.SKU, 
                        quantity, 
                        null,
                        _fulfillmentLatency));

                    _quantityToSend.Add(listing.SKU, quantity);
                }
                Log.Info("Compose feed");
                var merchant = db.Companies.Get(companyId).AmazonFeedMerchantIdentifier;
                var document = FeedHelper.ComposeFeed(quantMessages, merchant, Type.ToString());
                return new DocumentInfo
                {
                    XmlDocument = document,
                    NodesCount = index
                };
            }
            return null;
        }

        protected override void UpdateEntitiesBeforeSubmitFeed(IUnitOfWork db, long feedId)
        {

        }

        protected override void UpdateEntitiesAfterResponse(long feedId, 
            IList<FeedResultMessage> errorList)
        {
            DefaultActionUpdateEntitiesAfterResponse(feedId, errorList, ItemAdditionFields.UpdateQuantityError, 20);
        }
    }
}
