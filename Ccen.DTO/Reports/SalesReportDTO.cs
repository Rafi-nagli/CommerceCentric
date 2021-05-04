using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Amazon.DTO.Reports
{
    public class SalesReportDTO
    {
        public long StyleId { get; set; }
        public string StyleString { get; set; }

        public string FeatureValue { get; set; }

        public int NumberOfSoldUnits { get; set; }
        public decimal? AveragePrice { get; set; }
        public int? AveragePriceCount { get; set; }
        public decimal? MinCurrentPrice { get; set; }
        public decimal? MaxCurrentPrice { get; set; }
        public decimal? RemainingUnits { get; set; }
    }
}
