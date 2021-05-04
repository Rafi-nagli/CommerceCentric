using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Amazon.DTO.Listings
{
    public class SkuMappingDTO
    {
        public int Id { get; set; }

        public string SKU { get; set; }
        public string DSSKU { get; set; }

        public DateTime CreateDate { get; set; }
        public long? CreatedBy { get; set; }
    }
}
