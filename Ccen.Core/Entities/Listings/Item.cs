using System;
using System.ComponentModel.DataAnnotations;
using Amazon.Core.Helpers;
using Amazon.DTO;

namespace Amazon.Core.Entities
{
    public class Item : BaseDateAndByEntity
    {
        [Key]
        public int Id { get; set; }
        public string ASIN { get; set; }
        public string ParentASIN { get; set; }
        public int Market { get; set; }
        public string MarketplaceId { get; set; }

        public string SourceMarketId { get; set; }
        public string SourceMarketUrl { get; set; }

        public string Barcode { get; set; }
        public string Size { get; set; }

        public string ColorVariation { get; set; }

        public string Title { get; set; }

        public string StyleString { get; set; }
        public long? StyleId { get; set; }
        public long? StyleItemId { get; set; }

        public decimal? Rank { get; set; }


        public string OnMarketTemplateName { get; set; }
        public bool? IsAmazonParentASIN { get; set; }
        public bool? IsExistOnAmazon { get; set; }
        public DateTime? LastUpdateFromAmazon { get; set; }
        public string MarketParentASIN { get; set; }

        public long CompanyId { get; set; }

        [MaxLength(2048)]
        public string Description { get; set; }


        //Additional fields
        public string PrimaryImage { get; set; }
        [MaxLength(2048)]
        public string AdditionalImages { get; set; }

        public bool UseStyleImage { get; set; }

        public bool ImagesIgnored { get; set; }

        public string BrandName { get; set; }
        public string Type { get; set; }
        public decimal? ListPrice { get; set; }
        public string Color { get; set; }
        public string Department { get; set; }
        [MaxLength(2048)]
        public string Features { get; set; }
        [MaxLength(2048)]
        public string SearchKeywords { get; set; }

        public string SpecialSize { get; set; }
        public bool IsCheckedSpecialSize { get; set; }
        public DateTime? CheckSpecialSizeDate { get; set; }

        public int ItemPublishedStatus { get; set; }
        public DateTime? ItemPublishedStatusDate { get; set; }
        public string ItemPublishedStatusReason { get; set; }

        public int? ItemPublishedStatusBeforeRepublishing { get; set; }
        public DateTime? LastForceRepublishedDate { get; set; }

        public int? ItemPublishedStatusFromMarket { get; set; }
        public DateTime? ItemPublishedStatusFromMarketDate { get; set; }


        public override string ToString()
        {
            return ToStringHelper.ToString(this);
        }
    }
}
