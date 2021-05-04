
using System;

namespace Amazon.Core.Entities
{
    public class ZipCode
    {
        public long Id { get; set; }
        public string Zip{ get; set; }
        public long ZipIndex { get; set; }
        public decimal? AverageFirstClassDeliveryDays { get; set; }
        public decimal? AverageFlatDeliveryDays { get; set; }
        public decimal? AverageRegularDeliveryDays { get; set; }

        public DateTime? UpdateDate { get; set; }
        public DateTime? CreateDate { get; set; }
    }
}
