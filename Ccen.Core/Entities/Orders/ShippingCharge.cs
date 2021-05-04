using System;
using System.ComponentModel.DataAnnotations;

namespace Amazon.Core.Entities
{
    public class ShippingCharge
    {
        [Key]
        public int Id { get; set; }
        public int ShippingMethodId { get; set; }
        public decimal ChargePercent { get; set; }

        public long? CreatedBy { get; set; }
        public DateTime CreateDate { get; set; }
        
    }
}
