using Amazon.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Amazon.Core.Contracts
{
    public interface IShippingService
    {
        void ApplyCharges(IList<OrderShippingInfoDTO> shippings);
        decimal? ApplyCharges(int shippingMethodId, decimal? rateAmount);

        decimal GetUpCharge(int shippingMethodId, decimal? rateAmount);
    }
}
