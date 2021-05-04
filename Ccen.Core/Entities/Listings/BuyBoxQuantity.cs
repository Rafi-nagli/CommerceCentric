using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;

namespace Amazon.Core.Entities.Listings
{
    public class BuyBoxQuantity
    {
        [Key]
        public long Id { get; set; }

        public int Market { get; set; }
        public string MarketplaceId { get; set; }
        public string ASIN { get; set; }
        public int? Quantity { get; set; }
        public string SellerNickname { get; set; }
        public DateTime CreateDate { get; set; }
    }
}
