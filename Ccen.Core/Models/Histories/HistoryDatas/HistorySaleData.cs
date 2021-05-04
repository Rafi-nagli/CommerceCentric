using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Amazon.Core.Models.Histories.HistoryDatas
{
    public class HistorySaleData : IHistoryData
    {
        public decimal? SalePrice { get; set; }
        public DateTime? SaleStartDate { get; set; }
        public DateTime? SaleEndDate { get; set; }
        public int? SalePieces { get; set; }
    }
}
