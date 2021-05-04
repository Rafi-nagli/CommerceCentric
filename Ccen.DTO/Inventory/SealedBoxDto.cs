using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Amazon.DTO.Inventory
{
    public class SealedBoxDto
    {
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

        public DateTime? ExpectedReceiptDate { get; set; }

        public int OriginType { get; set; }
        public long? OriginId { get; set; }

        public DateTime? UpdateDate { get; set; }
        public DateTime? CreateDate { get; set; }
        public DateTime? OriginCreateDate { get; set; }

        public string UpdatedByName { get; set; }
    }
}
