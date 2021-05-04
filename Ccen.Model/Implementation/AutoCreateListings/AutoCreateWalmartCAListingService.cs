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
using Amazon.DTO.Orders;
using Walmart.Api;

namespace Amazon.Model.Implementation
{
    public class AutoCreateWalmartCAListingService : AutoCreateBaseListingService
    {
        private ILogService _log;
        private ITime _time;
        private IDbFactory _dbFactory;
        private ICacheService _cacheService;
        private IBarcodeService _barcodeService;
        private IEmailService _emailService;
        private bool _isDebug;

        public AutoCreateWalmartCAListingService(ILogService log,
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
            _log.Info("CreateWalmartCAListings");
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
                            where qty.Qty > 50
                                && qty.OneHasStrongQty
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
                    .Where(p => p.Market != (int)MarketType.None //Is online
                        && !p.Name.Contains("Saras")
                        && !p.Name.Contains("Sara's")
                        && !p.Name.Contains("Widgeon")
                        && p.MainLicense != "Widgeon"
                        && p.MainLicense != "Saras Prints")
                    .ToList();

                var sourceMarket = MarketType.Walmart;
                string sourceMakretplaceId = null;

                var existMarketItems = db.Items.GetAll().Where(i => i.Market == (int)MarketType.WalmartCA).ToList();
                var existStyleIdList = existMarketItems.Select(i => i.StyleId).ToList();
                var existParentASINs = db.ParentItems.GetAll()
                    .Where(i => i.Market == (int)MarketType.WalmartCA)
                    .Select(i => i.ASIN)
                    .ToList();

                //var newStyleInfoes = styleInfoList.Where(s => existMarketItems.All(i => i.StyleId != s.StyleId)).ToList();

                IList<MessageString> messages;

                var childItemCountQuery = from ch in db.Items.GetAll()
                                          where ch.Market == (int)sourceMarket
                                          group ch by ch.ParentASIN
                    into byParentASIN
                                          select new
                                          {
                                              ParentASIN = byParentASIN.Key,
                                              ChildCount = byParentASIN.Count()
                                          };

                var sourceParentItems = (from pi in db.ParentItems.GetAll()
                                         join chQty in childItemCountQuery on pi.ASIN equals chQty.ParentASIN
                                         where pi.Market == (int)sourceMarket
                                         orderby chQty.ChildCount descending, pi.CreateDate ascending
                                         select pi).ToList();
                var sourceItems = db.Items.GetAll().Where(i => i.Market == (int)sourceMarket).ToList();

                foreach (var sourceParenItem in sourceParentItems)
                {
                    _log.Info("Creating, Parent ASIN=" + sourceParenItem.ASIN);
                    var sourceStyleIdList = sourceItems.Where(i => i.ParentASIN == sourceParenItem.ASIN)
                        .Select(i => i.StyleId)
                        .ToList();

                    if (existParentASINs.Any(pi => pi == sourceParenItem.ASIN))
                    {
                        _log.Info("Skipped, ParentASIN already exists");
                        continue;
                    }

                    //If not multilistings, check styleId for existing
                    if (sourceStyleIdList.Distinct().Count() == 1)
                    {
                        var styleId = sourceStyleIdList.First();
                        if (existStyleIdList.Contains(styleId))
                        {
                            _log.Info("Skipped, Not multilisting, styleId already exists");
                            continue;
                        }
                    }

                    var model = CreateFromParentASIN(db,
                        sourceParenItem.ASIN,
                        (int)sourceMarket,
                        sourceMakretplaceId,
                        false,
                        false,
                        null,
                        out messages);
                    
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

                    model.ASIN = sourceParenItem.ASIN;

                    //if (model.Variations.Count > 10)
                    //{
                    //    _log.Info("Skipped, a lot of variations");
                    //    continue;
                    //}

                    var resultStyleIdsList = model.Variations.Select(v => v.StyleId).Distinct().ToList();
                    if (resultStyleIdsList.Count() == 1)
                    {
                        var styleId = resultStyleIdsList.First();
                        if (existStyleIdList.Contains(styleId))
                        {
                            _log.Info("Skipped, After filter by qty, not multilisting, styleId already exists");
                            continue;
                        }
                    }
                    else
                    {
                        var existStyleCount = resultStyleIdsList.Count(rs => existStyleIdList.Contains(rs));
                        if (existStyleCount > resultStyleIdsList.Count() / (decimal)2)
                        {
                            _log.Info("Skipped, After filter by qty, multilitsting has >1/2 styleIds already exists");
                            continue;
                        }
                    }

                    model.Market = (int)MarketType.WalmartCA;
                    model.MarketplaceId = null;

                    PrepareData(model);
                    PreparePrice(model);

                    Save(model, null, db, _time.GetAppNowTime(), null);

                    //if (!_isDebug)
                    //    _emailService.SendSystemEmailToAdmin("Auto-created Walmart listing, ParentASIN: " + model.ASIN, "");

                    //Add to exist list the new items
                    existMarketItems.AddRange(model.Variations.Select(i => new Item()
                    {
                        StyleId = i.StyleId
                    }));

                    existStyleIdList.AddRange(model.Variations.Select(i => i.StyleId).ToList());
                }
            }
        }

        protected void PreparePrice(ParentItemDTO parentItemDto)
        {
            var priceService = new PriceService(_dbFactory);
            IList<RateByCountryDTO> allRates = new List<RateByCountryDTO>();
            using (var db = _dbFactory.GetRWDb())
            {
                foreach (var item in parentItemDto.Variations)
                {
                    var rateForMarketplace = RateHelper.GetRatesByStyleItemId(db, item.StyleItemId.Value);

                    var newPrice = priceService.GetMarketPrice(item.CurrentPrice,
                        null,
                        item.IsPrime,
                        item.IsFBA,
                        item.Weight,
                        (MarketType)item.Market,
                        item.MarketplaceId,
                        rateForMarketplace);

                    _log.Info("Price changed, SKU=" + item.SKU + ": " + item.CurrentPrice + "=>" + newPrice);
                    item.CurrentPrice = newPrice;
                }
            }
        }
    }
}
