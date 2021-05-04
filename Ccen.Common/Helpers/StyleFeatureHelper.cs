using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Amazon.DTO.Inventory;

namespace Amazon.Common.Helpers
{
    public class StyleFeatureHelper
    {
        public const int MAIN_CHARACTER = 2;
        public const int BRAND = 2;
        public const int MAIN_LICENSE = 12;
        public const int SUB_LICENSE1 = 13;
        public const int SUB_LICENSE2 = 14;
        public const int GENDER = 3;
        public const int COLOR1 = 10;
        public const int ITEMSTYLE = 4;
        public const int SLEEVE = 6;
        public const int MATERIAL = 9;
        public const int SHIPPING_SIZE = 16;
        public const int SIZE = 18;

        public const int SEASON = 8;
        public const int MAIN_ITEMSTYLE = 27;
        public const int SUB_ITEMSTYLE = 28;

        public const int TRENDS = 29;

        public const int OCCASION = 25;

        //public const int FIT_INTERNATIONAL_FLAT = 19;
        public const int MATERIAL_COMPOSITION = 20;
        public const int INTERNATIONAL_PACKAGE = 21;
        public const int EXCESSIVE_SHIPMENT = 22;
        public const int PRODUCT_TYPE = 31;
        public const int HOLIDAY = 24;
        
        public const string PACKAGE_LENGTH_KEY = "Special Package Length (in)";
        public const string PACKAGE_WIDTH_KEY = "Special Package Width (in)";
        public const string PACKAGE_HEIGHT_KEY = "Special Package Height (in)";

        public const string MAIN_LICENSE_KEY = "Main License";
        public const string MAIN_CHARACTER_KEY = "Main characters";
        public const string GLOBAL_LICENSE_KEY = "Global License";
        public const string BRAND_KEY = "Brand";        

        public const int ADDITIONAL_CATEGORIES = 66;

        public const string DisableImagesSync = "Disable Image Sync";

        public const string GenderName = "Gender";
        public const string ProductType = "Product Type";
        public const string MaterialName = "Material";
        public const string OccasionName = "Occasion";
        public const string PublishedAtName = "Published At";

        public const string ItemStyleKey = "Item style";
        public const string ItemTypeKey = "Item Type";

        public const string WMKeywordsName = "WMKeywords";



        public static bool CompareCategoryNames(string name1, string name2)
        {
            name1 = name1 ?? "";
            name2 = name2 ?? "";

            if (String.Compare(name1, name2, StringComparison.InvariantCultureIgnoreCase) == 0)
                return true;

            var name1Formatted = name1.Replace(" ", "").Replace("-", "").Replace("_", "").Replace(",", "").Replace(".", "").Replace("&", "").Replace("\\", "").Replace("/", "").Replace("\r", "").Replace("\n", "");
            var name2Formatted = name2.Replace(" ", "").Replace("-", "").Replace("_", "").Replace(",", "").Replace(".", "").Replace("&", "").Replace("\\", "").Replace("/", "").Replace("\r", "").Replace("\n", "");

            return String.Compare(name1Formatted, name2Formatted, StringComparison.InvariantCultureIgnoreCase) == 0;
        }

        public static string[] PrepareBulletPoints(string[] bulletPoints)
        {
            var notEmptyList = bulletPoints?.Where(b => !String.IsNullOrEmpty(b)).ToList() ?? new List<string>();
            var hasCotton100 = notEmptyList.Any(b => b.IndexOf("100% Cotton", StringComparison.InvariantCultureIgnoreCase) >= 0);
            var hasSnagFit = notEmptyList.Any(b => b.IndexOf("Snag fit", StringComparison.InvariantCultureIgnoreCase) >= 0);

            if (hasCotton100 && !hasSnagFit)
            {
                for (var i = 0; i < notEmptyList.Count; i++)
                {
                    notEmptyList[i] = notEmptyList[i].Replace("100% Cotton", "100% Cotton, Snag Fit");
                }
            }
            return notEmptyList.ToArray();
        }

        public static string GetFeatureValueByName(IList<FeatureValueDTO> features, string featureName)
        {
            return features.FirstOrDefault(f => f.FeatureName == featureName)?.Value;
        }

        public static string GetFeatureValueById(IList<FeatureValueDTO> features, long featureId)
        {
            return features.FirstOrDefault(f => f.FeatureId == featureId)?.Value;
        }

        public static string GetFeatureValue(FeatureValueDTO featureValue)
        {
            if (featureValue != null)
                return featureValue.Value;
            return null;
        }
        
        public static string PrepareMainLicense(string mainLicense, string subLicense)
        {
            if (mainLicense == "DC Comics/Marvel")
            {
                if (subLicense == "Spiderman")
                {
                    return "Marvel";
                }
                return "DC Comics";
            }
            return mainLicense;
        }

        public static void AddOrUpdateFeatureValueByName(IList<FeatureValueDTO> features, long styleId, string featureName, string newValue)
        {
            var existFeature = features.FirstOrDefault(f => f.FeatureName == featureName
                && f.StyleId == styleId);
            if (existFeature == null)
            {
                features.Add(new FeatureValueDTO()
                {
                    StyleId = styleId,
                    FeatureName = featureName,                    
                    Value = newValue,
                });
            }
            else
            {
                existFeature.Value = newValue;
            }
        }
    }
}
