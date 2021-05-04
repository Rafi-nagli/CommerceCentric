using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Amazon.Api;
using Amazon.Common.Helpers;
using Amazon.Core;
using Amazon.Core.Contracts;
using Amazon.Core.Contracts.Factories;
using Amazon.Core.Models;
using Amazon.Core.Models.Bargains;
using Amazon.Core.Models.Calls;
using Amazon.DAL;
using Amazon.Model.Implementation;
using Amazon.Web.Models;

namespace Amazon.Web.ViewModels.Bargains
{
    public class BargainViewModel
    {
        public string Barcode { get; set; }
        public string Size { get; set; }
        public string Color { get; set; }

        public string Name { get; set; }
        public string WalmartImage { get; set; }

        public string WalmartThumbnail
        {
            get { return WalmartImage; }
        }

        public bool AmazonAvailable { get; set; }
        public bool AmazonInStock { get; set; }
        public string ASIN { get; set; }
        public string AmazonUrl
        {
            get { return !String.IsNullOrEmpty(ASIN) ? "https://www.amazon.com/dp/" + ASIN : null; }
        }

        public decimal? AmazonPrice { get; set; }
        
        public bool WalmartAvailable { get; set; }
        public bool WalmartInStock { get; set; }
        public string WalmartItemId { get; set; }

        public string WalmartUrl
        {
            get { return !String.IsNullOrEmpty(WalmartItemId) ? "https://www.walmart.com/ip/" + WalmartItemId : null; }
        }

        public decimal? WalmartPrice { get; set; }

        public BargainViewModel()
        {
            
        }

        public BargainViewModel(BargainItem item)
        {
            Barcode = item.Barcode;
            Size = StringHelper.GetFirstNotEmpty(item.WalmartItem?.Size, item.AmazonItem?.Size);
            Color = StringHelper.GetFirstNotEmpty(item.WalmartItem?.Color, item.AmazonItem?.Color);

            Name = item.Name;
            WalmartImage = item.WalmartImage;

            AmazonAvailable = item.AvailableOnAmazon;
            AmazonInStock = item.AmazonItem?.AmazonRealQuantity > 0;
            WalmartAvailable = item.AvailableOnWalmart;
            WalmartInStock = item.WalmartItem?.AmazonRealQuantity > 0;

            ASIN = item.AmazonItem != null ? item.AmazonItem.ASIN : null;
            WalmartItemId = item.WalmartItem != null ? item.WalmartItem.SourceMarketId : null;

            AmazonPrice = item.AmazonPrice;
            WalmartPrice = item.WalmartPrice;
        }


        public static CallResult<string> Export(IDbFactory dbFactory,
            ILogService log,
            ITime time,
            long companyId,
            BargainSearchFilterViewModel filter)
        {
            var service = new BargainSearchService(dbFactory, log, time);

            var marketplaceManager = new MarketplaceKeeper(dbFactory, false);
            marketplaceManager.Init();

            AmazonApi api = (AmazonApi)new MarketFactory(marketplaceManager.GetAll(), time, log, dbFactory, null)
                .GetApi(companyId, MarketType.Amazon, MarketplaceKeeper.AmazonComMarketplaceId);

            var result = service.Search(filter.GetModel(), api);

            var fileName = "BargainsSearch_" + DateTime.Now.ToString("MM_dd_yyyy_hh_mm_ss") + ".xls";
            var filePath = UrlHelper.GetBargainExportFilePath(fileName);

            service.ExportBargains(result.Bargains.Where(bi => bi.AvailableOnWalmart).ToList(), filePath);

            var fileUrl = UrlHelper.GetBargainExportUrl(fileName);
            return CallResult<string>.Success(fileUrl);
        }

        public static BargainSearchResultViewModel GetAll(IDbFactory dbFactory,
            ILogService log,
            ITime time,
            long companyId,
            BargainSearchFilterViewModel filter)
        {
            var service = new BargainSearchService(dbFactory, log, time);

            var marketplaceManager = new MarketplaceKeeper(dbFactory, false);
            marketplaceManager.Init();

            AmazonApi api = (AmazonApi)new MarketFactory(marketplaceManager.GetAll(), time, log, dbFactory, null)
                .GetApi(companyId, MarketType.Amazon, MarketplaceKeeper.AmazonComMarketplaceId);

            var result = service.Search(filter.GetModel(), api);

            return new BargainSearchResultViewModel()
            {
                TotalResults = result.Total,
                Bargains = result.Bargains.Select(b => new BargainViewModel(b)).ToList()
            };
        }
    }
}