using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using Amazon.Api.Feeds;
using Amazon.Common.Helpers;
using Amazon.Core;
using Amazon.Core.Contracts;
using Amazon.Core.Contracts.Factories;
using Amazon.Core.Entities;
using Amazon.Core.Entities.Enums;
using Amazon.Core.Models;
using Amazon.Core.Models.Enums;
using Amazon.DTO;
using Amazon.DTO.Orders;
using Amazon.Model.StampsCom;
using Google.Geocoding.Api.Core;


namespace Amazon.Model.Implementation.Markets.Amazon.Feeds
{
    public class FulfillmentDataUpdater : BaseFeedUpdater
    {
        private IList<OrderShippingInfo> Shippings { get; set; }
        private IList<MailLabelInfo> MailInfoes { get; set; }

        protected override AmazonFeedType Type
        {
            get { return AmazonFeedType.OrderFulfillment; }
        }

        protected override string AmazonFeedName
        {
            get { return "_POST_ORDER_FULFILLMENT_DATA_"; }
        }

        public FulfillmentDataUpdater(ILogService log, 
            ITime time,
            IDbFactory dbFactory) : base(log, time, dbFactory)
        {

        }

        protected override DocumentInfo ComposeDocument(IUnitOfWork db, 
            long companyId, 
            MarketType market, 
            string marketplaceId,
            IList<string> asinList)
        {
            Log.Info("Get orders");
            var shippingMethodList = db.ShippingMethods.GetAllAsDto().ToList();
            var fromDate = Time.GetAppNowTime().AddDays(-7);

            var shippingLabels = db.OrderShippingInfos.GetOrdersToFulfillAsDTO(market, marketplaceId)
                .Where(sh => sh.ShippingDate > fromDate).ToList();
            var shippingInfoIds = shippingLabels.Select(i => i.Id).ToList();
            Shippings = db.OrderShippingInfos.GetFiltered(sh => shippingInfoIds.Contains(sh.Id)).ToList();
            shippingLabels = PrepareOrderShippings(db, shippingLabels);

            var mailLabels = db.MailLabelInfos.GetInfosToFulfillAsDTO(market, marketplaceId)
                .Where(sh => sh.ShippingDate > fromDate).ToList();
            var mailInfoIds = mailLabels.Select(i => i.Id).ToList();
            MailInfoes = db.MailLabelInfos.GetFiltered(m => mailInfoIds.Contains(m.Id)).ToList();
            mailLabels = PrepareMailShippings(db, mailLabels);

            var allShippings = shippingLabels;
            allShippings.AddRange(mailLabels);

            if (asinList != null)
                allShippings = allShippings.Where(sh => asinList.Contains(sh.AmazonIdentifier)).ToList();

            if (allShippings.Any())
            {
                var orderMessages = new List<XmlElement>();
                var index = 0;
                var merchant = db.Companies.Get(companyId).AmazonFeedMerchantIdentifier;
                foreach (var shipping in allShippings)
                {
                    index++;
                    Log.Info("add order " + index + ", shippingId=" + shipping.Id);

                    var shippingMethod = shippingMethodList.FirstOrDefault(m => m.Id == shipping.ShippingMethodId);

                    string shippingService = String.Empty;
                    if (string.IsNullOrEmpty(shipping.ShippingService)) //NOTE: this options using Mail Labels, todo: make join
                    {
                        if (shippingMethod != null)
                            shippingService = shippingMethod.Name;
                        Log.Info("shipping service=" + shippingService);
                    }
                    else
                    {
                        shippingService = shipping.ShippingService;
                    }

                    OrderHelper.PrepareSourceItemOrderId(shipping.Items);
                    shipping.Items = OrderHelper.GroupBySourceItemOrderId(shipping.Items);

                    var carrier = shippingMethod?.CarrierName;
                    if (!String.IsNullOrEmpty(shipping.CustomCurrier))
                        carrier = shipping.CustomCurrier;

                    orderMessages.Add(FeedHelper.ComposeOrderFulfillmentMessage(index, 
                        shipping.AmazonIdentifier,
                        shipping.ShippingDate, 
                        shipping.OrderDate, 
                        ShippingUtils.FormattedToMarketShippingService(shippingService, shippingMethod.IsInternational),
                        shippingMethod != null ? ShippingUtils.FormattedToMarketCurrierName(carrier, shippingMethod.IsInternational, market) : String.Empty,
                        shipping.TrackingNumber,
                        shipping.Items));

                    if (shipping.IsFromMailPage)
                    {
                        MailInfoes.First(m => m.Id == shipping.Id).MessageIdentifier = index;
                    }
                    else
                    {
                        Shippings.First(o => o.Id == shipping.Id).MessageIdentifier = index;
                    }
                }
                Log.Info("Compose feed");
                var document = FeedHelper.ComposeFeed(orderMessages, merchant, Type.ToString());
                return new DocumentInfo
                {
                    XmlDocument = document,
                    NodesCount = index
                };
            }
            return null;
        }

        private List<ShippingDTO> PrepareMailShippings(IUnitOfWork db,
            List<ShippingDTO> mailInfoes)
        {
            var shippings = new List<ShippingDTO>();
            foreach (var mailInfo in mailInfoes)
            {
                var orderShippings = db.OrderShippingInfos.GetAllAsDto().Where(sh => sh.OrderId == mailInfo.OrderId
                    && sh.IsActive
                    && !String.IsNullOrEmpty(sh.TrackingNumber));
                if (orderShippings.Count() > 1)
                {
                    foreach (var orderShipping in orderShippings)
                    {
                        var newShipping = mailInfo.Clone();

                        newShipping.Items = db.OrderItems.GetByShippingInfoIdAsDto(orderShipping.Id)
                                //Remove canceled items with 0 price
                                .Where(m => m.ItemPrice > 0 || m.QuantityOrdered > 0).ToList();
                        shippings.Add(newShipping);
                    }
                }
                else
                {
                    mailInfo.Items = db.OrderItems.GetByOrderIdAsDto(mailInfo.OrderId)
                        //Remove canceled items with 0 price
                        .Where(m => m.ItemPrice > 0 || m.QuantityOrdered > 0).ToList();
                    shippings.Add(mailInfo);
                }
            }
            return shippings;
        }

        private List<ShippingDTO> PrepareOrderShippings(IUnitOfWork db,
            List<ShippingDTO> shippings)
        {
            foreach (var shipping in shippings)
            {
                shipping.Items = db.OrderItems.GetByShippingInfoIdAsDto(shipping.Id)
                    //Remove canceled items with 0 price
                    .Where(m => m.ItemPrice > 0 || m.QuantityOrdered > 0).ToList();
            }
            return shippings;
        }


        protected override void UpdateEntitiesBeforeSubmitFeed(IUnitOfWork db, long feedId)
        {
            foreach (var order in Shippings)
            {
                order.FeedId = feedId;
                order.IsFeedSubmitted = true;
            }
            foreach (var order in MailInfoes)
            {
                order.FeedId = feedId;
                order.IsUpdateRequired = false;
            }
        }

        protected override void UpdateEntitiesAfterResponse(long feedId, IList<FeedResultMessage> errorList)
        {
            var now = Time.GetAppNowTime();

            using (var db = DbFactory.GetRWDb())
            {
                var shippingInfos = db.OrderShippingInfos.GetFiltered(o => o.FeedId == feedId).ToList();
                var mailInfoes = db.MailLabelInfos.GetFiltered(m => m.FeedId == feedId).ToList();
                if (errorList.Any())
                {
                    foreach (var shipping in shippingInfos)
                    {
                        if (errorList.All(e => e.MessageId != shipping.MessageIdentifier))
                        {
                            shipping.IsFulfilled = true;
                        }
                        else
                        {
                            shipping.IsFeedSubmitted = false;
                            shipping.FeedId = null;
                            Log.Warn("Order not fulfilled, orderId=" + shipping.OrderId);
                        }
                        shipping.UpdateDate = now;
                    }

                    foreach (var mail in mailInfoes)
                    {
                        if (mail.MessageIdentifier.HasValue)
                        {
                            if (errorList.All(e => e.MessageId != mail.MessageIdentifier.Value))
                            {
                                mail.IsFulfilled = true;
                            }
                            else
                            {
                                mail.IsUpdateRequired = true;
                                mail.FeedId = null;
                                Log.Warn("Mail Order not fulfilled, orderId=" + mail.OrderId);
                            }
                        }
                    }
                }
                else
                {
                    foreach (var shippingInfo in shippingInfos)
                        shippingInfo.IsFulfilled = true;

                    foreach (var mailInfo in mailInfoes)
                        mailInfo.IsFulfilled = true;
                }
                db.Commit();
            }
        }

        //protected override void MarkAllAsProcessed(IUnitOfWork db, Feed feed)
        //{
            
        //    db.Commit();
        //}
    }
}
