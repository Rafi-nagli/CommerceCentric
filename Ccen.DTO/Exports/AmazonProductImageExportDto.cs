using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Amazon.DTO.Exports
{
    public class AmazonProductImageExportDto
    {
        public int MessageID { get; set; }
        public string Update { get; set; }//Hardcode to word “update”

        public long? Id { get; set; }
        public string SKU { get; set; }
        public string ImageUrl { get; set; }
        public int ImageNumber { get; set; }

        public string Type { get; set; }
    }
}
