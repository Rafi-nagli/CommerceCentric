using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Amazon.Common.Helpers;
using Amazon.Core.Models;
using Amazon.DTO.Inventory;
using Amazon.Model.Implementation.Markets;

namespace Amazon.Web.ViewModels.Inventory.Prices
{
    public class MarketPriceViewViewModel
    {
        public long SaleId { get; set; }

        public MarketType Market { get; set; }
        public string MarketplaceId { get; set; }

        public string MarketName { get; set; }

        public string MarketCurrency { get; set; }
        public decimal? SalePrice { get; set; }
        public decimal? SFPSalePrice { get; set; }
        public decimal? SalePercent { get; set; }

        public bool ApplyToNewListings { get; set; }
        public int ListingsCount { get; set; }


        public MarketPriceViewViewModel(StyleItemSaleToMarketDTO saleToMarket)
        {
            SaleId = saleToMarket.SaleId;
            Market = (MarketType)saleToMarket.Market;
            MarketplaceId = saleToMarket.MarketplaceId;
            MarketName = MarketHelper.GetShortName(saleToMarket.Market, saleToMarket.MarketplaceId);
            MarketCurrency = PriceHelper.GetCurrencySymbol(Market, MarketplaceId);
            SalePrice = saleToMarket.SalePrice;
            SFPSalePrice = saleToMarket.SFPSalePrice;
            SalePercent = saleToMarket.SalePercent;
            ApplyToNewListings = saleToMarket.ApplyToNewListings;
        }
    }
}