using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Amazon.Api.Exports;
using Amazon.Common.Helpers;
using Amazon.Core;
using Amazon.Core.Contracts;
using Amazon.Core.Contracts.Factories;
using Amazon.Core.Contracts.Markets;
using Amazon.Core.Entities;
using Amazon.Core.Helpers;
using Amazon.Core.Models;
using Amazon.Core.Models.Calls;
using Amazon.Core.Models.SystemActions;
using Amazon.DTO;
using Amazon.DTO.Inventory;
using Walmart.Api;

namespace Amazon.Model.Implementation
{
    public class AutoCreateWalmartListingService : AutoCreateBaseListingService
    {
        private ILogService _log;
        private ITime _time;
        private IDbFactory _dbFactory;
        private ICacheService _cacheService;
        private IBarcodeService _barcodeService;
        private IEmailService _emailService;
        public IWalmartOpenApi _openApi;
        private bool _isDebug;

        public AutoCreateWalmartListingService(ILogService log,
            ITime time,
            IDbFactory dbFactory,
            ICacheService cacheService,
            IBarcodeService barcodeService,
            IEmailService emailService,
            IWalmartOpenApi openApi,
            IItemHistoryService itemHistoryService,
            bool isDebug) : base(log, time, dbFactory, cacheService, barcodeService, emailService, itemHistoryService, isDebug)
        {
            _log = log;
            _time = time;
            _dbFactory = dbFactory;
            _cacheService = cacheService;
            _barcodeService = barcodeService;
            _emailService = emailService;
            _openApi = openApi;
            _isDebug = isDebug;
        }
        
        public override void CreateListings()
        {
            _log.Info("CreateWalmartListings");
            using (var db = _dbFactory.GetRWDb())
            {
                var groupByStyle = from siCache in db.StyleItemCaches.GetAll()
                                   group siCache by siCache.StyleId
                    into byStyle
                                   select new
                                   {
                                       StyleId = byStyle.Key,
                                       OneHasStrongQty = byStyle.Any(s => s.RemainingQuantity > 20), //NOTE: >20
                                       Qty = byStyle.Sum(s => s.RemainingQuantity)
                                   };

                var fromDate = new DateTime(2017, 12, 12);
                var query = from s in db.Styles.GetAll()
                            join sCache in db.StyleCaches.GetAll() on s.Id equals sCache.Id
                            join qty in groupByStyle on s.Id equals qty.StyleId
                            where qty.Qty > 30
                                && qty.OneHasStrongQty
                            //where s.CreateDate >= fromDate
                            orderby qty.Qty descending
                            where !s.Deleted
                            select new
                            {
                                StyleId = s.Id,
                                StyleString = s.StyleID,
                                ParentASIN = sCache.AssociatedASIN,
                                Market = sCache.AssociatedMarket,
                                MarketplaceId = sCache.AssociatedMarketplaceId,
                                Qty = qty.Qty,
                                Name = s.Name,
                                MainLicense = sCache.MainLicense,
                            };



                var styleInfoList = query
                    .Where(p => p.MarketplaceId == MarketplaceKeeper.AmazonComMarketplaceId
                        && !p.Name.Contains("Saras")
                        && !p.Name.Contains("Sara's")
                        && !p.Name.Contains("Widgeon")
                        && p.MainLicense != "Widgeon"
                        && p.MainLicense != "Saras Prints")
                    .ToList();


                var existMarketItems = db.Items.GetAll().Where(i => i.Market == (int)MarketType.Walmart).ToList();

                var newStyleInfoes = styleInfoList.Where(s => existMarketItems.All(i => i.StyleId != s.StyleId)).ToList();

                IList<MessageString> messages;
                foreach (var styleInfo in newStyleInfoes)
                {
                    _log.Info("Creating styleId=" + styleInfo.StyleString + " (" + styleInfo.StyleId + ")" + ", ASIN=" +
                              styleInfo.ParentASIN);
                    if (existMarketItems.Any(i => i.StyleId == styleInfo.StyleId))
                    {
                        _log.Info("Skipped, style already exists");
                        continue;
                    }

                    var model = CreateFromParentASIN(db,
                        styleInfo.ParentASIN,
                        styleInfo.Market.Value,
                        styleInfo.MarketplaceId,
                        false,
                        false,
                        5,
                        out messages);

                    if (model == null)
                    {
                        var item = db.Items.GetAllViewAsDto(MarketType.Walmart, "")
                                .FirstOrDefault(i => i.StyleId == styleInfo.StyleId);
                        if (item != null)
                        {
                            model = CreateFromStyle(db,
                                styleInfo.StyleString,
                                null,
                                MarketType.Walmart,
                                null,
                                out messages);

                            model.Variations.ToList().ForEach(v => v.CurrentPrice = item.CurrentPrice);
                            model.Variations.ToList().ForEach(v => v.Color = item.Color);
                        }
                    }

                    if (model == null || model.Variations == null)
                    {
                        _log.Info("Skipped, variations is NULL");
                        continue;
                    }

                    if (model.Variations.Count == 0)
                    {
                        _log.Info("Skipped, no variations");
                        continue;
                    }

                    if (model.Variations.Count > 10)
                    {
                        _log.Info("Skipped, a lot of variations");
                        continue;
                    }

                    model.Market = (int)MarketType.Walmart;
                    model.MarketplaceId = null;

                    PrepareData(model);

                    var existAnyBarcode = false;
                    foreach (var variant in model.Variations)
                    {
                        var foundItems =((WalmartOpenApi)_openApi).SearchProductsByBarcode(variant.Barcode, WalmartUtils.ApparelCategoryId);
                        if (foundItems.IsSuccess && foundItems.Total > 0)
                        {
                            existAnyBarcode = true;
                        }
                    }

                    if (!existAnyBarcode)
                    {
                        foreach (var variant in model.Variations)
                        {
                            var barcodeInfo = _barcodeService.AssociateBarcodes(variant.SKU, _time.GetAppNowTime(), null);
                            if (barcodeInfo != null)
                            {
                                _log.Info("Set custom barcode for: " + variant.SKU + " :" + variant.Barcode + "=>" + barcodeInfo.Barcode);
                                variant.Barcode = barcodeInfo.Barcode;
                            }
                        }
                    }

                    Save(model, null, db, _time.GetAppNowTime(), null);

                    _emailService.SendSystemEmailToAdmin("Auto-created Walmart listing, ParentASIN: " + model.ASIN, "");

                    //Add to exist list the new items
                    existMarketItems.AddRange(model.Variations.Select(i => new Item()
                    {
                        StyleId = i.StyleId
                    }));
                }
            }
        }
    }
}
