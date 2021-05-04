using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;

namespace Amazon.Core.Entities
{
    public class QuantityHistory : BaseDateAndByEntity
    {
        [Key]
        public long Id { get; set; }
        public int QuantityChanged { get; set; }
        public int Type { get; set; }

        public long ListingId { get; set; }

        //Additional linked entity info (temporary storing additional info to easy migration to StyleId in future)
        public string SKU { get; set; }
        public string StyleString { get; set; }
        public long? StyleId { get; set; }
        public long? StyleItemId { get; set; }
        public string Size { get; set; }
        
        //Additional Info (also using to prevent double substraction)
        public string OrderId { get; set; }
    }
}
