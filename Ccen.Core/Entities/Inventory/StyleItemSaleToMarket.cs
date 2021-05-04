using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;

namespace Amazon.Core.Entities.Inventory
{
    public class StyleItemSaleToMarket
    {
        [Key]
        public long Id { get; set; }
            
        public long SaleId { get; set; }
        public int Market { get; set; }
        public string MarketplaceId { get; set; }

        public decimal? SalePrice { get; set; }
        public decimal? SFPSalePrice { get; set; }
        public decimal? SalePercent { get; set; }
        public bool ApplyToNewListings { get; set; }


        public DateTime CreateDate { get; set; }
        public DateTime? UpdateDate { get; set; }
        public long? CreatedBy { get; set; }
    }
}
