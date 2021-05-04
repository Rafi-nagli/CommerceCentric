using Ccen.DTO.Trackings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ccen.Core.Contracts
{
    public interface ITrackingNumberService
    {
        CustomTrackingNumberDTO AttachTrackingNumber(long shippingInfoId,
            string sourceTrackingNumber,
            DateTime when,
            long? by);


        bool HasAvailableTrackingNumber();
        bool ApplyTrackingNumber(string filename, string trackingNumber);
    }
}
