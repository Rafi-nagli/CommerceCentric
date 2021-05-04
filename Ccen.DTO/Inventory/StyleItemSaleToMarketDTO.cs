using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Amazon.DTO.Inventory
{
    public class StyleItemSaleToMarketDTO
    {
        public long Id { get; set; }

        public long SaleId { get; set; }
        public int Market { get; set; }
        public string MarketplaceId { get; set; }

        public decimal? SalePrice { get; set; }
        public decimal? SFPSalePrice { get; set; }
        public decimal? SalePercent { get; set; }
        public bool ApplyToNewListings { get; set; }
        //public bool ApplyToExistListings { get; set; }


        public DateTime CreateDate { get; set; }
        public long? CreatedBy { get; set; }


        public IList<StyleItemSaleToListingDTO> Listings { get; set; } 
    }
}
