using Amazon.Core.Helpers;

namespace Amazon.Core.Models.SystemActions.SystemActionDatas
{
    public class ReturnOrderItemInput
    {
        public string ItemOrderId { get; set; }
        public int Quantity { get; set; }
        public decimal? TotalPaidAmount { get; set; }
        public decimal RefundItemPrice { get; set; }
        public decimal RefundItemTax { get; set; }
        public decimal RefundShippingPrice { get; set; }
        public decimal RefundShippingTax { get; set; }
        public decimal DeductShippingPrice { get; set; }
        public decimal DeductPrepaidLabelCost { get; set; }

        public decimal AdjustmentRefund { get; set; }
        public decimal AdjustmentFee { get; set; }

        public string Reason { get; set; }
        public string Feedback { get; set; }
        public string Notes { get; set; }

        public override string ToString()
        {
            return ToStringHelper.ToString(this);
        }
    }
}
