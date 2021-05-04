using System;
using System.ComponentModel.DataAnnotations;

namespace Amazon.Core.EntitiesInventory
{
    public class Inventory
    {
        [Key]
        public long Id { get; set; }
        public string Description { get; set; }
        public DateTime InventoryDate { get; set; }
    }
}
