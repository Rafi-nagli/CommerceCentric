namespace Amazon.Core.Models.Items
{
    public class ItemBarcodeSearchFilter
    {
        public string Keywords { get; set; }
        public string CategoryId { get; set; }
        public decimal? MinPrice { get; set; }
        public decimal? MaxPrice { get; set; }

        public int StartIndex { get; set; }
        public int LimitCount { get; set; }
    }
}
