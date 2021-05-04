using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Amazon.Common.Helpers;
using Amazon.Core;
using Amazon.Core.Contracts;
using Amazon.Core.Contracts.Factories;
using Amazon.Core.Models;
using Amazon.Model.Implementation.Charts;
using Amazon.Web.Models;
using Amazon.Web.ViewModels.Companies;

namespace Amazon.Web.ViewModels
{
    public class SyncStatusViewModel
    {
        private ISettingsService _settings;

        public SyncStatusViewModel(ISettingsService settingService, IDbFactory dbFactory)
        {
            _settings = settingService;
            _settings.Init();

            _marketplaces = new List<MarketplaceViewModel>();
            foreach (var info in _settings.MarketSettings)
            {
                if (info.MarketplaceId == MarketplaceKeeper.AmazonDeMarketplaceId
                    || info.MarketplaceId == MarketplaceKeeper.AmazonEsMarketplaceId
                    || info.MarketplaceId == MarketplaceKeeper.AmazonFrMarketplaceId
                    || info.MarketplaceId == MarketplaceKeeper.AmazonItMarketplaceId

                    || info.Market == MarketType.Magento
                    || info.Market == MarketType.Shopify)
                    continue;

                _marketplaces.Add(new MarketplaceViewModel(info, _settings));
            }

            _listingErrors = new List<ListingErrorViewModel>();

            using (var db = dbFactory.GetRWDb())
            {
                foreach (var info in _settings.MarketSettings)
                {
                    if (info.MarketplaceId == MarketplaceKeeper.AmazonComMarketplaceId
                        || (info.Market == MarketType.eBay && info.MarketplaceId == MarketplaceKeeper.eBayPA)
                        || info.Market == MarketType.Walmart)
                    {
                        var marketTag = (int)info.Market + "_" + info.MarketplaceId;
                        var chartId = db.Charts.GetAll().FirstOrDefault(c => c.ChartTag == marketTag
                            && c.ChartName == ChartHelper.ListingErrorChartName)?.Id;

                        if (chartId.HasValue)
                        {
                            var count = db.ChartPoints.GetAllAsDto()
                                .OrderByDescending(p => p.Date)
                                .FirstOrDefault(p => p.ChartId == chartId)?.Value;
                            _listingErrors.Add(new ListingErrorViewModel()
                            {
                                Count = (int)Math.Round(count ?? 0M),
                                Market = (int)info.Market,
                                MarketplaceId = info.MarketplaceId
                            });
                        }
                    }
                }
            }
        }

        private IList<MarketplaceViewModel> _marketplaces;
        public IList<MarketplaceViewModel> Marketplaces
        {
            get { return _marketplaces; }
        }
        
        private IList<ListingErrorViewModel> _listingErrors;
        public IList<ListingErrorViewModel> ListingErrors
        {
            get { return _listingErrors; }
        }
    }
}