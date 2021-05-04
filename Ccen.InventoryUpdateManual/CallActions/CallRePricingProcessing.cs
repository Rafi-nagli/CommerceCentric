using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using Amazon.Api;
using Amazon.Api.Models;
using Amazon.Common.ExcelExport;
using Amazon.Common.Helpers;
using Amazon.Core.Contracts;
using Amazon.Core.Contracts.Factories;
using Amazon.Core.Entities.Enums;
using Amazon.Core.Entities.Inventory;
using Amazon.Core.Models;
using Amazon.Core.Models.Settings;
using Amazon.Core.Models.Stamps;
using Amazon.DTO;
using Amazon.DTO.Orders;
using Amazon.DTO.Users;
using Amazon.Model.Implementation;
using Amazon.Web.Models;
using NPOI.HSSF.UserModel;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;

namespace Amazon.InventoryUpdateManual.CallActions
{
    public class CallRePricingProcessing
    {
        private CompanyDTO _company;
        private ILogService _log;
        private IDbFactory _dbFactory;
        private ITime _time;
        private IStyleManager _styleManager;
        private IPriceService _priceService;
        private ISettingsService _settingService;
        private ISystemActionService _actionService;
        private IItemHistoryService _itemChangeHistory;

        public CallRePricingProcessing(CompanyDTO company,
            IStyleManager styleManager,
            IPriceService priceService,
            ISystemActionService actionService,
            ISettingsService settingService,
            IItemHistoryService itemChangeHistory,
            IDbFactory dbFactory,
            ILogService log,
            ITime time)
        {
            _company = company;
            _styleManager = styleManager;
            _priceService = priceService;
            _log = log;
            _dbFactory = dbFactory;
            _time = time;
            _actionService = actionService;
            _settingService = settingService;
            _itemChangeHistory = itemChangeHistory;
        }

        public void FixupWalmartPrices()
        {
            var priceManager = new PriceManager(_log, _time, _dbFactory, _actionService, _settingService);
            using (var db = _dbFactory.GetRWDb())
            {
                priceManager.FixupWalmartPrices(db);
            }
        }

        public void SyncWmSalesWithAmzSales()
        {
            using (var db = _dbFactory.GetRWDb())
            {
                var styleItemSales = db.StyleItemSales.GetAll().Where(s => !s.IsDeleted).ToList();

                var allSaleToMarkets = db.StyleItemSaleToMarkets.GetAll().ToList();
                var allSaleToListings = db.StyleItemSaleToListings.GetAll().ToList();

                foreach (var styleItemSale in styleItemSales)
                {
                    var saleToMarkets = allSaleToMarkets.Where(s => s.SaleId == styleItemSale.Id).ToList();
                    var amzSaleToMarket = saleToMarkets.FirstOrDefault(s => s.MarketplaceId == MarketplaceKeeper.AmazonComMarketplaceId);
                    var wmSaleToMarket = saleToMarkets.FirstOrDefault(s => s.Market == (int)MarketType.Walmart);

                    if (amzSaleToMarket == null)
                        continue;

                    var styleItemWeight = db.StyleItems.GetAll().FirstOrDefault(s => s.Id == styleItemSale.StyleItemId)?.Weight;

                    if (amzSaleToMarket != null && wmSaleToMarket == null)
                    {
                        _log.Info("Add Walmart SaleToMarket, SalePrice=" + amzSaleToMarket.SalePrice);
                        wmSaleToMarket = new StyleItemSaleToMarket()
                        {
                            SaleId = styleItemSale.Id,
                            SalePercent = amzSaleToMarket.SalePercent,
                            SalePrice = amzSaleToMarket.SalePrice - 0.02M,
                            SFPSalePrice = amzSaleToMarket.SFPSalePrice - 0.02M,
                            ApplyToNewListings = amzSaleToMarket.ApplyToNewListings,
                            Market = (int)MarketType.Walmart,
                            MarketplaceId = null,
                            CreateDate = _time.GetAppNowTime(),
                            CreatedBy = null
                        };
                        db.StyleItemSaleToMarkets.Add(wmSaleToMarket);
                        db.Commit();                        
                    }
                    else
                    {
                        var newSalePrice = amzSaleToMarket.SalePrice - 0.02M;
                        _log.Info("Change SalePrice, " + wmSaleToMarket.SalePrice + "=>" + newSalePrice);
                        wmSaleToMarket.SalePrice = newSalePrice;
                        wmSaleToMarket.SFPSalePrice = amzSaleToMarket.SFPSalePrice - 0.02M;
                    }

                    var dbWmListings = (from l in db.Listings.GetAll()
                                        join i in db.Items.GetAll() on l.ItemId equals i.Id
                                        where i.StyleItemId == styleItemSale.StyleItemId
                                         && i.Market == (int)MarketType.Walmart
                                         && !l.IsRemoved
                                        select l).ToList();

                    foreach (var dbWmListing in dbWmListings)
                    {
                        var dbSaleToListing = allSaleToListings.FirstOrDefault(s => s.ListingId == dbWmListing.Id && s.SaleId == styleItemSale.Id);
                        if (dbSaleToListing == null)
                        {
                            _log.Info("Add Walmart SaleToListing, SKU=" + dbWmListing.SKU + ", IsPrime=" + dbWmListing.IsPrime + ", IsFBA=" + dbWmListing.IsFBA);
                            dbSaleToListing = new StyleItemSaleToListing()
                            {
                                SaleToMarketId = wmSaleToMarket.Id,
                                SaleId = styleItemSale.Id,
                                ListingId = dbWmListing.Id,                                
                                CreateDate = _time.GetAppNowTime(),
                                CreatedBy = null,
                            };
                            db.StyleItemSaleToListings.Add(dbSaleToListing);
                        }

                        var newDefaultPrice = _priceService.ApplyMarketSpecified(wmSaleToMarket.SalePrice,
                            wmSaleToMarket.SFPSalePrice,
                            (MarketType)dbWmListing.Market,
                            dbWmListing.MarketplaceId,
                            styleItemWeight,
                            dbWmListing.IsPrime,
                            dbWmListing.IsFBA);

                        if (wmSaleToMarket.SalePrice != newDefaultPrice)
                        {
                            _log.Info("OverrideSalePrice: " + dbSaleToListing.OverrideSalePrice + " => " + newDefaultPrice);
                            dbSaleToListing.OverrideSalePrice = newDefaultPrice;
                        }
                        else
                        {
                            dbSaleToListing.OverrideSalePrice = null;
                        }                        
                    }
                    db.Commit();
                }
            }
        }

        public void UpdatePrices(string filepath)
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
                
                for (var i = 1; i <= sheet.LastRowNum; i++)
                {
                    var row = sheet.GetRow(i);
                    if (row.GetCell(0) != null)
                    {
                        try
                        {
                            items.Add(new ItemDTO()
                            {
                                SKU = row.GetCell(0).ToString(),
                                CurrentPrice = PriceHelper.RoundToTwoPrecision(ExcelHelper.TryGetCellDecimal(row.GetCell(1))) ?? 0,
                            });
                        }
                        catch (Exception ex)
                        {
                            _log.Info("Issue with processing: " + ex.Message);
                        }
                    }
                }

                using (var db = _dbFactory.GetRWDb())
                {
                    foreach (var item in items)
                    {
                        var dbListing = db.Listings.GetAll().FirstOrDefault(l => l.SKU == item.SKU && l.Market == (int)MarketType.Walmart);
                        dbListing.CurrentPrice = item.CurrentPrice;
                        dbListing.PriceUpdateRequested = true;
                        dbListing.IsPrime = true;
                        var dbSaleToMarkets = (from sl in db.StyleItemSaleToListings.GetAll()
                                               join sm in db.StyleItemSaleToMarkets.GetAll() on sl.SaleToMarketId equals sm.Id
                                               where sl.ListingId == dbListing.Id
                                               && sm.Market == (int)MarketType.Walmart
                                               select sm).ToList();
                        dbSaleToMarkets.ForEach(sm => sm.SalePrice = item.CurrentPrice);

                        db.Commit();
                    }
                }
            }
        }

        public void UpdateIntlSales(MarketType market, string marketplaceId)
        {
            using (var db = _dbFactory.GetRWDb())
            {
                var caListings = db.Listings.GetAll().Where(l => l.Market == (int)market
                    && (l.MarketplaceId == marketplaceId || String.IsNullOrEmpty(marketplaceId))).ToList();
                var caItems = db.Items.GetAll().Where(l => l.Market == (int)market
                    && (l.MarketplaceId == marketplaceId || String.IsNullOrEmpty(marketplaceId))).ToList();
                var caStyleItemIds = caItems.Where(i => i.StyleItemId.HasValue).Select(i => i.StyleItemId).ToList();
                var caStyleItems = db.StyleItems.GetAll().Where(si => caStyleItemIds.Contains(si.Id)).ToList();
                var caStyleIds = caStyleItems.Select(si => si.StyleId).ToList();
                var caStyleCaches = db.StyleCaches.GetAll().Where(sc => caStyleIds.Contains(sc.Id)).ToList();
                var caStyleItemSales = db.StyleItemSales.GetAll().Where(ss => caStyleItemIds.Contains(ss.StyleItemId)).ToList();
                var caSaleIds = caStyleItemSales.Select(ss => ss.Id).ToList();
                var caSaleToMarkets = db.StyleItemSaleToMarkets.GetAll()
                    .Where(s => caSaleIds.Contains(s.SaleId)
                        && (s.Market == (int)market
                            && (s.MarketplaceId == marketplaceId || String.IsNullOrEmpty(marketplaceId))))
                    .ToList();

                var allRates = db.RateByCountries.GetAllAsDto().ToList();

                foreach (var caListing in caListings)
                {
                    var caItem = caItems.FirstOrDefault(i => i.Id == caListing.ItemId);
                    if (caItem == null
                        || !caItem.StyleId.HasValue
                        || !caItem.StyleItemId.HasValue)
                        continue;
                    var caStyleCache = caStyleCaches.FirstOrDefault(sc => sc.Id == caItem.StyleId.Value);
                    if (caStyleCache == null)
                        continue;
                    if (caStyleCache.InternationalPackageValue == "Regular")
                        continue;

                    var styleItem = caStyleItems.FirstOrDefault(si => si.Id == caItem.StyleItemId.Value);
                    if (styleItem == null)
                        continue;

                    double? weight = styleItem != null ? styleItem.Weight : null;

                    IList<RateByCountryDTO> rates = null;
                    if (weight.HasValue && weight > 0)
                        rates = allRates.Where(r => r.Weight == Math.Floor(weight.Value)).ToList();

                    decimal? caRateActualCostRegular = null;
                    decimal? caRateActualCostFlat = null;
                    if (rates != null && rates.Any())
                    {
                        caRateActualCostRegular = rates.FirstOrDefault(r => r.Country == "CA" && r.PackageType == "Regular")?.Cost;
                        caRateActualCostFlat = rates.FirstOrDefault(r => r.Country == "CA" && r.PackageType == "LargeEnvelopeOrFlat")?.Cost;
                    }
                    if (!caRateActualCostRegular.HasValue
                        || !caRateActualCostFlat.HasValue)
                        continue;

                    var sale = caStyleItemSales.FirstOrDefault(ss => ss.StyleItemId == styleItem.Id
                        && !ss.IsDeleted);
                    if (sale == null)
                        continue;
                    var saleToMarket = caSaleToMarkets.FirstOrDefault(ss => ss.SaleId == sale.Id);
                    if (saleToMarket == null)
                        continue;

                    var newPrice = saleToMarket.SalePrice + Math.Round(PriceHelper.Convert(caRateActualCostRegular.Value, PriceHelper.CADtoUSD));// - caRateActualCostFlat.Value, PriceHelper.CADtoUSD));
                    if (saleToMarket.SalePrice != newPrice)
                    {
                        _log.Info("Sale Price changed: " + caListing.SKU + " - " + saleToMarket.SalePrice + " => " + newPrice);
                        saleToMarket.SalePrice = newPrice;
                        caListing.PriceUpdateRequested = true;
                    }

                    //var newPrice = caListing.CurrentPrice
                    //    + Math.Round(PriceHelper.Convert(caRateActualCostRegular.Value, PriceHelper.CADtoUSD));// - caRateActualCostFlat.Value
                    //_log.Info("Price changed: " + caListing.CurrentPrice + " => " + newPrice);
                    //caListing.CurrentPrice = newPrice;
                    //caListing.PriceUpdateRequested = true;
                    //caListing.UpdateDate = _time.GetAppNowTime();
                }
                db.Commit();
            }


            //using (var db = _dbFactory.GetRWDb())
            //{
            //    var rateForMarketplace = RateHelper.GetRatesByStyleItemId(db, styleItemId);

            //    var marketPrices = db.StyleItemSaleToMarkets.GetAll().Where(s => s.SaleId == saleId).ToList();
            //    foreach (var marketPrice in marketPrices)
            //    {
            //        if (marketPrice.SalePrice.HasValue)
            //        {
            //            marketPrice.SalePrice = RateHelper.CalculateForMarket((MarketType) marketPrice.Market,
            //                marketPrice.MarketplaceId,
            //                newSalePrice,
            //                rateForMarketplace[MarketplaceKeeper.AmazonComMarketplaceId],
            //                rateForMarketplace[MarketplaceKeeper.AmazonCaMarketplaceId],
            //                rateForMarketplace[MarketplaceKeeper.AmazonUkMarketplaceId]);
            //        }
            //    }
            //}
        }

        public void UpdateEBayPricesAddedShippingCost()
        {
            var market = MarketType.eBay;
            var marketplaceId = MarketplaceKeeper.eBayPA;
            _log.Info("Begin Update Regular Prices");
            using (var db = _dbFactory.GetRWDb())
            {
                var allRates = db.RateByCountries.GetAllAsDto().ToList();

                var dbListings = db.Listings.GetAll().Where(l => l.Market == (int)market
                    && l.MarketplaceId == marketplaceId
                    && !l.IsRemoved)
                    .ToList();
                var dbItems = db.Items.GetAll().Where(i => i.Market == (int)market
                    && i.MarketplaceId == marketplaceId)
                    .ToList();
                
                foreach (var dbListing in dbListings)
                {
                    var dbItem = dbItems.FirstOrDefault(i => i.Id == dbListing.ItemId);

                    if (dbItem == null
                        || !dbItem.StyleId.HasValue
                        || !dbItem.StyleItemId.HasValue)
                        continue;

                    var eBayStyleCache = db.StyleCaches.GetAll().FirstOrDefault(sc => sc.Id == dbItem.StyleId);
                    if (eBayStyleCache == null)
                        continue;

                    var styleItem = db.StyleItems.GetAll().FirstOrDefault(si => si.Id == dbItem.StyleItemId);
                    if (styleItem == null)
                        continue;

                    double? weight = styleItem != null ? styleItem.Weight : null;
                    string shippingSize = eBayStyleCache != null ? eBayStyleCache.ShippingSizeValue : null;
                    var item = new OrderItemRateInfo()
                    {
                        Quantity = 1,
                        ShippingSize = shippingSize
                    };

                    IList<RateByCountryDTO> rates = null;
                    if (weight.HasValue && weight > 0)
                        rates = allRates.Where(r => r.Weight == Math.Floor(weight.Value)).ToList();

                    var localPackageType = PackageTypeCode.Flat;
                    if (!String.IsNullOrEmpty(shippingSize))
                    {
                        localPackageType = ShippingServiceUtils.IsSupportFlatEnvelope(new List<OrderItemRateInfo>() { item })
                                ? PackageTypeCode.Flat
                                : PackageTypeCode.Regular;
                    }

                    decimal? eBayRateActualCost = null;
                    if (rates != null && rates.Any())
                    {
                        var usPackageType = localPackageType.ToString();
                        eBayRateActualCost = rates.FirstOrDefault(r => r.Country == "US" && r.PackageType == usPackageType)?.Cost;
                    }
                    if (!eBayRateActualCost.HasValue)
                        continue;

                    if (eBayRateActualCost.HasValue)
                    {
                        var newPrice = PriceHelper.RoundToFloor99(dbListing.CurrentPrice + eBayRateActualCost.Value);
                        _log.Info("Sale Price changed, SKU=" + dbListing.SKU + ": " + dbListing.CurrentPrice + " => " + newPrice);
                        dbListing.CurrentPrice = newPrice;
                        dbListing.CurrentPriceUpdateDate = _time.GetAppNowTime();
                        dbListing.PriceUpdateRequested = true;
                        dbListing.PriceUpdateRequestedDate = _time.GetAppNowTime();
                    }
                }

                db.Commit();
            }


            _log.Info("Begin Update Sales");
            using (var db = _dbFactory.GetRWDb())
            {
                var allRates = db.RateByCountries.GetAllAsDto().ToList();

                var styleItemSales = db.StyleItemSales.GetAll().Where(s => !s.IsDeleted && !s.CloseDate.HasValue).ToList();

                var styleItemIds = styleItemSales.Select(i => i.StyleItemId).ToList();
                var styleItems = db.StyleItems.GetAll().Where(si => styleItemIds.Contains(si.Id)).ToList();

                var styleIds = styleItems.Select(si => si.StyleId).ToList();
                var styleCaches = db.StyleCaches.GetAll().Where(sc => styleIds.Contains(sc.Id)).ToList();

                var saleIds = styleItemSales.Select(ss => ss.Id).ToList();

                var eBaySaleToMarkets = db.StyleItemSaleToMarkets.GetAll()
                    .Where(s => saleIds.Contains(s.SaleId)
                        && (s.Market == (int)market
                            && (s.MarketplaceId == marketplaceId)))
                    .ToList();
               
                var onlyCaSaleList = new List<long>();
                foreach (var eBaySaleToMarket in eBaySaleToMarkets)
                {
                    if (!eBaySaleToMarket.SalePrice.HasValue || eBaySaleToMarket.SalePrice == 0)
                        continue;

                    var sale = styleItemSales.FirstOrDefault(s => s.Id == eBaySaleToMarket.SaleId);
                    if (sale == null)
                        continue;

                    var styleItem = styleItems.FirstOrDefault(si => si.Id == sale.StyleItemId);
                    if (styleItem == null)
                        continue;
                    var eBayStyleCache = styleCaches.FirstOrDefault(sc => sc.Id == styleItem.StyleId);
                    if (eBayStyleCache == null)
                        continue;

                    double? weight = styleItem != null ? styleItem.Weight : null;
                    string shippingSize = eBayStyleCache != null ? eBayStyleCache.ShippingSizeValue : null;
                    var item = new OrderItemRateInfo()
                    {
                        Quantity = 1,
                        ShippingSize = shippingSize
                    };

                    IList<RateByCountryDTO> rates = null;
                    if (weight.HasValue && weight > 0)
                        rates = allRates.Where(r => r.Weight == Math.Floor(weight.Value)).ToList();

                    var localPackageType = PackageTypeCode.Flat;
                    if (!String.IsNullOrEmpty(shippingSize))
                    {
                        localPackageType = ShippingServiceUtils.IsSupportFlatEnvelope(new List<OrderItemRateInfo>() { item })
                                ? PackageTypeCode.Flat
                                : PackageTypeCode.Regular;
                    }

                    decimal? eBayRateActualCost = null;
                    if (rates != null && rates.Any())
                    {
                        var usPackageType = localPackageType.ToString();
                        eBayRateActualCost = rates.FirstOrDefault(r => r.Country == "US" && r.PackageType == usPackageType)?.Cost;
                    }
                    if (!eBayRateActualCost.HasValue)
                        continue;

                    if (eBayRateActualCost.HasValue)
                    {
                        var newPrice = PriceHelper.RoundToFloor99(eBaySaleToMarket.SalePrice.Value + eBayRateActualCost.Value);
                        _log.Info("Sale Price changed, saleId=" + eBaySaleToMarket.SaleId + ": " + eBaySaleToMarket.SalePrice + " => " + newPrice);
                        eBaySaleToMarket.SalePrice = newPrice;
                        eBaySaleToMarket.UpdateDate = _time.GetAppNowTime();
                    }
                }

                db.Commit();
            }
        }

        public void UpdateWalmartPricesBasedOnUS()
        {
            //var rePriceService = new RePriceService();
            //IDictionary<string, decimal?> rateForMarketplace = null;// RateHelper.GetRatesByStyleItemId(db, styleItemId);
            //using (var db = _dbFactory.GetRWDb())
            //{
            //    var toUpdateInfo = (from l in db.Items.GetAllViewActual()
            //                        select new ItemDTO()
            //                        {
            //                            Id = i.Id,
            //                            ListingEntityId = l.Id,
            //                            CurrentPrice = l.CurrentPrice,
            //                            SalePrice = l.SalePrice,
            //                            IsPrime = l.IsPrime,
            //                            Weight = l.Weight,
            //                        }).ToList();
            //    var sourceListings = db.Items.GetAllViewActual().Where(m => m.Market == (int)MarketType.Amazon
            //        && m.MarketplaceId == MarketplaceKeeper.AmazonComMarketplaceId).ToList();

            //    foreach (var toUpdateListing in toUpdateListings)
            //    {
            //        var sourceListingMinPrice = sourceListings.Min(l => l.ite)
            //    }
            //}

        }


        public void UpdateIntlSalesBasedOnUS(MarketType market, string marketplaceId)
        {
            var usShipping = RateService.GetMarketShippingAmount(MarketType.Amazon,
                MarketplaceKeeper.AmazonComMarketplaceId); // 4.49M;

            //TODO: check
            var currency = PriceHelper.GetCurrencyAbbr(market, marketplaceId);
            var convertRate = PriceHelper.GetExchangeRateToUSD(currency);
            var caShipping = PriceHelper.Convert(RateService.GetMarketShippingAmount(market, marketplaceId), convertRate);// PriceHelper.USDtoCADRevert);


            using (var db = _dbFactory.GetRWDb())
            {
                var styleItemSales = db.StyleItemSales.GetAll().Where(s => !s.IsDeleted && !s.CloseDate.HasValue).ToList();

                var styleItemIds = styleItemSales.Select(i => i.StyleItemId).ToList();
                var styleItems = db.StyleItems.GetAll().Where(si => styleItemIds.Contains(si.Id)).ToList();

                var styleIds = styleItems.Select(si => si.StyleId).ToList();
                var styleCaches = db.StyleCaches.GetAll().Where(sc => styleIds.Contains(sc.Id)).ToList();

                var saleIds = styleItemSales.Select(ss => ss.Id).ToList();

                var caSaleToMarkets = db.StyleItemSaleToMarkets.GetAll()
                    .Where(s => saleIds.Contains(s.SaleId)
                        && (s.Market == (int)market
                            && (s.MarketplaceId == marketplaceId || String.IsNullOrEmpty(marketplaceId))))
                    .ToList();
                var usSaleToMarkets = db.StyleItemSaleToMarkets.GetAll()
                    .Where(s => saleIds.Contains(s.SaleId)
                        && (s.Market == (int)MarketType.Amazon
                            && s.MarketplaceId == MarketplaceKeeper.AmazonComMarketplaceId))
                    .ToList();

                var allRates = db.RateByCountries.GetAllAsDto().ToList();

                var onlyCaSaleList = new List<long>();
                foreach (var caSaleToMarket in caSaleToMarkets)
                {
                    if (!caSaleToMarket.SalePrice.HasValue || caSaleToMarket.SalePrice == 0)
                        continue;

                    var sale = styleItemSales.FirstOrDefault(s => s.Id == caSaleToMarket.SaleId);
                    if (sale == null)
                        continue;
                    var usSaleToMarket = usSaleToMarkets.FirstOrDefault(sm => sm.SaleId == sale.Id);
                    if (usSaleToMarket == null || !usSaleToMarket.SalePrice.HasValue || usSaleToMarket.SalePrice == 0)
                    {
                        onlyCaSaleList.Add(sale.Id);
                        continue;
                    }
                    var styleItem = styleItems.FirstOrDefault(si => si.Id == sale.StyleItemId);
                    if (styleItem == null)
                        continue;
                    var caStyleCache = styleCaches.FirstOrDefault(sc => sc.Id == styleItem.StyleId);
                    if (caStyleCache == null)
                        continue;

                    double? weight = styleItem != null ? styleItem.Weight : null;
                    string shippingSize = caStyleCache != null ? caStyleCache.ShippingSizeValue : null;
                    var item = new OrderItemRateInfo()
                    {
                        Quantity = 1,
                        ShippingSize = shippingSize
                    };

                    IList<RateByCountryDTO> rates = null;
                    if (weight.HasValue && weight > 0)
                        rates = allRates.Where(r => r.Weight == Math.Floor(weight.Value)).ToList();

                    var localPackageType = PackageTypeCode.Flat;
                    if (!String.IsNullOrEmpty(shippingSize))
                    {
                        localPackageType = ShippingServiceUtils.IsSupportFlatEnvelope(new List<OrderItemRateInfo>() { item })
                                ? PackageTypeCode.Flat
                                : PackageTypeCode.Regular;
                    }

                    decimal? caRateActualCostRegular = null;
                    decimal? usRateActualCost = null;
                    if (rates != null && rates.Any())
                    {
                        var usPackageType = localPackageType.ToString();

                        caRateActualCostRegular = rates.FirstOrDefault(r => r.Country == "CA" && r.PackageType == "Regular")?.Cost;
                        usRateActualCost = rates.FirstOrDefault(r => r.Country == "US" && r.PackageType == usPackageType)?.Cost;
                    }
                    if (!caRateActualCostRegular.HasValue
                        || !usRateActualCost.HasValue)
                        continue;
                    
                    var usIncome = usSaleToMarket.SalePrice.Value + usShipping - usRateActualCost.Value;
                    var newPrice = usIncome - (caShipping - caRateActualCostRegular.Value);
                    newPrice = PriceHelper.Convert(newPrice, 1 / convertRate);// PriceHelper.CADtoUSDRevert);

                    if (caSaleToMarket.SalePrice != caSaleToMarket.SalePrice)
                    {
                        _log.Info("Sale Price changed, saleId=" + caSaleToMarket.SaleId + ": " + caSaleToMarket.SalePrice + " => " + newPrice);
                        caSaleToMarket.SalePrice = newPrice;
                        caSaleToMarket.UpdateDate = _time.GetAppNowTime();
                    }
                }
                db.Commit();
            }
        }

        public void UpdateIntlPricesBasedOnUS(MarketType market, 
            string marketplaceId, 
            IList<string> skuList,
            bool onlyRise)
        {
            var usShipping = RateService.GetMarketShippingAmount(MarketType.Amazon,
                MarketplaceKeeper.AmazonComMarketplaceId); // 4.49M;
            var currency = PriceHelper.GetCurrencyAbbr(market, marketplaceId);
            var convertRate = PriceHelper.GetExchangeRateToUSD(currency);
            //var marketShipping = PriceHelper.Convert(RateService.GetMarketShippingAmount(market, marketplaceId), convertRate);// PriceHelper.USDtoCADRevert);
            //var marketExtra = RateService.GetMarketExtraAmount((MarketType)market, marketplaceId);

            using (var db = _dbFactory.GetRWDb())
            {
                var marketListings = db.Listings.GetAll().Where(l => l.Market == (int)market
                    && (l.MarketplaceId == marketplaceId || String.IsNullOrEmpty(marketplaceId))).ToList();
                if (skuList != null && skuList.Any())
                    marketListings = marketListings.Where(l => skuList.Contains(l.SKU)).ToList();

                var marketItems = db.Items.GetAll().Where(l => l.Market == (int)market
                    && (l.MarketplaceId == marketplaceId || String.IsNullOrEmpty(marketplaceId))).ToList();

                var usItemInfos = (from i in db.Items.GetAll()
                                   join l in db.Listings.GetAll() on i.Id equals l.ItemId
                                   where l.MarketplaceId == MarketplaceKeeper.AmazonComMarketplaceId
                                     && l.Market == (int)MarketType.Amazon
                                     && l.IsRemoved == false
                                   orderby l.IsFBA ascending,
                                        l.IsPrime ascending,                                   
                                        l.CurrentPrice ascending
                                   select new
                                   {
                                       SKU = l.SKU,
                                       IsPrime = l.IsPrime,
                                       IsFBA = l.IsFBA,
                                       CurrentPrice = l.CurrentPrice,
                                       StyleId = i.StyleId,
                                       StyleItemId = i.StyleItemId
                                   }).ToList();

                var marketStyleItemIds = marketItems.Where(i => i.StyleItemId.HasValue).Select(i => i.StyleItemId).ToList();
                var marketStyleItems = db.StyleItems.GetAll().Where(si => marketStyleItemIds.Contains(si.Id)).ToList();
                var marketStyleIds = marketStyleItems.Select(si => si.StyleId).ToList();
                var marketStyleCaches = db.StyleCaches.GetAll().Where(sc => marketStyleIds.Contains(sc.Id)).ToList();
                var allRates = db.RateByCountries.GetAllAsDto().ToList();
                
                foreach (var marketListing in marketListings)
                {
                    var caItem = marketItems.FirstOrDefault(i => i.Id == marketListing.ItemId);
                    if (caItem == null
                        || !caItem.StyleId.HasValue
                        || !caItem.StyleItemId.HasValue)
                    {
                        _log.Info("Skipped SKU=" + marketListing.SKU + ". No caItem or styleId/styleItemId");
                        continue;
                    }
                    var caStyleCache = marketStyleCaches.FirstOrDefault(sc => sc.Id == caItem.StyleId.Value);
                    if (caStyleCache == null)
                    {
                        _log.Info("Skipped SKU=" + marketListing.SKU + ". No caStyleCache");
                        continue;
                    }
                    //if (caStyleCache.InternationalPackageValue == "Regular")
                    //    continue;

                    var styleItem = marketStyleItems.FirstOrDefault(si => si.Id == caItem.StyleItemId.Value);
                    if (styleItem == null)
                    {
                        _log.Info("Skipped SKU=" + marketListing.SKU + ". No styleItem");
                        continue;
                    }

                    double? weight = styleItem != null ? styleItem.Weight : null;

                    //var usListing = usListings.FirstOrDefault(l => l.SKU == caListing.SKU);
                    var usItemInfo = usItemInfos.FirstOrDefault(i => i.StyleItemId == caItem.StyleItemId.Value);
                    decimal? usPrice = usItemInfo?.CurrentPrice;
                    if (usItemInfo?.IsPrime == true || usItemInfo?.IsFBA == true)
                    {
                        usPrice -= _priceService.GetPrimePart((decimal?)weight);
                    }

                    if (!usPrice.HasValue)
                    {
                        _log.Info("Skipped SKU=" + marketListing.SKU + ". No usPrice");
                        continue;
                    }
                    
                    string shippingSize = caStyleCache != null ? caStyleCache.ShippingSizeValue : null;
                    var item = new OrderItemRateInfo()
                    {
                        Quantity = 1,
                        ShippingSize = shippingSize
                    };

                    IList<RateByCountryDTO> rates = null;
                    if (!weight.HasValue)
                        weight = 8; //Default weight for price recalculation
                    if (weight.HasValue && weight > 0)
                        rates = allRates.Where(r => r.Weight == Math.Ceiling(weight.Value < 1.0 ? 1.0 : weight.Value)).ToList();

                    var localPackageType = PackageTypeCode.Flat;
                    if (!String.IsNullOrEmpty(shippingSize))
                    {
                        localPackageType = ShippingServiceUtils.IsSupportFlatEnvelope(new List<OrderItemRateInfo>() {item})
                                ? PackageTypeCode.Flat
                                : PackageTypeCode.Regular;
                    }

                    decimal? caRateActualCostRegular = null;
                    decimal? ukRateActualCostRegular = null;
                    decimal? auRateActualCostRegular = null;
                    decimal? usRateActualCost = null;
                    if (rates != null && rates.Any())
                    {
                        var usPackageType = localPackageType.ToString();

                        caRateActualCostRegular = rates.FirstOrDefault(r => r.Country == "CA" && r.PackageType == "Regular")?.Cost;
                        ukRateActualCostRegular = rates.FirstOrDefault(r => r.Country == "GB" && r.PackageType == "Regular")?.Cost;
                        auRateActualCostRegular = rates.FirstOrDefault(r => r.Country == "AU" && r.PackageType == "Regular")?.Cost;

                        usRateActualCost = rates.FirstOrDefault(r => r.Country == "US" && r.PackageType == usPackageType)?.Cost;
                    }
                    if (!usRateActualCost.HasValue)
                    {
                        _log.Info("Skipped SKU=" + marketListing.SKU + ". No usRateActualCost");
                        continue;
                    }

                    //var newPrice = RateHelper.CalculateForMarket(market,
                    //    marketplaceId,
                    //    usPrice.Value,
                    //    usRateActualCost.Value,
                    //    caRateActualCostRegular.Value,
                    //    ukRateActualCostRegular.Value,
                    //    auRateActualCostRegular.Value,
                    //    usShipping,
                    //    marketShipping,
                    //    marketExtra);

                    //newPrice = _priceService.ApplyMarketSpecified(newPrice,
                    //    null,
                    //    market,
                    //    marketplaceId,
                    //    weight,
                    //    marketListing.IsPrime);

                    var newPrice = _priceService.GetMarketPrice(usPrice.Value,
                        null,
                        marketListing.IsPrime,
                        marketListing.IsFBA,
                        weight,
                        market,
                        marketplaceId,
                        new Dictionary<string, decimal?>()
                        {
                            { MarketplaceKeeper.AmazonComMarketplaceId, usRateActualCost.Value },
                            { MarketplaceKeeper.AmazonCaMarketplaceId, caRateActualCostRegular.Value },
                            { MarketplaceKeeper.AmazonUkMarketplaceId, ukRateActualCostRegular.Value },
                            { MarketplaceKeeper.AmazonAuMarketplaceId, auRateActualCostRegular.Value },
                        });

                    if (marketListing.CurrentPrice != newPrice)
                    {
                        if (!onlyRise || newPrice > marketListing.CurrentPrice)
                        {
                            _log.Info("Price changed, SKU=" + marketListing.SKU + ": " + marketListing.CurrentPrice + " => " + newPrice + ", usBasePrice: " + usPrice);
                            _itemChangeHistory.LogListingPrice(PriceChangeSourceType.EnterNewPrice,
                                    marketListing.Id,
                                    marketListing.SKU,
                                    newPrice,
                                    marketListing.CurrentPrice,
                                    _time.GetAppNowTime(),
                                    null);
                            marketListing.CurrentPrice = newPrice;
                            marketListing.PriceUpdateRequested = true;
                            marketListing.UpdateDate = _time.GetAppNowTime();
                        }
                    }
                }
                db.Commit();
            }
        }




        public void UpdateIntlPrices(MarketType market, string marketplaceId)
        {
            using (var db = _dbFactory.GetRWDb())
            {
                var caListings = db.Listings.GetAll().Where(l => l.Market == (int)market
                    && (l.MarketplaceId == marketplaceId || String.IsNullOrEmpty(marketplaceId))).ToList();
                var caItems = db.Items.GetAll().Where(l => l.Market == (int)market
                    && (l.MarketplaceId == marketplaceId || String.IsNullOrEmpty(marketplaceId))).ToList();
                var caStyleItemIds = caItems.Where(i => i.StyleItemId.HasValue).Select(i => i.StyleItemId).ToList();
                var caStyleItems = db.StyleItems.GetAll().Where(si => caStyleItemIds.Contains(si.Id)).ToList();
                var caStyleIds = caStyleItems.Select(si => si.StyleId).ToList();
                var caStyleCaches = db.StyleCaches.GetAll().Where(sc => caStyleIds.Contains(sc.Id)).ToList();
                var allRates = db.RateByCountries.GetAllAsDto().ToList();


                foreach (var caListing in caListings)
                {
                    var caItem = caItems.FirstOrDefault(i => i.Id == caListing.ItemId);
                    if (caItem == null 
                        || !caItem.StyleId.HasValue
                        || !caItem.StyleItemId.HasValue)
                        continue;
                    var caStyleCache = caStyleCaches.FirstOrDefault(sc => sc.Id == caItem.StyleId.Value);
                    if (caStyleCache == null)
                        continue;
                    if (caStyleCache.InternationalPackageValue == "Regular")
                        continue;

                    var styleItem = caStyleItems.FirstOrDefault(si => si.Id == caItem.StyleItemId.Value);
                    if (styleItem == null)
                        continue;

                    double? weight = styleItem != null ? styleItem.Weight : null;

                    IList<RateByCountryDTO> rates = null;
                    if (weight.HasValue && weight > 0)
                        rates = allRates.Where(r => r.Weight == Math.Floor(weight.Value)).ToList();

                    decimal? caRateActualCostRegular = null;
                    decimal? caRateActualCostFlat = null;
                    if (rates != null && rates.Any())
                    {
                        caRateActualCostRegular = rates.FirstOrDefault(r => r.Country == "CA" && r.PackageType == "Regular")?.Cost;
                        caRateActualCostFlat = rates.FirstOrDefault(r => r.Country == "CA" && r.PackageType == "LargeEnvelopeOrFlat")?.Cost;
                    }
                    if (!caRateActualCostRegular.HasValue
                        || !caRateActualCostFlat.HasValue)
                        continue;

                    var newPrice = caListing.CurrentPrice
                        + Math.Round(PriceHelper.Convert(caRateActualCostRegular.Value, PriceHelper.CADtoUSD));// - caRateActualCostFlat.Value
                    _log.Info("Price changed, SKU=" + caListing.SKU + ": " + caListing.CurrentPrice + " => " + newPrice);
                    caListing.CurrentPrice = newPrice;
                    caListing.PriceUpdateRequested = true;
                    caListing.UpdateDate = _time.GetAppNowTime();
                }
                db.Commit();
            }
            

            //using (var db = _dbFactory.GetRWDb())
                //{
                //    var rateForMarketplace = RateHelper.GetRatesByStyleItemId(db, styleItemId);

                //    var marketPrices = db.StyleItemSaleToMarkets.GetAll().Where(s => s.SaleId == saleId).ToList();
                //    foreach (var marketPrice in marketPrices)
                //    {
                //        if (marketPrice.SalePrice.HasValue)
                //        {
                //            marketPrice.SalePrice = RateHelper.CalculateForMarket((MarketType) marketPrice.Market,
                //                marketPrice.MarketplaceId,
                //                newSalePrice,
                //                rateForMarketplace[MarketplaceKeeper.AmazonComMarketplaceId],
                //                rateForMarketplace[MarketplaceKeeper.AmazonCaMarketplaceId],
                //                rateForMarketplace[MarketplaceKeeper.AmazonUkMarketplaceId]);
                //        }
                //    }
                //}
        }

        public void MigrateUSSaleTo(MarketType market)
        {
            var correction = PriceHelper.GetSalePriceCorrectionByMarket(market);
            using (var db = _dbFactory.GetRWDb())
            {
                var allDestListings = db.Items.GetAllViewActual().Where(m => m.Market == (int) market).ToList();
                var styleItemIdList = allDestListings.Select(l => l.StyleItemId).ToList();
                var saleList = db.StyleItemSales.GetAllAsDto().Where(s => styleItemIdList.Contains(s.StyleItemId)).ToList();
                var saleIdList = saleList.Select(s => s.Id).ToList();
                var allMarketSaleList =
                    db.StyleItemSaleToMarkets.GetAllAsDto().Where(s => saleIdList.Contains(s.SaleId)).ToList();

                foreach (var sale in saleList)
                {
                    var usMarketSale = allMarketSaleList.FirstOrDefault(
                            m => m.SaleId == sale.Id && m.MarketplaceId == MarketplaceKeeper.AmazonComMarketplaceId);
                    if (usMarketSale != null && usMarketSale.SalePrice.HasValue)
                    {
                        var destMarketSale = allMarketSaleList.FirstOrDefault(m => m.SaleId == sale.Id && m.Market == (int) market);
                        if (destMarketSale == null)
                        {
                            var destListings = allDestListings.Where(l => l.StyleItemId == sale.StyleItemId).ToList();
                            if (destListings.Any())
                            {
                                var newSalePrice = usMarketSale.SalePrice + correction;
                                var newMarketSale = new StyleItemSaleToMarket()
                                {
                                    Market = (int) market,
                                    MarketplaceId = null,
                                    SaleId = sale.Id,
                                    SalePrice = newSalePrice,
                                    SalePercent = usMarketSale.SalePercent,
                                    ApplyToNewListings = usMarketSale.ApplyToNewListings,
                                    CreateDate = _time.GetAppNowTime(),
                                    CreatedBy = null,
                                };
                                db.StyleItemSaleToMarkets.Add(newMarketSale);
                                db.Commit();

                                foreach (var listing in destListings)
                                {
                                    _log.Info("Add Sale to, SKU=" + listing.SKU + ", currentPrice=" + listing.CurrentPrice + ", salePrice=" + newSalePrice);

                                    db.StyleItemSaleToListings.Add(new StyleItemSaleToListing()
                                    {
                                        SaleId = sale.Id,
                                        SaleToMarketId = newMarketSale.Id,
                                        ListingId = listing.ListingEntityId.Value,
                                        CreatedBy = null,
                                        CreateDate = _time.GetAppNowTime()
                                    });

                                    var dbListing = db.Listings.Get(listing.ListingEntityId.Value);
                                    dbListing.PriceUpdateRequested = true;
                                }
                                db.Commit();
                            }
                        }
                    }
                }
            }
        }

        public void CallSalesEndChecker()
        {
            var saleService = new SaleManager(_log, _time);
            using (var db = _dbFactory.GetRWDb())
            {
                var sale = db.StyleItemSales.GetAll().FirstOrDefault(s => s.Id == 1415);
                saleService.UpdateSoldPieces(db, sale, _time.GetAppNowTime());
                saleService.CheckSaleEndForAll(db);
            }
        }

        //public void MoveSales()
        //{
        //    using (var db = _dbFactory.GetRWDb())
        //    {
        //        //db.Sales.GetActiveSales()
        //        var allSaleListings = db.Items.GetAllViewAsDto().Where(i => i.SalePrice.HasValue).ToList();
        //        var allSales = db.Sales.GetActiveSales().ToList();
        //        foreach (var saleListing in allSaleListings)
        //        {
        //            _log.Info("Listing Id=" + saleListing.ListingEntityId + ", market=" + saleListing.Market + ", marketplaceId=" + saleListing.MarketplaceId);
        //            var sale = allSales.FirstOrDefault(s => s.Id == saleListing.SaleId);
        //            if (sale != null)
        //            {
        //                if (saleListing.StyleItemId.HasValue && saleListing.ListingEntityId.HasValue)
        //                {
        //                    var styleItemSale = db.StyleItemSales.GetAll().FirstOrDefault(s => s.StyleItemId == saleListing.StyleItemId.Value);
        //                    if (styleItemSale == null)
        //                    {
        //                        styleItemSale = new StyleItemSale()
        //                        {
        //                            StyleItemId = saleListing.StyleItemId.Value,
        //                            SaleStartDate = sale.SaleStartDate ?? sale.CreateDate,
        //                            SaleEndDate = sale.SaleEndDate,
        //                            MaxPiecesOnSale = sale.MaxPiecesOnSale,
        //                            MaxPiecesMode = saleListing.SaleAttachMode == (int) SaleAttachModes.Parent
        //                                ? (int) MaxPiecesOnSaleMode.ByStyle
        //                                : (int) MaxPiecesOnSaleMode.BySize,
        //                            CreateDate = _time.GetAppNowTime(),
        //                        };
        //                        db.StyleItemSales.Add(styleItemSale);
        //                        db.Commit();
        //                    }
        //                    else
        //                    {
        //                        if (styleItemSale.SaleStartDate != saleListing.SaleStartDate)
        //                            _log.Info("Sale exist, current StartDate=" + styleItemSale.SaleStartDate + ", expect StartDate=" + saleListing.SaleStartDate);
        //                        if (styleItemSale.SaleEndDate != saleListing.SaleEndDate)
        //                            _log.Info("Sale exist, current EndDate=" + styleItemSale.SaleEndDate + ", expect EndDate=" + saleListing.SaleEndDate);
        //                        if (styleItemSale.MaxPiecesOnSale != saleListing.MaxPiecesOnSale)
        //                            _log.Info("Sale exist, current MaxPiecesOnSale=" + styleItemSale.MaxPiecesOnSale + ", expect MaxPiecesOnSale=" + saleListing.MaxPiecesOnSale);
        //                        styleItemSale.MaxPiecesOnSale = Math.Max(styleItemSale.MaxPiecesOnSale ?? 0, saleListing.MaxPiecesOnSale ?? 0);
        //                        styleItemSale.SaleStartDate =  DateHelper.Min(styleItemSale.SaleStartDate, saleListing.SaleStartDate);
        //                        styleItemSale.SaleEndDate = DateHelper.Max(styleItemSale.SaleEndDate, saleListing.SaleEndDate);
        //                    }

        //                    var saleStyleItemMarket = db.StyleItemSaleToMarkets.GetAll().FirstOrDefault(sm => sm.SaleId == styleItemSale.Id
        //                                                                                && sm.Market == saleListing.Market
        //                                                                                && (sm.MarketplaceId == saleListing.MarketplaceId ||
        //                                                                                 String.IsNullOrEmpty(saleListing.MarketplaceId)));

        //                    if (saleStyleItemMarket == null)
        //                    {
        //                        saleStyleItemMarket = new StyleItemSaleToMarket()
        //                        {
        //                            SaleId = styleItemSale.Id,
        //                            Market = saleListing.Market,
        //                            MarketplaceId = saleListing.MarketplaceId,
        //                            SalePrice = saleListing.SalePrice,
        //                            ApplyToNewListings = false,
        //                            CreateDate = _time.GetAppNowTime(),
        //                        };
        //                        db.StyleItemSaleToMarkets.Add(saleStyleItemMarket);
        //                        db.Commit();
        //                    }
        //                    else
        //                    {
        //                        if (saleStyleItemMarket.SalePrice != saleListing.SalePrice)
        //                            _log.Info("Sale market exist, current SalePrice = " + saleStyleItemMarket.SalePrice + ", expect SalePrice = " + saleListing.SalePrice);
        //                    }

        //                    var saleStyleItemListing = db.StyleItemSaleToListings.GetAll().FirstOrDefault(sl => sl.SaleId == styleItemSale.Id
        //                                    && sl.ListingId == saleListing.ListingEntityId.Value);
        //                    if (saleStyleItemListing == null)
        //                    {
        //                        saleStyleItemListing = new StyleItemSaleToListing()
        //                        {
        //                            SaleToMarketId = saleStyleItemMarket.Id,
        //                            SaleId = styleItemSale.Id,
        //                            ListingId = saleListing.ListingEntityId.Value,
        //                            CreateDate = _time.GetAppNowTime(),
        //                        };
        //                        db.StyleItemSaleToListings.Add(saleStyleItemListing);
        //                        db.Commit();
        //                    }
        //                    else
        //                    {
        //                        _log.Error("Sale already exist");
        //                    }
        //                }
        //                else
        //                {
        //                    _log.Error("Listing has empty styleItemId, listingId=" + saleListing.Id + ", sku=" + saleListing.SKU);
        //                }
        //            }
        //            else
        //            {
        //                _log.Error("Empty sale, listingId=" + saleListing.Id + ", sku=" + saleListing.SKU);
        //            }
        //        }
        //    }
        //}

        public void CallUpdateRanks(AmazonApi api)
        {
            var settings = new SettingsService(_dbFactory);
            var ratingUpdater = new RatingUpdater(_log, _time);

            using (var db = _dbFactory.GetRWDb())
            {
                _log.Info("Start update lowest prices");
                ratingUpdater.UpdateLowestPrice(api, db);

                _log.Info("Start update myprice");
                ratingUpdater.UpdateMyPrice(api, db);

                _log.Info("Start update rating");
                ratingUpdater.UpdateRatingByProductApi(api, db);

                _log.Info("End process");
            }
        }

        public void CallUpdateLowestPrices(AmazonApi api)
        {
            using (var db = _dbFactory.GetRWDb())
            {
                var settings = new SettingsService(_dbFactory);
                var ratingUpdater = new RatingUpdater(_log, _time);

                _log.Info("Start update lowest prices");
                ratingUpdater.UpdateLowestPrice(api, db);

                _log.Info("End process");
            }
        }

        public void CallUpdateMyPrices(AmazonApi api)
        {
            using (var db = _dbFactory.GetRWDb())
            {
                var settings = new SettingsService(_dbFactory);
                var ratingUpdater = new RatingUpdater(_log, _time);
                _log.Info("Start update myprice");
                ratingUpdater.UpdateMyPrice(api, db);

                _log.Info("End process");
            }
        }

        public void CallUpdateBuyBoxPrices(AmazonApi api)
        {
            var buyBoxService = new BuyBoxService(_log, _dbFactory, _time);
            _log.Info("Start update buybox, marketplaceId=" + api.MarketplaceId);
            buyBoxService.Update(api);

            _log.Info("End process");
        }

        public void CallUpdateBuyBoxPriceForSKU(AmazonApi api, string sku)
        {
            var buyBoxService = new BuyBoxService(_log, _dbFactory, _time);
            _log.Info("Start update buybox, marketplaceId=" + api.MarketplaceId);
            buyBoxService.Update(api, new string[] { sku });

            _log.Info("End process");
        }

        //public void RequestQuantityByAdv(IMarketApi marketApi)
        //{
        //    var asinList = new List<string>() {"B00XE2LDZ2", "B00XE2LEAG"};
        //    var amazonApi = (AmazonApi) marketApi;
        //    var response = amazonApi.RetrieveOffersWithMerchant(_log, asinList);
        //    _log.Info(response.ToString());
        //}

        public void RequestQuantityForAllMarketUnprocessedAsins(IMarketApi marketApi)
        {
            var competitorsQtyService = new CompetitorQuantityCheckerService(_log, _time, _dbFactory);

            var amazonApi = (AmazonApi)marketApi;

            competitorsQtyService.ProcessAllMarketASINs(amazonApi);
        }

        public void TestRequestQuantity(IMarketApi marketApi)
        {
            var asinList = new List<string>() {"B00XE2LDZ2", "B00XE2LEAG"};
            var competitorsQtyService = new CompetitorQuantityCheckerService(_log, _time, _dbFactory);

            var amazonApi = (AmazonApi) marketApi;

            CompetitorQuantityCheckerService.CartInfo cartInfo = null;
            var resultList = competitorsQtyService.RequestQuantities(amazonApi, asinList, ref cartInfo);

            _log.Info(resultList.ToString());
        }

        public void ProcessAmazonSQS()
        {
            var marketplaces = new MarketplaceKeeper(_dbFactory, false);
            marketplaces.Init();

            var buyBoxService = new BuyBoxService(_log, _dbFactory, _time);

            var amazonSQSAccount = _company.SQSAccounts.FirstOrDefault(a => a.Type == (int) SQSAccountType.Amazon);

            var amazonSQS = new AmazonSQSReader(_log,
                _time,
                amazonSQSAccount.AccessKey,
                amazonSQSAccount.SecretKey,
                amazonSQSAccount.EndPointUrl);

            var count = 1;
            while (count > 0)
            {
                count = buyBoxService.ProcessOfferChanges(amazonSQS,
                    marketplaces.GetAll()
                        .Where(m => m.Market == (int) MarketType.Amazon || m.Market == (int) MarketType.AmazonEU || m.Market == (int)MarketType.AmazonAU)
                        .Select(m => m.SellerId)
                        .ToList());
                _log.Info("Count=" + count);
            }
        }

        public void TestParseSQSMessage()
        {
            var fileName = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Files/Xml/AmazonNotification.xml");
            using (var sr = File.OpenRead(fileName))
            {
                XmlSerializer serializer = new XmlSerializer(typeof(Notification));
                Notification n = (Notification)serializer.Deserialize(sr);
                _log.Info(n.ToString());
            }
        }

        public void TestSQSGetMessages()
        {
            var accessKey = "AKIAJ2WRO7OWKLFXTCDA";
            var secretKey = "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";
            var url = "https://sqs.us-east-1.amazonaws.com/838390836071/amazon_notification";

            var service = new AmazonSQSReader(_log, _time, accessKey, secretKey, url);
            var messages = service.GetNotification();

            _log.Info("Messages: " + messages.Count.ToString());
        }

        //public void TestCallAdvCartCreate()
        //{
        //    string marketplaceId = "ATVPDKIKX0DER";
        //    MarketplaceDTO marketplace = null;
        //    using (var db = _dbFactory.GetRWDb())
        //    {
        //        var marketplaces = db.Marketplaces.GetAllAsDto().Where(c => c.CompanyId == _company.Id);
        //        if (!String.IsNullOrEmpty(marketplaceId))
        //            marketplace = marketplaces.FirstOrDefault(m => m.MarketplaceId == marketplaceId);
        //    }

        //    var advApi = new AmazonAdvApi(_time, marketplace.Key3, marketplace.Key4, marketplace.Key5, marketplaceId);
        //    var createResponse = advApi.CartCreate(_log,
        //        new List<CartItemInfo>() 
        //        { 
        //            new CartItemInfo()
        //            {
        //                ASIN = "B01F80VVGI",
        //                Quantity = 1,
        //            }
        //        });

        //    var cartId = createResponse.Cart[0].CartId;
        //    var hmac = createResponse.Cart[0].HMAC;

        //    var items = createResponse.Cart[0].CartItems.CartItem.ToArray().Select(i => new CartItemInfo()
        //    {
        //        CartItemId = i.CartItemId,
        //        Quantity = Int32.Parse(i.Quantity)
        //    }).ToList();

        //    var addResponse = advApi.CartAdd(_log, cartId, hmac, new List<CartItemInfo>()
        //    {
        //        new CartItemInfo()
        //        {
        //            ASIN = "B017A516AK",
        //            Quantity = 999,
        //        }
        //    });
        //    var getResponse = advApi.CartGet(_log, cartId, hmac);
        //    var clearResponse = advApi.CartClear(_log, cartId, hmac);
        //    var getResponse2 = advApi.CartGet(_log, cartId, hmac);


        //    _log.Info(createResponse.ToString());
        //    _log.Info(addResponse.ToString());
        //    _log.Info(getResponse.ToString());
        //    _log.Info(clearResponse.ToString());
        //    _log.Info(getResponse2.ToString());
        //}
    }
}
