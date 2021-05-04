using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Amazon.DTO.Inventory
{
    public class FeatureValueDTO
    {
        public int Id { get; set; }
        public int FeatureId { get; set; }
        public string DisplayValue { get; set; }
        public string Value { get; set; }
        public string ZhValue { get; set; }
        public string KrValue { get; set; }
        public string TwValue { get; set; }

        public string ExtendedValue { get; set; }
        public string ExtendedData { get; set; }
        public bool IsRequiredManufactureBarcode { get; set; }
        public int Order { get; set; }

        //Additional
        public long? StyleId { get; set; }
        public string FeatureName { get; set; }
    }
}
