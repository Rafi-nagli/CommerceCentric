using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Amazon.DTO
{
    public class StyleFeatureValueDTO
    {
        public int Id { get; set; }
        public int FeatureId { get; set; }

        public int? FeatureValueId { get; set; }
        public string Value { get; set; }

        public long StyleId { get; set; }
        public int Type { get; set; }

        public int SortOrder { get; set; }

        //Additional 
        public string FeatureName { get; set; }
        public object ObjValue { get; set; }
    }
}
