using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Amazon.Web.ViewModels.Inventory.Prices;

namespace Amazon.Web.ViewModels.Inventory
{
    public class SizePriceViewModel
    {
        public enum SizePriceApplyModes
        {
            Sale = 0,
            Permanent = 1
        }

        public enum SizeMarketApplyModes
        {
            All = 0,
            OnlyAmazonUS = 1
        }

        public long StyleItemId { get; set; }
        public string SizeGroupName { get; set; }
        public string Size { get; set; }
        public string Color { get; set; }

        public double? Weight { get; set; }

        public decimal? MinListingPrice { get; set; }
        public decimal? MaxListingPrice { get; set; }
        public DateTime? LastChangeDate { get; set; }

        public int? RemainingQuantity { get; set; }

        public IList<MarketPriceViewViewModel> SaleToMarkets { get; set; }
        
        //Manually

        //INPUT
        public long? SaleId { get; set; }
        public decimal? SalePrice { get; set; }
        public decimal? SFPSalePrice { get; set; }
        public DateTime? SaleStartDate { get; set; }
        public DateTime? SaleEndDate { get; set; }
        public int? MaxPiecesOnSale { get; set; }
        public int MaxPiecesMode { get; set; }

        public decimal? InitSalePrice { get; set; }
        public decimal? InitSFPSalePrice { get; set; }
        public decimal? NewSalePrice { get; set; }
        public decimal? NewSFPSalePrice { get; set; }
        public int? PiecesSoldOnSale { get; set; }

        public int ApplyMode { get; set; }
        public int MarketMode { get; set; }
        public decimal? MinPrice { get; set; }
        public decimal? MaxPrice { get; set; }
    }
}