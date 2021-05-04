using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Amazon.Core.Contracts;
using Amazon.Core.Contracts.Factories;
using Amazon.Core.Models;
using Amazon.Core.Models.Calls;
using Amazon.Model.Implementation;
using Amazon.Web.ViewModels.Products;
using CsvHelper;
using CsvHelper.Configuration;

namespace Amazon.InventoryUpdateManual.Models
{
    public class ImportRelist
    {
        private ILogService _log;
        private ITime _time;
        private IDbFactory _dbFactory;
        private ICacheService _cacheService;
        private IBarcodeService _barcodeService;
        private IEmailService _emailService;
        private ISystemActionService _actionService;
        private IAutoCreateListingService _autoCreateListingService;

        public ImportRelist(ILogService log,
            ITime time,
            IDbFactory dbFactory,
            ICacheService cacheService,
            IEmailService emailService,
            ISystemActionService actionService)
        {
            _log = log;
            _time = time;
            _dbFactory = dbFactory;
            _cacheService = cacheService;
            _emailService = emailService;
            _actionService = actionService;

            var itemHistoryService = new ItemHistoryService(_log, _time, _dbFactory);
            _barcodeService = new BarcodeService(_log, _time, _dbFactory);
            _autoCreateListingService = new AutoCreateNonameListingService(_log, _time, _dbFactory, _cacheService, _barcodeService, _emailService, itemHistoryService, AppSettings.IsDebug);
        }

        public void ImportWalmartItemsCsv(string filepath)
        {
            StreamReader streamReader = new StreamReader(filepath);
            CsvReader reader = new CsvReader(streamReader, new CsvConfiguration
            {
                HasHeaderRecord = true,
                Delimiter = ",",
                TrimFields = true,
            });

            var relistSkuList = new List<string>();
            while (reader.Read())
            {
                var sku = reader["SKU"];
                var statusReason = reader["Status Change Reason"];

                if (statusReason.Contains("Customers would save drastically by purchasing this item on a competing website"))
                    relistSkuList.Add(sku);
            }

            RelistSKUsIfNeed(relistSkuList);
        }

        public void RelistByDbStatus()
        {
            using (var db = _dbFactory.GetRWDb())
            {
                var items = db.Items.GetAll().Where(l => l.Market == (int) MarketType.Walmart
                                                           && l.ItemPublishedStatus == (int)PublishedStatuses.PublishedInactive
                                                           && l.ItemPublishedStatusReason.Contains("price")
                                                           && l.ItemPublishedStatusReason.Contains("satisfied")).ToList();
                var itemIdList = items.Select(i => i.Id).ToList();
                var listings = db.Listings.GetAll().Where(l => itemIdList.Contains(l.ItemId)).ToList();
                var skuList = listings.Select(l => l.SKU).ToList();

                RelistSKUsIfNeed(skuList);
            }
        }


        private void RelistSKUsIfNeed(IList<string> relistSkuList)
        {
            using (var db = _dbFactory.GetRWDb())
            {
                var listingsWithIssues =
                    db.Items.GetAllViewActual()
                        .Where(l => relistSkuList.Contains(l.SKU) && l.Market == (int)MarketType.Walmart)
                        .ToList();
                var styleItemIds = listingsWithIssues.Select(l => l.StyleItemId).Distinct().ToList();
                var allListings =
                    db.Items.GetAllViewActual()
                        .Where(l => styleItemIds.Contains(l.StyleItemId) && l.Market == (int)MarketType.Walmart)
                        .ToList();

                foreach (var issueListing in listingsWithIssues)
                {
                    var styleItemListings = allListings.Where(l => l.StyleItemId == issueListing.StyleItemId).ToList();
                    if (styleItemListings.Count != 1)
                    {
                        _log.Info("Skipped. Style size has more than 1 or 0 market listings = " + styleItemListings.Count);
                        continue;
                    }
                    if (styleItemListings.Sum(l => l.RealQuantity) <= 5)
                    {
                        _log.Info("Skipped. Listing with low qty, qty < 5");
                        continue;
                    }

                    _log.Info("Create clone with our barcodes, for=" + issueListing.SKU);

                    var parentASIN = issueListing.ParentASIN;
                    //Clone it, with comment
                    var parentItem = db.ParentItems.GetByASIN(parentASIN, MarketType.Walmart, null);
                    IList<MessageString> messages;
                    var itemHistoryService = new ItemHistoryService(_log, _time, _dbFactory);
                    var result = ItemEditViewModel.Clone(db,
                        _cacheService,
                        _barcodeService,
                        _actionService,
                        _autoCreateListingService,
                        itemHistoryService,
                        parentItem.Id,
                        _time.GetAppNowTime(),
                        null,
                        "[system] Copy created due the original barcodes are non-competitive",
                        out messages);

                    if (messages.Any())
                    {
                        _log.Info("ASIN=" + issueListing.ASIN + " has clone issues");
                        foreach (var msg in messages)
                        {
                            _log.Info("ASIN: " + issueListing.ASIN + ", issue: " + msg);
                        }
                    }
                    else
                    {
                        _log.Info("ASIN=" + issueListing.ASIN + " was cloned");
                    }

                    allListings.AddRange(db.Items.GetAllViewActual()
                        .Where(i => i.ParentASIN == result.ASIN && i.Market == (int)MarketType.Walmart)
                        .ToList());
                }
            }
        }
    }
}
