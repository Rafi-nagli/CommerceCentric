using System;

namespace Amazon.DTO.ScanOrders
{
    public class ScanOrderDTO
    {
        public long? Id { get; set; }
        public DateTime OrderDate { get; set; }
        public bool IsFBA { get; set; }
        public string Description { get; set; }
        public string FileName { get; set; }
    }
}
