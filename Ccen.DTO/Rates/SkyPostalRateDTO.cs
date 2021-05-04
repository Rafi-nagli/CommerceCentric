using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Amazon.DTO.Orders
{
    public class SkyPostalRateDTO
    {
        public long Id { get; set; }        
        public int ServiceType { get; set; }
        public int Zone { get; set; }
        public decimal Weight { get; set; }
        public decimal Rate { get; set; }
        public int StartZip { get; set; }
        public int EndZip { get; set; }
    }
}
