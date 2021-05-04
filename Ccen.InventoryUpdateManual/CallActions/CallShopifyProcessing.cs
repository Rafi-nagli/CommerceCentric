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
using Shopify.Api;

namespace Amazon.InventoryUpdateManual.CallActions
{
    public class CallShopifyProcessing
    {
        private ILogService _log;
        private ITime _time;
        private IDbFactory _dbFactory;
        private ICacheService _cacheService;
        private IEmailService _emailService;
        private IBarcodeService _barcodeService;
        private IAutoCreateListingService _autoCreateListingService;
        private IStyleHistoryService _styleHistoryService;

        public CallShopifyProcessing(ILogService log,
            ITime time,
            ICacheService cacheService,
            IEmailService emailService,
            IDbFactory dbFactory,
            IStyleHistoryService styleHistoryService)
        {
            _dbFactory = dbFactory;
            _cacheService = cacheService;
            _barcodeService = new BarcodeService(log, time, dbFactory);
            _log = log;
            _time = time;
            _emailService = emailService;
            _styleHistoryService = styleHistoryService;

            var itemHistoryService = new ItemHistoryService(_log, _time, _dbFactory);
            _autoCreateListingService = new AutoCreateNonameListingService(_log, _time, dbFactory, cacheService, _barcodeService, _emailService, itemHistoryService, AppSettings.IsDebug);
        }

        public void SendItemUpdates(ShopifyApi api)
        {
            var sync = new ShopifyItemsSync(_log,
                        _time,
                        _dbFactory);
            sync.SendItemUpdates(api);
        }

        public void ImportShopifyListings(IMarketApi api)
        {
            var itemSyncService = new ShopifyItemsImporter(_log,
                _time,
                _dbFactory,
                _styleHistoryService,
                null);
            itemSyncService.Import(api, null, ShopifyItemsImporter.ImportModes.Full, false, true, true, importDescription: false);
        }

        public void CreateShopifyListings()
        {
            using (var db = _dbFactory.GetRWDb())
            {
                var styleIds = db.Items.GetAll()
                    .Where(p => p.Market == (int)MarketType.Groupon)
                    .Select(i => new {
                        i.ParentASIN,
                         i.StyleId,
                        i.Market,
                        i.MarketplaceId
                        })
                    .Distinct()
                    .ToList();

                var existMarketItems = db.Items.GetAll().Where(i => i.Market == (int)MarketType.Shopify
                    && i.MarketplaceId == MarketplaceKeeper.ShopifyEveryCh).ToList();

                IList<MessageString> messages;
                foreach (var styleInfo in styleIds)
                {
                    if (existMarketItems.Any(i => i.StyleId == styleInfo.StyleId))
                        continue;

                    var model = _autoCreateListingService.CreateFromParentASIN(db,
                        styleInfo.ParentASIN,
                        styleInfo.Market,
                        styleInfo.MarketplaceId,
                        false,
                        false,
                        0,
                        out messages);

                    model.Market = (int)MarketType.Shopify;
                    model.MarketplaceId = MarketplaceKeeper.ShopifyEveryCh;

                    if (model.Variations.Select(i => i.StyleId).Distinct().Count() != 1)
                    {
                        _log.Info("Parent ASIN is multilisting");
                        continue;
                    }

                    _autoCreateListingService.PrepareData(model);
                    _autoCreateListingService.Save(model, null, db, _time.GetAppNowTime(), null);
                }
            }
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
