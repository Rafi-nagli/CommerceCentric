using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using Amazon.Api;
using Amazon.Api.Models.Xml;
using Amazon.Common.Helpers;
using Amazon.Core;
using Amazon.Core.Contracts;
using Amazon.Core.Contracts.Factories;
using Amazon.Core.Entities;
using Amazon.Core.Entities.Enums;
using Amazon.Core.Entities.Listings;
using Amazon.Core.Models;
using Amazon.DTO;
using Amazon.Model.Implementation.Markets;
using log4net;

namespace Amazon.Model.Implementation
{
    public class BuyBoxService
    {
        private ILogService _log;
        private IDbFactory _dbFactory;
        private ITime _time;

        public BuyBoxService(ILogService log, IDbFactory dbFactory, ITime time)
        {
            _log = log;
            _dbFactory = dbFactory;
            _time = time;
        }

        public void Update(AmazonApi api)
        {
            Update(api, null);
        }

        public void Update(AmazonApi api, string[] skuList)
        {
            using (var db = _dbFactory.GetRWDb()) 
            { 
                _log.Debug("BuyBoxService.Update begin");
                var market = api.Market;
                var marketplaceId = api.MarketplaceId;

                var itemsQuery = db.Items.GetAllViewAsDto().Where(i => i.Market == (int)market 
                    && i.MarketplaceId == marketplaceId);

                if (skuList != null && skuList.Any())
                {
                    itemsQuery = itemsQuery.Where(i => skuList.Contains(i.SKU));
                }

                var items = itemsQuery.ToList();

                var buyBoxResponses = RequestItemsBuyBoxByProductApi(_log, items, api);
            
                db.BuyBoxStatus.UpdateBulk(_log,
                    _time,
                    buyBoxResponses.Select(b => new BuyBoxStatusDTO()
                    {
                        ASIN = b.ASIN,
                        CheckedDate = b.CheckedDate,
                        WinnerPrice = b.WinnerPrice,
                        Status = b.Status,
                        WinnerMerchantName = b.WinnerMerchantName,
                    }).ToList(),
                    market,
                    marketplaceId);
                
                db.Commit();
                _log.Debug("BuyBoxService.Update end");
            }
        }

        public class ItemBuyBoxResponse
        {
            public string ASIN { get; set; }
            public decimal? WinnerPrice { get; set; }
            public decimal? WinnerSalePrice { get; set; }
            public decimal? WinnerAmountSaved { get; set; }
            public string WinnerMerchantName { get; set; }

            public BuyBoxStatusCode Status { get; set; }
            public DateTime CheckedDate { get; set; }
        }

        private IList<ItemBuyBoxResponse> RequestItemsBuyBoxByProductApi(ILogService logger, IList<ItemDTO> items, AmazonApi api)
        {
            var results = new List<ItemBuyBoxResponse>();
            var index = 0;
            var step = 20;
            var sleep = new StepSleeper(TimeSpan.FromSeconds(1), 10);
            /*
             * Maximum request quota	Restore rate	Hourly request quota
                20 requests	10 items every second	36000 requests per hour
            */
            while (index < items.Count)
            {
                var checkedItems = items.Skip(index).Take(step).ToList();
                var resp = RetryHelper.ActionWithRetriesWithCallResult(() =>
                    api.GetCompetitivePricingForSKU(checkedItems.Select(i => i.SKU).ToList()),
                    logger);

                if (resp.IsFail)
                {
                    var message = resp.Message ?? "";

                    //Request is throttled ---> Amazon.OrdersApi.Runtime.MwsException: Request is throttled
                    //...
                    if (message.IndexOf("ServiceUnavailable", StringComparison.InvariantCultureIgnoreCase) >= 0 
                        || message.IndexOf("throttled", StringComparison.InvariantCultureIgnoreCase) >= 0)
                    {
                        //do sleep
                    }
                    //NOTE: temporary do always
                    _log.Info("Sleep 5 min");
                    Thread.Sleep(5 * 60 * 1000);
                }
                else
                {
                    foreach (var item in resp.Data)
                    {
                        results.Add(new ItemBuyBoxResponse()
                        {
                            ASIN = item.ASIN,
                            CheckedDate = DateTime.UtcNow,
                            Status = item.BuyBoxStatus ?? BuyBoxStatusCode.NoWinner,
                            WinnerPrice = item.LowestPrice,
                        });
                    }

                    index += step;
                    sleep.NextStep();
                }
            }
            return results;
        }


        //private IList<ItemBuyBoxResponse> RequestItemsBuyBoxByAdv(ILogService logger, IList<Item> items, AmazonApi api)
        //{
        //    var results = new List<ItemBuyBoxResponse>();
        //    var index = 0;
        //    //var indexToSleep = 0;
        //    while (index < items.Count)
        //    {
        //        var checkedItems = items.Skip(index).Take(10).ToList();
        //        var resp = RetryHelper.ActionWithRetries(() => 
        //            api.RetrieveOffersWithMerchant(logger, checkedItems.Select(i => i.ASIN).ToList()), 
        //            logger);
        //        if (resp != null && resp.Items != null && resp.Items.Any() && resp.Items[0].Item != null)
        //        {
        //            foreach (var item in checkedItems)
        //            {
        //                var status = BuyBoxStatusCode.Undefined;
        //                decimal? price = null;
        //                decimal? salePrice = null;
        //                decimal? amountSaved = null;
        //                string merchantName = null;
        //                var el = resp.Items[0].Item.FirstOrDefault(i => i.ASIN == item.ASIN);
        //                if (el != null && el.Offers != null)
        //                {
        //                    if (el != null && el.Offers != null && el.Offers.Offer != null)
        //                    {
        //                        var itemId = item.Id;
        //                        var firstOffer = el.Offers.Offer.FirstOrDefault();
        //                        if (firstOffer != null)
        //                        {
        //                            merchantName = firstOffer.Merchant.Name;
        //                            if (String.IsNullOrEmpty(merchantName))
        //                            {
        //                                status = BuyBoxStatusCode.NoWinner;
        //                            }
        //                            else
        //                            {
        //                                status = (merchantName == AddressService.Name)
        //                                    ? BuyBoxStatusCode.Win
        //                                    : BuyBoxStatusCode.NotWin;
        //                            }

        //                            if (firstOffer.OfferListing != null && firstOffer.OfferListing.Length > 0)
        //                            {
        //                                //Note: firstOffer.OfferListing[0].Price.Amount stored price in "1499" format without any separators
        //                                if (firstOffer.OfferListing[0].Price != null)
        //                                {
        //                                    price = StringHelper.TryGetInt(firstOffer.OfferListing[0].Price.Amount);
        //                                    if (price != null)
        //                                        price = price/100;
        //                                }

        //                                if (firstOffer.OfferListing[0].SalePrice != null)
        //                                {
        //                                    salePrice = StringHelper.TryGetInt(firstOffer.OfferListing[0].SalePrice.Amount);
        //                                    if (salePrice != null)
        //                                        salePrice = salePrice/100;
        //                                }

        //                                if (firstOffer.OfferListing[0].AmountSaved != null)
        //                                {
        //                                    amountSaved = StringHelper.TryGetInt(firstOffer.OfferListing[0].AmountSaved.Amount);
        //                                    if (amountSaved != null)
        //                                        amountSaved = amountSaved/100;
        //                                }
        //                            }
        //                        }
        //                    }
        //                }
        //                results.Add(new ItemBuyBoxResponse()
        //                {
        //                    ASIN = item.ASIN,
        //                    CheckedDate = DateTime.UtcNow,
        //                    Status = status,
        //                    WinnerPrice = price,
        //                    WinnerSalePrice = salePrice,
        //                    WinnerAmountSaved = amountSaved,
        //                    WinnerMerchantName = merchantName
        //                });
        //            }
        //        }
        //        index += 10;

        //        Thread.Sleep(TimeSpan.FromSeconds(5));
        //    }
        //    return results;
        //}


        public int ProcessOfferChanges(AmazonSQSReader reader, IList<string> sellerIds)
        {
            var index = 0;
            var maxCount = 100;
            var existNotification = true;

            using (var db = _dbFactory.GetRWDb())
            {
                while (index < maxCount
                       && existNotification)
                {
                    var notifications = reader.GetNotification();
                    existNotification = notifications.Any();

                    _log.Info("Receive notifications, count=" + notifications.Count);

                    foreach (var n in notifications)
                    {
                        try
                        {
                            var changeDate =
                                AmazonSQSReader.TryGetDate(
                                    n.NotificationPayload.AnyOfferChangedNotification.OfferChangeTrigger
                                        .TimeOfOfferChange);
                            if (!changeDate.HasValue)
                                throw new ArgumentNullException("ChangeDate");

                            _log.Info("Notification change date=" + changeDate);

                            LowestPrice buyBoxPrice = null;
                            OfferCount buyBoxAmazonOffers = null;
                            OfferCount buyBoxMerchantOffers = null;
                            LowestPrice merchantLowestPrices = null;
                            LowestPrice amazonLowestPrices = null;
                            OfferCount merchantOffers = null;
                            OfferCount amazonOffers = null;

                            if (n.NotificationPayload.AnyOfferChangedNotification != null
                                && n.NotificationPayload.AnyOfferChangedNotification.Summary != null)
                            {
                                if (n.NotificationPayload.AnyOfferChangedNotification.Summary.BuyBoxPrices != null)
                                    buyBoxPrice = n.NotificationPayload.AnyOfferChangedNotification.Summary.BuyBoxPrices
                                        .FirstOrDefault(
                                            b => b.Condition == "new");

                                if (n.NotificationPayload.AnyOfferChangedNotification.Summary.BuyBoxEligibleOffers !=
                                    null)
                                {
                                    buyBoxAmazonOffers = n.NotificationPayload.AnyOfferChangedNotification.Summary
                                        .BuyBoxEligibleOffers
                                        .FirstOrDefault(o => o.FulfillmentChannel == "Amazon" && o.Condition == "new");
                                    buyBoxMerchantOffers =
                                        n.NotificationPayload.AnyOfferChangedNotification.Summary.BuyBoxEligibleOffers
                                            .FirstOrDefault(
                                                o => o.FulfillmentChannel == "Merchant" && o.Condition == "new");
                                }

                                if (n.NotificationPayload.AnyOfferChangedNotification.Summary.OfferCounts != null)
                                {
                                    amazonOffers = n.NotificationPayload.AnyOfferChangedNotification.Summary.OfferCounts
                                        .FirstOrDefault(o => o.FulfillmentChannel == "Amazon" && o.Condition == "new");
                                    merchantOffers = n.NotificationPayload.AnyOfferChangedNotification.Summary
                                        .OfferCounts
                                        .FirstOrDefault(o => o.FulfillmentChannel == "Merchant" && o.Condition == "new");
                                }

                                if (n.NotificationPayload.AnyOfferChangedNotification.Summary.LowestPrices != null)
                                {
                                    amazonLowestPrices = n.NotificationPayload.AnyOfferChangedNotification.Summary
                                        .LowestPrices
                                        .FirstOrDefault(
                                            p => p.FulfillmentChannel == "Amazon"
                                                 && p.Condition == "new");

                                    merchantLowestPrices = n.NotificationPayload.AnyOfferChangedNotification.Summary
                                        .LowestPrices.FirstOrDefault(
                                            p => p.FulfillmentChannel == "Merchant"
                                                 && p.Condition == "new");
                                }
                            }

                            var offerChangeEvent = new OfferChangeEvent()
                            {
                                ASIN = n.NotificationPayload.AnyOfferChangedNotification.OfferChangeTrigger.ASIN,
                                MarketplaceId =
                                    n.NotificationPayload.AnyOfferChangedNotification.OfferChangeTrigger.MarketplaceId,
                                ChangeDate = changeDate.Value,

                                BuyBoxLandedPrice = buyBoxPrice != null && buyBoxPrice.LandedPrice != null
                                        ? decimal.Parse(buyBoxPrice.LandedPrice.Amount)
                                        : (decimal?) null,
                                BuyBoxListingPrice = buyBoxPrice != null && buyBoxPrice.ListingPrice != null
                                        ? decimal.Parse(buyBoxPrice.ListingPrice.Amount)
                                        : (decimal?) null,
                                BuyBoxShipping = buyBoxPrice != null && buyBoxPrice.Shipping != null
                                        ? decimal.Parse(buyBoxPrice.Shipping.Amount)
                                        : (decimal?) null,
                                BuyBoxEligibleOffersAmazon =
                                    buyBoxAmazonOffers != null
                                        ? AmazonSQSReader.TryGetInt(buyBoxAmazonOffers.Value)
                                        : (int?) null,
                                BuyBoxEligibleOffersMerchant =
                                    buyBoxMerchantOffers != null
                                        ? AmazonSQSReader.TryGetInt(buyBoxMerchantOffers.Value)
                                        : (int?) null,

                                ListPrice =
                                    AmazonSQSReader.GetPrice(
                                        n.NotificationPayload.AnyOfferChangedNotification.Summary.ListPrice),
                                LowestLandedPriceAmazon =
                                    amazonLowestPrices != null
                                        ? AmazonSQSReader.GetPrice(amazonLowestPrices.LandedPrice)
                                        : null,
                                LowestListingPriceAmazon =
                                    amazonLowestPrices != null
                                        ? AmazonSQSReader.GetPrice(amazonLowestPrices.ListingPrice)
                                        : null,
                                LowestShippingAmazon =
                                    amazonLowestPrices != null
                                        ? AmazonSQSReader.GetPrice(amazonLowestPrices.Shipping)
                                        : null,

                                LowestLandedPriceMerchant =
                                    merchantLowestPrices != null
                                        ? AmazonSQSReader.GetPrice(merchantLowestPrices.LandedPrice)
                                        : null,
                                LowestListingPriceMerchant =
                                    merchantLowestPrices != null
                                        ? AmazonSQSReader.GetPrice(merchantLowestPrices.ListingPrice)
                                        : null,
                                LowestShippingMerchant =
                                    merchantLowestPrices != null
                                        ? AmazonSQSReader.GetPrice(merchantLowestPrices.Shipping)
                                        : null,

                                NumberOfOffersAmazon =
                                    amazonOffers != null ? AmazonSQSReader.TryGetInt(amazonOffers.Value) : (int?) null,
                                NumberOfOffersMerchant =
                                    merchantOffers != null
                                        ? AmazonSQSReader.TryGetInt(merchantOffers.Value)
                                        : (int?) null,

                                CreateDate = _time.GetAppNowTime(),
                            };

                            db.OfferChangeEvents.Add(offerChangeEvent);
                            db.Commit();

                            var offers = new List<OfferInfo>();
                            foreach (var offer in n.NotificationPayload.AnyOfferChangedNotification.Offers)
                            {
                                var newOffer = new OfferInfo()
                                {
                                    OfferChangeEventId = offerChangeEvent.Id,

                                    IsBuyBoxWinner = offer.IsBuyBoxWinner,
                                    IsFeaturedMerchant = offer.IsFeaturedMerchant,
                                    IsFulfilledByAmazon = offer.IsFulfilledByAmazon,
                                    ShipsDomestically = offer.ShipsDomestically,

                                    SellerId = offer.SellerId,
                                    FeedbackCount =
                                        offer.SellerFeedbackRating != null
                                            ? AmazonSQSReader.TryGetInt(offer.SellerFeedbackRating.FeedbackCount)
                                            : null,
                                    SellerPositiveFeedbackRating =
                                        offer.SellerFeedbackRating != null
                                            ? AmazonSQSReader.TryGetInt(
                                                offer.SellerFeedbackRating.SellerPositiveFeedbackRating)
                                            : null,

                                    ListingPrice = AmazonSQSReader.GetPrice(offer.ListingPrice),
                                    Shipping = AmazonSQSReader.GetPrice(offer.Shipping),

                                    ShippingTimeMaximumHours =
                                        AmazonSQSReader.TryGetInt(offer.ShippingTime.MaximumHours),
                                    ShippingTimeMinimumHours =
                                        AmazonSQSReader.TryGetInt(offer.ShippingTime.MinimumHours),
                                    ShippingTimeAvailabilityType = offer.ShippingTime.AvailabilityType,

                                    ShipsFromCountry = offer.ShipsFrom != null ? offer.ShipsFrom.Country : null,
                                    ShipsFromState = offer.ShipsFrom != null ? offer.ShipsFrom.State : null,

                                    CreateDate = _time.GetAppNowTime(),
                                };

                                db.OfferInfoes.Add(newOffer);
                                offers.Add(newOffer);
                            }
                            db.Commit();

                            //Update buybox
                            BuyBoxStatusCode status = BuyBoxStatusCode.NoWinner;
                            var winOffer = offers.FirstOrDefault(o => o.IsBuyBoxWinner);
                            if (winOffer != null)
                            {
                                if (sellerIds.Contains(winOffer.SellerId))
                                    status = BuyBoxStatusCode.Win;
                                else
                                    status = BuyBoxStatusCode.NotWin;
                            }

                            db.BuyBoxStatus.Update(_log,
                                _time,
                                new BuyBoxStatusDTO()
                                {
                                    ASIN = offerChangeEvent.ASIN,
                                    MarketplaceId = offerChangeEvent.MarketplaceId,
                                    CheckedDate = offerChangeEvent.ChangeDate,
                                    WinnerPrice = offerChangeEvent.BuyBoxListingPrice,

                                    Status = status,
                                },
                                AmazonUtils.GetMarketByMarketplaceId(offerChangeEvent.MarketplaceId),
                                offerChangeEvent.MarketplaceId);
                        }
                        catch (Exception ex)
                        {
                            _log.Info("Process notification", ex);
                        }
                    }
                    index++;
                }
            }

            return index;
        }
    }
}
