using System;
using System.ComponentModel.DataAnnotations;

namespace Amazon.Core.Entities
{
    public class ParentItem : BaseDateAndByEntity
    {
        [Key]
        public int Id { get; set; }
        public string ASIN { get; set; }
        public int Market { get; set; }
        public string MarketplaceId { get; set; }

        public string SourceMarketId { get; set; }
        public string SourceMarketUrl { get; set; }

        public string GroupId { get; set; }

        public string AmazonName { get; set; }

        public string SKU { get; set; }
        public bool OnHold { get; set; }
        public bool ForceEnableColorVariations { get; set; }

        public string ImageSource { get; set; }
        public string ManualImage { get; set; }

        public bool IsAutoParentDesc { get; set; }
        public string Description { get; set; }
        public string BulletPoint1 { get; set; }
        public string BulletPoint2 { get; set; }
        public string BulletPoint3 { get; set; }
        public string BulletPoint4 { get; set; }
        public string BulletPoint5 { get; set; }


        //Additional fields
        public string BrandName { get; set; }
        public string Type { get; set; }
        public decimal? ListPrice { get; set; }
        public string Color { get; set; }
        public string Department { get; set; }
        [MaxLength(2048)]
        public string Features { get; set; }
        [MaxLength(2048)]
        public string AdditionalImages { get; set; }
        [MaxLength(2048)]
        public string SearchKeywords { get; set; }

        public int? Rank { get; set; }
        public DateTime? RankUpdateDate { get; set; }

        public string PromotionId { get; set; }
        public DateTime? PromotionCreateDate { get; set; }

        public bool LockMarketUpdate { get; set; }


        //Additional Field
        public bool? IsAmazonUpdated { get; set; }
        public DateTime? LastUpdateFromAmazon { get; set; }
    }
}
