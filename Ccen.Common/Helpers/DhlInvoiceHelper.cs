using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Amazon.Core.Models;

namespace Amazon.Common.Helpers
{
    public class DhlInvoiceHelper
    {
        public static string ToString(DhlInvoiceStatusEnum status)
        {
            switch ((DhlInvoiceStatusEnum)status)
            {
                case DhlInvoiceStatusEnum.Matched:
                    return "Matched";
                case DhlInvoiceStatusEnum.Incorrect:
                    return "Incorrect";
                case DhlInvoiceStatusEnum.DhlNotified:
                    return "Dhl Notified";
                case DhlInvoiceStatusEnum.OrderNotFound:
                    return "Order Not Found";
                case DhlInvoiceStatusEnum.RefundApproved:
                    return "Refund Approved";
                case DhlInvoiceStatusEnum.Rejected:
                    return "Rejected";
                default:
                    return "-";
            }
        }
    }
}
