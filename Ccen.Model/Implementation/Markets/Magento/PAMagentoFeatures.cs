using Magento.Api.Wrapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Amazon.Model.Implementation.Markets.Magento
{
    public class PAMagentoFeatures : IMagentoFeatures
    {
        public const string StoreIdAttributeName = "store_id";
        public const string WebSiteIdAttributeName = "website_id";
        public const string TaxClassIdAttributeName = "tax_class_id";
        public const string GiftMessageAvailableAttributeName = "gift_message_available";
        public const string SwFeaturedAttributeName = "sw_featured";
        public const string FeaturedAttributeName = "featured";
        public const string VisibilityAttributeName = "visibility";

        public const string BestDialAttributeName = "best_deals";
        public const string NewArrivalAttributeName = "new_arrival";

        public const string ColorAttributeName = "color";
        public const string SizeAttributeName = "size";

        public const string CategoryAttributeName = "category_ids";
        public const string UpcAttributeName = "upc";
        public const string DescriptionAttributeName = "description";
        public const string MetaDescriptionAttributeName = "meta_description";
        public const string MetaTitleAttributeName = "meta_title";
        public const string MetaKeywordAttributeName = "meta_keyword";
        public const string UrlKeyAttributeName = "url_key";
        public const string BulletPoint1AttributeName = "feature_1";
        public const string BulletPoint2AttributeName = "feature_2";
        public const string BulletPoint3AttributeName = "feature_3";
        public const string BulletPoint4AttributeName = "feature_4";
        public const string BulletPoint5AttributeName = "feature_5";



        public const string ListPriceAttributeName = "list_price";
        public const string MSRPAttributeName = "msrp";
        public const string SpecialPriceAttributeName = "special_price";
        public const string SpecialFromDateAttributeName = "special_from_date";
        public const string SpecialToDateAttributeName = "special_to_date";

        public const string ImageAttributeName = "image";
        public const string SmallImageAttributeName = "small_image";
        public const string ThumbnailAttributeName = "thumbnail";

        public const string CountryOfManufacture = "country_of_manufacture";
        public const string MovementFeatureName = "movement";
        public const string CalendarFeatureName = "calendar";
        public const string DialFeatureName = "dial";
        public const string GenderFeatureName = "gender";
        public const string MfnFeatureName = "mfn";
        public const string ManufacturerFeatureName = "manufacturer";
        public const string WaterResistantFeatureName = "water_resistant";
        public const string CrownFeatureName = "crown";
        public const string CrystalFeatureName = "crystal";
        public const string ClaspTypeFeatureName = "clasp_type";
        public const string BandColorFeatureName = "band_color";
        public const string BandMaterialFeatureName = "band_material";
        public const string BandWidthFeatureName = "band_width";
        public const string CaseMaterialFeatureName = "case_material";
        public const string CaseShapeFeatureName = "case_shape";
        public const string CaseColorFeatureName = "case_color";
        public const string CaseWidthFeatureName = "case_width";
        public const string CaseHeightFeatureName = "case_height";
        public const string CaseThicknessFeatureName = "case_thickness";

        //Sunglasses
        public const string Feature1Name = "feature1";
        public const string Feature2Name = "feature2";
        public const string Feature3Name = "feature3";
        public const string ColorName = "color";
        public const string FrameColorName = "frame_color";
        public const string FrameMaterialName = "frame_material";
        public const string LensColorName = "lens_color";
        public const string LensColorMapAmazonName = "lens_color_map_amazo";
        public const string LensMaterialName = "lens_material";
        public const string LensPolarizationTypeAmazonName = "lens_polarization_type_amazon";
        public const string LensProtectionName = "lens_protection";
        public const string MadeInName = "made_in";
        public const string SizeBridgeName = "size_bridge";
        public const string SizeEyeName = "size_eye";
        public const string SizeTempleName = "size_temple";
        public const string StyleName = "style";
        public const string TypeName = "type";

        public const string ProductTypeName = "product_type";

        string IMagentoFeatures.ListPriceAttributeName => ListPriceAttributeName;
        string IMagentoFeatures.MSRPAttributeName => MSRPAttributeName;

        string IMagentoFeatures.SpecialPriceAttributeName => SpecialPriceAttributeName;

        string IMagentoFeatures.SpecialFromDateAttributeName => SpecialFromDateAttributeName;

        string IMagentoFeatures.SpecialToDateAttributeName => SpecialToDateAttributeName;

        string IMagentoFeatures.BestDialAttributeName => BestDialAttributeName;
    }
}
