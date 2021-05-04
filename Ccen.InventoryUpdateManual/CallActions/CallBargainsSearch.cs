using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Amazon.Api;
using Amazon.Api.Exports;
using Amazon.Common.ExcelExport;
using Amazon.Common.Helpers;
using Amazon.Core.Contracts;
using Amazon.Core.Contracts.Factories;
using Amazon.Core.Models;
using Amazon.Core.Models.Bargains;
using Amazon.DTO;
using Amazon.Model.Implementation;
using Walmart.Api;
using Walmart.Api.Core.Models.OpenApi;


namespace Amazon.InventoryUpdateManual.CallActions
{
    public class CallBargainsSearch
    {
        private IDbFactory _dbFactory;
        private ILogService _log;
        private ITime _time;
        private AmazonApi _amazonApi;


        public CallBargainsSearch(IDbFactory dbFactory,
            ILogService log,
            ITime time,
            AmazonApi amazonApi)
        {
            _log = log;
            _dbFactory = dbFactory;
            _time = time;
            _amazonApi = amazonApi;
        }

        public class StylePositionOnWM
        {
            public long StyleId { get; set; }
            public string StyleString { get; set; }
            public string SearchKeywords { get; set; }
            public int RequestedSearchResults { get; set; }
            public int? BestPosition { get; set; }
            public string BuyBoxWinner { get; set; }
            public string BestPositionUrl { get; set; }
        }

        public void FindListingsPositionInSearch()
        {
            var results = new List<StylePositionOnWM>();

            using (var db = _dbFactory.GetRWDb())
            {
                var openApi = new WalmartOpenApi(_log, "trn9fdghvb8p9gjj9j6bvjwx");

                var wmItems = db.Items.GetAll().Where(i => i.ItemPublishedStatus == (int) PublishedStatuses.Published
                                                              && i.Market == (int) MarketType.Walmart).ToList();

                var styleIdList = wmItems.Where(i => i.StyleId.HasValue).Select(i => i.StyleId.Value).ToList();
                //var styleList = db.Styles.GetAll().Where(st => styleIdList.Contains(st.Id)).ToList();

                var groupByStyle = from siCache in db.StyleItemCaches.GetAll()
                                   group siCache by siCache.StyleId
                    into byStyle
                                   select new
                                   {
                                       StyleId = byStyle.Key,
                                       OneHasStrongQty = byStyle.Any(s => s.RemainingQuantity > 20), //NOTE: >20
                                       Qty = byStyle.Sum(s => s.RemainingQuantity)
                                   };

                var styleList = (from s in db.Styles.GetAll()
                    join sCache in db.StyleCaches.GetAll() on s.Id equals sCache.Id
                    join qty in groupByStyle on s.Id equals qty.StyleId
                    where styleIdList.Contains(s.Id)
                    orderby qty.Qty descending
                    select s)
                    .Take(100)
                    .ToList();

                var mainLicenseFeatures = db.StyleFeatureValues.GetFeatureValueByStyleIdByFeatureId(styleIdList, new[] { StyleFeatureHelper.MAIN_LICENSE }).ToList();
                var subLicenseFeatures = db.StyleFeatureValues.GetFeatureValueByStyleIdByFeatureId(styleIdList, new[] { StyleFeatureHelper.SUB_LICENSE1 }).ToList();
                var itemStyleFeatures = db.StyleFeatureValues.GetFeatureValueByStyleIdByFeatureId(styleIdList, new[] { StyleFeatureHelper.ITEMSTYLE }).ToList();

                var resultsCache = new Dictionary<string, IList<OpenItem>>();

                foreach (var style in styleList)
                {
                    var mainLicenseFeature = mainLicenseFeatures.FirstOrDefault(f => f.StyleId == style.Id);
                    var subLicenseFeature = subLicenseFeatures.FirstOrDefault(f => f.StyleId == style.Id);
                    var itemStyleFeature = itemStyleFeatures.FirstOrDefault(f => f.StyleId == style.Id);

                    var brandName = ItemExportHelper.GetBrandName(mainLicenseFeature?.Value, subLicenseFeature?.Value);
                    var manufacture = brandName;
                    if (String.IsNullOrEmpty(manufacture))
                        manufacture = style.Manufacturer;
                    
                    var itemStyle = itemStyleFeature?.Value ?? "Pajamas";
                    itemStyle = itemStyle.Replace("– 2pc", "").Replace("– 3pc", "").Replace("– 4pc", "").Trim();

                    var keywords = StringHelper.JoinTwo(" ", manufacture, itemStyle);

                    _log.Info("Searching: " + keywords);
                    IList<OpenItem> foundItems = new List<OpenItem>();
                    if (resultsCache.ContainsKey(keywords))
                    {
                        foundItems = resultsCache[keywords];
                    }
                    else
                    {
                        var searchResult = openApi.SearchProducts(keywords,
                            WalmartUtils.ApparelCategoryId,
                            null,
                            null,
                            1,
                            100);
                        if (searchResult.IsSuccess)
                        {
                            foundItems = searchResult.Data;
                            resultsCache.Add(keywords, searchResult.Data);
                        }
                    }

                    int? position = null;
                    string positionItemId = null;
                    string buyBoxWinner = null;
                    var itemsByStyle = wmItems.Where(i => i.StyleId == style.Id).ToList();
                    var itemIds = itemsByStyle.Where(i => !String.IsNullOrEmpty(i.SourceMarketId)).Select(i => i.SourceMarketId).ToList();
                    for (int i = 0; i < foundItems.Count; i++)
                    {
                        if (itemIds.Contains(foundItems[i].ItemId)
                            && position == null)
                        {
                            position = i + 1;
                            positionItemId = foundItems[i].ItemId;
                            buyBoxWinner = foundItems[i].SellerInfo;
                        }
                    }

                    _log.Info(style.StyleID + ", position: " + position + ", " + keywords + ", " + buyBoxWinner + ", " + positionItemId);

                    results.Add(new StylePositionOnWM()
                    {
                        StyleId = style.Id,
                        StyleString = style.StyleID,
                        SearchKeywords = keywords,
                        RequestedSearchResults = foundItems.Count,
                        BestPosition = position,
                        BuyBoxWinner = buyBoxWinner,
                        BestPositionUrl = !String.IsNullOrEmpty(positionItemId) ? String.Format("https://www.walmart.com/ip/item/{0}", positionItemId) : ""
                    });

                }
            }

            var filename = "WMListingPositions_" + DateTime.Now.ToString("MM_dd_yyyy_hh_mm_ss") + ".xls";
            var filepath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, filename);

            var b = new ExportColumnBuilder<StylePositionOnWM>();
            var columns = new List<ExcelColumnInfo>()
            {
                b.Build(p => p.StyleString, "Style Id", 25),
                b.Build(p => p.SearchKeywords, "Search Keywords", 25),
                b.Build(p => p.RequestedSearchResults, "Requested search results", 35),
                b.Build(p => p.BestPosition, "Best Position", 15),
                b.Build(p => p.BuyBoxWinner, "Buy Box Winner", 20),
                b.Build(p => p.BestPositionUrl, "Best Position Url", 45)
            };

            using (var stream = ExcelHelper.Export(results, columns))
            {
                stream.Seek(0, SeekOrigin.Begin);
                using (var fileStream = File.Create(filepath))
                {
                    stream.CopyTo(fileStream);
                }
            }
        }

        public void CallBargainSearch()
        {
            var service = new BargainSearchService(_dbFactory, _log, _time);
            var result = service.Search(new BargainSearchFilter()
                {
                    Keywords = "pajama",
                    CategoryId = "5438", 
                    MinPrice = 0,
                    MaxPrice = 5,
                    StartIndex = 1,
                    LimitCount = 500,
                },
                _amazonApi);
            
            var filename = "BargainsSearch_" + DateTime.Now.ToString("MM_dd_yyyy_hh_mm_ss") + ".xls";
            var filepath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, filename);
            service.ExportBargains(result.Bargains, filepath);
        }

        public void SearchByBarcode(string barcode)
        {
            var openApi = new WalmartOpenApi(_log, "trn9fdghvb8p9gjj9j6bvjwx");
            var products = openApi.SearchProductsByBarcode(barcode, WalmartUtils.ApparelCategoryId);
            _log.Info(products.ToString());
        }
    }
}
