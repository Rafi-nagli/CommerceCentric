using System.ComponentModel.DataAnnotations;

namespace Amazon.Core.EntitiesInventory
{
    public class ViewBarcode
    {
        [Key]
        public string Barcode { get; set; }
    }
}
