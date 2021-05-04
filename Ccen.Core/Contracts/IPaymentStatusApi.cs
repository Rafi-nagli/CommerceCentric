using Amazon.DTO.Orders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Amazon.Core.Contracts
{
    public interface IPaymentStatusApi
    {
        PaymentInfoDTO GetOrderRisk(string orderId);
    }
}
