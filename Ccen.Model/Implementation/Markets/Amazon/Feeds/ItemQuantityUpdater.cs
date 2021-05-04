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

namespace Amazon.Model.Implementation.Markets.Amazon.Feeds
{
    public class ItemQuantityUpdater : BaseFeedUpdater
    {
        protected IList<Listing> Listings { get; set; }

        protected override AmazonFeedType Type
        {
            get { return AmazonFeedType.Inventory; }
        }

        protected override string AmazonFeedName
        {
            get { return "_POST_INVENTORY_AVAILABILITY_DATA_"; }
        }

        private long _fulfillmentLatency;

        public ItemQuantityUpdater(ILogService log, 
            ITime time,
            IDbFactory dbFactory,
            long fulfillmentLatency) : base(log, time, dbFactory)
        {
            _fulfillmentLatency = fulfillmentLatency;
        }

        /// <summary>
        /// Keep sended to Amazon quantity (use it after get submit feed status to update AmazonRealQuantity)
        /// </summary>
        private Dictionary<string, int> _quantityToSend = new Dictionary<string, int>();

        protected override DocumentInfo ComposeDocument(IUnitOfWork db, 
            long companyId, 
            MarketType market,
            string marketplaceId,
            IList<string> asinList)
        {
            Log.Info("Get listings for quantity update");
            if (asinList == null || !asinList.Any())
            {
                Listings = db.Listings.GetQuantityUpdateRequiredList(market, marketplaceId);
            }
            else
            {
                Listings = db.Listings.GetAll().Where(l => asinList.Contains(l.SKU)
                        && l.Market == (int)market
                        && (l.MarketplaceId == marketplaceId || String.IsNullOrEmpty(marketplaceId)))
                    .ToList();
            }


            _quantityToSend = new Dictionary<string, int>();

            if (Listings.Any())
            {
                var quantMessages = new List<XmlElement>();
                var index = 0;

                foreach (var listing in Listings)
                {
                    //Skip processing duplicate SKU
                    if (_quantityToSend.ContainsKey(listing.SKU))
                        continue;
                    
                    index++;
                    //NOTE: Case when we have 2 duplicate listings with different styleId (in case when manufacturer change StyleId)
                    var realQuantity = Listings.Where(l => l.SKU == listing.SKU).Sum(l => l.RealQuantity);

                    var quantity = listing.DisplayQuantity.HasValue ?
                        Math.Min(listing.DisplayQuantity.Value, realQuantity)
                        : listing.RealQuantity;

                    //NOTE: 101 qty rule
                    if (quantity >= 30)
                        quantity = 101;

                    Log.Info("add listing " + index
                                + ", listingId=" + listing.Id
                                + ", SKU=" + listing.SKU
                                + ", quantity=" + quantity
                                + " (display=" + listing.DisplayQuantity + ", real=" + realQuantity + "(" + listing.RealQuantity + ")" + ")" +
                                " , restockDate=" + listing.RestockDate);

                    quantMessages.Add(FeedHelper.ComposeInventoryMessage(index, listing.SKU, quantity, listing.RestockDate, _fulfillmentLatency));
                    listing.MessageIdentifier = index;

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
            foreach (var listing in Listings)
            {
                //listing.QuantityUpdateRequested = false;
                listing.InventoryFeedId = feedId;
                if (_quantityToSend.ContainsKey(listing.SKU))
                    listing.SendToAmazonQuantity = _quantityToSend[listing.SKU];
                else
                    listing.SendToAmazonQuantity = null;
            }
        }

        protected override void UpdateEntitiesAfterResponse(long feedId, 
            IList<FeedResultMessage> errorList)
        {
            using (var db = DbFactory.GetRWDb())
            {
                var feed = db.Feeds.Get(feedId);
                var listings = db.Listings.GetFiltered(l => l.InventoryFeedId == feedId).ToList();

                foreach (var listing in listings)
                {
                    if (errorList.Any(e => e.MessageId == listing.MessageIdentifier))
                    {
                        listing.InventoryFeedId = null;
                        listing.QuantityUpdateRequested = true;
                        //NOTE: keep QuantityUpdateRequestedDate

                        Log.Warn("listing not updated, listingId=" + listing.Id);
                    }
                    else
                    {
                        if (!listing.QuantityUpdateRequestedDate.HasValue
                            || feed.SubmitDate > listing.QuantityUpdateRequestedDate.Value.AddMinutes(5)) //NOTE: qty should be updated before submitting feed (with a 100% gap)
                        {
                            listing.QuantityUpdateRequested = false;
                        }
                        listing.AmazonRealQuantity = listing.SendToAmazonQuantity;
                        listing.LastQuantityUpdatedOnMarket = Time.GetAppNowTime();

                        Log.Info("Updated listing, listingId=" + listing.ListingId + ", SKU=" + listing.SKU + ", Qty=" + listing.SendToAmazonQuantity);
                    }
                }
                db.Commit();
            }
        }
    }
}
