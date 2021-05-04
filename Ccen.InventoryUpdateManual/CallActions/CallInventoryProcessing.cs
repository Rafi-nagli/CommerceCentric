using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using Amazon.Api;
using Amazon.Api.Models;
using Amazon.Common.Helpers;
using Amazon.Common.Threads;
using Amazon.Core;
using Amazon.Core.Contracts;
using Amazon.Core.Contracts.Factories;
using Amazon.Core.Contracts.Parser;
using Amazon.Core.Entities.Enums;
using Amazon.Core.Entities.Inventory;
using Amazon.Core.Models;
using Amazon.Core.Models.Settings;
using Amazon.DAL;
using Amazon.DTO;
using Amazon.DTO.Users;
using Amazon.Model.Implementation;
using Amazon.Model.Implementation.Markets.Amazon.Feeds;
using Amazon.Model.Implementation.Markets.eBay;
using Amazon.ReportParser.Processing.Listings;
using Amazon.Web.Models;
using CsvHelper;
using eBay.Api;
using Magento.Api.Wrapper;
using Amazon.Core.Models.Calls;
using Amazon.Model.Implementation.Markets.Magento;
using Amazon.Model.SyncService.Threads.Simple.UpdateAmazonInfo;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using NPOI.HSSF.UserModel;
using CsvHelper.Configuration;
using Amazon.Core.Models.Items;
using Amazon.DTO.Orders;
using Amazon.Model.General;

namespace Amazon.InventoryUpdateManual.CallActions
{
    class CallInventoryProcessing
    {
        private CompanyDTO _company;
        private ILogService _log;
        private IDbFactory _dbFactory;
        private ITime _time;
        private IStyleManager _styleManager;
        private ICacheService _cacheService;
        private IEmailService _emailService;
        private IMarketCategoryService _amazonCategoryService;

        public CallInventoryProcessing(CompanyDTO company,
            IStyleManager styleManager,
            IEmailService emailService,
            ICacheService cacheService,
            IMarketCategoryService categoryService,
            IDbFactory dbFactory,
            ILogService log,
            ITime time)
        {
            _company = company;
            _emailService = emailService;
            _styleManager = styleManager;
            _cacheService = cacheService;
            _amazonCategoryService = categoryService;
            _log = log;
            _dbFactory = dbFactory;
            _time = time;
        }

        private class BoxPriceInfo
        {
            public long StyleId { get; set; }
            public long BoxId { get; set; }
            public bool IsSealed { get; set; }
            public int Quantity { get; set; }
            public decimal Price { get; set; }
            public bool IsPriceChanged { get; set; }
        }

        public void UpdateBoxPrices()
        {
            using (var db = _dbFactory.GetRWDb())
            {
                IList<BoxPriceInfo> boxInfoes = new List<BoxPriceInfo>();
                //OpenBoxes
                var openBoxes = db.OpenBoxes.GetAll().ToList();
                var openBoxItems = db.OpenBoxItems.GetAll().ToList();

                //Sealed Boxes
                var sealedBoxes = db.SealedBoxes.GetAll().ToList();
                var sealedBoxItems = db.SealedBoxItems.GetAll().ToList();

                foreach (var openBox in openBoxes)
                {
                    boxInfoes.Add(new BoxPriceInfo()
                    {
                        StyleId = openBox.StyleId,
                        BoxId = openBox.Id,
                        IsSealed = false,
                        Quantity = openBox.BoxQuantity * (openBoxItems.Where(ob => ob.BoxId == openBox.Id).Sum(ob => ob.Quantity)),
                        Price = openBox.Price
                    });
                }

                foreach (var sealedBox in sealedBoxes)
                {
                    boxInfoes.Add(new BoxPriceInfo()
                    {
                        StyleId = sealedBox.StyleId,
                        BoxId = sealedBox.Id,
                        IsSealed = false,
                        Quantity = sealedBox.BoxQuantity * (sealedBoxItems.Where(ob => ob.BoxId == sealedBox.Id).Sum(ob => ob.BreakDown)),
                        Price = sealedBox.PajamaPrice
                    });
                }

                var styleIds = boxInfoes.Select(i => i.StyleId).Distinct().ToList();
                foreach (var styleId in styleIds)
                {
                    var styleBoxes = boxInfoes.Where(b => b.StyleId == styleId).ToList();
                    var nullBoxes = styleBoxes.Where(b => b.Price == 0).ToList();
                    var notNullBoxes = styleBoxes.Where(b => b.Price > 0).ToList();

                    decimal priceAvg = 0M;
                    int itemCount = 0;
                    if (notNullBoxes.Count() > 0 && nullBoxes.Count() > 0)
                    {
                        foreach (var notNullBox in notNullBoxes)
                        {
                            priceAvg += notNullBox.Quantity * notNullBox.Price;
                            itemCount += notNullBox.Quantity;
                        }
                        var newPrice = PriceHelper.RoundRoundToTwoPrecision(priceAvg / itemCount);
                        _log.Info("Set price for open styleId=" + styleId + ", price=" + newPrice);
                        nullBoxes.ForEach(b =>
                        {
                            b.Price = newPrice;
                            b.IsPriceChanged = true;
                        });
                    }
                }

                foreach (var boxInfo in boxInfoes.Where(b => b.IsPriceChanged))
                {
                    if (boxInfo.IsSealed)
                    {
                        _log.Info("Set price for sealedbox styleId=" + boxInfo.StyleId + ", price=" + boxInfo.Price);
                        var sealedBox = sealedBoxes.FirstOrDefault(b => b.Id == boxInfo.BoxId);
                        sealedBox.PajamaPrice = boxInfo.Price;
                    }
                    else
                    {
                        _log.Info("Set price for openbox styleId=" + boxInfo.StyleId + ", price=" + boxInfo.Price);
                        var openBox = openBoxes.FirstOrDefault(b => b.Id == boxInfo.BoxId);
                        openBox.Price = boxInfo.Price;
                    }
                }

                db.Commit();


                //var openStyleIds = openBoxes.Select(b => b.StyleId).Distinct().ToList();

                //foreach (var styleId in openStyleIds)
                //{
                //    var styleBoxes = openBoxes.Where(b => b.StyleId == styleId).ToList();
                //    var nullBoxes = styleBoxes.Where(b => b.Price == 0).ToList();
                //    var notNullBoxes = styleBoxes.Where(b => b.Price > 0).ToList();

                //    decimal priceAvg = 0M;
                //    int itemCount = 0;
                //    if (notNullBoxes.Count() > 0 && nullBoxes.Count() > 0)
                //    {
                //        foreach (var notNullBox in notNullBoxes)
                //        {
                //            var boxItems = openBoxItems.Where(i => i.BoxId == notNullBox.Id).ToList();
                //            priceAvg += notNullBox.BoxQuantity * boxItems.Sum(i => i.Quantity) * notNullBox.Price;
                //            itemCount += notNullBox.BoxQuantity * boxItems.Sum(i => i.Quantity);
                //        }
                //        var newPrice = PriceHelper.RoundRoundToTwoPrecision(priceAvg / itemCount);
                //        _log.Info("Set price for open styleId=" + styleId + ", price=" + newPrice);
                //        nullBoxes.ForEach(b => b.Price = newPrice);
                //    }
                //}

                ////Sealed Boxes
                //var sealedBoxes = db.SealedBoxes.GetAll().ToList();
                //var sealedBoxItems = db.SealedBoxItems.GetAll().ToList();

                //var sealedStyleIds = sealedBoxes.Select(b => b.StyleId).Distinct().ToList();

                //foreach (var styleId in sealedStyleIds)
                //{
                //    var styleBoxes = sealedBoxes.Where(b => b.StyleId == styleId).ToList();
                //    var nullBoxes = styleBoxes.Where(b => b.PajamaPrice == 0).ToList();
                //    var notNullBoxes = styleBoxes.Where(b => b.PajamaPrice > 0).ToList();

                //    decimal priceAvg = 0M;
                //    int itemCount = 0;
                //    if (notNullBoxes.Count() > 0 && nullBoxes.Count() > 0)
                //    {
                //        foreach (var notNullBox in notNullBoxes)
                //        {
                //            var boxItems = sealedBoxItems.Where(i => i.BoxId == notNullBox.Id).ToList();
                //            priceAvg += notNullBox.BoxQuantity * boxItems.Sum(i => i.BreakDown) * notNullBox.PajamaPrice;
                //            itemCount += notNullBox.BoxQuantity * boxItems.Sum(i => i.BreakDown);
                //        }
                //        var newPrice = PriceHelper.RoundRoundToTwoPrecision(priceAvg / itemCount);
                //        _log.Info("Set price for sealed styleId=" + styleId + ", price=" + newPrice);
                //        nullBoxes.ForEach(b => b.PajamaPrice = newPrice);
                //    }
                //}
            }
        }

        public void CheckSizeMapping()
        {
            var sizeMappingService = new SizeMappingService(_log, _time, _dbFactory);
            sizeMappingService.CheckItemsSizeMappingIssue();
        }

        public void CallGetProductByBarcode(AmazonApi api)
        {
            var product = api.GetProductForBarcode(new List<string>() {"024054530351"});
            _log.Info(product.ToString());
        }

        public void CallKioskBarcodeWoStyleNotification()
        {
            var kioskBarcodeService = new KioskBarcodeService(_dbFactory,
                _emailService,
                _time,
                _log);
            kioskBarcodeService.CheckOrders();
        }
        
        public void CallProcessTestListings(AmazonApi api)
        {
            var lineProcessing = new ListingLineProcessing(new ParseContext()
                {
                    Log = _log,
                    StyleManager = _styleManager,
                    CompanyId = _company.Id,
                    Time = _time,
                }, 
                _time,
                true);
            using (var db = _dbFactory.GetRWDb())
            {
                var items = new List<ItemDTO>()
                {
                    new ItemDTO()
                    {
                        SKU = "1557-lips-16",
                        ASIN = "B01LMK1AIC",
                        Size = "16",
                        Color = "Lips",
                        ParentASIN = "B00KDESDKW",
                        IsExistOnAmazon = true,
                    }
                };
                lineProcessing.ProcessExistingItems(db, api, items);
            }
        }

        public void CallProcessCategoryReport(IMarketApi api, string path)
        {
            var parser = new CategoryListingParser();
            var context = new ParseContext()
            {
                Log = _log,
                DbFactory = _dbFactory
            };
            parser.Init(context);

            var file = path;

            _log.Info(string.Format("Open file:{0}", file));
            parser.Open(file);

            _log.Info(string.Format("Begin parse items"));
            var items = parser.GetReportItems(api.Market, api.MarketplaceId);
            _log.Info(string.Format("End parse items, count=" + items.Count));

            _log.Info("Process file");
            parser.Process(api, _time, new AmazonReportInfo(), items);
        }

        public void CallUpdateParentItem(IMarketApi api, string parentAsin)
        {
            var list = new List<string>() { parentAsin };
            var parentASINsWithError = new List<string>();
            var parents = api.GetItems(_log, _time, MarketItemFilters.Build(list), ItemFillMode.Defualt, out parentASINsWithError);
        }
        
        public void CallSendPriceToAmazon(AmazonApi amazonApi)
        {
            
        }

        public void CallUpdateAllParentItemsStyleIdEBay(IMarketApi api)
        {
            using (var db = new UnitOfWork(_log))
            {
                var parentItems = db.ParentItems.GetAllAsDto(MarketType.eBay, api.MarketplaceId).ToList();
                foreach (var parentItem in parentItems)
                {
                    var parentASINsWithError = new List<string>();
                    var piList = api.GetItems(_log, _time, MarketItemFilters.Build(new List<string> { parentItem.ASIN }), ItemFillMode.Defualt, out parentASINsWithError);
                    foreach (var pi in piList)
                    {
                        foreach (var item in pi.Variations)
                        {
                            var itemDb = db.Items.GetByASIN(item.ASIN, api.Market, api.MarketplaceId);
                            if (itemDb != null && !itemDb.StyleItemId.HasValue)
                            {
                                var styleItem = _styleManager.FindOrCreateStyleAndStyleItemForItem(db,
                                    ItemType.Pajama,
                                    item,
                                    false,
                                    false);

                                itemDb.StyleString = item.StyleString;

                                if (styleItem != null)
                                {
                                    if (styleItem.StyleItemId > 0)
                                    {
                                        itemDb.StyleId = styleItem.StyleId;
                                        itemDb.StyleItemId = styleItem.StyleItemId;
                                    }
                                    else
                                    {
                                        if (!itemDb.StyleId.HasValue)
                                            itemDb.StyleId = styleItem.StyleId;
                                    }
                                }

                                db.Commit();
                            }
                        }
                    }
                }
            }
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
                
                var asinsWithError = new List<string>() {};

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

        public void CallUpdateItem(AmazonApi api, string asin)
        {
            var asinsWithError = new List<string>();
            var asinList = new List<string> {asin};
            var result = api.GetProductForASIN(asinList);

            Console.WriteLine(result);

            //var item = api.GetItems(_log, _time, new List<string>() { asin }, ItemFillMode.Defualt, out asinsWithError);
            var itemWithError = new List<ItemDTO>();
            //api.FillWithAdditionalInfo(_log, _time, new List<ItemDTO>()
            //{
            //    new ItemDTO()
            //    {
            //        ASIN = asin
            //    }
            //},
            //IdType.SKU,
            //ItemFillMode.IncludeBarcode,
            //out itemWithError);

            api.FillWithAdditionalInfoByAdvAPI(_log, _time, new List<ItemDTO>()
            {
                new ItemDTO() { ASIN = asin }
            },
            out itemWithError);
            api.RetrieveOffers(_log, new List<string> { asin });
        }


        public void CallGetRank(AmazonApi api, string asin)
        {
            var asinsWithError = new List<string>();
            //var rank1 = api.RetrieveRanks(_log, new List<string>() {asin});
            var rank2 = api.GetItems(_log, _time, MarketItemFilters.Build(new List<string>() { asin }), ItemFillMode.Defualt, out asinsWithError);
            //_log.Info(rank1.Items.Count().ToString());
            _log.Info(rank2.Count().ToString());
        }

        public void CallGetItemsByASIN(AmazonApi api, string asin)
        {
            var itemsWithError = new List<ItemDTO>();
            //api.GetProductForBarcode(new List<string>() { "000716612344" });
            //api.GetProductForASIN(new List<string>() { asin });
            //api.GetProductForBarcode(new List<string>() { "767862123444" });
            //api.FillWithAdditionalInfo(_log,
            //    _time,
            //    new List<ItemDTO>()
            //    {
            //        new ItemDTO() { SKU = "1010CFF-CLEO" }
            //    },
            //    IdType.SKU,
            //    ItemFillMode.Defualt,
            //    out itemsWithError);
            api.GetMyPriceByASIN(new List<string>() { asin });
            //api.GetProductForASIN(new List<string>() { })
            //api.FillWithAdditionalInfo(_log,
            //    _time,
            //    new List<ItemDTO>()
            //    {
            //        new ItemDTO() { ASIN = asin }
            //    },
            //    IdType.ASIN,
            //    ItemFillMode.Defualt,
            //    out itemsWithError);
        }

        public void CallGetItemsBySKU(AmazonApi api, string sku)
        {
            var itemsWithError = new List<ItemDTO>();            
            //api.GetProductForBarcode(new List<string>() { "000716612344" });

            api.FillWithAdditionalInfo(_log, 
                _time, 
                new List<ItemDTO>()
                {
                    new ItemDTO() { SKU = sku }
                },
                IdType.SKU,
                ItemFillMode.Defualt, 
                out itemsWithError);
        }

        public void CallGetItemsBySKUAndCheckStyle(AmazonApi api, string sku)
        {
            var itemsWithError = new List<ItemDTO>();
            var item = new ItemDTO() {SKU = sku};
            api.FillWithAdditionalInfo(_log,
                _time,
                new List<ItemDTO>()
                {
                    item
                },
                IdType.SKU,
                ItemFillMode.Defualt, 
                out itemsWithError);

            var styleHistoryService = new StyleHistoryService(_log, _time, _dbFactory);
            var styleManager = new StyleManager(_log, _time, styleHistoryService);
            using (var db = new UnitOfWork(_log))
            {
                item.StyleString = SkuHelper.RetrieveStyleIdFromSKU(db, item.SKU, item.Name);

                var styleItem = styleManager.FindOrCreateStyleAndStyleItemForItem(db, 
                    ItemType.Pajama,
                    item, 
                    false,
                    false);


            }
        }

        public void CallGetParentASIN(AmazonApi api, List<string> asinList)
        {
            //var rank1 = api.RetrieveRanks(_log, new List<string>() {asin});
            var items = api.GetMyPriceByASIN(asinList);
            //_log.Info(rank1.Items.Count().ToString());
            _log.Info(items.Count().ToString());
        }


        public void CallUpdatePriceData(AmazonApi api, IList<string> skuList)
        {
            var updated = false;
            while (!updated)
            {
                var updater = new ItemPriceUpdater(_log, _time, _dbFactory);
                var unprocessedFeed = updater.GetUnprocessedFeedId(api.MarketplaceId);
                if (unprocessedFeed != null)
                {
                    _log.Info("Update unprocessed feed");
                    updater.UpdateSubmittedFeed(api,
                        unprocessedFeed,
                        AppSettings.FulfillmentResponseDirectory);
                    updated = true;
                }
                else
                {
                    _log.Info("Submit new feed");
                    updater.SubmitFeed(api, _company.Id, skuList, AppSettings.FulfillmentRequestDirectory);
                }
                Thread.Sleep(60000);
            }
        }

        public void CallUpdateListingData(AmazonApi api, IList<string> asinList)
        {
            var updated = false;
            while (true)
            {
                var dataUpdater = new ItemDataUpdater(_log, _time, _dbFactory, _amazonCategoryService);
                var unprocessedFeed = dataUpdater.GetUnprocessedFeedId(api.MarketplaceId);
                if (unprocessedFeed != null)
                {
                    _log.Info("Update unprocessed data feed");
                    dataUpdater.UpdateSubmittedFeed(api,
                        unprocessedFeed,
                        AppSettings.FulfillmentResponseDirectory);
                    Thread.Sleep(5000);
                }
                else
                {
                    _log.Info("Submit new data feed");
                    dataUpdater.SubmitFeed(api, _company.Id, asinList, AppSettings.FulfillmentRequestDirectory);
                    Thread.Sleep(60000);
                }
            }
        }

        public void CallUpdatePriceRuleData(AmazonApi api, IList<string> asinList)
        {
            var updated = false;
            while (true)
            {
                var dataUpdater = new ItemPriceRuleUpdater(_log, _time, _dbFactory);
                var unprocessedFeed = dataUpdater.GetUnprocessedFeedId(api.MarketplaceId);
                if (unprocessedFeed != null)
                {
                    _log.Info("Update unprocessed image feed");
                    dataUpdater.UpdateSubmittedFeed(api,
                        unprocessedFeed,
                        AppSettings.FulfillmentResponseDirectory);
                    Thread.Sleep(5000);
                }
                else
                {
                    _log.Info("Submit new image feed");
                    dataUpdater.SubmitFeed(api, _company.Id, asinList, AppSettings.FulfillmentRequestDirectory);
                    Thread.Sleep(60000);
                }
            }
        }

        public void CallUpdateListingImageData(AmazonApi api, IList<string> asinList)
        {
            var updated = false;
            while (true)
            {
                var dataUpdater = new ItemImageUpdater(_log, _time, _dbFactory);
                var unprocessedFeed = dataUpdater.GetUnprocessedFeedId(api.MarketplaceId);
                if (unprocessedFeed != null)
                {
                    _log.Info("Update unprocessed image feed");
                    dataUpdater.UpdateSubmittedFeed(api,
                        unprocessedFeed,
                        AppSettings.FulfillmentResponseDirectory);
                    Thread.Sleep(5000);
                }
                else
                {
                    _log.Info("Submit new image feed");
                    dataUpdater.SubmitFeed(api, _company.Id, asinList, AppSettings.FulfillmentRequestDirectory);
                    Thread.Sleep(60000);
                }
            }
        }

        public void CallUpdateQuantityData(AmazonApi api, IList<string> asinList)
        {
            var updated = false;
            while (true)
            {
                var quantityUpdater = new ItemQuantityUpdater(_log, _time, _dbFactory, AppSettings.AmazonFulfillmentLatencyDays);
                var unprocessedFeed = quantityUpdater.GetUnprocessedFeedId(api.MarketplaceId);
                if (unprocessedFeed != null)
                {
                    _log.Info("Update unprocessed quantity feed");
                    quantityUpdater.UpdateSubmittedFeed(api,
                        unprocessedFeed,
                        AppSettings.FulfillmentResponseDirectory);
                    Thread.Sleep(5000);
                }
                else
                {
                    _log.Info("Submit new quantity feed");
                    quantityUpdater.SubmitFeed(api, _company.Id, asinList, AppSettings.FulfillmentRequestDirectory);
                    Thread.Sleep(60000);
                }
            }
        }

        public void CallStyleManagerCreateItem(string styleString, 
            string amazonSize,
            string department)
        {
            using (var db = _dbFactory.GetRWDb())
            {
                var style = db.Styles.GetAllAsDto().FirstOrDefault(s => s.StyleID == styleString);

                _styleManager.FindOrCreateStyleAndStyleItemForItem(db,
                    style.ItemTypeId ?? ItemType.Pajama,
                    new ItemDTO()
                    {
                        StyleId = style.Id,
                        StyleString = style.StyleID,
                        Size = amazonSize,
                        Department = department
                    },
                    true,
                    false);
            }
        }

        public void ImportDescription(string filename)
        {
            using (var db = _dbFactory.GetRWDb())
            {
                using (var stream = new FileStream(filename, FileMode.Open, FileAccess.ReadWrite))
                {
                    IWorkbook workbook = null;
                    if (filename.EndsWith(".xlsx"))
                        workbook = new XSSFWorkbook(stream);
                    else
                        workbook = new HSSFWorkbook(stream);

                    var sheet = workbook.GetSheetAt(0);

                    for (var i = 1; i <= sheet.LastRowNum; i++)
                    {
                        var row = sheet.GetRow(i);
                        var styleId = GetStrValue(row.GetCell(0));
                        var title = GetStrValue(row.GetCell(1));
                        var desc = GetStrValue(row.GetCell(2));
                        var bullet1 = GetStrValue(row.GetCell(3));
                        var bullet2 = GetStrValue(row.GetCell(4));
                        var bullet3 = GetStrValue(row.GetCell(5));
                        var bullet4 = GetStrValue(row.GetCell(6));
                        var bullet5 = GetStrValue(row.GetCell(7));
                        var searchTerms = GetStrValue(row.GetCell(8));
                        var fillingStatus = GetStrValue(row.GetCell(9)).Replace(" ", "");
                        
                        if (String.IsNullOrEmpty(styleId))
                            continue;

                        _log.Info("StyleId: " + styleId);

                        var dbStyle = db.Styles.GetAll().FirstOrDefault(st => st.StyleID == styleId && !st.Deleted);
                        dbStyle.Name = title;
                        dbStyle.Description = desc;
                        dbStyle.BulletPoint1 = bullet1;
                        dbStyle.BulletPoint2 = bullet2;
                        dbStyle.BulletPoint3 = bullet3;
                        dbStyle.BulletPoint4 = bullet4;
                        dbStyle.BulletPoint5 = bullet5;
                        dbStyle.SearchTerms = searchTerms;
                        dbStyle.FillingStatus = (int)Enum.Parse(typeof(FillingStyleStatuses), fillingStatus);

                        db.Commit();
                    }
                }                
            }
        }

        private string GetStrValue(ICell cell)
        {
            if (cell == null)
                return "";
            return cell.ToString();
        }

        public void ImportProperties(string filename)
        {
            using (var db = _dbFactory.GetRWDb())
            {
                using (var stream = new FileStream(filename, FileMode.Open, FileAccess.ReadWrite))
                {
                    IWorkbook workbook = null;
                    if (filename.EndsWith(".xlsx"))
                        workbook = new XSSFWorkbook(stream);
                    else
                        workbook = new HSSFWorkbook(stream);

                    var sheet = workbook.GetSheetAt(0);

                    for (var i = 1; i <= sheet.LastRowNum; i++)
                    {
                        var row = sheet.GetRow(i);
                        var styleId = row.GetCell(0).ToString();
                        var manufacturer = row.GetCell(1).ToString();
                        var msrp = row.GetCell(2).ToString();
                        var fillingStatus = row.GetCell(3).ToString().Replace(" ", "");
                        var pictureStatus = row.GetCell(4).ToString().Replace(" ", "");
                        var itemStyle = row.GetCell(5).ToString();
                        var mainLicense = row.GetCell(6).ToString();
                        var subLicense = row.GetCell(7).ToString();
                        var shippingSize = row.GetCell(8).ToString();

                        var dbStyle = db.Styles.GetAll().FirstOrDefault(st => st.StyleID == styleId && !st.Deleted);
                        dbStyle.Manufacturer = manufacturer;
                        dbStyle.MSRP = StringHelper.TryGetDecimal(msrp);
                        dbStyle.PictureStatus = (int)Enum.Parse(typeof(StylePictureStatuses), pictureStatus);
                        dbStyle.FillingStatus = (int)Enum.Parse(typeof(FillingStyleStatuses), fillingStatus);
                        
                        db.Commit();

                        var allTextFeatures = db.StyleFeatureTextValues.GetAllWithFeature().Where(f => f.StyleId == dbStyle.Id).ToList();
                        var allValueFeatures = db.StyleFeatureValues.GetAllWithFeature().Where(f => f.StyleId == dbStyle.Id).ToList();

                        UpdateTextFeature(db, dbStyle.Id, allTextFeatures, StyleFeatureHelper.BRAND, manufacturer, _time.GetAppNowTime());
                        UpdateValueFeature(db, dbStyle.Id, allTextFeatures, StyleFeatureHelper.ITEMSTYLE, itemStyle, _time.GetAppNowTime());
                        UpdateValueFeature(db, dbStyle.Id, allTextFeatures, StyleFeatureHelper.MAIN_LICENSE, mainLicense, _time.GetAppNowTime());
                        UpdateValueFeature(db, dbStyle.Id, allTextFeatures, StyleFeatureHelper.SUB_LICENSE1, subLicense, _time.GetAppNowTime());
                        UpdateValueFeature(db, dbStyle.Id, allTextFeatures, StyleFeatureHelper.SHIPPING_SIZE, shippingSize, _time.GetAppNowTime());
                    }
                }
            }
        }

        private void UpdateTextFeature(IUnitOfWork db, 
            long styleId, 
            IList<StyleFeatureValueDTO> featureValues, 
            int featureId, 
            string featureValue,
            DateTime when)
        {
            var existFeature = featureValues.FirstOrDefault(f => f.FeatureId == featureId);
            if (existFeature == null)
            {
                db.StyleFeatureTextValues.Add(new Core.Entities.Features.StyleFeatureTextValue()
                {
                    StyleId = styleId,
                    FeatureId = featureId,
                    Value = featureValue,
                    CreateDate = when
                });
            }
            else
            {
                var dbExistFeature = db.StyleFeatureTextValues.GetAll().FirstOrDefault(f => f.Id == existFeature.Id);
                dbExistFeature.Value = featureValue;
            }

            db.Commit();
        }

        private void UpdateValueFeature(IUnitOfWork db,
            long styleId,
            IList<StyleFeatureValueDTO> featureValues,
            int featureId,
            string featureValue,
            DateTime when)
        {
            var dbFeatureValue = db.FeatureValues.GetAll().FirstOrDefault(f => f.FeatureId == featureId
                    && f.Value == featureValue);

            var existFeature = featureValues.FirstOrDefault(f => f.FeatureId == featureId);
            if (existFeature == null)
            {
                db.StyleFeatureValues.Add(new Core.Entities.Features.StyleFeatureValue()
                {
                    StyleId = styleId,
                    FeatureId = featureId,
                    FeatureValueId = dbFeatureValue.Id,
                    CreateDate = when
                });
            }
            else
            {
                var dbExistFeature = db.StyleFeatureValues.GetAll().FirstOrDefault(f => f.Id == existFeature.Id);
                dbExistFeature.FeatureId = dbFeatureValue.Id;
            }

            db.Commit();
        }

        public void CallUpdateInventoryData(AmazonApi api, UserDTO user)
        {
            var threads = new List<IThread>();
            threads.Add(new UpdateListingsQtyOnAmazonThread("UpdateListingsQtyOnAmazonCOM", api, user.Id, null, TimeSpan.FromMinutes(1), TimeSpan.FromMinutes(2)));
            threads.Add(new UpdateListingsPriceOnAmazonThread("UpdateListingsPriceOnAmazonCOM", api, user.Id, null, TimeSpan.FromMinutes(1), TimeSpan.FromMinutes(2)));
            foreach (var thread in threads)
            {
                thread.Start();
            }
        }

        public void UpdateFileWithShippingPrice(string filepath)
        {
            var results = new List<ItemDTO>();

            using (var stream = new FileStream(filepath, FileMode.Open, FileAccess.ReadWrite))
            {
                IWorkbook workbook = null;
                if (filepath.EndsWith(".xlsx"))
                    workbook = new XSSFWorkbook(stream);
                else
                    workbook = new HSSFWorkbook(stream);

                var sheet = workbook.GetSheetAt(0);

                var headerRow = sheet.GetRow(0);

                for (var i = 1; i <= sheet.LastRowNum; i++)
                {
                    var row = sheet.GetRow(i);
                    if (row != null && row.GetCell(2) != null)
                    {
                        try
                        {
                            results.Add(new ItemDTO()
                            {
                                Weight = (double?)StringHelper.TryGetDecimal(row.GetCell(2).ToString()),
                            });
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine(ex.Message);
                        }
                    }
                }
            }
            _log.Info("Parsed count: " + results.Count);

            var weightList = results.Where(r => r.Weight.HasValue).Select(r => r.Weight.Value).Distinct().ToList();

            var newRateTable = new List<ItemDTO>();

            var serviceFactory = new ServiceFactory();
            var weightService = new WeightService();

            var rateProviders = serviceFactory.GetShipmentProviders(_log,
                _time,
                _dbFactory,
                weightService,
                _company.ShipmentProviderInfoList,
                null,
                null,
                null,
                null);

            var stampsRateProvider = rateProviders.FirstOrDefault(r => r.Type == ShipmentProviderType.Stamps);

            var companyAddress = new CompanyAddressService(_company);

            var address = RateHelper.GetSampleUSAddress();

            foreach (var weight in weightList)
            {
                RateDTO rate = null;
                if (weight >= 16)
                {
                    rate = new RateDTO()
                    {
                        Amount = 6.99M
                    };
                }
                else
                {
                    rate = RateHelper.GetRougeChipestRate(_log,
                        stampsRateProvider,
                        companyAddress.GetReturnAddress(MarketIdentifier.Empty()),
                        address,
                        weight,
                        DateTime.Today,
                        ShippingTypeCode.Standard,
                        PackageTypeCode.Regular);

                    if (rate.ServiceIdentifier != "STAMPS_USPS_1_5")
                        rate.Amount = null;
                }

                newRateTable.Add(new ItemDTO()
                {
                    Weight = weight,
                    Cost = rate.Amount ?? 0
                });
            }
            
            using (var stream = new FileStream(filepath, FileMode.Open, FileAccess.ReadWrite))
            {
                IWorkbook workbook = null;
                if (filepath.EndsWith(".xlsx"))
                    workbook = new XSSFWorkbook(stream);
                else
                    workbook = new HSSFWorkbook(stream);

                var sheet = workbook.GetSheetAt(0);

                var headerRow = sheet.GetRow(0);

                for (var i = 1; i <= sheet.LastRowNum; i++)
                {
                    var row = sheet.GetRow(i);
                    if (row != null && row.GetCell(2) != null)
                    {
                        var weight = (double?)StringHelper.TryGetDecimal(row.GetCell(2).ToString());
                        var cost = newRateTable.FirstOrDefault(r => r.Weight == weight)?.Cost;
                        if (!cost.HasValue || cost == 0)
                            cost = 6.99M;

                        var priceCell = row.CreateCell(4);
                        priceCell.SetCellValue((double)cost.Value);
                    }
                }

                var filename = Path.GetFileNameWithoutExtension(filepath);
                var ext = Path.GetExtension(filepath);
                var path = Path.GetDirectoryName(filepath);
                var newFileName = Path.Combine(path, filename + "_prices" + ext);
                using (FileStream writeStream = new FileStream(newFileName, FileMode.Create, FileAccess.Write))
                {
                    workbook.Write(writeStream);
                }
            }
        }

        public void UpdateFileWithStyleInfo(string filename)
        {
            using (var db = _dbFactory.GetRWDb())
            {
                var fileName = Path.GetFileNameWithoutExtension(filename);
                var filePath = Path.GetDirectoryName(filename);
                using (TextWriter writer = new StreamWriter(Path.Combine(filePath, fileName + "_updated.csv")))
                {
                    using (var csvWriter = new CsvWriter(writer, new CsvConfiguration
                    {
                        HasHeaderRecord = true,
                        Delimiter = ",",
                        TrimFields = true,
                    }))
                    {
                        csvWriter.Configuration.Encoding = Encoding.UTF8;

                        using (StreamReader streamReader = new StreamReader(filename))
                        {
                            using (CsvReader reader = new CsvReader(streamReader, new CsvConfiguration
                            {
                                HasHeaderRecord = true,
                                Delimiter = ",",
                                TrimFields = true,
                            }))
                            {
                                reader.Read();
                                foreach (var field in reader.FieldHeaders)
                                    csvWriter.WriteField(field);
                                csvWriter.NextRecord();

                                while (reader.Read())
                                {
                                    var record = reader.CurrentRecord;
                                    var asin = reader["ASIN/CASE"];
                                    if (!String.IsNullOrEmpty(asin))
                                    {
                                        var item = db.Items.GetAll().FirstOrDefault(i => i.ASIN == asin
                                            && i.Market == (int)MarketType.Amazon
                                            && i.MarketplaceId == MarketplaceKeeper.AmazonComMarketplaceId);
                                        var style = db.Styles.GetAll().FirstOrDefault(st => st.Id == item.StyleId);
                                        var location = db.StyleLocations.GetAll().OrderByDescending(l => l.IsDefault)
                                            .FirstOrDefault(l => l.StyleId == style.Id);

                                        record[5] = style?.StyleID;
                                        record[6] = style?.OriginalStyleID;
                                        record[7] = location?.Isle + "/" + location?.Section + "/" + location?.Shelf;
                                    }
                                    foreach (var field in record)
                                        csvWriter.WriteField(field);
                                    csvWriter.NextRecord();
                                }
                            }
                        }
                    }
                }
            }
        }

        public void UpdateFileWithQty(string filepath)
        {
            var results = new List<ItemDTO>();

            using (var stream = new FileStream(filepath, FileMode.Open, FileAccess.ReadWrite))
            {
                IWorkbook workbook = null;
                if (filepath.EndsWith(".xlsx"))
                    workbook = new XSSFWorkbook(stream);
                else
                    workbook = new HSSFWorkbook(stream);

                var sheet = workbook.GetSheetAt(0);

                var headerRow = sheet.GetRow(0);

                for (var i = 1; i <= sheet.LastRowNum; i++)
                {
                    var row = sheet.GetRow(i);
                    if (row != null && row.GetCell(22) != null)
                    {
                        try
                        {
                            results.Add(new ItemDTO()
                            {
                                Barcode = StringHelper.TrimWhitespace(row.GetCell(22).ToString()),
                            });
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine(ex.Message);
                        }
                    }
                }
            }
            _log.Info("Parsed count: " + results.Count);

            var barcodeList = results.Select(i => i.Barcode).ToList();
            IList<ItemDTO> styleInfoes;
            IList<ItemDTO> listingInfoes;
            using (var db = _dbFactory.GetRWDb())
            {
                styleInfoes = (from si in db.StyleItemCaches.GetAll()
                              join sib in db.StyleItemBarcodes.GetAll() on si.Id equals sib.StyleItemId
                              select new ItemDTO
                              {
                                  Barcode = sib.Barcode,
                                  RemainingQuantity = si.RemainingQuantity
                              }).ToList();

                listingInfoes = (from i in db.Items.GetAll()
                                 join sic in db.StyleItemCaches.GetAll() on i.StyleItemId equals sic.Id
                                 select new ItemDTO()
                                 {
                                     Barcode = i.Barcode,
                                     RemainingQuantity = sic.RemainingQuantity
                                 }).ToList();
            }


            using (var stream = new FileStream(filepath, FileMode.Open, FileAccess.ReadWrite))
            {
                IWorkbook workbook = null;
                if (filepath.EndsWith(".xlsx"))
                    workbook = new XSSFWorkbook(stream);
                else
                    workbook = new HSSFWorkbook(stream);

                var sheet = workbook.GetSheetAt(0);

                var headerRow = sheet.GetRow(0);

                for (var i = 1; i <= sheet.LastRowNum; i++)
                {
                    var row = sheet.GetRow(i);
                    if (row != null && row.GetCell(22) != null)
                    {
                        var barcode = StringHelper.TrimWhitespace(row.GetCell(22).ToString());
                        var styleInfo = styleInfoes.FirstOrDefault(st => st.Barcode == barcode);
                        var listingInfo = listingInfoes.FirstOrDefault(l => l.Barcode == barcode);
                        if (styleInfo != null)
                        {
                            var qtyCell = row.CreateCell(38);
                            qtyCell.SetCellValue(styleInfo.RemainingQuantity ?? 0);
                        }
                        if (styleInfo == null && listingInfo != null)
                        {
                            var qtyCell = row.CreateCell(38);
                            qtyCell.SetCellValue(listingInfo.RemainingQuantity ?? 0);
                        }
                    }
                }

                var filename = Path.GetFileNameWithoutExtension(filepath);
                var ext = Path.GetExtension(filepath);
                var path = Path.GetDirectoryName(filepath);
                var newFileName = Path.Combine(path, filename + "_changes" + ext);
                using (FileStream writeStream = new FileStream(newFileName, FileMode.Create, FileAccess.Write))
                {
                    workbook.Write(writeStream);
                }
            }
        }
    }
}
