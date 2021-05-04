using System;

namespace Amazon.DTO
{
    public class PriceHistoryDTO
    {
        //public long Id { get; set; }
        //public int ItemId { get; set; }
        public string SKU { get; set; }
        public DateTime ChangeDate { get; set; }
        public decimal Price { get; set; }

        public TimeSpan JsChangeDate
        {
            get { return ChangeDate.Subtract(new DateTime(1970, 1, 1).Date); }
        }
    }
}
