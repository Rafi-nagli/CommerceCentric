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
using Amazon.Core.Models.Stamps;
using Amazon.Core.Models.SystemActions;
using Amazon.DTO;
using Amazon.DTO.Inventory;
using Amazon.DTO.Orders;
using Walmart.Api;

namespace Amazon.Model.Implementation
{
    public class AutoCreateAmazonAUListingService : AutoCreateBaseListingService
    {
        private ILogService _log;
        private ITime _time;
        private IDbFactory _dbFactory;
        private ISystemActionService _actionService;

        public AutoCreateAmazonAUListingService(ILogService log,
            ITime time,
            IDbFactory dbFactory,
            ICacheService cacheService,
            IBarcodeService barcodeService,
            IEmailService emailService,
            ISystemActionService actionService,
            IItemHistoryService itemHistoryService,
            bool isDebug) : base(log, time, dbFactory, cacheService, barcodeService, emailService, itemHistoryService, isDebug)
        {
            _log = log;
            _time = time;
            _dbFactory = dbFactory;
            _actionService = actionService;
        }


        public override void CreateListings()
        {
            using (var db = _dbFactory.GetRWDb())
            {
                var groupByStyle = from siCache in db.StyleItemCaches.GetAll()
                                   group siCache by siCache.StyleId
                                   into byStyle
                                   select new
                                   {
                                       StyleId = byStyle.Key,
                                       Qty = byStyle.Sum(s => s.RemainingQuantity)
                                   };

                var query = from s in db.Styles.GetAll()
                            join sCache in db.StyleCaches.GetAll() on s.Id equals sCache.Id
                            //Not long sleeve
                            //join sleeve in shortSleeveFeatures on s.Id equals sleeve.StyleId
                            join qty in groupByStyle on s.Id equals qty.StyleId
                            where qty.Qty > 50
                            orderby qty.Qty descending
                            where !s.Deleted
                            select new
                            {
                                StyleId = s.Id,
                                StyleString = s.StyleID,
                                ParentASIN = sCache.AssociatedASIN,
                                Market = sCache.AssociatedMarket,
                                MarketplaceId = sCache.AssociatedMarketplaceId,
                                Qty = qty.Qty
                            };

                var styleInfoList = query
                    .Where(p => p.MarketplaceId == MarketplaceKeeper.AmazonComMarketplaceId)
                    .Take(150)
                    .ToList();

                var existMarketItems = db.Items.GetAll().Where(i => i.Market == (int)MarketType.AmazonAU).ToList();

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
                        0,
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

                    if (model.Variations.Select(v => v.StyleId).Distinct().Count() > 1)
                    {
                        _log.Info("Skipped multi-color variation");
                        continue;
                    }

                    if (model.Variations.Count > 12)
                    {
                        _log.Info("Skipped, a lot of variations");
                        continue;
                    }

                    model.Market = (int)MarketType.AmazonAU;
                    model.MarketplaceId = MarketplaceKeeper.AmazonAuMarketplaceId;

                    PrepareData(model);

                    PreparePrices(model);

                    Save(model, null, db, _time.GetAppNowTime(), null);

                    AddActions(db, model);

                    //Add to exist list the new items
                    existMarketItems.AddRange(model.Variations.Select(i => new Item()
                    {
                        StyleId = i.StyleId
                    }));
                }
            }
        }

        private void AddActions(IUnitOfWork db, ParentItemDTO parentItemDto)
        {
            foreach (var item in parentItemDto.Variations)
            {
                var actions = new List<SystemActionType>()
                {
                    SystemActionType.UpdateOnMarketProductRelationship,
                    SystemActionType.UpdateOnMarketProductImage,
                };

                foreach (var actionType in actions)
                {
                    var newAction = new SystemActionDTO()
                    {
                        ParentId = null,
                        Status = (int) SystemActionStatus.None,
                        Type = (int) actionType,
                        Tag = item.Id.ToString(),
                        InputData = null,

                        CreateDate = _time.GetUtcTime(),
                        CreatedBy = null,
                    };
                    db.SystemActions.AddAction(newAction);
                }
            }
            db.Commit();
        }

        private void PreparePrices(ParentItemDTO parentItemDto)
        {
            IList<RateByCountryDTO> allRates = new List<RateByCountryDTO>();
            using (var db = _dbFactory.GetRWDb())
            {
                allRates = db.RateByCountries.GetAllAsDto().ToList();
                foreach (var item in parentItemDto.Variations)
                {
                    double? weight = item.Weight;
                    var shippingSize = item.StyleId.HasValue ? db.StyleCaches.GetAll().FirstOrDefault(sc => sc.Id == item.StyleId.Value)?.ShippingSizeValue : null;
                    item.CurrentPriceCurrency = PriceHelper.GetCurrencyAbbr((MarketType)item.Market, item.MarketplaceId);
                    var newPrice = CalculatePrice(weight,
                        shippingSize,
                        item.CurrentPrice,
                        allRates,
                        (MarketType)item.Market,
                        item.MarketplaceId);
                    _log.Info("Price changed, SKU=" + item.SKU + ": " + item.CurrentPrice + "=>" + newPrice);
                    if (newPrice.HasValue)
                        item.CurrentPrice = newPrice.Value;
                    else
                        _log.Error("Convert price issue: " + item.SKU);
                }
            }
        }

        private decimal? CalculatePrice(double? weight, 
            string shippingSize,
            decimal usListingPrice,
            IList<RateByCountryDTO> allRates,
            MarketType forMarket,
            string forMarketplaceId)
        {
            var usShipping = RateService.GetMarketShippingAmount(MarketType.Amazon,
                MarketplaceKeeper.AmazonComMarketplaceId); // 4.49M;
            var currency = PriceHelper.GetCurrencyAbbr(forMarket, forMarketplaceId);
            var caShipping = PriceHelper.ConvertToUSD(RateService.GetMarketShippingAmount(forMarket, forMarketplaceId), currency);
            
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
                return null;

            var usIncome = usListingPrice + usShipping - usRateActualCost.Value;
            var newPrice = usIncome - (caShipping - caRateActualCostRegular.Value);
            newPrice = PriceHelper.ConvertFromUSD(newPrice, currency);

            return newPrice;
        }
    }
}
