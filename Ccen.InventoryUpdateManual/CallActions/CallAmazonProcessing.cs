using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Amazon.Api;
using Amazon.Common.ExcelExport;
using Amazon.Common.Helpers;
using Amazon.Common.Threads;
using Amazon.Core.Contracts;
using Amazon.Core.Contracts.Factories;
using Amazon.Core.Entities;
using Amazon.Core.Entities.Categories;
using Amazon.Core.Entities.Enums;
using Amazon.Core.Models;
using Amazon.Core.Models.Calls;
using Amazon.Core.Models.DropShippers;
using Amazon.Core.Models.Items;
using Amazon.DAL;
using Amazon.DTO;
using Amazon.DTO.Inventory;
using Amazon.DTO.Users;
using Amazon.Model.Implementation;
using Amazon.Model.Implementation.Errors;
using Amazon.Model.Implementation.Listings.Rules;
using Amazon.Model.Implementation.Markets.Amazon.Feeds;
using Amazon.Model.Implementation.Markets.Amazon.Readers;
using Amazon.Model.Implementation.Markets.Magento;
using Amazon.Model.Implementation.Sync;
using Amazon.Model.SyncService.Models.AmazonReports;
using Amazon.Model.SyncService.Threads.Simple.UpdateAmazonInfo;
using Amazon.ReportParser.LineParser;
using Amazon.Utils;
using Magento.Api.Wrapper;
using NPOI.HSSF.UserModel;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;

namespace Amazon.InventoryUpdateManual.CallActions
{
    public class CallAmazonProcessing
    {
        private ILogService _log;
        private ITime _time;
        private CompanyDTO _company;
        private IDbFactory _dbFactory;
        private ICacheService _cacheService;
        private IEmailService _emailService;
        private IPriceManager _priceManager;
        private IMarketCategoryService _categoryService;
        private ISystemActionService _actionService;
        private IItemHistoryService _itemHistoryService;
        private IBarcodeService _barcodeService;
        private IAutoCreateListingService _autoCreateListingService;

        public CallAmazonProcessing(CompanyDTO company,
            ILogService log,
            ITime time,
            ICacheService cacheService,
            IEmailService emailService,
            IMarketCategoryService categoryService,
            IDbFactory dbFactory,
            IPriceManager priceManager,
            ISystemActionService actionService,
            IItemHistoryService itemHistoryService)
        {
            _log = log;
            _time = time;
            _company = company;
            _dbFactory = dbFactory;
            _categoryService = categoryService;
            _cacheService = cacheService;
            _emailService = emailService;
            _priceManager = priceManager;
            _actionService = actionService;
            _barcodeService = new BarcodeService(log, time, dbFactory);
            _autoCreateListingService = new AutoCreateAmazonAUListingService(log, 
                time, 
                dbFactory, 
                _cacheService, 
                _barcodeService, 
                _emailService, 
                _actionService,
                itemHistoryService,
                AppSettings.IsDebug);
            _itemHistoryService = itemHistoryService;
        }

        public void FillMissingBarcodes(AmazonApi api)
        {
            using (var db = _dbFactory.GetRWDb())
            {
                var listingIds = (from i in db.FBAPickListEntries.GetAll()
                                 select i.ListingId).ToList();

                var items = (from i in db.Items.GetAll()
                             join l in db.Listings.GetAll() on i.Id equals l.ItemId
                             where listingIds.Contains(l.Id)
                             select i).ToList();

                //var items = (from i in db.Items.GetAll()
                //             join l in db.Listings.GetAll() on i.Id equals l.ItemId
                //             where i.MarketplaceId == MarketplaceKeeper.AmazonComMarketplaceId
                //                 && !l.IsRemoved
                //                 && String.IsNullOrEmpty(i.Barcode)
                //             select i)
                //             .OrderByDescending(l => l.CreateDate)
                //             .Take(2000)
                //             .ToList();

                //var itemsToUpdate = items.Select(i => new ItemDTO()
                //{
                //    ASIN = i.ASIN
                //}).ToList();
                var itemsToUpdate = new List<ItemDTO>()
                {
                    new ItemDTO()
                    {
                        ASIN = "B07FQRCSHS"
                    },
                                        new ItemDTO()
                    {
                        ASIN = "B07C855XWC"
                    },
                                                            new ItemDTO()
                    {
                        ASIN = "B07C82XY5H"
                    }
                };

                List<ItemDTO> itemsWithError = new List<ItemDTO>();
                api.FillWithAdditionalInfoByAdvAPI(_log, _time, itemsToUpdate, out itemsWithError);

                foreach (var itemDto in itemsToUpdate)
                {
                    if (!String.IsNullOrEmpty(itemDto.Barcode))
                    {
                        var dbItems = db.Items.GetAll().Where(i => i.ASIN == itemDto.ASIN).ToList();
                        foreach (var dbItem in dbItems)
                        {
                            _log.Info("ASIN: " + dbItem.ASIN + ", Barcode: " + dbItem.Barcode + "=>" + itemDto.Barcode);
                            dbItem.Barcode = itemDto.Barcode;
                        }
                    }
                }

                db.Commit();
            }
        }

        //public void CallFixImageDefects()
        //{
        //    var service = new ItemAutoFixIssueService(_log, _time, _dbFactory, _actionService, _itemHistoryService);
        //    service.ProcessDefectsRules();
        //}

        //public void CallFixItemColor()
        //{
        //    var service = new ItemAutoFixIssueService(_log, _time, _dbFactory, _actionService, _itemHistoryService);
        //    var rules = new List<IItemAutoFixRule>()
        //    {
        //        new FixColorIssueRule(_dbFactory, _log)
        //    };
        //    service.ProcessRules(rules);
        //}

        //public void CallFixItemBrand()
        //{
        //    var service = new ItemAutoFixIssueService(_log, _time, _dbFactory, _actionService, _itemHistoryService);
        //    var rules = new List<IItemAutoFixRule>()
        //    {
        //        new FixBrandIssueRule(_dbFactory, _log)
        //    };
        //    service.ProcessRules(rules);
        //}

        //public void CallRequestUnpublishUPCIssueListings()
        //{
        //    var service = new ItemAutoFixIssueService(_log, _time, _dbFactory, _actionService, _itemHistoryService);
        //    service.RequestUnpublishWithUPCIssueListings();
        //}

        //public void CallRequestFixRelationship()
        //{
        //    var service = new ItemAutoFixIssueService(_log, _time, _dbFactory, _actionService, _itemHistoryService);
        //    service.RequestUpdatesForUngroupedListings();
        //}

        //public void CallRequestFixPublishingInProgress()
        //{
        //    var service = new ItemAutoFixIssueService(_log, _time, _dbFactory, _actionService, _itemHistoryService);
        //    service.RequestUpdatesForPublishingInProgressListings();
        //}

        public void ExportUnderwearException()
        {
            var underwearSKUs = new List<string>();
            using (var db = _dbFactory.GetRWDb())
            {
                underwearSKUs = (from l in db.Listings.GetAll()
                                     join i in db.Items.GetAll() on l.ItemId equals i.Id
                                     join st in db.Styles.GetAll() on i.StyleId equals st.Id
                                     join t in db.StyleFeatureValues.GetAll() on st.Id equals t.StyleId
                                     where !l.IsRemoved
                                        && l.Market == (int)MarketType.Amazon
                                        && l.MarketplaceId == MarketplaceKeeper.AmazonComMarketplaceId
                                        && t.FeatureValueId == 342 //NOTE: ItemStyle, Underwear
                                        && l.RealQuantity > 0
                                     select l.SKU).ToList();
            }

            
        }

        public void CreateSFPListings()
        {
            var now = _time.GetAppNowTime();
            using (var db = _dbFactory.GetRWDb())
            {
                db.DisableValidation();
                db.DisableAutoDetectChanges();
                db.DisableProxyCreation();

                var existPrimeItems = (from i in db.Items.GetAll()
                                       join l in db.Listings.GetAll() on i.Id equals l.ItemId
                                       where i.MarketplaceId == MarketplaceKeeper.AmazonComMarketplaceId
                                         && l.IsPrime
                                         && !l.IsRemoved
                                       select new
                                       {
                                           StyleItemId = i.StyleItemId,
                                           ParentASIN = i.ParentASIN,
                                           Quantity = l.RealQuantity
                                       }).ToList();

                var topStyles = (from sic in db.StyleItemCaches.GetAll()
                                 join st in db.Styles.GetAll() on sic.StyleId equals st.Id
                                 group sic by sic.StyleId into byStyle 
                                 select new
                                {
                                    StyleId = byStyle.Key,
                                    Quantity = byStyle.Sum(si => si.RemainingQuantity)
                                }).ToList();

                //topStyles = topStyles.Where(st => st.Quantity > 100).ToList();

                topStyles = topStyles.OrderByDescending(st => st.Quantity)
                    .ToList();
                
                var styleIdToCopy = topStyles.Select(st => st.StyleId).ToList();

                var toCopyItems = (from i in db.Items.GetAll()
                                  join l in db.Listings.GetAll() on i.Id equals l.ItemId
                                  where i.MarketplaceId == MarketplaceKeeper.AmazonComMarketplaceId
                                        && styleIdToCopy.Contains(i.StyleId)
                                        && !l.IsRemoved
                                        && !l.IsPrime
                                        && !l.IsFBA
                                        && l.RealQuantity > 0
                                  select new ItemDTO()
                                {
                                    Id = i.Id,
                                    SKU = l.SKU,
                                    StyleId = i.StyleId,
                                    StyleItemId = i.StyleItemId,
                                    ParentASIN = i.ParentASIN
                                })
                                .ToList();

                foreach (var itemToCopy in toCopyItems)
                {
                    var existPrime = existPrimeItems.FirstOrDefault(i => i.ParentASIN == itemToCopy.ParentASIN
                        && i.StyleItemId == itemToCopy.StyleItemId);
                    if (existPrime == null)
                    {                        
                        var dbItemToCopy = db.Items.GetAll().FirstOrDefault(i => i.Id == itemToCopy.Id);
                        var dbListingToCopy = db.Listings.GetAll().FirstOrDefault(l => l.ItemId == itemToCopy.Id
                            && !l.IsRemoved);

                        var salePrice = (from sl in db.StyleItemSaleToListings.GetAll()
                                        join sm in db.StyleItemSaleToMarkets.GetAll() on sl.SaleToMarketId equals sm.Id
                                        join s in db.StyleItemSales.GetAll() on sm.SaleId equals s.Id
                                        where sl.ListingId == dbListingToCopy.Id
                                         && sm.MarketplaceId == MarketplaceKeeper.AmazonComMarketplaceId
                                         && !s.IsDeleted
                                         && (s.SaleStartDate.HasValue && s.SaleStartDate < now)
                                         && sm.SalePrice.HasValue
                                        select sm.SalePrice).FirstOrDefault();

                        //var dbViewItem = db.Items.GetAllViewActual().FirstOrDefault(i => i.Id == dbItemToCopy.Id);
                        //if (dbViewItem == null)
                        //{
                        //    _log.Info("dbViewItem = null");
                        //    continue;
                        //}
                        _log.Info("Copy listing, SKU=" + itemToCopy.SKU + ", price=" + dbListingToCopy.CurrentPrice + ", salePrice=" + salePrice);

                        var isOversizeTemplate = dbItemToCopy.OnMarketTemplateName == AmazonTemplateHelper.OversizeTemplate;

                        dbItemToCopy.Id = 0;
                        dbItemToCopy.ItemPublishedStatus = (int)PublishedStatuses.New;
                        dbItemToCopy.ItemPublishedStatusDate = null;
                        dbItemToCopy.ItemPublishedStatusBeforeRepublishing = null;
                        dbItemToCopy.ItemPublishedStatusFromMarket = null;
                        dbItemToCopy.ItemPublishedStatusFromMarketDate = null;
                        dbItemToCopy.ItemPublishedStatusReason = null;
                        dbItemToCopy.OnMarketTemplateName = AmazonTemplateHelper.PrimeTemplate;
                        dbItemToCopy.CreateDate = _time.GetAppNowTime();
                        dbItemToCopy.UpdateDate = _time.GetAppNowTime();
                        dbItemToCopy.CreatedBy = null;
                        dbItemToCopy.UpdatedBy = null;
                        db.Items.Add(dbItemToCopy);
                        db.Commit();

                        var newSKU = SkuHelper.AddPrimePostFix(dbListingToCopy.SKU);
                        var price = salePrice ?? dbListingToCopy.CurrentPrice;
                        dbListingToCopy.ItemId = dbItemToCopy.Id;
                        dbListingToCopy.Id = 0;
                        dbListingToCopy.ListingId = newSKU;
                        dbListingToCopy.IsPrime = true;
                        dbListingToCopy.CurrentPrice = isOversizeTemplate ? (price + 9.49M) : (price + 7.49M);
                        dbListingToCopy.ListingPriceFromMarket = null;
                        dbListingToCopy.AmazonCurrentPrice = null;
                        dbListingToCopy.AmazonCurrentPriceUpdateDate = null;
                        dbListingToCopy.AmazonRealQuantity = null;
                        dbListingToCopy.AmazonRealQuantityUpdateDate = null;
                        dbListingToCopy.PriceFromMarketUpdatedDate = null;
                        dbListingToCopy.SKU = newSKU;
                        dbListingToCopy.OpenDate = _time.GetAppNowTime();
                        dbListingToCopy.CreateDate = _time.GetAppNowTime();
                        dbListingToCopy.UpdateDate = _time.GetAppNowTime();
                        dbListingToCopy.CreatedBy = null;
                        dbListingToCopy.UpdatedBy = null;
                        db.Listings.Add(dbListingToCopy);
                        db.Commit();
                        _log.Info("Copyied to SKU=" + dbListingToCopy.SKU + ", newPrice=" + dbListingToCopy.CurrentPrice);
                    }
                }
            }
        }

        public void FixupParentASINsForAU()
        {
            using (var db = _dbFactory.GetRWDb())
            {
                var parentItems = db.ParentItems.GetAll().Where(pi => pi.Market == (int) MarketType.AmazonAU).ToList();
                var items = db.Items.GetAll().Where(i => i.Market == (int) MarketType.AmazonAU).ToList();
                foreach (var parentItem in parentItems)
                {
                    var item = items.FirstOrDefault(i => i.ParentASIN == parentItem.ASIN);
                    if (item != null)
                    {
                        var allItemsForStyle = items.Where(i => i.StyleId == item.StyleId).ToList();
                        allItemsForStyle.ForEach(i => i.ParentASIN = parentItem.ASIN);
                    }
                }
                db.Commit();
            }
        }

        public void CreateAmazonAUListings()
        {
            _autoCreateListingService.CreateListings();
        }

        public void ProcessInactiveReport(string filename)
        {
            var parser = new ListingLineParser(_log);
            var lines = File.ReadAllLines(filename);
            var headers = (lines.FirstOrDefault() ?? "").Split('\t');
 
            var items = new List<ItemDTO>();
            foreach (var line in lines.Skip(1))
            {
                items.Add((ItemDTO)parser.Parse((line ?? "").Split('\t'), headers));
            }

            _log.Info("Count: " + items.Count);
            using (var db = _dbFactory.GetRWDb())
            {
                var skuList = items.Select(i => i.SKU).ToList();
                var dbItems = (from l in db.Listings.GetAll()
                               join i in db.Items.GetAll() on l.ItemId equals i.Id
                               join si in db.StyleItems.GetAll() on i.StyleItemId equals si.Id
                               where skuList.Contains(l.SKU)
                               select new ItemDTO
                               {
                                   RemainingQuantity = si.Quantity,
                                   SKU = l.SKU,
                                   RealQuantity = l.RealQuantity
                               }).ToList();

                var itemsWithIssue = new List<ItemDTO>();
                foreach (var item in items)
                {
                    var dbItem = dbItems.FirstOrDefault(l => l.SKU == item.SKU);
                    if (dbItem != null)
                    {
                        if (dbItem.RemainingQuantity >= 10)
                        {
                            itemsWithIssue.Add(dbItem);
                        }
                    }
                }

                _log.Info("Count with issue: " + itemsWithIssue.Count());
                _log.Info("SKUs: " + String.Join(", ", itemsWithIssue.Select(i => i.SKU).ToList()));
            }
        }

        public void UpdateBuyBoxPrices(AmazonApi api)
        {
            using (var db = _dbFactory.GetRWDb())
            {
                var itemsToCheck = (from st in db.Styles.GetAll()
                                    join si in db.StyleItems.GetAll() on st.Id equals si.StyleId
                                    join sib in db.StyleItemBarcodes.GetAll() on si.Id equals sib.StyleItemId
                                    where !st.Deleted
                                        && si.Quantity > 0
                                        && st.DropShipperId.HasValue
                                    orderby st.Id descending
                                    select sib.Barcode)
                                    .Distinct()
                                    //.Take(50)
                                    .ToList()
                                    .Select(i => new ReportLine()
                                    {
                                        Barcode = i
                                    })
                                    .ToList();

                var index = 0;
                var pageSize = 10;
                while (index < itemsToCheck.Count)
                {
                    var pageItems = itemsToCheck.Skip(index).Take(pageSize).ToList();

                    var barcodesToFill = pageItems
                        .Where(i => !String.IsNullOrEmpty(i.Barcode)
                                                          || i.Barcode != "N/A")
                        .Select(i => new ItemDTO()
                        {
                            Barcode = i.Barcode
                        })
                        .ToList();

                    var itemWithError = new List<ItemDTO>();
                    api.FillWithAdditionalInfoByAdvAPIUsingBarcode(_log, _time, barcodesToFill, "Fashion", out itemWithError);

                    foreach (var item in pageItems)
                    {
                        var amazonInfo = barcodesToFill.FirstOrDefault(bc => bc.Barcode == item.Barcode);
                        if (amazonInfo != null)
                        {
                            item.AmazonName = amazonInfo.Name;
                            item.AmazonLink = "http://www.amazon.com/dp/" + amazonInfo.ASIN;
                            item.ASIN = amazonInfo.ASIN;
                            item.AmazonBuyBoxPrice = amazonInfo.LowestPrice;
                        }
                    }

                    index += pageSize;
                }

                var items = itemsToCheck
                    .Where(i => !String.IsNullOrEmpty(i.ASIN))
                    .Select(i => new BuyBoxStatusDTO()
                    {
                        Barcode = i.Barcode,
                        Market = (int)api.Market,
                        MarketplaceId = api.MarketplaceId,
                        WinnerPrice = i.AmazonBuyBoxPrice
                    })
                    .ToList();

                db.BuyBoxStatus.UpdateBulkByBarcode(items, api.Market, api.MarketplaceId, _time.GetAppNowTime());
            }
        }


        private List<ItemDTO> LoadItemFromFile(string filepath)
        {
            var items = new List<ItemDTO>();

            using (var stream = new FileStream(filepath, FileMode.Open, FileAccess.ReadWrite))
            {
                IWorkbook workbook = null;
                if (filepath.EndsWith(".xlsx"))
                    workbook = new XSSFWorkbook(stream);
                else
                    workbook = new HSSFWorkbook(stream);

                var sheet = workbook.GetSheetAt(0);

                var headerRow = sheet.GetRow(0);

                var modelColumnIndex = headerRow.Cells.FirstOrDefault(c => c != null
                                                                           &&
                                                                           (StringHelper.IsEqualNoCase(
                                                                               StringHelper.TrimWhitespace(
                                                                                   c.StringCellValue), "SKU")
                                                                            ||
                                                                            StringHelper.IsEqualNoCase(
                                                                                StringHelper.TrimWhitespace(
                                                                                    c.StringCellValue), "Model")
                                                                            ||
                                                                            StringHelper.IsEqualNoCase(
                                                                                StringHelper.TrimWhitespace(
                                                                                    c.StringCellValue), "Product Id")))?
                    .ColumnIndex;

                var minPriceColumnIndex = headerRow.Cells.FirstOrDefault(c => c != null
                                                                              &&
                                                                              (StringHelper.IsEqualNoCase(
                                                                                  StringHelper.TrimWhitespace(
                                                                                      c.StringCellValue),
                                                                                  "Minimum for BUY BOX")
                                                                              || StringHelper.IsEqualNoCase(
                                                                                  StringHelper.TrimWhitespace(
                                                                                      c.StringCellValue),
                                                                                  "Min Price")))?.ColumnIndex;
                var maxPriceColumnIndex = headerRow.Cells.FirstOrDefault(c => c != null
                                                                              &&
                                                                              (StringHelper.IsEqualNoCase(
                                                                                  StringHelper.TrimWhitespace(
                                                                                      c.StringCellValue),
                                                                                  "Amazon Sell Price")
                                                                                  || StringHelper.IsEqualNoCase(
                                                                                  StringHelper.TrimWhitespace(
                                                                                      c.StringCellValue),
                                                                                  "Max Price")))?.ColumnIndex;

                for (var i = 1; i <= sheet.LastRowNum; i++)
                {
                    var row = sheet.GetRow(i);
                    if (row.GetCell(0) != null)
                    {
                        try
                        {
                            items.Add(new ItemDTO()
                            {
                                SKU = row.GetCell(modelColumnIndex.Value).ToString(),
                                CurrentPrice =
                                    (maxPriceColumnIndex.HasValue && row.GetCell(maxPriceColumnIndex.Value) != null)
                                        ? PriceHelper.RoundToTwoPrecision(ExcelHelper.TryGetCellDecimal(row.GetCell(maxPriceColumnIndex.Value))) ?? 0
                                        : 0,
                                SalePrice =
                                    (minPriceColumnIndex.HasValue && row.GetCell(minPriceColumnIndex.Value) != null)
                                        ? PriceHelper.RoundToTwoPrecision(ExcelHelper.TryGetCellDecimal(row.GetCell(minPriceColumnIndex.Value))) ?? 0
                                        : 0,
                            });
                        }
                        catch (Exception ex)
                        {
                            _log.Info("Issue with processing: " + ex.Message);
                        }
                    }
                }
            }

            return items;
        }


        public void CallReadListingData(AmazonApi api)
        {
            var reportFactory = new AmazonReportFactory(_time);
            var thread = new UpdateAmazonRequestedReportThread("UpdateListingsReport" + "Com",
                    (AmazonApi)api,
                    _company.Id,
                    null,
                    reportFactory.GetReportService(AmazonReportType._GET_MERCHANT_LISTINGS_DATA_, api.MarketplaceId),
                    TimeSpan.FromMinutes(1));
            thread.Start();
        }

        public void CallDeleteAmazonCAFBAListings(AmazonApi api)
        {
            if (api.MarketplaceId != MarketplaceKeeper.AmazonCaMarketplaceId)
            {
                throw new NotSupportedException("Not supported");
            }

            using (var db = _dbFactory.GetRWDb())
            {
                //var listingSKUs = db.Listings.GetAll().Where(l => l.Market == (int)MarketType.Amazon
                //        && l.MarketplaceId == MarketplaceKeeper.AmazonCaMarketplaceId
                //        && l.IsFBA
                //        && !l.IsRemoved)
                //    .Select(l => l.SKU)
                //    .ToList();

                //_log.Info("All FBA candidates: " + listingSKUs.Count());
                //listingSKUs = listingSKUs.Where(sku => StringHelper.ContainsNoCase(sku, "-fba")).ToList();

                //_log.Info("Actual FBAs: " + listingSKUs.Count());

                CallDeleteListingData(api, null);
            }            
        }

        public void CallDeleteListingData(AmazonApi api, IList<string> skuList)
        {
            var updated = false;
            long? feedId = null;
            while (true)
            {
                var dataUpdater = new DeleteItemUpdater(_log, _time, _dbFactory);
                string unprocessedFeedId = null;
                if (feedId.HasValue)
                    unprocessedFeedId = dataUpdater.GetUnprocessedFeedId(feedId.Value);
                
                if (unprocessedFeedId != null)
                {
                    _log.Info("Update unprocessed data feed");
                    dataUpdater.UpdateSubmittedFeed(api,
                        unprocessedFeedId,
                        AppSettings.FulfillmentResponseDirectory);
                    Thread.Sleep(5000);
                }
                else
                {
                    _log.Info("Submit new data feed");
                    feedId = dataUpdater.SubmitFeed(api, _company.Id, skuList, AppSettings.FulfillmentRequestDirectory);
                    Thread.Sleep(60000);
                }
            }
        }

        public void CallUpdateForNotPublishedLocked(AmazonApi api)
        {
            using (var db = _dbFactory.GetRWDb())
            {
                var parentAsins = db.ParentItems.GetAll().Where(pi => pi.LockMarketUpdate 
                    && pi.Market == (int)MarketType.Amazon)
                    .Select(pi => pi.ASIN)
                    .ToList();
                var itemSKUs = (from i in db.Items.GetAll()
                             join l in db.Listings.GetAll() on i.Id equals l.ItemId
                             where i.ItemPublishedStatus == (int)PublishedStatuses.New
                                 && parentAsins.Contains(i.ParentASIN)
                                 && i.MarketplaceId == MarketplaceKeeper.AmazonComMarketplaceId
                                 && !l.IsRemoved
                             select l.SKU).ToList();

                CallUpdateListingData(api, itemSKUs);
            }
        }

        public void CallUpdateAllChildListingData(AmazonApi api, IList<string> skuList)
        {
            using (var db = _dbFactory.GetRWDb())
            {
                var items = db.Items.GetAllViewAsDto().Where(i => skuList.Contains(i.SKU)
                    && i.Market == (int)api.Market
                    && i.MarketplaceId == api.MarketplaceId).ToList();
                var parentASINs = items.Select(i => i.ParentASIN).ToList();
                var allSKUs = db.Items.GetAllViewAsDto().Where(i => parentASINs.Contains(i.ParentASIN)
                        && i.Market == (int)api.Market
                        && i.MarketplaceId == api.MarketplaceId)
                    .Select(i => i.SKU).ToList();

                CallUpdateListingData(api, allSKUs);
            }
        }

        public void CallUpdateListingData(AmazonApi api, IList<string> skuList)
        {
            var updated = false;
            long? feedId = null;
            while (true)
            {
                var dataUpdater = new ItemDataUpdater(_log, _time, _dbFactory, _categoryService);
                string unprocessedFeedId = null;
                if (feedId.HasValue)
                    unprocessedFeedId = dataUpdater.GetUnprocessedFeedId(feedId.Value);

                if (unprocessedFeedId != null)
                {
                    _log.Info("Update unprocessed data feed");
                    dataUpdater.UpdateSubmittedFeed(api,
                        unprocessedFeedId,
                        AppSettings.FulfillmentResponseDirectory);
                    Thread.Sleep(5000);
                }
                else
                {
                    _log.Info("Submit new data feed");
                    feedId = dataUpdater.SubmitFeed(api, _company.Id, skuList, AppSettings.FulfillmentRequestDirectory);
                    Thread.Sleep(60000);
                }
            }
        }

        public void CallUpdateAllItemsPriceData(AmazonApi api)
        {

        }

        public void CallUpdatePriceData(AmazonApi api, IList<string> skuList)
        {
            var updated = false;
            long? feedId = null;
            while (!updated)
            {
                var updater = new ItemPriceUpdater(_log, _time, _dbFactory);
                string unprocessedFeedId = null;
                if (feedId.HasValue)
                    unprocessedFeedId = updater.GetUnprocessedFeedId(feedId.Value);
                if (unprocessedFeedId != null)
                {
                    _log.Info("Update unprocessed feed");
                    updater.UpdateSubmittedFeed(api,
                        unprocessedFeedId,
                        AppSettings.FulfillmentResponseDirectory);
                    updated = true;
                }
                else
                {
                    _log.Info("Submit new feed");
                    feedId = updater.SubmitFeed(api, _company.Id, skuList, AppSettings.FulfillmentRequestDirectory);
                }
                Thread.Sleep(60000);
            }
        }

        public void CallUpdatePriceRuleData(AmazonApi api, IList<string> skuList)
        {
            var updated = false;
            long? feedId = null;
            while (!updated)
            {
                var updater = new ItemPriceRuleUpdater(_log, _time, _dbFactory);
                string unprocessedFeedId = null;
                //if (feedId.HasValue)
                    unprocessedFeedId = updater.GetUnprocessedFeedId(api.MarketplaceId);
                if (unprocessedFeedId != null)
                {
                    _log.Info("Update unprocessed feed");
                    updater.UpdateSubmittedFeed(api,
                        unprocessedFeedId,
                        AppSettings.FulfillmentResponseDirectory);
                    updated = true;
                }
                else
                {
                    _log.Info("Submit new feed");
                    feedId = updater.SubmitFeed(api, _company.Id, skuList, AppSettings.FulfillmentRequestDirectory);
                }
                Thread.Sleep(60000);
            }
        }


        public void CallUpdateListingImageData(AmazonApi api, IList<string> asinList)
        {
            var updated = false;
            long? feedId = null;
            while (!updated)
            {
                var dataUpdater = new ItemImageUpdater(_log, _time, _dbFactory);
                string unprocessedFeedId = null;
                if (feedId.HasValue)
                    unprocessedFeedId = dataUpdater.GetUnprocessedFeedId(api.MarketplaceId);
                if (unprocessedFeedId != null)
                {
                    _log.Info("Update unprocessed image feed");
                    dataUpdater.UpdateSubmittedFeed(api,
                        unprocessedFeedId,
                        AppSettings.FulfillmentResponseDirectory);
                    Thread.Sleep(5000);
                }
                else
                {
                    _log.Info("Submit new image feed");
                    feedId = dataUpdater.SubmitFeed(api, _company.Id, asinList, AppSettings.FulfillmentRequestDirectory);
                    Thread.Sleep(60000);
                }
            }
        }

        public void CallUpdateListingRelationshipData(AmazonApi api, IList<string> skuList)
        {
            var updated = false;
            long? feedId = null;
            while (true)
            {
                var dataUpdater = new ItemRelationshipUpdater(_log, _time, _dbFactory);
                string unprocessedFeedId = null;
                if (feedId.HasValue)
                    unprocessedFeedId = dataUpdater.GetUnprocessedFeedId(feedId.Value);

                if (unprocessedFeedId != null)
                {
                    _log.Info("Update unprocessed relationship feed");
                    dataUpdater.UpdateSubmittedFeed(api,
                        unprocessedFeedId,
                        AppSettings.FulfillmentResponseDirectory);
                    Thread.Sleep(5000);
                }
                else
                {
                    _log.Info("Submit new relationship feed");
                    feedId = dataUpdater.SubmitFeed(api, _company.Id, skuList, AppSettings.FulfillmentRequestDirectory);
                    Thread.Sleep(60000);
                }
            }
        }

        public void CallUpdateQuantityData(AmazonApi api, IList<string> asinList)
        {
            var updated = false;
            long? feedId = null;
            while (true)
            {
                var quantityUpdater = new ItemQuantityUpdater(_log, _time, _dbFactory, AppSettings.AmazonFulfillmentLatencyDays);
                string unprocessedFeedId = null;
                if (feedId.HasValue)
                    unprocessedFeedId = quantityUpdater.GetUnprocessedFeedId(feedId.Value);

                if (unprocessedFeedId != null)
                {
                    _log.Info("Update unprocessed quantity feed");
                    quantityUpdater.UpdateSubmittedFeed(api,
                        unprocessedFeedId,
                        AppSettings.FulfillmentResponseDirectory);
                    Thread.Sleep(5000);
                }
                else
                {
                    _log.Info("Submit new quantity feed");
                    feedId = quantityUpdater.SubmitFeed(api, _company.Id, asinList, AppSettings.FulfillmentRequestDirectory);
                    Thread.Sleep(60000);
                }
            }
        }


        public class ReportLine
        {
            public long StyleId { get; set; }
            public string SKU { get; set; }
            public string ASIN { get; set; }
            public string Barcode { get; set; }
            public string AmazonLink { get; set; }
            public decimal? AmazonBuyBoxPrice { get; set; }
            public string DropShipperName { get; set; }
            public decimal? Cost { get; set; }
            public decimal? MagentoPrice { get; set; }
            public decimal? MagentoSalePrice { get; set; }
            public DateTime? MagentoSaleEndDate { get; set; }
            public string StyleName { get; set; }
            public string AmazonName { get; set; }

            public int Quantity { get; set; }
            public int CostMode { get; set; }
            public decimal? CostMultiplier { get; set; }

            public string IsFoundOnAmazon
            {
                get { return String.IsNullOrEmpty(ASIN) ? "No" : "Yes"; }
            }

            public decimal? FormattedCost
            {
                get
                {
                    return CostMode == (int)CostModes.PercentFromSalePrice
                        ? CostMultiplier * (MagentoSalePrice ?? MagentoPrice ?? 0)
                        : Cost;
                }
            }
        }

        public void GetProductInfoByBarcode(AmazonApi api)
        {
            using (var db = _dbFactory.GetRWDb())
            {
                var listingQuery = from i in db.Items.GetAllViewActual()
                                   where i.Market == (int)MarketType.Magento
                                   select i;

                var dsItemQuery = from dsi in db.DSItems.GetAll()
                                  where dsi.Status == (int)DSItemStatuses.Active
                                  select dsi;

                var itemsToCheck = (from st in db.Styles.GetAll()
                                    join ds in db.DropShippers.GetAll() on st.DropShipperId equals ds.Id
                                    join si in db.StyleItems.GetAll() on st.Id equals si.StyleId
                                    join sib in db.StyleItemBarcodes.GetAll() on si.Id equals sib.StyleItemId
                                    join dsItem in dsItemQuery on new { StyleItemId = (long?)si.Id, DropShipperId = ds.Id } equals new { dsItem.StyleItemId, dsItem.DropShipperId } into withDSItem
                                    from dsItem in withDSItem.DefaultIfEmpty()
                                    join vi in listingQuery on st.Id equals vi.StyleId
                                    where //(st.Manufacturer == "Invicta" || st.Manufacturer == "Technamarine")
                                        dsItem.DropShipperId == DSHelper.VLCDsId
                                        //&& si.Quantity > 0
                                        && !st.Deleted
                                    orderby st.Id descending
                                    select new ReportLine
                                    {
                                        StyleId = st.Id,
                                        SKU = st.StyleID,
                                        StyleName = st.Name,
                                        DropShipperName = ds.Name,
                                        CostMode = ds.CostMode,
                                        CostMultiplier = ds.CostMultiplier,
                                        Cost = dsItem.Cost,
                                        Quantity = si.Quantity ?? 0,
                                        Barcode = sib.Barcode,
                                        MagentoPrice = vi.CurrentPrice,
                                        MagentoSalePrice = vi.SalePrice,
                                        MagentoSaleEndDate = vi.SaleStartDate,
                                    })
                                    //.Take(50)
                                    .ToList();

                var index = 0;
                var pageSize = 10;
                while (index < itemsToCheck.Count)
                {
                    var pageItems = itemsToCheck.Skip(index).Take(pageSize).ToList();

                    var barcodesToFill = pageItems
                        .Where(i => !String.IsNullOrEmpty(i.Barcode)
                                                          || i.Barcode != "N/A")
                        .Select(i => new ItemDTO()
                        {
                            Barcode = i.Barcode
                        })
                        .ToList();

                    var itemWithError = new List<ItemDTO>();
                    api.FillWithAdditionalInfoByAdvAPIUsingBarcode(_log, _time, barcodesToFill, "Fashion", out itemWithError);

                    foreach (var item in pageItems)
                    {
                        var amazonInfo = barcodesToFill.FirstOrDefault(bc => bc.Barcode == item.Barcode);
                        if (amazonInfo != null)
                        {
                            item.AmazonName = amazonInfo.Name;
                            item.AmazonLink = "http://www.amazon.com/dp/" + amazonInfo.ASIN;
                            item.ASIN = amazonInfo.ASIN;
                            item.AmazonBuyBoxPrice = amazonInfo.LowestPrice;
                        }
                    }

                    index += pageSize;
                }

                var b = new ExportColumnBuilder<ReportLine>();
                var columns = new List<ExcelColumnInfo>()
                {
                    b.Build(p => p.SKU, "SKU", 15),
                    b.Build(p => p.Barcode, "Barcode", 15),
                    b.Build(p => p.IsFoundOnAmazon, "Found on Amazon", 15),
                    b.Build(p => p.ASIN, "ASIN", 15),

                    b.Build(p => p.AmazonLink, "AmazonLink", 15),
                    b.Build(p => p.AmazonBuyBoxPrice, "Amazon Buy Box Price", 15),
                    b.Build(p => p.DropShipperName, "Dropshipper", 15),
                    b.Build(p => p.Quantity, "Quantity", 15),
                    b.Build(p => p.FormattedCost, "Cost", 15),
                    b.Build(p => p.MagentoPrice, "Magento price", 15),
                    b.Build(p => p.MagentoSalePrice, "Magento sale price", 15),
                    b.Build(p => p.MagentoSaleEndDate, "Magento sale end date", 15),
                    b.Build(p => p.StyleName, "Out name", 15),
                    b.Build(p => p.AmazonName, "Amazon name", 15),
                };

                var filepath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory,
                    //"DWS_Invicta_Technamarine_AmazonInfo_" + DateTime.Now.ToString("yyyyMMddHHmmss") + ".xls");
                    "DWS_VLC_AmazonInfo_" + DateTime.Now.ToString("yyyyMMddHHmmss") + ".xls");

                using (var stream = ExcelHelper.Export(itemsToCheck, columns))
                {
                    stream.Seek(0, SeekOrigin.Begin);
                    using (var fileStream = File.Create(filepath))
                    {
                        stream.CopyTo(fileStream);
                    }
                }
            }
        }

        public void GetProductInfoByBarcode(AmazonApi api, string barcode)
        {
            var result = api.GetProductForBarcode(new List<string>() { barcode });
            _log.Info(result.ToString());
        }

        public void GetProductInfoByASIN(AmazonApi api, string asin)
        {
            var result = api.GetProductForASIN(new List<string>() { asin });
            _log.Info(result.ToString());
        }

        public void CallUpdateItem(AmazonApi api)
        {
            using (var db = _dbFactory.GetRWDb())
            {
                var dbItems = (from i in db.Items.GetAll()
                               join l in db.Listings.GetAll() on i.Id equals l.ItemId
                               join fbi in db.FBAPickListEntries.GetAll() on l.Id equals fbi.ListingId
                               where String.IsNullOrEmpty(i.Barcode)
                               select i).ToList();

                var asinsWithError = new List<string>() { };

                foreach (var dbItem in dbItems)
                {
                    var items = api.GetItems(_log, _time, MarketItemFilters.Build(new List<string>() { dbItem.ASIN }), ItemFillMode.Defualt,
                        out asinsWithError);
                    var itemWithError = new List<ItemDTO>();
                    api.FillWithAdditionalInfo(_log, _time, new List<ItemDTO>()
                        {
                            new ItemDTO()
                            {
                                ASIN = dbItem.ASIN,
                            }
                        },
                        IdType.ASIN,
                        ItemFillMode.IncludeBarcode,
                        out itemWithError);

                    api.FillWithAdditionalInfoByAdvAPI(_log, _time, new List<ItemDTO>()
                    {
                        new ItemDTO() {ASIN = dbItem.ASIN}
                    }, out itemWithError);

                    if (String.IsNullOrEmpty(dbItem.Barcode))
                    {
                        dbItem.Barcode = items.FirstOrDefault()?.Barcode;
                        db.Commit();
                    }

                }
            }
        }

        public void ProcessRequestedReport(AmazonReportType reportType,
            AmazonApi api,
            IList<string> asinList)
        {
            var processed = false;
            AmazonReportService reportService = null;
            var reportSettings = new AmazonReportFactory(_time).GetReportService(reportType, api.MarketplaceId);
            var syncInfo = new DbSyncInformer(_dbFactory, _log, _time, SyncType.Listings,
                api.MarketplaceId, MarketType.Amazon, null);

            var styleHistoryService = new StyleHistoryService(_log, _time, _dbFactory);
            var itemHistoryService = new ItemHistoryService(_log, _time, _dbFactory);
            var styleManager = new StyleManager(_log, _time, styleHistoryService);
            var notificationService = new NotificationService(_log, _time, _dbFactory);
            var actionService = new SystemActionService(_log, _time);

            using (var db = _dbFactory.GetRWDb())
            {
                while (!processed)
                {
                    if (reportService == null)
                    {
                        _log.Info("Create reportService");
                        reportService = new AmazonReportService(reportType,
                            _company.Id,
                            api,
                            _log,
                            _time,
                            _dbFactory,
                            syncInfo,
                            styleManager,
                            notificationService,
                            styleHistoryService,
                            itemHistoryService,
                            actionService,
                            reportSettings.GetParser(),
                            AppSettings.ReportDirectory);

                        var lastUnsavedReport = db.InventoryReports.GetLastUnsaved((int)reportService.ReportType, api.MarketplaceId);
                        if (lastUnsavedReport == null || (DateTime.UtcNow - lastUnsavedReport.RequestDate) > TimeSpan.FromHours(3))
                        {
                            _log.Info("Request report");
                            reportService.RequestReport();
                        }
                        else
                        {
                            syncInfo.OpenLastSync();
                        }
                    }
                    else
                    {
                        _log.Info("Save report");
                        if (reportService.SaveRequestedReport())
                        {
                            Console.WriteLine("Report Saved");
                            _log.Info("Process report");
                            reportService.ProcessReport(asinList);
                            reportService = null;
                            processed = true;
                            return;
                        }
                        else
                        {
                            var requestStatus = reportService.GetRequestStatus();
                            Console.WriteLine(requestStatus);
                            if (requestStatus.Status == "_CANCELLED_")
                            {
                                db.InventoryReports.MarkProcessedByRequestId(reportService.ReportRequestId,
                                    ReportProcessingResultType.Cancelled);
                                reportService = null;
                            }

                            _log.Info("Could not save report");
                        }
                    }
                    Thread.Sleep(60000);
                }
            }
        }
    }
}
