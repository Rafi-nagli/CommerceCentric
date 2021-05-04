using System;
using System.Collections.Generic;
using Amazon.DTO.Inventory;
using Amazon.DTO.Listings;

namespace Amazon.DTO
{
    public class ParentItemDTO : ICloneable
    {
        public int Id { get; set; }
        public int Market { get; set; }
        public string MarketplaceId { get; set; }
        
        public long? DropShipperId { get; set; }
        public string DropShipperName { get; set; }

        public string SourceMarketId { get; set; }
        public string SourceMarketUrl { get; set; }

        public string AmazonName { get; set; }
        
        public string ASIN { get; set; }

        public string GroupId { get; set; }
        public string ImageSource { get; set; }
        public string ManualImage { get; set; }

        public string SKU { get; set; }
        
        public bool OnHold { get; set; }
        public bool ForceEnableColorVariations { get; set; }
        public bool ForceEnableSubStyleVariations { get; set; }

        public bool IsAutoParentDesc { get; set; }
        public string Description { get; set; }
        public string BulletPoint1 { get; set; }
        public string BulletPoint2 { get; set; }
        public string BulletPoint3 { get; set; }
        public string BulletPoint4 { get; set; }
        public string BulletPoint5 { get; set; }


        //Additional fields
        public string Barcode { get; set; }
        public string BrandName { get; set; }
        public string Type { get; set; }
        public decimal? Price { get; set; }
        public decimal? ListPrice { get; set; }
        public string Color { get; set; }
        public string Department { get; set; }
        public string Features { get; set; }

        public string AdditionalImages { get; set; }
        public IList<ImageDTO> Images { get; set; }
        
        public string SearchKeywords { get; set; }

        public int? Rank { get; set; }

        public DateTime? PublishRequestedDate { get; set; }

        public CommentDTO LastComment { get; set; }
        public string PriceRange { get; set; }


        public bool HasListings { get; set; }
        public bool? IsAmazonUpdated { get; set; }
                
        public DateTime? LastUpdateFromAmazon { get; set; }

        public bool LockMarketUpdate { get; set; }

        public string PositionsInfo { get; set; }

        //From Cache
        public bool? HasPriceDifferencesWithAmazon { get; set; }
        public bool? HasQuantityDifferencesWithAmazon { get; set; }
        public bool? HasChildWithFakeParentASIN { get; set; }

        public DateTime? LastChildOpenDate { get; set; }
        public decimal? MinChildPrice { get; set; }
        public decimal? MaxChildPrice { get; set; }


        //Additional
        public ItemImageDTO LargeImage { get; set; }
        public List<ItemShortInfoDTO> ChildItems { get; set; }
        public List<ItemShortInfoDTO> StyleItems { get; set; }
        public List<ItemDTO> Variations { get; set; }

        //Additonal/Temp
        public string TempParentASIN { get; set; }
        public string Tags { get; set; }

        public object Clone()
        {
            return this.MemberwiseClone();
        }
    }
}
