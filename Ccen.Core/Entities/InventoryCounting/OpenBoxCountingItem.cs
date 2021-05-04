using System.ComponentModel.DataAnnotations;

namespace Amazon.Core.Entities.Inventory
{
    public class OpenBoxCountingItem : BaseDateAndByEntity
    {
        [Key]
        public long Id { get; set; }
        public long BoxId { get; set; }
        public long StyleItemId { get; set; }
        
        public string Size { get; set; }
        public string Color { get; set; }

        public int Quantity { get; set; }
    }
}
