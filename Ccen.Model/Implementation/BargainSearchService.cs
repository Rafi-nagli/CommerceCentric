using Amazon.Core.Contracts;
using Amazon.Core.Contracts.Factories;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Amazon.Api;
using Amazon.Common.ExcelExport;
using Amazon.Common.Helpers;
using Amazon.Core.Models.Bargains;
using Amazon.DTO;
using Walmart.Api;

namespace Amazon.Model.Implementation
{
    public class BargainSearchService
    {
        private IDbFactory _dbFactory;
        private ILogService _log;
        private ITime _time;

        public BargainSearchService(IDbFactory dbFactory,
            ILogService log,
            ITime time)
        {
            _dbFactory = dbFactory;
            _log = log;
            _time = time;
        }

        public BargainSearchResult Search(BargainSearchFilter filter,
            AmazonApi api)
        {
            //"id": "5438",
            //"name": "Apparel",

            var openApi = new WalmartOpenApi(_log, "trn9fdghvb8p9gjj9j6bvjwx");
            var searchResult = openApi.SearchProducts(filter.Keywords, 
                filter.CategoryId, 
                filter.MinPrice, 
                filter.MaxPrice, 
                filter.StartIndex, 
                filter.LimitCount);

            var results = new List<BargainItem>();

            if (searchResult.IsSuccess)
            {
                var walmartItems = searchResult.Data.Where(i => //i.AvailableOnline && 
                        !String.IsNullOrEmpty(i.UPC)).ToList();
                var index = 0;
                var step = 5;
                while (index < walmartItems.Count)
                {
                    var stepWalmartItems = walmartItems.Skip(index).Take(step).ToList();
                    var amazonItems = api.GetProductForBarcode(stepWalmartItems.Select(i => i.UPC).ToList());

                    var newItems = new List<BargainItem>();
                    foreach (var walmartItem in stepWalmartItems)
                    {
                        var amazonItem = amazonItems.FirstOrDefault(i => i.Barcode == walmartItem.UPC);
                        newItems.Add(new BargainItem()
                        {
                            Barcode = walmartItem.UPC,

                            Name = walmartItem.Name,
                            WalmartImage = walmartItem.ThumbnailImage,

                            AmazonItem = amazonItem,
                            AmazonPrice = amazonItem != null ? amazonItem.CurrentPrice : (decimal?)null,
                            AvailableOnAmazon = amazonItem != null ? amazonItem.AmazonRealQuantity > 0 : false,

                            WalmartPrice = walmartItem.SalePrice,
                            AvailableOnWalmart = walmartItem.AvailableOnline,

                            WalmartItem = new ItemDTO()
                            {
                                SourceMarketId = walmartItem.ItemId,
                                CurrentPrice = walmartItem.SalePrice ?? 0,
                                AmazonRealQuantity = walmartItem.Stock == "Available" ? 1 : 0,
                                Size = walmartItem.Size,
                                Color = walmartItem.Color,
                                Name = walmartItem.Name,
                                Barcode = walmartItem.UPC,
                            }
                        });
                    }

                    var asinList = newItems.Where(i => i.AmazonItem != null
                                                       && !String.IsNullOrEmpty(i.AmazonItem.ASIN))
                                                       .Select(i => i.AmazonItem.ASIN)
                                                       .ToList();

                    if (asinList.Any())
                    {
                        var amazonPrices = api.GetLowestOfferListingsForASIN(asinList).ToList();

                        foreach (var amazonPrice in amazonPrices)
                        {
                            var item = newItems.FirstOrDefault(
                                    i => i.AmazonItem != null && i.AmazonItem.ASIN == amazonPrice.ASIN);
                            if (item != null)
                            {
                                item.AmazonPrice = amazonPrice.LowestPrice;
                                item.AmazonItem.AmazonRealQuantity = 1; //NOTE: has offer => Available
                            }
                        }
                    }

                    results.AddRange(newItems);

                    index += step;
                }
            }

            return new BargainSearchResult()
            {
                Bargains = results,
                StartIndex = filter.StartIndex,
                Total = searchResult.Total,
            };
        }

        private class BargainItemExport
        {
            public string Barcode { get; set; }
            public string Size { get; set; }
            public string Color { get; set; }

            public bool AvailableOnWalmart { get; set; }
            public bool WalmartInStock { get; set; }
            public decimal? WalmartPrice { get; set; }
            public string WalmartItemId { get; set; }
            public string WalmartItemUrl { get; set; }

            public bool AvailableOnAmazon { get; set; }
            public bool AmazonInStock { get; set; }
            public decimal? AmazonPrice { get; set; }
            public string AmazonASIN { get; set; }
            public string AmazonUrl { get; set; }

            public BargainItemExport()
            {
                
            }

            public BargainItemExport(BargainItem item)
            {
                Barcode = item.Barcode;
                Size = StringHelper.GetFirstNotEmpty(item.WalmartItem?.Size, item.AmazonItem?.Size);
                Color = StringHelper.GetFirstNotEmpty(item.WalmartItem?.Color, item.AmazonItem?.Color);

                AvailableOnAmazon = item.AmazonItem != null;
                AmazonInStock = item.AmazonItem != null && item.AmazonItem.AmazonRealQuantity > 0;
                AmazonPrice = item.AmazonPrice;
                AmazonASIN = item.AmazonItem?.ASIN;
                AmazonUrl = item.AmazonItem != null ? "https://www.amazon.com/dp/" + item.AmazonItem?.ASIN : null;

                AvailableOnWalmart = item.WalmartItem != null;
                WalmartInStock = item.WalmartItem != null && item.WalmartItem.AmazonRealQuantity > 0;
                WalmartPrice = item.WalmartPrice;
                WalmartItemId = item.WalmartItem?.SourceMarketId;
                WalmartItemUrl = item.WalmartItem != null
                    ? "https://www.walmart.com/ip/" + item.WalmartItem?.SourceMarketId
                    : null;
            }
        }

        public void ExportBargains(IList<BargainItem> bargainList, string filepath)
        {
            var b = new ExportColumnBuilder<BargainItemExport>();
            var columns = new List<ExcelColumnInfo>()
            {
                b.Build(p => p.Barcode, "Barcode", 15),
                b.Build(p => p.Size, "Size", 15),
                b.Build(p => p.Color, "Color", 15),
                b.Build(p => p.WalmartInStock, "Walmart InStock", 15),
                b.Build(p => p.WalmartPrice, "Walmart Price", 15),
                b.Build(p => p.WalmartItemId, "ItemId", 15),
                b.Build(p => p.WalmartItemUrl, "Walmart Url", 15),

                b.Build(p => p.AmazonInStock, "Amazon InStock", 15),
                b.Build(p => p.AmazonPrice, "Amazon Price", 15),
                b.Build(p => p.AmazonASIN, "ASIN", 15),
                b.Build(p => p.AmazonUrl, "Amazon Url", 15),
            };


            //var filename = "BargainsSearch_" + DateTime.Now.ToString("MM_dd_yyyy_hh_mm_ss") + ".xls";
            //var filepath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, filename);
            var items = bargainList.Select(bi => new BargainItemExport(bi)).ToList();
            using (var stream = ExcelHelper.Export(items, columns))
            {
                stream.Seek(0, SeekOrigin.Begin);
                using (var fileStream = File.Create(filepath))
                {
                    stream.CopyTo(fileStream);
                }
            }
        }
    }
}
