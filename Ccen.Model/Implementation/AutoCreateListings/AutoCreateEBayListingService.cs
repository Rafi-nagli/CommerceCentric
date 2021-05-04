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
    public class AutoCreateEBayListingService : AutoCreateBaseListingService
    {
        private ILogService _log;
        private ITime _time;
        private IDbFactory _dbFactory;

        public AutoCreateEBayListingService(ILogService log,
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

                var shortSleeveFeatures = db.StyleFeatureValues.GetAll().Where(sfv => sfv.FeatureId == StyleFeatureHelper.SLEEVE
                                                                && sfv.FeatureValueId != 14 && sfv.FeatureValueId != 336);

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
                    .Take(500)
                    .ToList();

                var existMarketItems = db.Items.GetAll().Where(i => i.Market == (int)MarketType.eBay).ToList();

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

                    model.Market = (int)MarketType.eBay;
                    model.MarketplaceId = null;

                    PrepareData(model);
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
