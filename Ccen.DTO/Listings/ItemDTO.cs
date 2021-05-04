
using System;
using System.Collections.Generic;
using Amazon.DTO.Inventory;
using Amazon.DTO.Listings;

namespace Amazon.DTO
{
    public class ItemDTO : IReportItemDTO
    {
        public int Id { get; set; }

        public int Market { get; set; }
        public string MarketplaceId { get; set; }

        public long? ListingEntityId { get; set; }

        public string SourceMarketId { get; set; }
        public string SourceMarketUrl { get; set; }

        public string Name { get; set; }
        public string Description { get; set; }
        public string ListingId { get; set; }
        public string ASIN { get; set; }
        
        /// <summary>
        /// Using only for main image temporary storage
        /// </summary>
        public string ImageUrl { get; set; }
        public string LargeImageUrl { get; set; }

        public string StyleImage { get; set; }
        public bool UseStyleImage { get; set; }

        public decimal CurrentPrice { get; set; }
        public string CurrentPriceCurrency { get; set; }
        public decimal? CurrentPriceInUSD { get; set; }
        public decimal? SourceCurrentPrice { get; set; }

        public decimal? Cost { get; set; }

        public bool IsManualPrice { get; set; }

        public decimal? BusinessPrice { get; set; }

        public string OnMarketTemplateName { get; set; }


        public int? DisplayQuantity { get; set; }
        public int RealQuantity { get; set; }
        public bool OnHold { get; set; }
        public DateTime? RestockDate { get; set; }

        public DateTime? AutoQuantityUpdateDate { get; set; }

        public decimal? SalePrice { get; set; }
        public DateTime? SaleStartDate { get; set; }
        public DateTime? SaleEndDate { get; set; }
        public int? MaxPiecesOnSale { get; set; }
        public int? PiecesSoldOnSale { get; set; }

        public long? SaleId { get; set; }


        public string Barcode { get; set; }

        public int ProductIdType { get; set; }

        public bool IsDefault { get; set; }
        public string SKU { get; set; }
        public bool IsFBA { get; set; }
        public bool IsPrime { get; set; }
        public decimal? EstimatedOrderHandlingFeePerOrder { get; set; }
        public decimal? EstimatedPickPackFeePerUnit { get; set; }
        public decimal? EstimatedWeightHandlingFeePerUnit { get; set; }

        public string StyleString { get; set; }
        public long? StyleId { get; set; }
        public bool? StyleOnHold { get; set; }
        
        public string ProductId { get; set; }
        public DateTime? OpenDate { get; set; }
        public string Condition { get; set; }
        public string Note { get; set; }
        public bool IsInternational { get; set; }

        public string Size { get; set; }
        public string ColorVariation { get; set; }
        public string SubStyleVariation { get; set; }

        public string StyleSize { get; set; }
        public string StyleColor { get; set; }

        public long? StyleItemId { get; set; }
        public bool? StyleItemOnHold { get; set; }

        public string ParentASIN { get; set; }
        public long? ParentId { get; set; }

        public double? Weight { get; set; }


        public bool? IsAmazonParentASIN { get; set; }
        public bool? IsExistOnAmazon { get; set; }
        public DateTime? LastUpdateFromAmazon { get; set; }


        public int? AmazonRealQuantity { get; set; }
        public decimal? AmazonCurrentPrice { get; set; }

        public decimal? ListingPriceFromMarket { get; set; }
        public decimal? ReqularPriceFromMarket { get; set; }
        public decimal? ShippingPriceFromMarket { get; set; }
        public DateTime? PriceFromMarketUpdatedDate { get; set; }
        public DateTime? QtyFromMarketUpdatedDate { get; set; }

        public int? SoldByAmazon { get; set; }
        public int? SoldByInventory { get; set; }
        public int? SoldByFBA { get; set; }
        public int? SoldBySpecialCase { get; set; }
        public int? TotalSoldByAmazon { get; set; }
        public int? TotalSoldByInventory { get; set; }
        public int? TotalSoldByFBA { get; set; }
        public int? TotalSoldBySpecialCase { get; set; }
        public int? TotalQuantity { get; set; }
        public int? RemainingQuantity { get; set; }

        //Additional fields
        public string BrandName { get; set; }
        public string Type { get; set; }
        public decimal? ListPrice { get; set; }
        public string Color { get; set; }
        public string Department { get; set; }
        public string Features { get; set; }
        public string AdditionalImages { get; set; }
        public string SearchKeywords { get; set; }
        
        public decimal? Rank { get; set; }
        public decimal? LowestPrice { get; set; }
        public BuyBoxStatusCode? BuyBoxStatus { get; set; }

        public decimal? MinPrice { get; set; }
        public decimal? MaxPrice { get; set; }

        public long? CompanyId { get; set; }
        public long? DropShipperId { get; set; }

        public int PublishedStatus { get; set; }
        public int? PublishedStatusFromMarket { get; set; }
        public DateTime? PuclishedStatusDate { get; set; }
        public string PublishedStatusReason { get; set; }


        public DateTime? CreateDate { get; set; }
        public DateTime? UpdateDate { get; set; }
        public int? CreatedBy { get; set; }
        public int? UpdatedBy { get; set; }


        //Calculated
        public DateTime? LastSoldDate { get; set; }

        public bool AutoGeneratedBarcode { get; set; }
        public int? OverridePublishedStatus { get; set; }

        //Navigations
        public ParentItemDTO Parent { get; set; }
        public List<ListingDTO> Listings { get; set; }
        public StyleEntireDto Style { get; set; }
        public IList<StyleFeatureValueDTO> FeatureList { get; set; }

        public IList<ImageDTO> Images { get; set; }


        //Additionals
        public string DropShipperName { get; set; }
        public string Tag { get; set; }

        public ItemDTO()
        {
            ParentASIN = String.Empty; //not null (sql join not works with null)!
        }
    }
}
