using Amazon.Core.Contracts;
using Amazon.Core.Models.Items;
using Amazon.DTO;
using Ccen.Web.ViewModels.Products.CheckBarcode;
using System.Collections.Generic;
using System.Linq;
using Walmart.Api;

namespace Amazon.Web.ViewModels.Products
{
    public class ItemBarcodeViewModel
    {
        public List<ItemBarcodeListingViewModel> Items { get; set; }

        public ItemBarcodeViewModel()
        {
            Items = new List<ItemBarcodeListingViewModel>();
        }

        public static List<string> GetUrlItemsByBarcodeFromAmazonApi(IMarketApi api, string barcode)
        {
            if (api == null)
            {
                return new List<string>() { "---" };
            }

            var itemDTOs = new List<ItemDTO>();
            try
            {
                itemDTOs = ((Amazon.Api.AmazonApi)api).GetProductForBarcode(new List<string>() { barcode }).ToList();
            }
            catch
            {
                return new List<string>() { "---" };
            }

            if (itemDTOs != null && itemDTOs.Count > 0)
            {
                return itemDTOs.Select(x => "https://www.amazon.com/dp/" + x.ASIN).ToList();
            }
            return new List<string>() { "---" };
        }

        public static List<string> GetUrlItemsByBarcodeFromWalmartApi(WalmartOpenApi api, ItemBarcodeSearchFilter filter)
        {
            var searchResult = api.SearchProducts(filter.Keywords,
                filter.CategoryId,
                filter.MinPrice,
                filter.MaxPrice,
                filter.StartIndex,
                filter.LimitCount);

            if (searchResult != null && searchResult.IsSuccess && searchResult.Data.Count > 0)
            {
                return searchResult.Data.Select(x => "https://www.walmart.com/ip/item/" + x.ItemId).ToList();
            }
            return new List<string>() { "---" };
        }
    }
}