namespace Amazon.Core.Models
{
    public class LabelPrintResult
    {
        public long ShipmentId { get; set; }
        public long OrderId { get; set; }
        public string AmazonIdentifier { get; set; }
        public string Message { get; set; }
    }
}
