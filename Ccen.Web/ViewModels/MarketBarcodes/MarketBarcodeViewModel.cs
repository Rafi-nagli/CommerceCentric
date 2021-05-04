using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Amazon.Core;
using Amazon.Core.Contracts;
using Amazon.Web.Models.SearchFilters;
using Walmart.Api;

namespace Amazon.Web.ViewModels.CustomBarcodes
{
    public class MarketBarcodeViewModel
    {
        public string ItemId { get; set; }
        public string Barcode { get; set; }
        public string Size { get; set; }
        public string Color { get; set; }
 
        
        public static IList<MarketBarcodeViewModel> SearchBarcodes(ILogService log,
            string productUrl)
        {
            var itemId = (productUrl ?? "").Split("/".ToCharArray()).LastOrDefault();
            var items = new List<MarketBarcodeViewModel>();
            if (!String.IsNullOrEmpty(itemId))
            {
                var walmartService = new WalmartOpenApi(log, "trn9fdghvb8p9gjj9j6bvjwx");
                var productResult = walmartService.LookupProductWithVariations(itemId);
                if (productResult.IsSuccess)
                {
                    items = productResult.Data.Select(p => new MarketBarcodeViewModel()
                    {
                        ItemId = p.SourceMarketId,
                        Barcode = p.Barcode,
                        Size = p.Size,
                        Color = p.Color,
                    }).ToList();
                }
            }

            return items;
        }
    }
}