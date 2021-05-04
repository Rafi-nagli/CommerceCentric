using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;

namespace Amazon.Core.Entities.Inventory
{
    public class StyleItemSaleToListing
    {
        [Key]
        public long Id { get; set; }

        public long SaleToMarketId { get; set; }

        public long SaleId { get; set; }
        public long ListingId { get; set; }

        public decimal? OverrideSalePrice { get; set; }

        public DateTime CreateDate { get; set; }
        public long? CreatedBy { get; set; }
    }
}
