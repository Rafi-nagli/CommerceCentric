using System.ComponentModel.DataAnnotations;

namespace Amazon.Core.Entities.Inventory
{
    public class OpenBoxItem : BaseDateAndByEntity
    {
        [Key]
        public long Id { get; set; }
        public long BoxId { get; set; }
        public long? StyleItemId { get; set; }
        public int Quantity { get; set; }
    }
}
