using Amazon.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Amazon.Core.Models
{
    public class BatchInfoToPrint
    {
        public long BatchId { get; set; }
        public string BatchName { get; set; }
        public DateTime Date { get; set; }
        public int NumberOfPackages { get; set; }
        public string FIMSAirBillNumber { get; set; }

        public Dictionary<string, int> Carriers { get; set; }
        public IList<StyleChangeInfo> StyleChanges { get; set; }

        public IList<OrderShippingInfoDTO> OrdersWithPrintError { get; set; }
        public IList<OrderShippingInfoDTO> OrdersWasManuallyRemoved { get; set; }
    }
}
