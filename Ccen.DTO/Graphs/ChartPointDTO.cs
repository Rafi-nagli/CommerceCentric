using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Amazon.DTO.Graphs
{
    public class ChartPointDTO
    {
        public long Id { get; set; }
        public long ChartId { get; set; }
        public DateTime Date { get; set; }
        public decimal Value { get; set; }
    }
}
