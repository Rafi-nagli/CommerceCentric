using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;

namespace Amazon.Core.Entities.Caches
{
    public class ListingCache : BaseCacheEntity
    {
        public int ItemId { get; set; }
        
        public int SoldQuantity { get; set; }
        public DateTime? MaxOrderDate { get; set; }
    }
}
