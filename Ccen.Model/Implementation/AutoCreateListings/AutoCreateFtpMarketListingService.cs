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
    public class AutoCreateFtpMarketListingService : AutoCreateBaseListingService
    {
        private ILogService _log;
        private ITime _time;
        private IDbFactory _dbFactory;
        private ICacheService _cacheService;
        private IBarcodeService _barcodeService;
        private IEmailService _emailService;
        private bool _isDebug;

        public AutoCreateFtpMarketListingService(ILogService log,
            ITime time,
            IDbFactory dbFactory,
            ICacheService cacheService,
            IBarcodeService barcodeService,
            IEmailService emailService,
            IItemHistoryService itemHistoryService,
            bool isDebug) : base(log, time, dbFactory, cacheService, barcodeService, emailService, itemHistoryService, isDebug)
        {
            _log = log;
            _time = time;
            _dbFactory = dbFactory;
            _cacheService = cacheService;
            _barcodeService = barcodeService;
            _emailService = emailService;
            _isDebug = isDebug;
        }
        
        public override void CreateListings()
        {
            _log.Info("CreateWalmartListings");

            var destMarket = MarketType.FtpMarket;
            var destMarketplaceId = "";

            var sourcePriceMarket = MarketType.Amazon;
            var sourcePriceMarketplaceId = MarketplaceKeeper.AmazonComMarketplaceId;

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

                var query = from s in db.Styles.GetAll()
                            join sCache in db.StyleCaches.GetAll() on s.Id equals sCache.Id
                            join qty in groupByStyle on s.Id equals qty.StyleId
                            where qty.Qty > 30
                                && qty.OneHasStrongQty
                            orderby qty.Qty descending
                            where !s.Deleted
                            select new
                            {
                                StyleId = s.Id,
                                StyleString = s.StyleID,
                                Qty = qty.Qty,
                                Name = s.Name,
                                MainLicense = sCache.MainLicense,
                            };
                
                var styleInfoList = query
                    .Where(p => !p.Name.Contains("Saras")
                        && !p.Name.Contains("Sara's")
                        && !p.Name.Contains("Widgeon")
                        && p.MainLicense != "Widgeon"
                        && p.MainLicense != "Saras Prints"
                        && !p.Name.Contains("Santa")
                        && !p.Name.Contains("Claus")
                        && !p.Name.Contains("Christmas")
                        && !p.Name.Contains("Rudolph")
                        && !p.Name.Contains("Dr. Seuss")
                        && !p.Name.Contains("Grinch"))
                    .ToList();

                var sourcePriceInfoes = (from i in db.Items.GetAll()
                                       join l in db.Listings.GetAll() on i.Id equals l.ItemId
                                       where i.StyleItemId.HasValue
                                            && i.StyleId.HasValue
                                            && i.Market == (int)sourcePriceMarket
                                            && i.MarketplaceId == sourcePriceMarketplaceId
                                       group new { i, l } by i.StyleItemId.Value into byStyleItem
                                       select new
                                       {
                                           StyleItemId = byStyleItem.Key,
                                           StyleId = byStyleItem.Max(s => s.i.StyleId.Value),
                                           Price = byStyleItem.Min(l => l.l.CurrentPrice)
                                       }).ToList();

                var existMarketItems = db.Items.GetAll().Where(i => i.Market == (int)MarketType.FtpMarket).ToList();

                var newStyleInfoes = styleInfoList.Where(s => existMarketItems.All(i => i.StyleId != s.StyleId)).ToList();

                newStyleInfoes = newStyleInfoes.Take(1000).ToList();

                IList<MessageString> messages;
                foreach (var styleInfo in newStyleInfoes)
                {
                    _log.Info("Creating styleId=" + styleInfo.StyleString + " (" + styleInfo.StyleId + ")");
                    if (existMarketItems.Any(i => i.StyleId == styleInfo.StyleId))
                    {
                        _log.Info("Skipped, style already exists");
                        continue;
                    }

                    if (!sourcePriceInfoes.Any(i => i.StyleId == styleInfo.StyleId))
                    {
                        _log.Info("Skipped, no source price info");
                        continue;
                    }

                    var model = CreateFromStyle(db,
                        styleInfo.StyleId,
                        destMarket,
                        destMarketplaceId,
                        out messages);

                    if (model == null || model.Variations == null)
                    {
                        _log.Info("Skipped, model/variations is NULL");
                        continue;
                    }

                    if (model.Variations.Count == 0)
                    {
                        _log.Info("Skipped, no variations");
                        continue;
                    }
                    
                    model.Market = (int)destMarket;
                    model.MarketplaceId = destMarketplaceId;

                    PrepareData(model);

                    foreach (var variant in model.Variations)
                    {
                        var price = sourcePriceInfoes.FirstOrDefault(p => p.StyleItemId == variant.StyleItemId)?.Price;
                        if (price == null || price == 0)
                        {
                            price = sourcePriceInfoes.Where(p => p.StyleId == variant.StyleId).Max(p => p.Price);
                        }
                        variant.CurrentPrice = (price ?? 0) + (variant.Weight <= 16 ? 5M : 7.5M);

                        var barcodeInfo = _barcodeService.AssociateBarcodes(variant.SKU, _time.GetAppNowTime(), null);
                        if (barcodeInfo != null)
                        {                            
                            variant.Barcode = barcodeInfo.Barcode;
                        }
                    }

                    model.Variations = model.Variations.Where(v => v.CurrentPrice > 0).ToList();

                    Save(model, null, db, _time.GetAppNowTime(), null);

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
