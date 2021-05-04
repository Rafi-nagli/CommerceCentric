using System;
using System.ComponentModel.DataAnnotations;
using Amazon.Core.Helpers;

namespace Amazon.Core.Entities
{
    public class ListingFBAInv : BaseDateEntity
    {
        [Key]
        public long Id { get; set; }

        public string SellerSKU { get; set; }
        public string FulfillmentChannelSKU { get; set; }
        public string ASIN { get; set; }
        public string WarehouseConditionCode { get; set; }
        public int? QuantityAvailable { get; set; }


        public bool IsRemoved { get; set; }
        public DateTime? UpdateDate { get; set; }
        public DateTime? CreateDate { get; set; }


        public override string ToString()
        {
            return ToStringHelper.ToString(this);
        }
    }
}
