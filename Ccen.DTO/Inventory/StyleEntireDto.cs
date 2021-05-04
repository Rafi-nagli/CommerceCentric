using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Amazon.DTO.Inventory
{
    public class StyleEntireDto
    {
        public long Id { get; set; }
        public string StyleID { get; set; }
        public string OriginalStyleID { get; set; }
        public string DSStyleId { get; set; }

        public long? DropShipperId { get; set; }
        public string PreorderCountry { get; set; }

        public bool AutoDSSelection { get; set; }
        public long? CreatedByDS { get; set; }
        public long? LastDropShipperId { get; set; }
        public DateTime? DSEffectiveDate { get; set; }

        public int Type { get; set; }
        public bool OnHold { get; set; }
        public bool SystemOnHold { get; set; }
        public string Name { get; set; }
        public string Manufacturer { get; set; }
        public string Description { get; set; }
        public string LongDescription { get; set; }

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


        public string Image { get; set; }

        public string AdditionalImages { get; set; }

        public bool Deleted { get; set; }
        public DateTime? ReSaveDate { get; set; }

        public DateTime? UpdateDate { get; set; }
        public DateTime? CreateDate { get; set; }

        public long? CreatedBy { get; set; }

        public int? ItemTypeId { get; set; }

        public string Comment { get; set; }
        public DateTime? CommentUpdateDate { get; set; }

        public int DisplayMode { get; set; }
        public bool RemovePriceTag { get; set; }
        public int FillingStatus { get; set; }
        public int PictureStatus { get; set; }
        public DateTime? PictureStatusUpdateDate { get; set; }

        //Additional Fields
        public string Tag { get; set; }
        public string SwatchImage { get; set; }
        public string ItemTypeName { get; set; }

        //Navigations
        public IList<StyleFeatureValueDTO> StyleFeatures { get; set; }
        public IList<StyleImageDTO> Images { get; set; }
        public IList<StyleItemDTO> StyleItems { get; set; }
    }
}
