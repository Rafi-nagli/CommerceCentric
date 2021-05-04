using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Amazon.Core;
using Amazon.Core.Models;
using Amazon.Core.Models.Stamps;
using Amazon.DTO;

namespace Amazon.Model.Implementation.Trackings.Rules
{
    public interface ITrackingRule
    {
        bool IsAccept(OrderToTrackDTO orderToTrackInfo, 
            string status, 
            DateTime? statusDate,
            IList<TrackingRecord> records);

        void Process(IUnitOfWork db,
            OrderToTrackDTO shipping,
            string status,
            DateTime? statusDate,
            IList<TrackingRecord> records,
            DateTime when);
    }
}
