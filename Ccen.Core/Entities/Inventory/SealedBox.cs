 using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Amazon.Core.Entities.Inventory
{
    public class SealedBox : BaseDateAndByEntity
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Key]
        public long Id { get; set; }
        public long StyleId { get; set; }
        public int Type { get; set; }
        public int Status { get; set; }
        public long? LinkedPreorder { get; set; }

        public string BoxBarcode { get; set; }
        public int BoxQuantity { get; set; }
        public bool Owned { get; set; }
        public decimal PajamaPrice { get; set; }
        public bool Deleted { get; set; }
        public bool Printed { get; set; }
        public bool PolyBags { get; set; }

        public bool Archived { get; set; }
        public bool ReInventory { get; set; }

        public DateTime? ExpectedReceiptDate { get; set; }

        public int OriginType { get; set; }
        public long? OriginId { get; set; }
        public string OriginTag { get; set; }
        public DateTime? OriginCreateDate { get; set; }
    }
}
