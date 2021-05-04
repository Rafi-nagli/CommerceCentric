using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;

namespace Amazon.Core.Entities
{
    public class NodePosition : BaseDateEntity
    {
        [Key]
        public int Id { get; set; }
        public string ASIN { get; set; }
        public int? Market { get; set; }
        public string MarketplaceId { get; set; }

        public string NodeName { get; set; }
        public string NodeIdentifier { get; set; }
        public int Position { get; set; }
    }
}
