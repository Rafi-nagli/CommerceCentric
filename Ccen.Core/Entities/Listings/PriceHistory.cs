using System;
using System.ComponentModel.DataAnnotations;
using Amazon.Core.Helpers;

namespace Amazon.Core.Entities
{
    public class PriceHistory
    {
        [Key]
        public long Id { get; set; }
        public long? ListingId { get; set; }
        public int Type { get; set; }
        public string SKU { get; set; }

        public DateTime ChangeDate { get; set; }
        public long? ChangedBy { get; set; }
        public decimal Price { get; set; }
        public decimal? OldPrice { get; set; }

        public override string ToString()
        {
            return ToStringHelper.ToString(this);
        }
    }
}
