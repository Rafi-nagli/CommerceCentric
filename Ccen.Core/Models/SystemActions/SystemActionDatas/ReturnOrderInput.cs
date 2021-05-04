using System;
using System.Collections.Generic;
using System.Linq;
using Amazon.Core.Contracts.SystemActions;
using Amazon.Core.Helpers;

namespace Amazon.Core.Models.SystemActions.SystemActionDatas
{
    public class ReturnOrderInput : ISystemActionInput
    {
        public long Id { get; set; }
        public long OrderId { get; set; }
        public string OrderNumber { get; set; }
        public string CustomerOrderNumber { get; set; }
        public string MarketOrderId { get; set; }
        public string OrderRefundId { get; set; }

        public string PaymentTransactionId { get; set; }

        public bool IncludeShipping { get; set; }
        public bool DeductShipping { get; set; }
        public bool IsDeductPrepaidLabelCost { get; set; }
        public int? RefundReason { get; set; }
        public int? RefundMoney { get; set; }
        public decimal? TotalPaidAmount { get; set; }

        public decimal? RefundAmount { get; set; }

        public bool ShippingFee { get; set; }
        public bool RestockingFee { get; set; }
        public string ChangingFeeReason { get; set; }

        public List<ReturnOrderItemInput> Items { get; set; }
        
        public override string ToString()
        {
            var text = ToStringHelper.ToString(this);
            text += ", Items=";
            if (Items != null)
                text += String.Join(", ", Items.Select(i => i.ToString()));
            else
                text += "[null]";
            return text;
        }
    }
}
