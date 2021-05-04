using System.ComponentModel.DataAnnotations;

namespace Amazon.Core.Views
{
    public class ViewStyleFeatureValue
    {
        [Key]
        public long Id { get; set; }
        public string StyleID { get; set; }
        public int FeatureId { get; set; }
        public string Value { get; set; }
        public string ExtendedValue { get; set; }
    }
}
