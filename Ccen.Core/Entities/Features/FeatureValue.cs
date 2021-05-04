using System.ComponentModel.DataAnnotations;

namespace Amazon.Core.Entities.Features
{
    public class FeatureValue
    {
        [Key]
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

        public virtual Feature Feature { get; set; }
    }
}
