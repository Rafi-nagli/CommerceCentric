using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Amazon.Core.Entities.Inventory
{
    public class Style : BaseDateAndByEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }
        public string StyleID { get; set; }
        public string OriginalStyleID { get; set; }
        public string DSStyleID { get; set; }

        public long? DropShipperId { get; set; }
        public string PreorderCountry { get; set; }
        public bool AutoDSSelection { get; set; }
        public DateTime? DSEffectiveDate { get; set; }

        public long? CreatedByDS { get; set; }
        public long? LinkedDSItemId { get; set; }

        public bool OnHold { get; set; }
        //public DateTime? OnHoldUpdateDate { get; set; }
        //public long? OnHoldUpdatedBy { get; set; }
        public bool SystemOnHold { get; set; }

        public int Type { get; set; }

        public string Name { get; set; }

        public string Manufacturer { get; set; }
        public string Description { get; set; }
        public string LongDescription { get; set; }

        public long? DescriptionUpdatedBy { get; set; }
        public DateTime? DescriptionUpdateDate { get; set; }

        public string Image { get; set; }
        public string AdditionalImages { get; set; }

        public decimal? MSRP { get; set; }
        public decimal? Price { get; set; }
        public decimal? Cost { get; set; }
        
        public string SearchTerms { get; set; }
        public string Tags { get; set; }
        public string BulletPoint1 { get; set; }
        public string BulletPoint2 { get; set; }
        public string BulletPoint3 { get; set; }
        public string BulletPoint4 { get; set; }
        public string BulletPoint5 { get; set; }

        public int? SourceType { get; set; }

        public bool Deleted { get; set; }

        public int? ItemTypeId { get; set; }

        public int DisplayMode { get; set; }

        public bool RemovePriceTag { get; set; }
        
        public int FillingStatus { get; set; }

        public int PictureStatus { get; set; }
        public DateTime? PictureStatusUpdateDate { get; set; }
        public long? PictureStatusUpdatedBy { get; set; }

        public string Comment { get; set; }
        public DateTime? CommentUpdateDate { get; set; }
        public long? CommentUpdatedBy { get; set; }
        
        public string LiteCountingStatus { get; set; }
        //public string LiteCountingName { get; set; }
        public DateTime? LiteCountingDate { get; set; }

        public string SourceMarketId { get; set; }

        public DateTime? ReSaveDate { get; set; }
        public long? ReSaveBy { get; set; }
    }
}
