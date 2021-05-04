using System.ComponentModel.DataAnnotations;

namespace Amazon.Core.EntitiesInventory
{
    public class Item
    {
        [Key]
        public int Id { get; set; }
        public string Barcode { get; set; }
        //public int Quantity { get; set; }
    }
}
