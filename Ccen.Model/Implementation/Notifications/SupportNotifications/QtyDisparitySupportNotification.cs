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
    public class QtyDisparitySupportNotification : ISupportNotification
    {
        public string Name { get { return "Quantity Disparity"; } }

        private IDbFactory _dbFactory;
        private IEmailService _emailService;
        private ILogService _log;
        private ITime _time;

        public IList<TimeSpan> When
        {
            get { return new List<TimeSpan>() { TimeSpan.FromHours(9) }; }
        }

        public QtyDisparitySupportNotification(IDbFactory dbFactory,
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


                var html = "<table><tr><th>Market</th><th>Critical Disparity</th><th>Total Disparity</th></tr>";
                var toCheckMarkets = new List<MarketplaceDTO>()
                {
                    new MarketplaceDTO() { Market = (int)MarketType.Amazon, MarketplaceId = MarketplaceKeeper.AmazonComMarketplaceId },
                    new MarketplaceDTO() { Market = (int)MarketType.Walmart },
                    new MarketplaceDTO() { Market = (int)MarketType.WalmartCA },
                    new MarketplaceDTO() { Market = (int)MarketType.eBay, MarketplaceId = MarketplaceKeeper.eBayPA },
                    new MarketplaceDTO() { Market = (int)MarketType.eBay, MarketplaceId = MarketplaceKeeper.eBayAll4Kids },                    
                };


                foreach (var marketplace in toCheckMarkets)
                {
                    var checkQtyQuery = db.Items.GetAllViewActual()
                        .Where(l => l.Market == marketplace.Market
                            && l.MarketplaceId == marketplace.MarketplaceId
                            && l.AmazonRealQuantity.HasValue
                            && l.RealQuantity != l.AmazonRealQuantity);

                    if (marketplace.Market == (int)MarketType.Amazon)
                    {
                        checkQtyQuery = checkQtyQuery.Where(i => !i.IsFBA
                            && !(i.RealQuantity == 30 && i.AmazonRealQuantity == 101));
                    }
                    if (marketplace.Market == (int)MarketType.Walmart)
                    {
                        checkQtyQuery = checkQtyQuery.Where(i => i.ItemPublishedStatus != (int)PublishedStatuses.PublishedInactive
                            && i.ItemPublishedStatus != (int)PublishedStatuses.Unpublished);
                    }

                    var totalQtyDisparity = checkQtyQuery.Count();
                    var criticalQtyDisparity = checkQtyQuery
                        .Where(l => l.RealQuantity == 0)
                        .Count();

                    html += "<tr><td>" + MarketHelper.GetMarketName(marketplace.Market, marketplace.MarketplaceId) + "</td><td>" + criticalQtyDisparity + "</td><td>" + totalQtyDisparity + "</td></tr>";
                }
                html += "</table>";

                foreach (var marketplace in toCheckMarkets)
                {
                    var skuHtml = "<p>" + MarketHelper.GetMarketName(marketplace.Market, marketplace.MarketplaceId) + "</p>";
                    skuHtml += "<table><tr><th>SKU</th><th>CCEN qty</th><th>Market qty</th></tr>";
                    var skuList = db.Listings.GetAll()
                        .Where(l => l.Market == marketplace.Market
                            && l.MarketplaceId == marketplace.MarketplaceId
                            && !l.IsRemoved
                            && l.AmazonRealQuantity.HasValue
                            && l.RealQuantity != l.AmazonRealQuantity
                            && l.RealQuantity == 0)
                        .OrderBy(l => l.SKU)
                        .ThenBy(l => l.Id)
                        .Select(l => new
                        {
                            SKU = l.SKU,
                            CCENQty = l.RealQuantity,
                            MarketQty = l.AmazonRealQuantity
                        })
                        .ToList();

                    foreach (var sku in skuList)
                    {
                        skuHtml += "<tr><td>" + sku.SKU + "</td><td>" + sku.CCENQty + "</td><td>" + sku.MarketQty + "</td></tr>";
                    }
                    skuHtml += "</table>";

                    html += "<br/>" + skuHtml;
                }
                
                _log.Info("Infoes: " + html);
                _emailService.SendSystemEmail("Quantity disparity",
                    html,
                    EmailHelper.SupportDgtexEmail + ", " + EmailHelper.IldarDgtexEmail,
                    null);
            }
        }
    }
}
