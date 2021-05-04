using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Amazon.Common.Helpers;
using Amazon.Core.Contracts;
using Amazon.Core.Contracts.Factories;
using Amazon.Core.Contracts.Notifications;
using Amazon.Core.Models;
using Amazon.Core.Models.SystemActions.SystemActionDatas;
using Amazon.DTO;
using Amazon.Model.Implementation.Markets;
using Amazon.Model.Models;
using Amazon.Web.Models;
using Newtonsoft.Json;

namespace Amazon.Model.Implementation.Notifications.SupportNotifications
{
    public class PriceDisparitySupportNotification : ISupportNotification
    {
        public string Name { get { return "Price Disparity"; } }

        private IDbFactory _dbFactory;
        private IEmailService _emailService;
        private ILogService _log;
        private ITime _time;

        public IList<TimeSpan> When
        {
            get { return new List<TimeSpan>() { TimeSpan.FromHours(9) }; }
        }

        public PriceDisparitySupportNotification(IDbFactory dbFactory,
            IEmailService emailService,
            ILogService log,
            ITime time)
        {
            _time = time;
            _log = log;
            _emailService = emailService;
            _dbFactory = dbFactory;
        }

        public void Check()
        {
            using (var db = _dbFactory.GetRWDb())
            {
                //var marketplaces = db.Marketplaces
                //    .GetAllAsDto()
                //    .Where(m => m.IsActive && !m.IsHidden)
                //    .OrderBy(m => m.SortOrder)
                //    .ThenBy(m => m.Id)
                //    .ToList();


                var html = "<table><tr><th>Market</th><th>Critical Disparity</th><th>Total Disparity</tr>";
                var toCheckMarkets = new List<MarketplaceDTO>()
                {
                    new MarketplaceDTO() { Market = (int)MarketType.Amazon, MarketplaceId = MarketplaceKeeper.AmazonComMarketplaceId },
                    new MarketplaceDTO() { Market = (int)MarketType.Amazon, MarketplaceId = MarketplaceKeeper.AmazonCaMarketplaceId },
                    new MarketplaceDTO() { Market = (int)MarketType.AmazonEU, MarketplaceId = MarketplaceKeeper.AmazonUkMarketplaceId },
                    new MarketplaceDTO() { Market = (int)MarketType.Walmart },
                    new MarketplaceDTO() { Market = (int)MarketType.WalmartCA },
                    new MarketplaceDTO() { Market = (int)MarketType.eBay, MarketplaceId = MarketplaceKeeper.eBayPA },
                    new MarketplaceDTO() { Market = (int)MarketType.eBay, MarketplaceId = MarketplaceKeeper.eBayAll4Kids },                    
                };


                foreach (var marketplace in toCheckMarkets)
                {
                    var checkPriceQuery = db.Items.GetAllViewActual()
                        .Where(l => l.Market == marketplace.Market
                            && l.MarketplaceId == marketplace.MarketplaceId
                            && l.AmazonCurrentPrice.HasValue
                            && (l.SalePrice ?? l.CurrentPrice) != l.AmazonCurrentPrice);

                    var totalPriceDisparity = checkPriceQuery.Count();
                    var criticalPriceDisparity = checkPriceQuery
                        .Where(l => (l.SalePrice ?? l.CurrentPrice) > l.AmazonCurrentPrice)
                        .Count();

                    html += "<tr><td>" + MarketHelper.GetMarketName(marketplace.Market, marketplace.MarketplaceId) + "</td><td>" + criticalPriceDisparity + "</td><td>" + totalPriceDisparity + "</td></tr>";
                }
                html += "</table>";

                foreach (var marketplace in toCheckMarkets)
                {
                    var skuHtml = "<p>" + MarketHelper.GetMarketName(marketplace.Market, marketplace.MarketplaceId) + "</p>";
                    skuHtml += "<table><tr><th>SKU</th><th>CCEN price</th><th>Market price</th></tr>";
                    var skuList = db.Items.GetAllViewActual()
                        .Where(l => l.Market == marketplace.Market
                            && l.MarketplaceId == marketplace.MarketplaceId
                            && l.AmazonCurrentPrice.HasValue
                            && l.IsExistOnAmazon == true
                            && (l.SalePrice ?? l.CurrentPrice) > l.AmazonCurrentPrice)
                        .OrderBy(l => l.SKU)
                        .ThenBy(l => l.Id)
                        .Select(l => new
                        {
                            SKU = l.SKU,
                            CCENPrice = l.SalePrice ?? l.CurrentPrice,
                            MarketPrice = l.AmazonCurrentPrice
                        })
                        .ToList();

                    foreach (var sku in skuList)
                    {
                        skuHtml += "<tr><td>" + sku.SKU + "</td><td>" + sku.CCENPrice + "</td><td>" + sku.MarketPrice + "</td></tr>";
                    }

                    skuHtml += "</table>";

                    html += "<br/>" + skuHtml;


                }
                
                _log.Info("Infoes: " + html);
                _emailService.SendSystemEmail("Price disparity",
                    html,
                    EmailHelper.SupportDgtexEmail + ", " + EmailHelper.IldarDgtexEmail,
                    null);
            }
        }
    }
}
