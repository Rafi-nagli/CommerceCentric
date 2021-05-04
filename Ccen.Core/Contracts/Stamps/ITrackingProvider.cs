using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Amazon.Core.Models;
using Amazon.Core.Models.Settings;
using Amazon.DTO.Orders;

namespace Amazon.Core.Contracts.Stamps
{
    public interface ITrackingProvider
    {
        CarrierGroupType Carrier { get; }
        IList<TrackingState> TrackShipments(IList<TrackingNumberToCheckDto> trackingNumbers);
    }
}
