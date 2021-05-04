using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Amazon.DTO.Exports
{
    public class AmazonProductRelationshipExportDto
    {
        public int MessageID { get; set; }
        public string Update { get; set; }//Hardcode to word “update”

        public IList<int> ItemIds { get; set; }
        public string SKU { get; set; }
        public IList<string> VariationSKUs { get; set; }
    }
}
