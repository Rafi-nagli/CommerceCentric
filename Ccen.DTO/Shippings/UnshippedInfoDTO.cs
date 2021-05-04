
using System;

namespace Amazon.DTO
{
    public class UnshippedInfoDTO
    {
        public int Count { get; set; }
        public decimal Price { get; set; }
        //public DateTime? LastProcessedDate { get; set; }
        public string LastProcessedDate { get; set; }
    }
}
