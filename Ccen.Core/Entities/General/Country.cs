using System.ComponentModel.DataAnnotations;

namespace Amazon.Core.Entities
{
    public class Country
    {
        [Key]
        public int CountryId { get; set; }
        public string CountryName { get; set; }
        public string CountryCode2 { get; set; }
        public string CountryCode3 { get; set; }
        public int CountryCodeN { get; set; }
        public int Order { get; set; }
        public bool IsInsure { get; set; }
    }
}
