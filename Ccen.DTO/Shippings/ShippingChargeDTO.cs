
using System;

namespace Amazon.DTO
{
    public class ShippingChargeDTO
    {
        public int Id { get; set; }
        public int ShippingMethodId { get; set; }
        public decimal ChargePercent { get; set; }

        public long? CreatedBy { get; set; }
        public DateTime CreateDate { get; set; }
    }
}
