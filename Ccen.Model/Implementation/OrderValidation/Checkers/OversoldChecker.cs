using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Amazon.Common.Helpers;
using Amazon.Core;
using Amazon.Core.Contracts;
using Amazon.Core.Entities;
using Amazon.Core.Entities.Orders;
using Amazon.Core.Models;
using Amazon.Core.Models.Validation;
using Amazon.DTO;
using Amazon.Model.Implementation.Markets;

namespace Amazon.Model.Implementation.Validation
{
    public class OversoldChecker
    {
        private ILogService _log;
        private ITime _time;
        private IEmailService _emailService;
        private ISettingsService _settings;


        public OversoldChecker(ILogService log, ITime time, IEmailService emailService, ISettingsService settings)
        {
            _log = log;
            _time = time;
            _emailService = emailService;
            _settings = settings;
        }

        public void ProcessResult(CheckResult result, Order dbOrder)
        {
            if (result.IsSuccess)
            {
                _log.Debug("Set OnHold by CheckIsOversold");
                dbOrder.OnHold = true;
            }
        }

        public CheckResult Check(IUnitOfWork db, DTOMarketOrder order, IList<ListingOrderDTO> orderItems)
        {
            if (!orderItems.Any())
                return new CheckResult() { IsSuccess = false };

            var styleItemIds = orderItems.Where(i => i.StyleItemId.HasValue).Select(i => i.StyleItemId).ToList();

            _log.Info("CheckIsOversold: listingIds=" + String.Join(",", orderItems.Select(i => i.ListingId).ToList()));

            var cacheItems = db.StyleItemCaches.GetFiltered(si => styleItemIds.Contains(si.Id)).ToList();

            //var oversoldItems = orderItems.Where(i => i.RealQuantity <= 0).ToList(); 
            var oversoldItems = cacheItems.Where(ch => ch.RemainingQuantity < 0).ToList();
            if (oversoldItems.Count > 0)
            {
                db.OrderNotifies.Add(
                    ComposeNotify(order.Id,
                        (int)OrderNotifyType.OversoldItem,
                        1,
                        String.Join(",", oversoldItems.Select(s => s.Id).ToList()),
                        _time.GetAppNowTime()));
                db.Commit();

                var oversoldStyleItemIds = oversoldItems.Select(i => i.Id).ToList();
                var oversoldOrderItems = orderItems.Where(oi => oi.StyleItemId.HasValue).Where(oi => oversoldStyleItemIds.Contains(oi.StyleItemId.Value)).ToList();

                var text = "Oversold #" + order.OrderId + " at " + DateHelper.ToDateTimeString(order.OrderDate) + " - " + String.Join(", ",
                                  oversoldOrderItems.Select(oi => oi.SKU + " on " + MarketHelper.GetMarketName(order.Market, order.MarketplaceId)));
                _emailService.SendSystemEmail(text,
                    text,
                    _settings.GetValue<string>(SettingsHelper.KeyOversoldToNotifiers),
                    _settings.GetValue<string>(SettingsHelper.KeyOversoldCcNotifiers)
                    //EmailHelper.RafiEmail + ";" + EmailHelper.IldarDgtexEmail,
                    //EmailHelper.SupportDgtexEmail);
                );

                //Disable listing
                foreach (var oversoldOrderItem in oversoldOrderItems)
                {
                    if (oversoldOrderItem.SourceListingId.HasValue)
                    {
                        _log.Info("Raise price for listingId: " + oversoldOrderItem.SourceListingId);
                        //NOTE: rise price 3x, send qty updates
                        var dbListing = db.Listings.Get(oversoldOrderItem.SourceListingId.Value);
                        dbListing.QuantityUpdateRequested = true;
                        dbListing.QuantityUpdateRequestedDate = _time.GetAppNowTime();
                        dbListing.RealQuantity = 0;
                        dbListing.PriceUpdateRequested = true;
                        dbListing.PriceUpdateRequestedDate = _time.GetAppNowTime();
                        dbListing.CurrentPrice = dbListing.CurrentPrice * 3;

                        var saleToListings = (from ms in db.StyleItemSaleToMarkets.GetAll() 
                                            join sl in db.StyleItemSaleToListings.GetAll() on ms.Id equals sl.SaleToMarketId
                                            where sl.ListingId == dbListing.Id                                                
                                            select ms).ToList();
                        saleToListings.ForEach(sl => sl.SalePrice = 3 * sl.SalePrice);
                        db.Commit();
                    }
                }


                return new CheckResult() { IsSuccess = true };
            }

            return new CheckResult() { IsSuccess = false };
        }

        private OrderNotify ComposeNotify(
            long orderId,
            int type,
            int status,
            string message,
            DateTime when)
        {
            return new OrderNotify()
            {
                OrderId = orderId,
                Status = status,
                Type = type,
                Message = message,
                CreateDate = when
            };
        }
    }
}
