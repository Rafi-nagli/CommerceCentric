using System;
using System.Collections.Generic;

namespace Amazon.DTO
{
    public class EmailInfoDTO
    {
        public long Id { get; set; }
        public string OrderId { get; set; }
        public string BuyerName { get; set; }
        public string Email { get; set; }
        public DateTime? FeedbackRequestDate { get; set; }
        public bool IsEmailed { get; set; }
        public IEnumerable<string> ItemNames { get; set; }
    }
}
