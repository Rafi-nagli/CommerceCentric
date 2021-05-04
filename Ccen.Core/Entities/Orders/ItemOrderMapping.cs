
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Amazon.Core.Entities
{
    public class ItemOrderMapping : BaseDateEntity
    {
        [Key]
        [Column(Order = 1)]
        public long OrderItemId { get; set; }
        [Key]
        [Column(Order = 2)]
        public long ShippingInfoId { get; set; }


        public int MappedQuantity { get; set; }
    }
}
