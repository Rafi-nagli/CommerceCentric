using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Amazon.Common.Helpers;
using Amazon.Core.Contracts;
using Amazon.Core.Entities;
using Amazon.Core.Models;
using Amazon.Model.Implementation.Markets;

namespace Amazon.Web.ViewModels
{
    public class MarketplaceViewModel
    {
        public MarketType Market { get; set; }
        public string MarketplaceId { get; set; }

        public int? ListingQtyAlert { get; set; }
        public int? ListingPriceAlert { get; set; }
        public int? OrderCountOnMarket { get; set; }
        public int? OrderCountInDb { get; set; }
        public DateTime? OrderSyncDate { get; set; }

        public TimeSpan OrderSyncInterval { get; set; }

        public DateTime? OrdersFulfillmentDate { get; set; }
        public TimeSpan OrdersFulfillmentInterval { get; set; }


        public DateTime? ListingsSyncDate { get; set; }
        public TimeSpan ListingsSyncInterval { get; set; }
        public DateTime? ListingsPriceSyncDate { get; set; }
        public TimeSpan ListingsPriceSyncInterval { get; set; }

        public DateTime? ListingsQuantityToAmazonSyncDate { get; set; }
        public TimeSpan ListingsQuantitySyncInterval { get; set; }


        public string OrdersSyncAgoDateFormatted
        {
            get
            {
                return DateHelper.ConvertToReadableStringAgo(OrderSyncDate, true);
            }
        }

        public string OrdersSyncAfterDateFormatted
        {
            get
            {
                return DateHelper.ConvertToReadableStringAfter(OrderSyncDate + OrderSyncInterval, true);
            }
        }

        public bool OrdersSyncAfterIsOvertime
        {
            get { return OrderSyncDate + OrderSyncInterval + OrderSyncInterval < DateTime.UtcNow; }
        }

        public string OrdersFulfillmentAgoDateFormatted
        {
            get
            {
                return DateHelper.ConvertToReadableStringAgo(OrdersFulfillmentDate, true);
            }
        }

        public string OrdersFulfillmentAfterDateFormatted
        {
            get
            {
                return DateHelper.ConvertToReadableStringAfter(OrdersFulfillmentDate + OrdersFulfillmentInterval, true);
            }
        }

        public string ListingsSyncAgoDateFormatted
        {
            get { return DateHelper.ConvertToReadableStringAgo(ListingsSyncDate, true); }
        }

        public string ListingsSyncAfterDateFormatted
        {
            get { return DateHelper.ConvertToReadableStringAfter(ListingsSyncDate + ListingsSyncInterval, true); }
        }

        public bool ListingsSyncAfterIsOvertime
        {
            get { return ListingsSyncDate + ListingsSyncInterval + ListingsSyncInterval < DateTime.UtcNow; }
        }

        public string ListingsPriceSyncAgoDateFormatted
        {
            get { return DateHelper.ConvertToReadableStringAgo(ListingsPriceSyncDate, true); }
        }

        public string ListingsPriceSyncAfterDateFormatted
        {
            get { return DateHelper.ConvertToReadableStringAfter(ListingsPriceSyncDate + ListingsPriceSyncInterval, true); }
        }

        public bool ListingsPriceSyncAfterIsOvertime
        {
            get { return ListingsPriceSyncDate + ListingsPriceSyncInterval + ListingsPriceSyncInterval < DateTime.UtcNow; }
        }

        public string ListingsQuantitySyncAgoDateFormatted
        {
            get { return DateHelper.ConvertToReadableStringAgo(ListingsQuantityToAmazonSyncDate, true); }
        }

        public string ListingsQuantitySyncAfterDateFormatted
        {
            get { return DateHelper.ConvertToReadableStringAgo(ListingsQuantityToAmazonSyncDate + ListingsQuantitySyncInterval, true); }
        }

        public bool ListingsQuantitySyncAfterIsOvertime
        {
            get { return ListingsQuantityToAmazonSyncDate + ListingsQuantitySyncInterval + ListingsQuantitySyncInterval < DateTime.UtcNow; }
        }

        public string MarketShortName
        {
            get { return MarketHelper.GetDotShortName((int)Market, MarketplaceId); }
        }


        public MarketplaceViewModel()
        {
            
        }

        public MarketplaceViewModel(MarketplaceInfo marketplace, ISettingsService settings)
        {
            Market = marketplace.Market;
            MarketplaceId = marketplace.MarketplaceId;

            ListingQtyAlert = marketplace.ListingQtyAlert;
            ListingPriceAlert = marketplace.ListingPriceAlert;
            OrderCountOnMarket = marketplace.OrderCountOnMarket;
            OrderCountInDb = marketplace.OrderCountInDb;

            OrderSyncDate = marketplace.OrderSyncDate;
            OrdersFulfillmentDate = marketplace.OrdersFulfillmentDate;
            ListingsSyncDate = marketplace.ListingsSyncDate;
            ListingsPriceSyncDate = marketplace.ListingsPriceSyncDate;
            ListingsQuantityToAmazonSyncDate = marketplace.ListingsQuantityToAmazonSyncDate;

            OrderSyncInterval = settings.GetOrdersSyncInterval(marketplace.Market, marketplace.MarketplaceId);
            OrdersFulfillmentInterval = settings.OrdersFulfillmentInterval;
            ListingsSyncInterval = settings.ListingsSyncInterval;
            ListingsPriceSyncInterval = settings.ListingsPriceSyncInterval;
            ListingsQuantitySyncInterval = settings.ListingsQuantitySyncInterval;
        }
    }
}