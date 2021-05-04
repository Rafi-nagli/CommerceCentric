using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Amazon.DTO
{
    public class OrderSettlementDTO : IReportItemDTO
    {
        public decimal Fee { get; set; }
    }
}
