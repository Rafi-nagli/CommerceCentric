using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Amazon.DTO.Inventory
{
    public class QuantityOperationDTO
    {
        public long Id { get; set; }
        public int Type { get; set; }
        public string OrderId { get; set; }
        public int? Market { get; set; }
        public string MarketplaceId { get; set; }
        public string Comment { get; set; }
        public DateTime? CreateDate { get; set; }
        public long? CreatedBy { get; set; }
        public string CreatedByName { get; set; }

        public IList<QuantityChangeDTO> QuantityChanges { get; set; }
        public IEnumerable<QuantityChangeDTO> QuantityChangesEnumerable { get; set; }
    }
}
