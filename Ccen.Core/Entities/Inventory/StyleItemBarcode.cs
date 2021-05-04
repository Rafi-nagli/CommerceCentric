using System.ComponentModel.DataAnnotations;

namespace Amazon.Core.Entities.Inventory
{
    public class StyleItemBarcode : BaseDateAndByEntity
    {
        [Key]
        public long Id { get; set; }
        public long StyleItemId { get; set; }
        public string Barcode { get; set; }
    }
}
