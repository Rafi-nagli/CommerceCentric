using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Amazon.Core.Contracts;
using Amazon.Core.Contracts.Factories;
using Amazon.Core.Models;
using Amazon.Core.Models.Calls;
using Amazon.Core.Models.Items;
using Amazon.Model.Implementation;
using Amazon.Model.Implementation.Markets.Shopify;
using Amazon.Model.Implementation.Markets.WooCommerce;
using Amazon.Model.Implementation.Sync;
using Shopify.Api;

namespace Amazon.InventoryUpdateManual.CallActions
{
    public class CallWooCommerceProcessing
    {
        private ILogService _log;
        private ITime _time;
        private IDbFactory _dbFactory;
        private ICacheService _cacheService;
        private IEmailService _emailService;
        private IBarcodeService _barcodeService;
        private IAutoCreateListingService _autoCreateListingService;

        public CallWooCommerceProcessing(ILogService log,
            ITime time,
            ICacheService cacheService,
            IEmailService emailService,
            IDbFactory dbFactory)
        {
            _dbFactory = dbFactory;
            _cacheService = cacheService;
            _barcodeService = new BarcodeService(log, time, dbFactory);
            _log = log;
            _time = time;
            _emailService = emailService;

            var itemHistoryService = new ItemHistoryService(_log, _time, _dbFactory);
            _autoCreateListingService = new AutoCreateNonameListingService(_log, _time, dbFactory, cacheService, _barcodeService, _emailService, itemHistoryService, AppSettings.IsDebug);
        }

        public void CallUpdateFulfillmentData(IMarketOrderUpdaterApi api, string orderString)
        {
            using (var db = _dbFactory.GetRWDb())
            {
                var service = new BaseOrderUpdater(api, _log, _time);
                service.UpdateOrders(db, !String.IsNullOrEmpty(orderString) ? new List<string>() { orderString } : null);
            }
        }

        public void SendItemUpdates(ShopifyApi api)
        {
            //var sync = new WooCommerceItemsSync(_log,
            //            _time,
            //            _dbFactory);
            //sync.SendItemUpdates(api);
        }

        public void ImportListings(IMarketApi api)
        {
            var itemSyncService = new WooCommerceItemsImporter(_log,
                _time,
                _dbFactory);
            itemSyncService.Import(api);
        }

        
        public void GetItems(IMarketApi api, IList<string> marketIds)
        {
            var asinsWithError = new List<string>();
            var items = api.GetItems(_log,
                _time,
                MarketItemFilters.Build(marketIds),
                ItemFillMode.Defualt,
                out asinsWithError);
        }

        public void GetOrders(IMarketApi api, string orderId)
        {
            if (String.IsNullOrEmpty(orderId))
            {
                var orders = api.GetOrders(_log, DateTime.UtcNow.AddDays(-4), null);
                _log.Info(orders.Count().ToString());
            }
            else
            {
                var orders = api.GetOrders(_log, new List<string>() { orderId });
                _log.Info(orders.Count().ToString());
            }
        }



    }
}
