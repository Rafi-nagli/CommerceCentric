using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Amazon.DTO.Graphs
{
    public class ChartDTO
    {
        public long Id { get; set; }
        public string ChartName { get; set; }
        public string ChartSubGroup { get; set; }
        public string ChartTag { get; set; }
        public DateTime CreateDate { get; set; }
    }
}
