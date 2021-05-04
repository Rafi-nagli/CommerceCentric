using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Amazon.DTO;

namespace Amazon.Model.Implementation.Labels
{
    public interface ILabelPurchaser
    {
        decimal ExpectedCost { get; }
        decimal ActualCost { get; }

        void PurshaseLabels(IList<OrderShippingInfoDTO> shippings, Action<OrderShippingInfoDTO> purchaseCallback);
    }
}
