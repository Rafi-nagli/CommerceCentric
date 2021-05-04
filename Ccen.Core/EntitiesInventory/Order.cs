using System;
using System.ComponentModel.DataAnnotations;

namespace Amazon.Core.EntitiesInventory
{
    public class Order
    {
        [Key]
        public long Id { get; set; }
        public bool IsFBA { get; set; }
        public string Description { get; set; }
        public DateTime OrderDate { get; set; }
        public string FileName { get; set; }

        public DateTime? LastCheckedDate { get; set; }
    }
}
