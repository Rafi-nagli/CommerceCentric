using System.Collections.Generic;
using System.Linq;
using Amazon.DTO;

namespace Amazon.Common.Helpers
{
    public class StyleOrderFields
    {
        public string Gender { get; set; }
        public string MainLicense { get; set; }
        public string SubLicense { get; set; }
        public string StyleId { get; set; }
    }

    public class StyleSortHelper
    {
        public static StyleOrderFields GetOrderFields(IList<StyleFeatureValueDTO> features)
        {
            var gender = features.FirstOrDefault(f => f.FeatureId == StyleFeatureHelper.GENDER);
            var mainLicense = features.FirstOrDefault(p => p.FeatureId == StyleFeatureHelper.MAIN_LICENSE);
            var subLicense = features.FirstOrDefault(p => p.FeatureId == StyleFeatureHelper.SUB_LICENSE1);
            return new StyleOrderFields
            {
                Gender = gender != null ? gender.Value : "",
                StyleId = gender != null ? gender.StyleId.ToString() : "",
                MainLicense = mainLicense != null ? mainLicense.Value : "",
                SubLicense = subLicense != null ? subLicense.Value : ""
            };
        }
    }
}
