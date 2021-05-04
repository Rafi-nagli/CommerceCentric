using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Amazon.DTO.Inventory
{
    public class OpenBoxDto
    {
        public long Id { get; set; }
        public long StyleId { get; set; }

        public int Type { get; set; }
        public int Status { get; set; }
        public long? LinkedPreorderBox { get; set; }

        public string BoxBarcode { get; set; }
        public int BoxQuantity { get; set; }
        public decimal Price { get; set; }
        public bool Deleted { get; set; }
        public bool Printed { get; set; }
        public bool PolyBags { get; set; }
        public bool Owned { get; set; }
        public bool Archived { get; set; }

        public DateTime? ExpectedReceiptDate { get; set; }

        public DateTime? OriginCreateDate { get; set; }
        public int OriginType { get; set; }
        public long? OriginId { get; set; }

        public DateTime? CreateDate { get; set; }
        public DateTime? UpdateDate { get; set; }

        public string UpdatedByName { get; set; }
        public string CreatedByName { get; set; }
    }
}
