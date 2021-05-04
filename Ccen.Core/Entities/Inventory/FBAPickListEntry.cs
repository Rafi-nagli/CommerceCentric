using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;

namespace Amazon.Core.Entities.Inventory
{
    public class FBAPickListEntry
    {
        [Key]
        public long Id { get; set; }

        public long FBAPickListId { get; set; }

        public long StyleId { get; set; }
        public string StyleString { get; set; }
        public long StyleItemId { get; set; }
        public int Quantity { get; set; }

        public long ListingId { get; set; }
        public string ListingParentASIN { get; set; }
        public string ListingASIN { get; set; }
        public string ListingSKU { get; set; }

        public decimal? Weight { get; set; }

        public DateTime CreateDate { get; set; }
        public long? CreatedBy { get; set; }
    }
}
